using System;

namespace FileStorage.Files
{
    /// <summary>
    /// User-defined metadata is a set of key-value pairs implemented in some of the storages (e.g. S3 or Azure)
    /// </summary>
    public class FileMeta : IEquatable<FileMeta>
    {
        public FileMeta(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key));
            if (string.IsNullOrEmpty(value)) throw new ArgumentException(nameof(value));

            Key = key;
            Value = value;
        }

        public string Key { get; private set; }

        public string Value { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as FileMeta);
        }

        public bool Equals(FileMeta other)
        {
            return other != null &&
                   Key == other.Key &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            var hashCode = 206514262;
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            return hashCode;
        }
    }
}
