using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CertificateSorter
{
    public class TextValues
    {
        public TextValues(int id, int objectId, string value, string handle)
        {
            Id = id;
            ObjectId = objectId;
            Value = value;
            Handle = handle;
            _certificate = new X509Certificate2(Encoding.ASCII.GetBytes(Value));
        }

        public int Id { get; }
        public int ObjectId { get; }
        public string Value { get; }
        public string Handle { get; }

        private X509Certificate2 _certificate;
        public X509Certificate2 Certificate
        {
            get => _certificate;
            private set => _certificate = value;
        }

        public void UpdateCertificate(X509Certificate2 updatedCertificate)
        {
            _certificate = updatedCertificate;
        }

        #region Overrides of Object

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Id: {Id}")
                .AppendLine($"ObjectId: {ObjectId}")
                .AppendLine($"Handle: {Handle}")
                .AppendLine($"Old Certificate: {Value}")
                .AppendLine($"New certificate: {Certificate.ExportCertToPem()}");

            return builder.ToString();
        }

        #endregion
    }
}
