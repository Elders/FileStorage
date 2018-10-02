using System;
using System.IO;

namespace FileStorage.Cache.InMemory
{
    public class LocalFileStorageCache : FileStorageCache
    {
        readonly string _cacheFolderPath;

        public LocalFileStorageCache(string cacheFolderPath)
            : base()
        {
            if (string.IsNullOrEmpty(cacheFolderPath)) throw new ArgumentException(nameof(cacheFolderPath));

            _cacheFolderPath = cacheFolderPath;

            if (Directory.Exists(_cacheFolderPath) == false)
                Directory.CreateDirectory(_cacheFolderPath);
        }

        protected override byte[] GetFileFromCache(string fileName)
        {
            return File.ReadAllBytes(Path.Combine(_cacheFolderPath, fileName));
        }

        protected override bool IsFileCached(string fileName)
        {
            return File.Exists(Path.Combine(_cacheFolderPath, fileName));
        }

        protected override bool SaveFileToCache(string fileName, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (ReferenceEquals(data, null) == true)
                throw new ArgumentNullException(nameof(data));

            var filePath = Path.Combine(_cacheFolderPath, fileName);

            File.WriteAllBytes(filePath, data);

            return true;
        }
    }
}
