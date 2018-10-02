using FileStorage.Files;
using System;
using System.Collections.Generic;

namespace FileStorage.Cache.InMemory
{
    public class InMemoryFileStorageCache : IFileStorageCache
    {
        readonly Dictionary<int, FileStorageFile> cachedFiles = new Dictionary<int, FileStorageFile>();

        public bool CacheFile(FileStorageFile file)
        {
            try
            {
                cachedFiles.Add(file.Info.GetHashCode(), file);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public FileStorageFile GetCachedFile(FileInformation fileInfo)
        {
            FileStorageFile result;

            cachedFiles.TryGetValue(fileInfo.GetHashCode(), out result);

            return result;
        }

        public bool HasCachedFile(FileInformation fileInfo)
        {
            return cachedFiles.ContainsKey(fileInfo.GetHashCode());
        }
    }
}
