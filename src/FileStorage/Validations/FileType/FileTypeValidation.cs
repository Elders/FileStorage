using FileStorage.MimeTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileStorage.Validations.FileType
{
    public class FileTypeValidation : IValidation
    {
        readonly IMimeTypeResolver _mimeTypeResolver;
        readonly IList<string> _allowedMimeTypes;

        public FileTypeValidation(IMimeTypeResolver mimeTypeResolver)
        {
            if (ReferenceEquals(null, mimeTypeResolver)) throw new ArgumentNullException(nameof(mimeTypeResolver));

            _mimeTypeResolver = mimeTypeResolver;
            _allowedMimeTypes = new List<string>();
        }

        public string GetErrorMessage()
        {
            return $"The file is not supported. Please make sure the file is one of these types: {string.Join(", ", _allowedMimeTypes)}";
        }

        public void AddMimeType(string type)
        {
            _allowedMimeTypes.Add(type);
        }

        public bool IsValid(byte[] data)
        {
            string currentFileMimeType = _mimeTypeResolver.GetMimeType(data);

            return _allowedMimeTypes.Any(x => currentFileMimeType == x);

        }
    }
}
