namespace FileStorage.Validations.FileSize
{
    public class SizeInKBytes : FileSize
    {
        public SizeInKBytes(long size) : base(size, "KBytes")
        {
        }

        public override long GetInBytes()
        {
            return _size * 1024;
        }
    }
}
