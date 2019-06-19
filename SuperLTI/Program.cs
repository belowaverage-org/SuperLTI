using System;
using System.Management.Automation;
using System.Windows.Forms;
using System.IO;

namespace SuperLTI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (File.Exists("SuperLTI.zip"))
            {
                Application.Run(new ProgressDialogHost(args));
            }
        }
    }
}
