using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace CertificateSorter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var certDirectoryPath = ConfigurationManager.AppSettings["CertificatesDirectory"];
                var certificateFiles = Directory.GetFiles(certDirectoryPath, "*.txt", SearchOption.AllDirectories);

                var collection = new X509Certificate2Collection();
                foreach (var file in certificateFiles)
                {
                    var certificate = new X509Certificate2();
                    certificate.Import(file);
                    collection.Add(certificate);
                }

                foreach (var item in collection)
                {
                    foreach (var extension in item.Extensions)
                    {
                        if (extension.Oid.FriendlyName == "Basic Constraints")
                        {
                            Console.WriteLine($"Subject: {item.Subject}, Is user certificate?: {!((X509BasicConstraintsExtension)extension).CertificateAuthority}");
                        }
                    }
                }

                Console.ReadLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}
