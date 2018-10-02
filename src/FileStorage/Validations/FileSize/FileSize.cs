namespace FileStorage.Validations.FileSize
{
    public abstract class FileSize
    {
        protected long _size;
        protected string _sizeFormat;

        public FileSize(long size, string sizeFormat)
        {
            _size = size;
            _sizeFormat = sizeFormat;
        }

        public abstract long GetInBytes();

        public string GetSizeFormat()
        {
            return this._sizeFormat;
        }

        public long GetSize()
        {
            return _size;
        }
    }
}
