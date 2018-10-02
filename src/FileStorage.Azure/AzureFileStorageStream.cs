using FileStorage.Files;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileStorage.Azure
{
    public class AzureFileStorageStream : Stream
    {
        long index = 0;
        long lenght = 0;
        int blockSize = 0;
        List<string> blockDataList;
        AzureStorageSettings storageSettings;
        CloudBlockBlob blockBlob;
        MemoryStream s;

        public AzureFileStorageStream(AzureStorageSettings storageSettings, string fileName, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            this.blockDataList = new List<string>();
            this.storageSettings = storageSettings;
            this.s = new MemoryStream();
            this.blockBlob = storageSettings.Container.GetBlockBlobReference(format + "/" + fileName);
            this.blockSize = storageSettings.BlockSizeInKB * 1000;
            foreach (var meta in metaInfo)
            {
                // The supported characters in the blob metadata must be ASCII characters.
                // https://github.com/Azure/azure-sdk-for-net/issues/178
                blockBlob.Metadata.Add(Uri.EscapeUriString(meta.Key), Uri.EscapeUriString(meta.Value));
            }
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return lenght;
            }
        }

        public override long Position
        {
            get; set;
        }

        public override void Flush()
        {
            if (s.Length > 0)
            {
                if (storageSettings.IsMimeTypeResolverEnabled && string.IsNullOrEmpty(blockBlob.Properties.ContentType))
                {
                    using (var mimeBytes = new MemoryStream())
                    {
                        var prev = s.Position;
                        s.Position = 0;
                        s.CopyTo(mimeBytes, 100);
                        mimeBytes.Position = 0;
                        blockBlob.Properties.ContentType = storageSettings.MimeTypeResolver.GetMimeType(mimeBytes.ToArray());
                        s.Position = prev;
                    }
                }

                var blockId = Convert.ToBase64String(BitConverter.GetBytes(index));
                blockDataList.Add(blockId);
                s.Position = 0;
                blockBlob.PutBlock(blockId, s, null);
                index = s.Length + index;
                lenght += s.Length;
                s = new MemoryStream();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            lenght = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            using (var uploadStream = new MemoryStream(buffer, offset, count))
            {
                uploadStream.Position = 0;
                while (uploadStream.Position < uploadStream.Length)
                {
                    var chunk = new byte[this.blockSize - s.Position];
                    var readBytes = uploadStream.Read(chunk, 0, chunk.Length);
                    s.Write(chunk, 0, readBytes);

                    if (s.Position >= this.blockSize)
                        Flush();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            blockBlob.PutBlockList(blockDataList);
            s.Dispose();
            base.Dispose(disposing);
        }
    }
}
