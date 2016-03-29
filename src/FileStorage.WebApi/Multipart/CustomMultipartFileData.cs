using System.Net.Http.Headers;

namespace FileStorage.WebApi.Multipart
{
    public class CustomMultipartFileData
    {
        public CustomMultipartFileData(HttpContentHeaders headers, string fileName)
        {
            Headers = headers;
            FileName = fileName;
        }

        public HttpContentHeaders Headers { get; private set; }

        public string FileName { get; private set; }
    }
}
