using FileStorage.MimeTypes;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileStorage.FileSystem
{
    public class LocalFileStorageSettings : FileStorageSettings
    {
        public string StorageFolder { get; private set; }
        public IMimeTypeResolver MimeTypeResolver { get; private set; }
        public bool IsMimeTypeResolverEnabled { get { return ReferenceEquals(MimeTypeResolver, null) == false; } }


        public LocalFileStorageSettings(string storageFolder)
        {
            if (Directory.Exists(storageFolder) == false)
                Directory.CreateDirectory(storageFolder);

            StorageFolder = storageFolder;
        }

        public LocalFileStorageSettings UseMimeTypeResolver(IMimeTypeResolver resolver)
        {
            if (ReferenceEquals(resolver, null) == true) throw new ArgumentNullException(nameof(resolver));
            MimeTypeResolver = resolver;
            return this;
        }
    }
}
