using SimpleLogger;
using SimpleLogger.Logging.Handlers;
using System;
using System.Configuration;
using System.IO;

namespace CertificateSorter.Helpers
{
    public class LoggingHelper
    {
        public static void Initialize()
        {
            var logFileLocation = ConfigurationManager.AppSettings["LogDirectory"];
            if (!Directory.Exists(logFileLocation))
                Directory.CreateDirectory(logFileLocation);
            var logFolderName = $"{DateTime.Now:yyyy-MM-dd}";
            var fullLogFileName = $"{Path.Combine(logFileLocation, logFolderName)}.log";
            Logger.LoggerHandlerManager.AddHandler(new FileLoggerHandler(fullLogFileName));
        }
    }
}
