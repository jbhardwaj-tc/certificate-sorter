using CertificateSorter.Helpers;
using SimpleLogger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;

namespace CertificateSorter
{
    class Program
    {
        private static readonly List<TextValues> Certificates = new List<TextValues>();

        static void Main(string[] args)
        {
            LoggingHelper.Initialize();
            try
            {
                Logger.Log(Logger.Level.Info, "Starting reading files...");
                var certDirectoryPath = ConfigurationManager.AppSettings["CertificatesDirectory"];
                var certificateFiles = Directory.GetFiles(certDirectoryPath);

                foreach (var file in certificateFiles)
                {
                    Console.WriteLine($"Processing file: {file}");
                    try
                    {
                        using (var archive = ZipFile.Open(file, ZipArchiveMode.Read))
                        {
                            var record = SqlHelper.ExtractTextValueData(Path.GetFileNameWithoutExtension(file));
                            var collection = archive.CreateCollection();
                            var certificate = collection.GetUserCertificate();

                            AddCertificateToUpdate(record, certificate);

                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Log(Logger.Level.Error, $"Error reading file: {Path.GetFileName(file)}. Exception: {exception.Message}");
                    }
                }

                SqlHelper.BulkUpdate(Certificates);
                Logger.Log(Logger.Level.Info, $"Read and processed a total of: {certificateFiles.Length} files");
                Console.ReadLine();
            }
            catch (Exception exception)
            {
                Logger.Log(Logger.Level.Error, exception.Message);
                throw;
            }
        }

        #region Helpers
        
        private static void AddCertificateToUpdate(TextValues textValues, X509Certificate2 userCertificate)
        {
            if (textValues.Certificate.Equals(userCertificate)) 
                return;

            textValues.UpdateCertificate(userCertificate);
            Logger.Log(Logger.Level.Info, textValues.ToString());
            Certificates.Add(textValues);
        }

        #endregion
    }
}
