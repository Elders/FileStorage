using FileStorage.MimeTypes;
using System;
using System.IO;

namespace FileStorage.FileSystem
{
    public class FileSystemFileStorageSettings : IFileStorageSettings<FileSystemFileStorageSettings>
    {
        public string StorageFolder { get; private set; }
        public IFileGenerator Generator { get; private set; }
        public bool IsGenerationEnabled { get { return ReferenceEquals(Generator, null) == false; } }
        public IMimeTypeResolver MimeTypeResolver { get; private set; }
        public bool IsMimeTypeResolverEnabled { get { return ReferenceEquals(MimeTypeResolver, null) == false; } }

        public FileSystemFileStorageSettings(string storageFolder)
        {
            if (Directory.Exists(storageFolder) == false)
                Directory.CreateDirectory(storageFolder);

            StorageFolder = storageFolder;
        }

        public FileSystemFileStorageSettings UseFileGenerator(IFileGenerator generator)
        {
            if (ReferenceEquals(generator, null) == true) throw new ArgumentNullException(nameof(generator));
            Generator = generator;
            return this;
        }

        public FileSystemFileStorageSettings UseMimeTypeResolver(IMimeTypeResolver resolver)
        {
            if (ReferenceEquals(resolver, null) == true) throw new ArgumentNullException(nameof(resolver));
            MimeTypeResolver = resolver;
            return this;
        }
    }
}
