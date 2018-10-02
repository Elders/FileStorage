namespace FileStorage.Validations.FileSize
{
    public class FileSizeValidation : IValidation
    {
        public FileSizeValidation(FileSize limitSize)
        {
            Limit = limitSize;
        }

        public FileSize Limit { get; private set; }

        public string GetErrorMessage()
        {
            return $"File size is too big! Try with a file up to {Limit.GetSize()} {Limit.GetSizeFormat()}.";
        }

        public bool IsValid(byte[] data)
        {
            return data.Length <= Limit.GetInBytes();
        }
    }
}
