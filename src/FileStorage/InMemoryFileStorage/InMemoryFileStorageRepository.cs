using System;
using System.Collections.Generic;
using System.IO;
using FileStorage.FileFormats;

namespace FileStorage.InMemoryFileStorage
{
    public class InMemoryFileStorageRepository : IFileStorageRepository
    {
        readonly Dictionary<string, InMemoryFile> storage = new Dictionary<string, InMemoryFile>();
        readonly InMemoryFileStorageSettings storageSettings;

        public InMemoryFileStorageRepository(InMemoryFileStorageSettings storageSettings)
        {
            if (ReferenceEquals(storageSettings, null) == true) throw new ArgumentNullException(nameof(storageSettings));
            this.storageSettings = storageSettings;
        }

        public IFile Download(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var uri = GetFileUri(fileName, format);
            var key = GetKey(fileName, format);

            if (string.IsNullOrEmpty(uri))
            {
                if (storageSettings.IsGenerationEnabled == true)
                {
                    var file = storageSettings.Generator.Generate(Download(fileName).Data, format);
                    return new LocalFile(file.Data, fileName);
                }

                if (storageSettings.IsGenerationEnabled == false && format != Original.FormatName)
                    throw new FileNotFoundException($"File {key} not found. Plugin in {typeof(IFileGenerator)} to generate it.");

                throw new FileNotFoundException($"File {key} not found");
            }

            var fileBytes = storage[key].Data;

            return new LocalFile(fileBytes, fileName);
        }

        public bool FileExists(string fileName, string format = "original")
        {
            var key = GetKey(fileName, format);
            return storage.ContainsKey(key);
        }

        public string GetFileUri(string fileName, string format = "original")
        {
            var key = GetKey(fileName, format);
            return storage.ContainsKey(key) ? key : string.Empty;
        }

        public Stream GetStream(string fileName, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            throw new NotImplementedException();
        }

        public void Upload(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));

            var key = GetKey(fileName, format);

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

            if (storage.ContainsKey(key))
                storage[key] = file;
            else
                storage.Add(key, file);
        }

        string GetKey(string fileName, string format)
        {
            return format + "/" + fileName;
        }


        class InMemoryFile
        {
            public InMemoryFile(byte[] data, Dictionary<string, string> metaInfo, string contentType)
            {
                Data = data;
                MetaInfo = metaInfo;
                ContentType = contentType;
            }

            public byte[] Data { get; private set; }

            public Dictionary<string, string> MetaInfo { get; private set; }

            public string ContentType { get; private set; }
        }
    }
}
