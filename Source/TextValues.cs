namespace CertificateSorter
{
    public class TextValues
    {
        public TextValues(int objectId, string value)
        {
            ObjectId = objectId;
            Value = value;
        }

        public int ObjectId { get; }
        public string Value { get; }
    }
}
