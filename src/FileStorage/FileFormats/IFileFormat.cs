namespace FileStorage.FileFormats
{
    public interface IFileFormat
    {
        IFileStorageRepository FileStorageRepository { get; }
        string Name { get; }

        bool FindFile(string fileName);
        bool Generate(string sourceId);
        byte[] Generate(byte[] data);
    }
}
