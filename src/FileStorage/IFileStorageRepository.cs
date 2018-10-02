using FileStorage.Converters;
using FileStorage.Converters.ImageProcessor;
using FileStorage.Files;
using System.Collections.Generic;
using System.IO;

namespace FileStorage
{
    public class SaveResult
    {
        public static SaveResult Successfull { get => new SaveResult(true, string.Empty); }

        public SaveResult(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public bool Success { get; private set; }
        public string Error { get; private set; }
    }

    public class ValidationResult
    {
        public static ValidationResult Successfull { get => new ValidationResult(true, string.Empty); }

        public ValidationResult(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public bool Success { get; private set; }
        public string Error { get; private set; }
    }


    public interface IFileStorageRepository
    {
        /// <remarks>
        /// meta data keys and values are escaped with Uri.EscapeUriString <see cref="System.Uri.EscapeUriString(string)"/> before uploading
        /// </remarks>
        SaveResult Save(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo);
        IFile Get(string fileName);
        string GetFileUri(string fileName);
        bool FileExists(string fileName);
        Stream GetStream(string fileName, IEnumerable<FileMeta> metaInfo);
        void Delete(string fileName);
        byte[] GetConverted(string fileName, ConverterContext options);
    }

    public interface IFileStorageRepositoryWithFSGenerator
    {
        /// <remarks>
        /// meta data keys and values are escaped with Uri.EscapeUriString <see cref="System.Uri.EscapeUriString(string)"/> before uploading
        /// </remarks>
        void Upload(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original");
        IFile Download(string fileName, string format = "original");
        string GetFileUri(string fileName, string format = "original");
        bool FileExists(string fileName, string format = "original");
        Stream GetStream(string fileName, IEnumerable<FileMeta> metaInfo, string format = "original");
        void Delete(string fileName);
    }
}
