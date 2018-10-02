using FileStorage.Files;

namespace FileStorage.Cache
{
    public interface IFileStorageCache
    {
        bool CacheFile(FileStorageFile file);
        bool HasCachedFile(FileInformation fileInfo);
        FileStorageFile GetCachedFile(FileInformation fileInfo);
    }
}
