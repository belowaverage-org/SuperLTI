using System;
using System.Management.Automation;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Drawing;

namespace SuperLTI
{
    public partial class ProgressDialogHost : Form
    {
        private int ProgressIntervalPercent = 0;
        private string ProgressIntervalDetails = " ";
        private string ProgressIntervalTitle = "SuperLTI";
        private ProgressDialog progDialog = null;
        private Timer ProgressInterval = new Timer();
        private Timer CancelInterval = new Timer();
        private Progress<ZipProgress> ZipProgress = new Progress<ZipProgress>();
        private PowerShell ps = null;
        private bool IgnoreProgressUI = false;
        private string[] Arguments = new string[0];
        private string ZipPath = string.Empty;
        private string WindowGUID = Guid.NewGuid().ToString();
        private IntPtr dialog = IntPtr.Zero;
        private Icon dialogIcon = new Icon(Properties.Resources.icon, 16, 16);
        public ProgressDialogHost(string[] args, string zipPath)
        {
            Arguments = args;
            ZipPath = zipPath;
            InitializeComponent();
            Icon = Properties.Resources.icon;
            progDialog = new ProgressDialog(Handle);
            progDialog.Title = WindowGUID;
            progDialog.CancelMessage = "Stopping SuperLTI...";
            progDialog.Maximum = 100;
            progDialog.Line1 = " ";
            progDialog.Line2 = " ";
            progDialog.Line3 = " ";
            try
            {
                progDialog.ShowDialog(
                    ProgressDialog.PROGDLG.Modal |
                    ProgressDialog.PROGDLG.AutoTime |
                    ProgressDialog.PROGDLG.Normal
                );
                dialog = Win32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "#32770", WindowGUID);
                progDialog.ResetTimer();
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
            // Add a var to indicate SuperLTI is what's starting the process since the script can't rely on SuperLTI.exe being the starting process
            ps.AddCommand("Set-Content").AddParameter("Path", "env:SuperLTI").AddParameter("Value", "$True");
            
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
        private void CancelInterval_Tick(object sender, EventArgs e)
        {
            if(progDialog.HasUserCancelled)
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
                progDialog.Title = "SuperLTI";
                if(progDialog.Value > (uint)Percent)
                {
                    progDialog.ResetTimer();
                }
                if (Activity != null)
                {
                    progDialog.Line1 = Activity;
                }
                if (Status != null)
                {
                    progDialog.Line2 = Status;
                }
                progDialog.Value = (uint)Percent;
            }));
        }
    }
}
