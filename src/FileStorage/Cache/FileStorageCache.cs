using FileStorage.Files;

namespace FileStorage.Cache.InMemory
{
    public abstract class FileStorageCache : IFileStorageCache
    {
        public bool CacheFile(FileStorageFile file)
        {
            return SaveFileToCache(file.Info.UniqueName, file.Data);
        }

        public FileStorageFile GetCachedFile(FileInformation fileInfo)
        {
            var result = GetFileFromCache(fileInfo.UniqueName);

            if (HasCachedFile(fileInfo))
                return new FileStorageFile(fileInfo, result);
            else
                return null;
        }

        public bool HasCachedFile(FileInformation fileInfo)
        {
            return IsFileCached(fileInfo.UniqueName);
        }

        protected abstract bool SaveFileToCache(string fileName, byte[] data);

        protected abstract byte[] GetFileFromCache(string fileName);

        protected abstract bool IsFileCached(string fileName);
    }
}
