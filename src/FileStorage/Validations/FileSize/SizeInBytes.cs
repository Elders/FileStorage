namespace FileStorage.Validations.FileSize
{
    public class SizeInBytes : FileSize
    {
        public SizeInBytes(long size) : base(size, "bytes")
        {
        }

        public override long GetInBytes()
        {
            return _size;
        }
    }
}
