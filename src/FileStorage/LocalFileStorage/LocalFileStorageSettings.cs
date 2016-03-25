using System;
using System.IO;
using FileStorage.Generators;

namespace FileStorage.LocalFileStorage
{
    public class LocalFileStorageSettings : IFileStorageSettings<LocalFileStorageSettings>
    {
        public string StorageFolder { get; private set; }
        public IFileGenerator Generator { get; private set; }
        public bool IsGenerationEnabled { get { return ReferenceEquals(Generator, null) == false; } }

        public LocalFileStorageSettings(string storageFolder)
        {
            if (Directory.Exists(storageFolder) == false)
                Directory.CreateDirectory(storageFolder);
        }

        public LocalFileStorageSettings UseFileGenerator(IFileGenerator generator)
        {
            if (ReferenceEquals(generator, null) == true) throw new ArgumentNullException(nameof(generator));
            Generator = generator;
            return this;
        }

    }
}