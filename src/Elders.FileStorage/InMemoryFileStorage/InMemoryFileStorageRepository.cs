using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileStorage.FileFormats;

namespace FileStorage.InMemoryFileStorage
{
    public class InMemoryFileStorageRepository : IFileStorageRepository
    {
        readonly Dictionary<string, InMemoryFile> storage = new Dictionary<string, InMemoryFile>();
        readonly InMemoryFileStorageSettings storageSettings;

        public InMemoryFileStorageRepository(InMemoryFileStorageSettings storageSettings)
        {
            if (storageSettings is null) throw new ArgumentNullException(nameof(storageSettings));
            this.storageSettings = storageSettings;
        }

        public async Task<IFile> DownloadAsync(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var uri = await GetFileUriAsync(fileName, format).ConfigureAwait(false);
            var key = GetKey(fileName, format);

            if (string.IsNullOrEmpty(uri))
            {
                if (storageSettings.IsGenerationEnabled == true)
                {
                    var downloadResult = await DownloadAsync(fileName).ConfigureAwait(false);
                    var file = storageSettings.Generator.Generate(downloadResult.Data, format);
                    return new LocalFile(file.Data, fileName);
                }

                if (storageSettings.IsGenerationEnabled == false && format != Original.FormatName)
                    throw new FileNotFoundException($"File {key} not found. Plugin in {typeof(IFileGenerator)} to generate it.");

                throw new FileNotFoundException($"File {key} not found");
            }

            var fileBytes = storage[key].Data;

            return new LocalFile(fileBytes, fileName);
        }

        public Task<bool> FileExistsAsync(string fileName, string format = "original")
        {
            var key = GetKey(fileName, format);
            return Task.FromResult(storage.ContainsKey(key));
        }

        public Task<string> GetFileUriAsync(string fileName, string format = "original")
        {
            var key = GetKey(fileName, format);
            return Task.FromResult(storage.ContainsKey(key) ? key : string.Empty);
        }

        public Stream GetStream(string fileName, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            throw new NotImplementedException();
        }

        public Task UploadAsync(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (metaInfo is null) throw new ArgumentNullException(nameof(metaInfo));

            var key = GetKey(fileName, format);

            var metaDictionary = new Dictionary<string, string>();
            foreach (var meta in metaInfo)
            {
                metaDictionary.Add(Uri.EscapeUriString(meta.Key), Uri.EscapeUriString(meta.Value));
            }

            var contentType = data.GetMimeType();

            var file = new InMemoryFile(data, metaDictionary, contentType);

            if (storage.ContainsKey(key))
                storage[key] = file;
            else
                storage.Add(key, file);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(string fileName)
        {
            foreach (var format in storageSettings.Generator.Formats)
            {
                var key = GetKey(fileName, format.Name);

                if (storage.ContainsKey(key))
                    storage.Remove(key);
            }

            return Task.CompletedTask;
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
