using System.Collections.Generic;

namespace FileStorage
{
    public interface IFileStorageRepository
    {
        void Upload(string fileName, byte[] data, List<FileMeta> metaInfo, string format = "original");
        LocalFile Download(string fileName, string format = "original");
        string GetFileUri(string fileName, string format = "original");
        bool FileExists(string fileName, string format = "original");
        byte[] Generate(byte[] data, string format);
    }
}