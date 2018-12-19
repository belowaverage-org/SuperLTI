using System.Windows.Forms;
using System.Management.Automation;

namespace LTInstall
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            Icon = Properties.Resources.icon;
            PowerShell ps = PowerShell.Create();
            ps.Streams.Progress.DataAdded += Progress_DataAdded;
            ps.InvocationStateChanged += Ps_InvocationStateChanged;
            ps.AddScript(Properties.Resources.install);
            ps.BeginInvoke();
        }

        private void Ps_InvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
        {
            if (e.InvocationStateInfo.State.ToString() == "Running")
            {
                progressBar.Style = ProgressBarStyle.Continuous;
            }
            if (e.InvocationStateInfo.State.ToString() == "Completed")
            {
                Application.Exit();
            }
        }

        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<ProgressRecord> progressRecords = (PSDataCollection<ProgressRecord>)sender;
            ProgressRecord progress = progressRecords[e.Index];
            progressBar.BeginInvoke(new MethodInvoker(delegate {
                if (progress.PercentComplete >= 0 && progress.PercentComplete <= 100)
                {
                    progressBar.Value = progress.PercentComplete;
                }
            }));
            statusLbl.BeginInvoke(new MethodInvoker(delegate {
                statusLbl.Text = progress.StatusDescription;
            }));
        }
    }
}
