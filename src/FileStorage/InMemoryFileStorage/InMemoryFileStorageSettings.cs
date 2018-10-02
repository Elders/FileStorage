using System;
using FileStorage.MimeTypes;

namespace FileStorage.InMemoryFileStorage
{
    public class InMemoryFileStorageSettings : FileStorageSettings
    {
        public IFileGenerator Generator { get; private set; }
        public bool IsGenerationEnabled { get { return ReferenceEquals(Generator, null) == false; } }
        public IMimeTypeResolver MimeTypeResolver { get; private set; }
        public bool IsMimeTypeResolverEnabled { get { return ReferenceEquals(MimeTypeResolver, null) == false; } }
    }
}
