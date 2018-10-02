using FileStorage.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileStorage.FileSystem
{
    public class LocalFileStorageRepository : FileStorageRepository<LocalFileStorageSettings>
    {
        public LocalFileStorageRepository(LocalFileStorageSettings storageSettings)
            : base(storageSettings)
        {
        }

        public override IFile Get(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var uri = GetFileUri(fileName);

            if (string.IsNullOrEmpty(uri))
                throw new FileNotFoundException($"File {Path.Combine(_storageSettings.StorageFolder, fileName)} not found");

            var fileBytes = File.ReadAllBytes(uri);

            var fileInfo = new FileInfo(uri);

            return new LocalFile(fileBytes, fileInfo.Name);
        }

        public override SaveResult Save(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (ReferenceEquals(data, null) == true)
                throw new ArgumentNullException(nameof(data));

            if (ReferenceEquals(metaInfo, null) == true)
                throw new ArgumentNullException(nameof(metaInfo));


            var baseResult = base.Save(fileName, data, metaInfo);
            if (baseResult.Success == false)
                return baseResult;

            if (fileName.Contains('.') == false && _storageSettings.IsMimeTypeResolverEnabled)
            {
                var fileExtension = _storageSettings.MimeTypeResolver.GetExtension(data);
                fileName = fileName + fileExtension;
            }

            var filePath = Path.Combine(_storageSettings.StorageFolder, fileName);
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            File.WriteAllBytes(filePath, data);

            return new SaveResult(true, string.Empty);
        }

        public override void Delete(string fileName)
        {
            var uri = GetFileUri(fileName);

            if (string.IsNullOrEmpty(uri) == false)
                File.Delete(uri);
        }

        public override bool FileExists(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var directoryInfo = new DirectoryInfo(_storageSettings.StorageFolder);
            FileInfo[] files = directoryInfo.GetFiles();
            var found = files.Any(x => x.Name.ToLowerInvariant() == fileName.ToLowerInvariant());

            return found;
        }

        public override string GetFileUri(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var directoryInfo = new DirectoryInfo(_storageSettings.StorageFolder);
            FileInfo[] files = directoryInfo.GetFiles();
            var found = files.SingleOrDefault(x => Path.GetFileNameWithoutExtension(x.FullName).ToLowerInvariant() == fileName.ToLowerInvariant());

            if (ReferenceEquals(found, null) == true)
                return string.Empty;

            return found.FullName;
        }

        public override Stream GetStream(string fileName, IEnumerable<FileMeta> metaInfo)
        {
            throw new NotImplementedException();
        }


    }
}
