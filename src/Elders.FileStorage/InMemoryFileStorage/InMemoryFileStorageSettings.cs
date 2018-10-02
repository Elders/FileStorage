using System;
using FileStorage.MimeTypes;

namespace FileStorage.InMemoryFileStorage
{
    public class InMemoryFileStorageSettings : IFileStorageSettings<InMemoryFileStorageSettings>
    {
        public IFileGenerator Generator { get; private set; }
        public bool IsGenerationEnabled { get { return ReferenceEquals(Generator, null) == false; } }
        public IMimeTypeResolver MimeTypeResolver { get; private set; }
        public bool IsMimeTypeResolverEnabled { get { return ReferenceEquals(MimeTypeResolver, null) == false; } }

        public InMemoryFileStorageSettings UseFileGenerator(IFileGenerator generator)
        {
            if (ReferenceEquals(generator, null) == true) throw new ArgumentNullException(nameof(generator));
            Generator = generator;
            return this;
        }

        public InMemoryFileStorageSettings UseMimeTypeResolver(IMimeTypeResolver resolver)
        {
            if (ReferenceEquals(resolver, null) == true) throw new ArgumentNullException(nameof(resolver));
            MimeTypeResolver = resolver;
            return this;
        }
    }
}