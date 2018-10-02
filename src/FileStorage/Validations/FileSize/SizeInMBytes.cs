namespace FileStorage.Validations.FileSize
{
    public class SizeInMBytes : FileSize
    {
        public SizeInMBytes(long size) : base(size, "MBytes")
        {
        }

        public override long GetInBytes()
        {
            return _size * 1024 * 1024;
        }
    }
}
