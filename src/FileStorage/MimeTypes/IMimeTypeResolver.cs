using System.Collections.Generic;

namespace FileStorage.MimeTypes
{
    public interface IMimeTypeResolver
    {
        string GetMimeType(byte[] data);
        IReadOnlyCollection<string> SupportedTypes { get; }
        string DefaultMimeType { get; }
    }
}
