using System;
using System.Windows.Forms;

namespace SuperLTI
{
    /// <summary>
    /// The PowerShell runspace passed to the "install engine's" PowerShell instance.
    /// This allows the PowerShell script to access these properties and methods from PowerShell.
    /// </summary>
    public class PSRunspace
    {
        /// <summary>
        /// Get / set the title of the main window.
        /// </summary>
        public string WindowTitle
        {
            get
            {
                return Program.MainForm.ProgressDialog.Title;
            }
            set
            {
                Program.MainForm.BeginInvoke(new Action(() => {
                    Program.MainForm.ProgressDialog.Title = value;
                }));
            }
        }
        /// <summary>
        /// Gets / sets the TopMost status of the main window. True means that the main window is always on top.
        /// </summary>
        public bool WindowTopMost
        {
            get
            {
                return Program.MainForm.TopMost;
            }
            set
            {
                Program.MainForm.Invoke(new Action(() => {
                    Program.MainForm.TopMost = value;
                }));
            }
        }
        /// <summary>
        /// Gets the raw SuperLTI arguments that were passed to SuperLTI on launch.
        /// </summary>
        public string[] RawArguments
        {
            get
            {
                return Program.Arguments;
            }
        }
    }
}
