namespace FileStorage.MimeTypes
{
    public class MimeTypeMapper
    {
        public MimeTypeMapper(string name, string extension, string mime, MimeTypePattern pattern)
        {
            Name = name;
            Extension = extension;
            Mime = mime;
            Pattern = pattern;
        }

        public string Name { get; private set; }
        public MimeTypePattern Pattern { get; private set; }
        public string Mime { get; private set; }
        public string Extension { get; private set; }
    }
}
