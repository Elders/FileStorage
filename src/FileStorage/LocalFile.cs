namespace FileStorage
{
    public class LocalFile
    {
        public LocalFile(byte[] data, string fileName)
        {
            Data = data;
            Name = fileName;
        }

        public byte[] Data { get; set; }
        public string Name { get; set; }
    }
}
