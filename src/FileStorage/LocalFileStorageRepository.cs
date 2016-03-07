using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileStorage.FileFormats;

namespace FileStorage
{
    public class LocalFileStorageRepository : IFileStorageRepository
    {
        private string storageFolder;
        private Dictionary<string, IFileFormat> formats;

        public LocalFileStorageRepository(string storageFolder)
        {
            Initialize(storageFolder);
        }

        public void Upload(string fileName, byte[] data, List<FileMeta> metaInfo, string format = "original")
        {
            var filePath = Path.Combine(storageFolder, format, fileName);

            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            File.WriteAllBytes(filePath, data);
        }

        public LocalFile Download(string fileName, string format = "original")
        {
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
            var formatInstance = formats[format];

            if (ReferenceEquals(null, formatInstance) == false)
            {
                if (!formatInstance.FindFile(fileName))
                {
                    if (formatInstance.Generate(fileName) == false)
                    {
                        return string.Empty;
                    }
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("This file format is not supported. {0}", format));
            }

            var directoryPath = Path.Combine(storageFolder, format);
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = directoryInfo.GetFiles();
            var found = files.SingleOrDefault(x => x.Name.ToLowerInvariant().Replace(x.Extension.ToLowerInvariant(), string.Empty) == fileName.ToLowerInvariant());

            return found.FullName;
        }

        public bool FileExists(string fileName, string format = "original")
        {
            var directoryPath = Path.Combine(storageFolder, format);
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = directoryInfo.GetFiles();
            var found = files.SingleOrDefault(x => x.Name.ToLowerInvariant().Replace(x.Extension.ToLowerInvariant(), string.Empty) == fileName.ToLowerInvariant());

            return found != null;
        }

        public byte[] Generate(byte[] data, string format)
        {
            var formatInstance = formats[format];

            if (ReferenceEquals(null, formatInstance) == false)
            {
                var newData = formatInstance.Generate(data);

                return newData;
            }
            else
            {
                throw new NotSupportedException(string.Format("This file format is not supported. {0}", format));
            }
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
