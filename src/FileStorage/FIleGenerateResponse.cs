namespace FileStorage
{
    public class FileGenerateResponse
    {
        public FileGenerateResponse(bool success, byte[] data, string error)
        {
            Success = success;
            Data = data;
            Error = error;
        }

        public bool Success { get; private set; }
        public byte[] Data { get; private set; }
        public string Error { get; private set; }
    }
}