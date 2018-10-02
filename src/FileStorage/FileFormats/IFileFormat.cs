namespace FileStorage.FileFormats
{
    public interface IFileFormat
    {
        string Name { get; }

        FileGenerateResponse Generate(byte[] data);
    }
}
