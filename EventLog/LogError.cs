using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EventLog
{
    public class LogError
    {
        private static string sApplication { get; set; }
        private static string sSource { get; set; }

        public static void WriteError(string Application, string Source, Exception ex)
        {
            LogError.sApplication = Application;
            LogError.sSource = Source;
            if (!System.Diagnostics.EventLog.Exists(LogError.sApplication) && !System.Diagnostics.EventLog.SourceExists(LogError.sSource))
            {
                System.Diagnostics.EventLog.CreateEventSource(LogError.sSource, LogError.sApplication);
            }
            try
            {
                System.Diagnostics.EventLog oLog = new System.Diagnostics.EventLog();
                oLog.Source = LogError.sSource;
                oLog.WriteEntry(String.Format("\r\n\r\nApplication Error:\r\n\r\n" +
                                             "Message: {0}\r\n" +
                                             "Query: {1}\r\n" +
                                             "Target: {2}\r\n" +
                                             "StackTrace: {3}\r\n",
                                             ex.Message,
                                             ex.Source,
                                             ex.TargetSite,
                                             ex.StackTrace
                                             ),
                                             System.Diagnostics.EventLogEntryType.Error);
            }
            catch (Exception)
            {

            }
        }

        public static void WriteMessage(string Application, string Source, string Message)
        {
            LogError.sApplication = Application;
            LogError.sSource = Source;
            if (!System.Diagnostics.EventLog.Exists(LogError.sApplication))
            {
                System.Diagnostics.EventLog.CreateEventSource(LogError.sSource, LogError.sApplication);
            }
            try
            {
                System.Diagnostics.EventLog oLog = new System.Diagnostics.EventLog();
                oLog.Source = LogError.sSource;
                oLog.WriteEntry(Message, System.Diagnostics.EventLogEntryType.Information);
            }
            catch (Exception)
            {

            }
        }
    }
}
