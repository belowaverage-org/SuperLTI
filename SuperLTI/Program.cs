using System;
using System.Windows.Forms;

namespace SuperLTI
{
    static class Program
    {
        public static Main MainForm;
        public static string[] Arguments;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Arguments = args;
            MainForm = new Main(args);
            Application.Run(MainForm);
        }
    }
}
