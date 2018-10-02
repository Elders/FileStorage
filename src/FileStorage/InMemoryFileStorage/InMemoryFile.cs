using System.Collections.Generic;

namespace FileStorage.InMemoryFileStorage
{
    public partial class InMemoryFileStorageRepository
    {
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
