using System;
using System.Collections.Generic;
using System.IO;
using FileStorage.AzureStorage;
using FileGenerator;
using FileStorage.MimeTypes;

namespace FileStorage.Playground
{
    class Program
    {
        static readonly string fileName = "pic2";
        static void Main(string[] args)
        {
            IFileGenerator generator = new FileGenerator.FileGenerator();
            generator.RegisterFormat(new MobileThumbnail());

            IMimeTypeResolver mimeTypeResolver = new DefaultMimeTypeResolver();

            IFileStorageRepository storage = Azure(generator, mimeTypeResolver);

            var bytes = File.ReadAllBytes($@"E:\{fileName}");
            var contentType = new FileStorage.MimeTypes.DefaultMimeTypeResolver().GetMimeType(bytes);
            var metaData = new List<FileMeta> { new FileMeta("key-kv", "value-kv") };

            storage.Upload(fileName, bytes, metaData);
            var url = storage.GetFileUri(fileName);

            var existsTest3 = storage.FileExists("gg");
            var existsTest4 = storage.FileExists(fileName);

            //var file = storage.Download(fileName);
            //var file2 = storage.Download(fileName, MobileThumbnail.FormatName);
            ////var file3 = fileStorage.Download("pic.PNG", "mobilefull");
            //var existsTest5 = storage.FileExists(fileName, "mobilefull");
            //var existsTest6 = storage.FileExists(fileName, "mobilethumbnail");

            //var path = $@"E:\Storage\{storage.GetType().Name}\{DateTime.Now.ToString("yyyyMMddHHmmssfff")}\";
            //if (Directory.Exists(path) == false)
            //    Directory.CreateDirectory(path);

            //var pathLocalWithFile = Path.Combine(path, file.Name);
            //File.WriteAllBytes(pathLocalWithFile, file.Data);
        }


        static IFileStorageRepository Azure(IFileGenerator generator, IMimeTypeResolver mimeTypeResolver)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=saylis;AccountKey=Jtov1nY+ADWDZbm5kzMLM8tAtCJILPYYQFcE2MSJQKZC2RN928zrxrLKAIOVKDQJJZVArNPK5tygHum0QaaqEQ==;BlobEndpoint=https://saylis.blob.core.windows.net/;TableEndpoint=https://saylis.table.core.windows.net/;QueueEndpoint=https://saylis.queue.core.windows.net/;FileEndpoint=https://saylis.file.core.windows.net/";
            var containerName = "venkov-container";

            var settings = new AzureStorageSettings(connectionString, containerName)
                .UseFileGenerator(generator)
                .UseUrlExpiration(new UrlExpiration(120))
                .UseMimeTypeResolver(mimeTypeResolver);
            var storage = new AzureFileStorageRepository(settings);

            return storage;
        }

        static IFileStorageRepository AmazonS3(IFileGenerator generator, IMimeTypeResolver mimeTypeResolver)
        {
            var accessKey = "AKIAIDDKF265DR3YXHAQ";
            var secretKey = "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";
            var region = "us-east-1";
            var bucketName = "tcos";

            var settings = new S3Storage.S3FileStorageSettings(accessKey, secretKey, region, bucketName)
                .UseFileGenerator(generator)
                .UseUrlExpiration(new UrlExpiration(120))
                .UseMimeTypeResolver(mimeTypeResolver);
            var storage = new S3Storage.S3FileStorageRepository(settings);

            return storage;
        }
    }

}
