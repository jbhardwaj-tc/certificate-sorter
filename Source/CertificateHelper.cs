using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CertificateSorter
{
    /// <summary>
    /// Defines extension methods for managing <see cref="X509Certificate2"/> type
    /// </summary>
    public static class CertificateHelper
    {
        /// <summary>
        /// Creates a <see cref="X509Certificate2Collection"/> from the Zip file containing the certificate files
        /// </summary>
        /// <param name="archive">Certificate ZipArchive from SSlStore</param>
        /// <returns>Extracted certificates collection</returns>
        public static X509Certificate2Collection CreateCollection(this ZipArchive archive)
        {
            var files = archive.Entries.Where(x => x.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));
            var collection = new X509Certificate2Collection();
            foreach (var entry in files)
            {
                try
                {
                    var certificate = new X509Certificate2();
                    using (var stream = entry.Open())
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            certificate.Import(ms.ToArray());
                            collection.Add(certificate);
                        }
                    }
                }
                catch (Exception) { }
            }

            return collection;
        }

        /// <summary>
        /// Gets the end user certificate from the certificate collection
        /// </summary>
        /// <param name="collection">Certificate collection</param>
        /// <returns>End entity certificate</returns>
        public static X509Certificate2 GetUserCertificate(this X509Certificate2Collection collection)
        {
            X509Certificate2 certificate = null;
            foreach (var item in collection)
            {
                if (IsEndEntityCertificate(item))
                {
                    certificate = item;
                    break;
                }
            }

            return certificate;
        }

        public static bool IsEndEntityCertificate(this X509Certificate2 certificate)
        {
            if (certificate == null)
                return false;

            foreach (var extension in certificate.Extensions)
            {
                if (extension.Oid.FriendlyName == "Basic Constraints")
                {
                    return !((X509BasicConstraintsExtension)extension).CertificateAuthority;
                }
            }

            return false;
        }

        /// <summary>
        /// Exports the <see cref="X509Certificate2"/> to PEM
        /// </summary>
        /// <param name="endEntityCertificate">Certificate to export</param>
        /// <returns>PEM representation of the certificate</returns>
        public static string ExportCertToPem(this X509Certificate2 endEntityCertificate)
        {
            var builder = new StringBuilder();
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(Convert.ToBase64String(endEntityCertificate.RawData, Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");

            return builder.ToString();
        }
    }
}
