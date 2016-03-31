namespace FileStorage.FileFormats
{
    public interface IFileFormat
    {
        string Name { get; }

        FIleGenerateResponse Generate(byte[] data);
    }
}
