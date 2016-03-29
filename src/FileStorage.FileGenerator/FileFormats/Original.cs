namespace FileStorage.FileGenerator.FileFormats
{
    public class Original : IFileFormat
    {
        public static string FormatName = typeof(Original).Name.ToLowerInvariant();

        public string Name { get { return FormatName; } }

        public FIleGenerateResponse Generate(byte[] data)
        {
            return new FIleGenerateResponse(true, data, string.Empty);
        }
    }
}
