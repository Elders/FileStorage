namespace FileStorage.MimeTypes
{
    public class MimeTypeMapper
    {
        public MimeTypeMapper(string name, string mime, MimeTypePattern pattern)
        {
            Name = name;
            Mime = mime;
            Pattern = pattern;
        }

        public string Name { get; private set; }
        public MimeTypePattern Pattern { get; private set; }
        public string Mime { get; private set; }
    }
}