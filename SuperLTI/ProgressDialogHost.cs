using System;
using System.Management.Automation;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;

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
        public ProgressDialogHost()
        {
            InitializeComponent();
            Icon = Properties.Resources.icon;
            progDialog = new ProgressDialog(Handle);
            progDialog.Title = "SuperLTI";
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
            if(!IgnoreProgressUI)
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
            if (!IgnoreProgressUI)
            {
                ps.Streams.Progress.DataAdded += Progress_DataAdded;
            }
            ps.InvocationStateChanged += Ps_InvocationStateChanged;
            ps.AddScript(Properties.Resources.SuperLTI);
            ps.BeginInvoke();
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
                FileInfo zip = new FileInfo("SuperLTI.zip");
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

        private void Ps_InvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
        {
            if (e.InvocationStateInfo.State.ToString() == "Completed")
            {
                Application.Exit();
            }
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
