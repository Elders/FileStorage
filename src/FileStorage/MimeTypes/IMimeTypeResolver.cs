namespace FileStorage.MimeTypes
{
    public interface IMimeTypeResolver
    {
        string GetMimeType(byte[] data);
    }
}