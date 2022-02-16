using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FileStorage
{
    public class GinormousFilesMultipartStreamProvider : MultipartStreamProvider
    {
        readonly IFileStorageRepository repository;
        readonly IFileGenerator generator;
        readonly NameValueCollection formData = new NameValueCollection();
        readonly List<CustomMultipartFileData> fileContents = new List<CustomMultipartFileData>();
        readonly Collection<bool> isFormData = new Collection<bool>();

        public GinormousFilesMultipartStreamProvider(IFileStorageRepository repository, IFileGenerator generator)
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
                var sourceName = contentDisposition.FileName;
                contentDisposition.FileName = Guid.NewGuid().ToString();

                return repository.GetStream(contentDisposition.FileName, new List<FileMeta> { new FileMeta("SourceName", sourceName) });
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
