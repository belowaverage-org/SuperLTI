using System;
using System.Management.Automation;
using System.Windows.Forms;
using ProgressReporting;

namespace SuperLTI
{
    public partial class ProgressDialogHost : Form
    {
        private ProgressReporter progReport = new ProgressReporter();

        private ProgressDialog progDialog = null;

        private int previousProgress = 0;
        public ProgressDialogHost()
        {
            InitializeComponent();
            progDialog = new ProgressDialog(Handle);
            progDialog.Title = "SuperLTI";
            progDialog.CancelMessage = "Cancelling operation...";
            progDialog.Maximum = 100;
            progDialog.Line1 = " ";
            progDialog.Line2 = " ";
            progDialog.Line3 = "Calculating Time Remaining...";
            progDialog.ShowDialog(
                ProgressDialog.PROGDLG.Normal
            );
            PowerShell ps = PowerShell.Create();
            ps.Streams.Progress.DataAdded += Progress_DataAdded;
            ps.InvocationStateChanged += Ps_InvocationStateChanged;
            ps.AddScript(Properties.Resources.SuperLTI);
            ps.BeginInvoke();
            progReport.Start(100);
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
                if(progress.PercentComplete < previousProgress)
                {
                    progReport.Restart(100);
                }
                else
                {
                    progReport.ReportProgress(progress.PercentComplete);
                }
                BeginInvoke(new Action(() => {
                    progDialog.Line1 = progress.Activity;
                    progDialog.Line2 = progress.StatusDescription;
                    progDialog.Line3 = GenerateTimeRemaining();
                    progDialog.Value = (uint)progress.PercentComplete;
                }));
                previousProgress = progress.PercentComplete;
            }
        }

        private string GenerateTimeRemaining()
        {
            string str = TimeSpan2.FromSeconds(progReport.RemainingTimeEstimate.Seconds).ToString("f");
            if(str == "")
            {
                str = " ";
            }
            return str;
        }
    }
}
