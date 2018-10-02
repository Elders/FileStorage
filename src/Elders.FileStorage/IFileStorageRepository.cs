using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileStorage
{
    public interface IFileStorageRepository
    {
        /// <remarks>
        /// meta data keys and values are escaped with Uri.EscapeUriString <see cref="System.Uri.EscapeUriString(string)"/> before uploading
        /// </remarks>
        Task UploadAsync(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original");
        Task<IFile> DownloadAsync(string fileName, string format = "original");
        Task<string> GetFileUriAsync(string fileName, string format = "original");
        Task<bool> FileExistsAsync(string fileName, string format = "original");
        Task<Stream> GetStreamAsync(string fileName, IEnumerable<FileMeta> metaInfo, string format = "original");
        Task DeleteAsync(string fileName);
    }
}
