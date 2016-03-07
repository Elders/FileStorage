namespace FileStorage.FileFormats
{
    public class Original : IFileFormat
    {
        public IFileStorageRepository FileStorageRepository { get; private set; }

        public Original(IFileStorageRepository repo)
        {
            FileStorageRepository = repo;
        }

        public static string FormatName = typeof(Original).Name.ToLowerInvariant();

        public string Name { get { return FormatName; } }

        public bool FindFile(string filename)
        {
            return FileStorageRepository.FileExists(filename, Name);
        }

        public bool Generate(string sourceName)
        {
            return false;
        }

        public byte[] Generate(byte[] data)
        {
            return data;
        }
    }
}
