using System.Management.Automation;
using System.Windows.Forms;

namespace SuperLTI
{
    public partial class ProgressDialogHost : Form
    {
        private ProgressDialog progDialog = null;
        public ProgressDialogHost()
        {
            InitializeComponent();
            progDialog = new ProgressDialog(Handle);
            progDialog.Title = "SuperLTI";
            progDialog.CancelMessage = "Please wait while the operation is cancelled";
            progDialog.Maximum = 100;
            progDialog.Value = 50;
            progDialog.Line1 = "Line One";
            progDialog.Line2 = " ";
            progDialog.Line3 = "Calculating Time Remaining...";
            progDialog.ShowDialog(
                ProgressDialog.PROGDLG.AutoTime |
                ProgressDialog.PROGDLG.NoMinimize |
                ProgressDialog.PROGDLG.Normal
            );
            /*PowerShell ps = PowerShell.Create();
            ps.Streams.Progress.DataAdded += Progress_DataAdded;
            ps.InvocationStateChanged += Ps_InvocationStateChanged;
            ps.AddScript(Properties.Resources.SuperLTI);
            ps.BeginInvoke();*/
        }

        private void Ps_InvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
        {
            if (e.InvocationStateInfo.State.ToString() == "Running")
            {
                //progressBar.Style = ProgressBarStyle.Continuous;
            }
            if (e.InvocationStateInfo.State.ToString() == "Completed")
            {
                //progDialog.
                Application.Exit();
            }
        }

        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<ProgressRecord> progressRecords = (PSDataCollection<ProgressRecord>)sender;
            ProgressRecord progress = progressRecords[e.Index];
            if (progress.PercentComplete >= 0 && progress.PercentComplete <= 100)
            {
                progDialog.Value = 50;
                //progDialog.Value = (uint)progress.PercentComplete;
            }
            //statusLbl.Text = progress.StatusDescription;
        }
    }
}
