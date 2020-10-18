using System.Diagnostics;

namespace SuperLTI
{
    public static class Logger
    {
        public static void WriteEventLog(string logText, EventLogEntryType messageType)
        {
            EventLog eventLog = new EventLog("Application")
            {
                Source = "SuperLTI"
            };
            if (!EventLog.SourceExists(eventLog.Source))
            {
                EventLog.CreateEventSource("SuperLTI", "Application");
            }
            eventLog.WriteEntry(logText, messageType);
        }
    }
}
