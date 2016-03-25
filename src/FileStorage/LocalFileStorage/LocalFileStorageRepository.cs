﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileStorage.FileFormats;
using FileStorage.Generators;

namespace FileStorage.LocalFileStorage
{
    public class LocalFileStorageRepository : IFileStorageRepository
    {
        readonly LocalFileStorageSettings storageSettings;

        public LocalFileStorageRepository(LocalFileStorageSettings storageSettings)
        {
            if (ReferenceEquals(storageSettings, null) == true) throw new ArgumentNullException(nameof(storageSettings));
            this.storageSettings = storageSettings;
        }

        public void Upload(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (ReferenceEquals(data, null) == true)
                throw new ArgumentNullException(nameof(data));

            if (ReferenceEquals(metaInfo, null) == true)
                throw new ArgumentNullException(nameof(metaInfo));

            var filePath = Path.Combine(storageSettings.StorageFolder, format, fileName);
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            File.WriteAllBytes(filePath, data);
        }

        public IFile Download(string fileName, string format = "original")
        {

            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var uri = this.GetFileUri(fileName, format);


            if (string.IsNullOrEmpty(uri))
            {
                if (storageSettings.IsGenerationEnabled == true)
                {
                    var file = storageSettings.Generator.Generate(Download(fileName).Data, format);
                    return new LocalFile(file.Data, fileName);
                }

                if (storageSettings.IsGenerationEnabled == false && format != Original.FormatName)
                    throw new FileNotFoundException($"File {Path.Combine(storageSettings.StorageFolder, format, fileName)} not found. Plugin in {typeof(IFileGenerator)} to generate it.");

                throw new FileNotFoundException($"File {Path.Combine(storageSettings.StorageFolder, format, fileName)} not found");
            }

            var fileBytes = File.ReadAllBytes(uri);

            var fileInfo = new FileInfo(uri);

            return new LocalFile(fileBytes, fileInfo.Name);
        }

        public string GetFileUri(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var directoryPath = Path.Combine(storageSettings.StorageFolder, format);
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = directoryInfo.GetFiles();
            var found = files.SingleOrDefault(x => x.Name.ToLowerInvariant() == fileName.ToLowerInvariant());

            if (ReferenceEquals(found, null) == true)
                return string.Empty;

            return found.FullName;
        }

        public bool FileExists(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var directoryPath = Path.Combine(storageSettings.StorageFolder, format);
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = directoryInfo.GetFiles();
            var found = files.SingleOrDefault(x => x.Name.ToLowerInvariant() == fileName.ToLowerInvariant());

            return found != null;
        }
    }
}
