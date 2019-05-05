using System;
using System.Management.Automation;
using System.Windows.Forms;
using ProgressReporting;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using Humanizer;

namespace SuperLTI
{
    public partial class ProgressDialogHost : Form
    {
        private ProgressReporter progReport = new ProgressReporter();
        private int ProgressIntervalPercent = 0;
        private string ProgressIntervalDetails = " ";
        private string ProgressIntervalTitle = "SuperLTI";
        private ProgressDialog progDialog = null;
        private int previousProgress = 0;
        private Timer ProgressInterval = new Timer();
        private Timer CancelInterval = new Timer();
        private Progress<ZipProgress> ZipProgress = new Progress<ZipProgress>();
        private PowerShell ps = null;
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
            progDialog.Line3 = "Calculating Time Remaining...";
            progDialog.ShowDialog(
                ProgressDialog.PROGDLG.Modal |
                ProgressDialog.PROGDLG.Normal
            );
        }

        private void ProgressInterval_Tick(object sender, EventArgs e)
        {
            UpdateProgress(ProgressIntervalPercent, ProgressIntervalTitle, ProgressIntervalDetails);
        }

        private async void ProgressDialogHost_Load(object sender, EventArgs e)
        {
            progReport.Start(100);
            ZipProgress.ProgressChanged += ZipProgress_ProgressChanged;
            CancelInterval.Interval = 100;
            CancelInterval.Start();
            CancelInterval.Tick += CancelInterval_Tick;
            ProgressInterval.Interval = 100;
            ProgressInterval.Tick += ProgressInterval_Tick;
            ProgressInterval.Start();
            await CopyAndExtractTask();
            ProgressInterval.Stop();
            ps = PowerShell.Create();
            ps.Streams.Progress.DataAdded += Progress_DataAdded;
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
                string format = "0.0";
                ProgressIntervalTitle = "Copying files...";
                Copy.CopyTo(zip, new FileInfo(@"C:\SuperLTI\SuperLTI.zip"), new Action<int>((int progress) => {
                    string copied = ((double)totalBytes * ((double)progress / 100)).Bytes().Humanize(format);
                    ProgressIntervalDetails = copied + " / " + totalBytes.Bytes().Humanize(format);
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
                UpdateProgress(progress.PercentComplete, progress.Activity, progress.StatusDescription);
            }
        }

        private string GenerateTimeRemaining()
        {
            string str = " ";
            if(progReport.RemainingTimeEstimate.Seconds >= 1)
            {
                str = progReport.RemainingTimeEstimate.Humanize();
            }
            return str;
        }

        private void UpdateProgress(int Percent, string Activity = null, string Status = null)
        {
            if (Percent < previousProgress)
            {
                progReport.Restart(100);
            }
            else
            {
                progReport.ReportProgress(Percent);
            }
            BeginInvoke(new Action(() => {
                if (Activity != null)
                {
                    progDialog.Line1 = Activity;
                }
                if (Status != null)
                {
                    progDialog.Line2 = Status;
                }
                progDialog.Line3 = GenerateTimeRemaining();
                progDialog.Value = (uint)Percent;
            }));
            previousProgress = Percent;
        }
    }
}
