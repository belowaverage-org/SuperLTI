using System;
using System.Management.Automation;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

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
            // packageName represents the custom zip and folder we're going to look for
            // Defaults to the name of the SuperLTI executable
            string packageName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            string zipPath = $"{packageName}.zip";

            // check if args contains the switch /SuperLTIPackage to specify a folder and/or zip name
            // Use the commandline option if it exists
            if (args.Length > 1 && Array.Exists(args, arg => arg.Equals("/SuperLTIPackage", StringComparison.OrdinalIgnoreCase)))
            {
                // Find the index in the array where /SuperLTIName exists, case insensitve, as long as exists
                int index = Array.FindIndex(args, arg => arg.IndexOf("/SuperLTIPackage", StringComparison.OrdinalIgnoreCase) >= 0);
                // The data will be the one right after the switch
                packageName = args[index + 1].ToString();
                
                if (args.Length > 2)
                {
                    // Remove the SuperLTI option from args and pass the rest to the script
                    List<string> argsList = new List<string>(args);
                    argsList.RemoveAt(index);
                    argsList.RemoveAt(index + 1);
                    args = argsList.ToArray();
                }
                else
                {
                    // No other args, so clear it
                    args = new string[0];
                }
            }

            if (! File.Exists(zipPath))
            {
                zipPath = FindZip(packageName);
            }            

            if (string.IsNullOrEmpty(zipPath))
            {
                Application.Exit();
            } 
            else
            {
                Application.Run(new ProgressDialogHost(args, zipPath));
            }
        }

        private static void WriteEventLog(string logText, EventLogEntryType messageType)
        {
            EventLog eventLog = new EventLog("Application");
            eventLog.Source = "SuperLTI";

            if (!EventLog.SourceExists(eventLog.Source)){
                EventLog.CreateEventSource("SuperLTI", "Application");
            }
            eventLog.WriteEntry(logText, messageType);
    }

        private static string FindZip(string packageName)
        {
            // packageName.zip
            // packageName/packageName.zip
            // packageName/SuperLTI.zip
            // SuperLTI.zip

            string zipName = string.Empty;

            // If the user gave us something with an extension make sure it's valid
            if (Path.HasExtension(packageName) && ! Path.GetExtension(packageName).Equals(".zip", StringComparison.OrdinalIgnoreCase)){
                WriteEventLog($"/SuperLTIPackage was given {packageName}, which is not a zip file. Attempting to match anyway", EventLogEntryType.Warning);
                packageName = Path.GetFileNameWithoutExtension(packageName);
            }

            // Determine if the user gave us a name or a full zip name
            if (Path.HasExtension(packageName)) 
            {
                if (File.Exists(packageName)) { return packageName; }
            } 
            else
            {
                zipName = $"{packageName}.zip";
                if (File.Exists(zipName)) { return zipName; }
            }

            // Just a name, look for a folder
            if (Directory.Exists(packageName))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(packageName);
                FileInfo[] fileInfo = directoryInfo.GetFiles();

                if (Array.FindIndex(fileInfo, file => file.Name.Equals(zipName, StringComparison.OrdinalIgnoreCase)) > -1)
                { 
                    // packagename/packagename.zip
                    return Array.Find(fileInfo, file => file.Name.Equals(zipName, StringComparison.OrdinalIgnoreCase)).FullName;
                }
                else if (Array.FindIndex(fileInfo, file => file.Name.Equals("SuperLTI.zip", StringComparison.OrdinalIgnoreCase)) > -1)
                {                    
                    // packagename/superlti.zip
                    return Array.Find(fileInfo, file => file.Name.Equals("SuperLTI.zip", StringComparison.OrdinalIgnoreCase)).FullName;
                }
            }

            // SuperLTI.zip
            if (File.Exists("SuperLTI.zip")) {
                return "SuperLTI.zip";
            }
            else
            {
                // No Zip and No Directory. So nothing
                WriteEventLog($"SuperLTI couldn't find {Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\{zipName}, {packageName}\\{zipName}, or {packageName}\\SuperLTI.zip", EventLogEntryType.Error);
                return string.Empty;
            }            
        }
    }
}
