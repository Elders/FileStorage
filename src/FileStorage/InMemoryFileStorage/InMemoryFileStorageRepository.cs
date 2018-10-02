using System;
using System.Collections.Generic;
using System.IO;
using FileStorage.FileFormats;
using FileStorage.Files;

namespace FileStorage.InMemoryFileStorage
{
    public partial class InMemoryFileStorageRepository : FileStorageRepository<InMemoryFileStorageSettings>
    {
        readonly Dictionary<string, InMemoryFile> storage = new Dictionary<string, InMemoryFile>();
        readonly InMemoryFileStorageSettings storageSettings;

        public InMemoryFileStorageRepository(InMemoryFileStorageSettings storageSettings)
            : base(storageSettings)
        {
            if (ReferenceEquals(storageSettings, null) == true) throw new ArgumentNullException(nameof(storageSettings));

            this.storageSettings = storageSettings;
        }

        public override IFile Get(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var uri = GetFileUri(fileName);

            if (string.IsNullOrEmpty(uri))
                throw new FileNotFoundException($"File {fileName} not found");

            var fileBytes = storage[fileName].Data;

            return new LocalFile(fileBytes, fileName);
        }

        public override bool FileExists(string fileName)
        {
            return storage.ContainsKey(fileName);
        }

        public override string GetFileUri(string fileName)
        {
            return FileExists(fileName) ? fileName : string.Empty;
        }

        public override Stream GetStream(string fileName, IEnumerable<FileMeta> metaInfo)
        {
            throw new NotImplementedException();
        }

        public override SaveResult Save(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));

            var baseResult = base.Save(fileName, data, metaInfo);
            if (baseResult.Success == false)
                return baseResult;

            var metaDictionary = new Dictionary<string, string>();
            foreach (var meta in metaInfo)
            {
                metaDictionary.Add(Uri.EscapeUriString(meta.Key), Uri.EscapeUriString(meta.Value));
            }

            var contentType = string.Empty;
            if (storageSettings.IsMimeTypeResolverEnabled)
            {
                contentType = storageSettings.MimeTypeResolver.GetMimeType(data);
            }

            var file = new InMemoryFile(data, metaDictionary, contentType);

            if (storage.ContainsKey(fileName))
                storage[fileName] = file;
            else
                storage.Add(fileName, file);


            return SaveResult.Successfull;
        }

        public override void Delete(string fileName)
        {
            if (storage.ContainsKey(fileName))
                storage.Remove(fileName);
        }
    }
}
