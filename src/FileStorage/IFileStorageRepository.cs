using System.Collections.Generic;

namespace FileStorage
{
    public interface IFileStorageRepository
    {
        void Upload(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original");
        IFile Download(string fileName, string format = "original");
        string GetFileUri(string fileName, string format = "original");
        bool FileExists(string fileName, string format = "original");
    }
}