using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FileStorage.Extensions;
using FileStorage.FileFormats;

namespace FileStorage.WebApi.Multipart
{
    public class CustomMultipartStreamProvider : MultipartStreamProvider
    {
        readonly IFileStorageRepository repository;
        readonly IFileGenerator generator;
        readonly NameValueCollection formData = new NameValueCollection();
        readonly List<CustomMultipartFileData> fileContents = new List<CustomMultipartFileData>();
        readonly Collection<bool> isFormData = new Collection<bool>();

        public CustomMultipartStreamProvider(IFileStorageRepository repository, IFileGenerator generator)
        {
            if (ReferenceEquals(repository, null) == true) throw new ArgumentNullException(nameof(repository));
            if (ReferenceEquals(generator, null) == true) throw new ArgumentNullException(nameof(generator));

            this.repository = repository;
            this.generator = generator;
        }

        public NameValueCollection FormData
        {
            get { return this.formData; }
        }

        public List<CustomMultipartFileData> FileData
        {
            get { return this.fileContents; }
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
            if (contentDisposition != null)
            {
                this.isFormData.Add(string.IsNullOrEmpty(contentDisposition.FileName));

                return new MemoryStream();
            }
            throw new InvalidOperationException("Did not find required 'Content-Disposition' header field in MIME multipart body part.");
        }

        public override async Task ExecutePostProcessingAsync()
        {
            for (int index = 0; index < Contents.Count; index++)
            {
                if (this.isFormData[index])
                {
                    HttpContent formContent = Contents[index];

                    ContentDispositionHeaderValue contentDisposition = formContent.Headers.ContentDisposition;
                    string formFieldName = UnquoteToken(contentDisposition.Name) ?? string.Empty;

                    string formFieldValue = await formContent.ReadAsStringAsync();
                    this.FormData.Add(formFieldName, formFieldValue);
                }
                else
                {
                    HttpContent formContent = Contents[index];

                    ContentDispositionHeaderValue contentDisposition = formContent.Headers.ContentDisposition;
                    contentDisposition.FileName = Guid.NewGuid().ToString();

                    var stream = formContent.ReadAsStreamAsync().Result;

                    var original = stream.ToByteArray();

                    await repository.UploadAsync(contentDisposition.FileName, original, new List<FileMeta>(), Original.FormatName);

                    foreach (var format in generator.Formats)
                    {
                        if (format.Name == Original.FormatName)
                            continue;

                        var file = format.Generate(original);
                        await repository.UploadAsync(contentDisposition.FileName, file.Data, new List<FileMeta> { new FileMeta("SourceName", contentDisposition.FileName) }, format.Name);
                    }


                    this.fileContents.Add(new CustomMultipartFileData(formContent.Headers, contentDisposition.FileName));
                }
            }
        }

        private static string UnquoteToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
            {
                return token.Substring(1, token.Length - 2);
            }

            return token;
        }
    }
}
