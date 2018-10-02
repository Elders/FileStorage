namespace FileStorage.Files
{
    public class FileStorageFile
    {
        public FileStorageFile(FileInformation info, byte[] data)
        {
            //TODO null check

            Info = info;
            Data = data;
        }

        public FileInformation Info { get; set; }

        public byte[] Data { get; set; }
    }
}
