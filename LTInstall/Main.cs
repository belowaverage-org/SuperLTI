using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            ps.AddScript(Properties.Resources.install);
            ps.Streams.Progress.DataAdded += Progress_DataAdded;
            ps.InvocationStateChanged += Ps_InvocationStateChanged;
            ps.BeginInvoke();
            
        }

        private void Ps_InvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
        {
            if (e.InvocationStateInfo.State.ToString() == "Running")
            {
                progressBar.Style = ProgressBarStyle.Continuous;
            }
            if(e.InvocationStateInfo.State.ToString() == "Completed")
            {
                Application.Exit();
            }
            
        }

        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<ProgressRecord> progressRecords = (PSDataCollection<ProgressRecord>)sender;
            ProgressRecord progress = progressRecords[e.Index];
            progressBar.Value = progress.PercentComplete;
            statusLbl.Text = progress.StatusDescription;
        }
    }
}
