using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;

namespace CertificateSorter
{
    class Program
    {
        private static readonly List<TextValues> Certificates = new List<TextValues>();

        static void Main(string[] args)
        {
            try
            {
                var certDirectoryPath = ConfigurationManager.AppSettings["CertificatesDirectory"];
                var certificateFiles = Directory.GetFiles(certDirectoryPath);

                foreach (var file in certificateFiles)
                {
                    var orderId = ExtractOrderId(file);
                    try
                    {
                        using (var archive = ZipFile.Open(file, ZipArchiveMode.Read))
                        {
                            var collection = archive.CreateCollection();
                            var certificate = collection.GetUserCertificate();

                            Certificates.Add(new TextValues(orderId, certificate.ExportCertToPem()));
                            Console.WriteLine($"User certificate found for file: {Path.GetFileName(file)}");
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($"Error reading file: {Path.GetFileName(file)}. Exception: {exception.Message}");
                    }
                }

                SqlHelper.BulkUpdate(Certificates);
                Console.ReadLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        #region Helpers
        
        /// <summary>
        /// Extracts the ascio order id from the file name
        /// </summary>
        /// <param name="file">Full file name</param>
        /// <returns>Ascio Order Id</returns>
        private static int ExtractOrderId(string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var orderId = fileName?.Replace("A", "");
            return Convert.ToInt32(orderId);
        } 

        #endregion
    }
}
