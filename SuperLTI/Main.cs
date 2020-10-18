using System;
using System.Management.Automation;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace SuperLTI
{
    public partial class Main : Form
    {
        public ProgressDialog ProgressDialog = null;
        private PowerShell ps = null;
        private int ProgressIntervalPercent = 0;
        private string ProgressIntervalDetails = " ";
        private string ProgressIntervalTitle = "SuperLTI";
        private string[] Arguments = new string[0];
        private string ZipPath = string.Empty;
        private readonly Runspace rs = RunspaceFactory.CreateRunspace();
        private readonly Timer ProgressInterval = new Timer();
        private readonly Timer CancelInterval = new Timer();
        private readonly Progress<ZipProgress> ZipProgress = new Progress<ZipProgress>();
        private readonly bool IgnoreProgressUI = false;
        private readonly string WindowGUID = Guid.NewGuid().ToString();
        private readonly IntPtr dialog = IntPtr.Zero;
        private readonly Icon dialogIcon = new Icon(Properties.Resources.icon, 16, 16);
        public Main(string[] args)
        {
            Arguments = args;
            InitializeComponent();
            Icon = Properties.Resources.icon;
            ProgressDialog = new ProgressDialog(Handle)
            {
                Title = WindowGUID,
                CancelMessage = "Stopping SuperLTI...",
                Maximum = 100,
                Line1 = " ",
                Line2 = " ",
                Line3 = " "
            };
            try
            {
                ProgressDialog.ShowDialog(
                    ProgressDialog.PROGDLG.Modal |
                    ProgressDialog.PROGDLG.AutoTime |
                    ProgressDialog.PROGDLG.Normal
                );
                dialog = Win32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "#32770", WindowGUID);
                ProgressDialog.ResetTimer();
            }
            catch (OutOfMemoryException) //Win32 Progress Dialog failed to show.
            {
                IgnoreProgressUI = true;
            }
        }
        private void ProgressInterval_Tick(object sender, EventArgs e)
        {
            UpdateProgress(ProgressIntervalPercent, ProgressIntervalTitle, ProgressIntervalDetails);
        }
        private async void ProgressDialogHost_Load(object sender, EventArgs e)
        {
            await ReadArguments();
            if (!IgnoreProgressUI)
            {
                ZipProgress.ProgressChanged += ZipProgress_ProgressChanged;
                CancelInterval.Interval = 1000;
                CancelInterval.Start();
                CancelInterval.Tick += CancelInterval_Tick;
                ProgressInterval.Interval = 250;
                ProgressInterval.Tick += ProgressInterval_Tick;
                ProgressInterval.Start();
            }
            await CopyAndExtractTask();
            ps = PowerShell.Create();
            ps.Runspace = rs;
            rs.Open();
            rs.SessionStateProxy.SetVariable("SuperLTI", new PSRunspace());
            if (!IgnoreProgressUI)
            {
                ps.Streams.Progress.DataAdded += Progress_DataAdded;
            }
            for(var count = 0; count < Arguments.Length; count++)
            {
                if(count % 2 == 0)
                {
                    ps.AddCommand("Set-Variable");
                    ps.AddParameter("Name", Arguments[count]);
                }
                else
                {
                    ps.AddParameter("Value", Arguments[count]);
                }
            }
            ps.AddScript(Properties.Resources.SuperLTI);
            await Task.Run(() => {
                ps.Invoke();
                Application.Exit();
            });
        }
        private Task ReadArguments()
        {
            return Task.Run(() => {
                // packageName represents the custom zip and folder we're going to look for
                // Defaults to the name of the SuperLTI executable
                string packageName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                ZipPath = $"{packageName}.zip";
                // check if args contains the switch /SuperLTIPackage to specify a folder and/or zip name
                // Use the commandline option if it exists
                if (Arguments.Length > 1 && Array.Exists(Arguments, arg => arg.Equals("/SuperLTIPackage", StringComparison.OrdinalIgnoreCase)))
                {
                    // Find the index in the array where /SuperLTIName exists, case insensitve, as long as exists
                    int index = Array.FindIndex(Arguments, arg => arg.IndexOf("/SuperLTIPackage", StringComparison.OrdinalIgnoreCase) >= 0);
                    // The data will be the one right after the switch
                    packageName = Arguments[index + 1].ToString();
                    if (Arguments.Length > 2)
                    {
                        // Remove the SuperLTI option from args and pass the rest to the script
                        List<string> argsList = new List<string>(Arguments);
                        argsList.RemoveAt(index);
                        argsList.RemoveAt(index + 1);
                        Arguments = argsList.ToArray();
                    }
                    else
                    {
                        // No other args, so clear it
                        Arguments = new string[0];
                    }
                }
                // If only one argument is passed, treat it as the ZipPath. (This allows users to drag and drop a zip over the SuperLTI executable).
                if (Arguments.Length == 1)
                {
                    ZipPath = Arguments[0];
                }
                if (!File.Exists(ZipPath))
                {
                    ZipPath = FindZip(packageName);
                }
                if (string.IsNullOrEmpty(ZipPath))
                {
                    Logger.WriteEventLog("Could not find a suitible SuperLTI zip file! SuperLTI is now exiting...", EventLogEntryType.Error);
                    Application.Exit();
                }
            });
        }
        private string FindZip(string packageName)
        {
            // packageName.zip
            // packageName/packageName.zip
            // packageName/SuperLTI.zip
            // SuperLTI.zip
            string zipName = string.Empty;
            // If the user gave us something with an extension make sure it's valid
            if (Path.HasExtension(packageName) && !Path.GetExtension(packageName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                Logger.WriteEventLog($"/SuperLTIPackage was given {packageName}, which is not a zip file. Attempting to match anyway", EventLogEntryType.Warning);
                packageName = Path.GetFileNameWithoutExtension(packageName);
            }
            // Determine if the user gave us a name or a full zip name
            if (Path.HasExtension(packageName))
            {
                if (File.Exists(packageName)) { return packageName; }
            }
            else
            {
                zipName = $"{packageName}.zip";
                if (File.Exists(zipName)) { return zipName; }
            }
            // Just a name, look for a folder
            if (Directory.Exists(packageName))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(packageName);
                FileInfo[] fileInfo = directoryInfo.GetFiles();
                if (Array.FindIndex(fileInfo, file => file.Name.Equals(zipName, StringComparison.OrdinalIgnoreCase)) > -1)
                {
                    // packagename/packagename.zip
                    return Array.Find(fileInfo, file => file.Name.Equals(zipName, StringComparison.OrdinalIgnoreCase)).FullName;
                }
                else if (Array.FindIndex(fileInfo, file => file.Name.Equals("SuperLTI.zip", StringComparison.OrdinalIgnoreCase)) > -1)
                {
                    // packagename/superlti.zip
                    return Array.Find(fileInfo, file => file.Name.Equals("SuperLTI.zip", StringComparison.OrdinalIgnoreCase)).FullName;
                }
            }
            // SuperLTI.zip
            if (File.Exists("SuperLTI.zip"))
            {
                return "SuperLTI.zip";
            }
            else
            {
                // No Zip and No Directory. So nothing
                Logger.WriteEventLog($"SuperLTI couldn't find {Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\{zipName}, {packageName}\\{zipName}, or {packageName}\\SuperLTI.zip", EventLogEntryType.Error);
                return string.Empty;
            }
        }
        private void CancelInterval_Tick(object sender, EventArgs e)
        {
            if(ProgressDialog.HasUserCancelled)
            {
                if(ps != null)
                {
                    ps.Stop();
                }
                Application.Exit();
            }
        }
        private void ZipProgress_ProgressChanged(object sender, ZipProgress e)
        {
            ProgressIntervalDetails = e.CurrentItem;
            ProgressIntervalPercent = (int)Math.Round((double)e.Processed / (double)e.Total);
        }
        private Task CopyAndExtractTask()
        {
            return Task.Run(() =>
            {
                Directory.CreateDirectory(@"C:\SuperLTI");
                FileInfo zip = new FileInfo(ZipPath);
                long totalBytes = zip.Length;
                ProgressIntervalTitle = "Copying files...";
                Copy.CopyTo(zip, new FileInfo(@"C:\SuperLTI\SuperLTI.zip"), new Action<int>((int progress) => {
                    double copied = ((double)totalBytes * ((double)progress / 100));
                    ProgressIntervalDetails = copied.ByteHumanize() + " / " + totalBytes.ByteHumanize();
                    ProgressIntervalPercent = progress;
                }));
                ProgressIntervalTitle = "Expanding files...";
                MyZipFileExtensions.ExtractToDirectory(ZipFile.Open(@"C:\SuperLTI\SuperLTI.zip", ZipArchiveMode.Read), @"C:\SuperLTI", ZipProgress);
            });
        }
        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<ProgressRecord> progressRecords = (PSDataCollection<ProgressRecord>)sender;
            ProgressRecord progress = progressRecords[e.Index];
            if (progress.PercentComplete >= 0 && progress.PercentComplete <= 100)
            {
                ProgressIntervalTitle = progress.Activity;
                ProgressIntervalDetails = progress.StatusDescription;
                ProgressIntervalPercent = progress.PercentComplete;
            }
        }
        private void UpdateProgress(int Percent, string Activity = null, string Status = null)
        {
            Win32.SendMessage(dialog, 0x80, 0, dialogIcon.Handle);
            Win32.SendMessage(dialog, 0x80, 1, Properties.Resources.icon.Handle);
            BeginInvoke(new Action(() => {
                if(ProgressDialog.Value > (uint)Percent)
                {
                    ProgressDialog.ResetTimer();
                }
                if (Activity != null)
                {
                    ProgressDialog.Line1 = Activity;
                }
                if (Status != null)
                {
                    ProgressDialog.Line2 = Status;
                }
                ProgressDialog.Value = (uint)Percent;
            }));
        }
    }
}
