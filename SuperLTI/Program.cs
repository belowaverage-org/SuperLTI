using System;
using System.Management.Automation;
using System.Windows.Forms;

namespace SuperLTI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new ProgressDialogHost());
        }
    }
}
