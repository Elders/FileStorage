using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileStorage.FileFormats;

namespace FileStorage.FileStorage
{
    public class LocalFileStorageRepository : IFileStorageRepository
    {
        string storageFolder;
        Dictionary<string, IFileFormat> formats;

        public LocalFileStorageRepository(string storageFolder)
        {
            if (string.IsNullOrWhiteSpace(storageFolder))
                throw new ArgumentNullException(nameof(storageFolder));

            Initialize(storageFolder);
        }

        public void Upload(string fileName, byte[] data, List<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (ReferenceEquals(data, null) == true)
                throw new ArgumentNullException(nameof(data));

            if (ReferenceEquals(metaInfo, null) == true)
                throw new ArgumentNullException(nameof(metaInfo));

            var filePath = Path.Combine(storageFolder, format, fileName);

            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            File.WriteAllBytes(filePath, data);
        }

        public LocalFile Download(string fileName, string format = "original")
        {
            if (formats.ContainsKey(format) == false)
                throw new NotSupportedException($"This file format is not supported. {format}");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var formatInstance = formats[format];

            if (formatInstance.FindFile(fileName) == false)
                formatInstance.Generate(fileName);

            var uri = this.GetFileUri(fileName, format);

            if (string.IsNullOrEmpty(uri))
            {
                throw new FileNotFoundException(Path.Combine(storageFolder, format, fileName));
            }

            var fileBytes = File.ReadAllBytes(uri);

            var fileInfo = new FileInfo(uri);

            return new LocalFile(fileBytes, fileInfo.Name);
        }

        public string GetFileUri(string fileName, string format = "original")
        {
            if (formats.ContainsKey(format) == false)
                throw new NotSupportedException($"This file format is not supported. {format}");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var formatInstance = formats[format];

            if (formatInstance.FindFile(fileName) == false)
                formatInstance.Generate(fileName);

            var directoryPath = Path.Combine(storageFolder, format);
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

            var directoryPath = Path.Combine(storageFolder, format);
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = directoryInfo.GetFiles();
            var found = files.SingleOrDefault(x => x.Name.ToLowerInvariant() == fileName.ToLowerInvariant());

            return found != null;
        }

        public byte[] Generate(byte[] data, string format)
        {
            if (ReferenceEquals(data, null) == true)
                throw new ArgumentNullException(nameof(data));

            if (formats.ContainsKey(format) == false)
                throw new NotSupportedException($"This file format is not supported. {format}");

            var formatInstance = formats[format];
            var newData = formatInstance.Generate(data);

            return newData;
        }

        private void Initialize(string storageFolder)
        {
            if (!Directory.Exists(storageFolder))
                Directory.CreateDirectory(storageFolder);

            this.storageFolder = storageFolder;

            ImageResizer.Configuration.Config.Current.UpgradeImageBuilder(new CustomImageBuilder());

            formats = new Dictionary<string, IFileFormat>();

            RegisterFormat(new MobileFull(this));
            if (!Directory.Exists(Path.Combine(storageFolder, MobileFull.FormatName)))
                Directory.CreateDirectory(Path.Combine(storageFolder, MobileFull.FormatName));

            RegisterFormat(new MobileThumbnail(this));
            if (!Directory.Exists(Path.Combine(storageFolder, MobileThumbnail.FormatName)))
                Directory.CreateDirectory(Path.Combine(storageFolder, MobileThumbnail.FormatName));

            RegisterFormat(new Original(this));
            if (!Directory.Exists(Path.Combine(storageFolder, Original.FormatName)))
                Directory.CreateDirectory(Path.Combine(storageFolder, Original.FormatName));
        }

        private void RegisterFormat(IFileFormat format)
        {
            formats.Add(format.Name, format);
        }
    }
}
