using System;
using System.Collections.Generic;
using System.IO;
using FileStorage.AzureStorage;
using FileStorage.Generators;

namespace FileStorage.Client
{
    class Program
    {
        static readonly string fileName = "IMG_20160203_232515.jpg";
        static void Main(string[] args)
        {
            string connectionString = string.Empty;
            string containerName = string.Empty;
            var bytes = File.ReadAllBytes($@"E:\{fileName}");

            var gggg = new FileStorage.MimeTypes.DefaultMimeTypeResolver().GetMimeType(bytes);
            var metaData = new List<FileMeta> { new FileMeta("key-kv", "value-kv") };


            var generator = new FileGenerator();
            generator.RegisterFormat(new MobileThumbnail());
            var s3Settings = new S3Storage.S3FileStorageSettings(string.Empty, string.Empty, string.Empty, string.Empty)
                .UseFileGenerator(generator)
                .UseUrlExpiration(new UrlExpiration(120));
            var s3storage = new S3Storage.S3FileStorageRepository(s3Settings);

            var azureStorageSettings = new AzureStorageSettings(connectionString, containerName);
            var storage = new AzureFileStorageRepository(azureStorageSettings);
            storage.Upload(fileName, bytes, metaData);
            var url = storage.GetFileUri(fileName);

            var existsTest3 = storage.FileExists("gg");
            var existsTest4 = storage.FileExists(fileName);

            var file = storage.Download(fileName);
            var file2 = storage.Download(fileName, MobileThumbnail.FormatName);
            //var file3 = fileStorage.Download("pic.PNG", "mobilefull");
            var existsTest5 = storage.FileExists(fileName, "mobilefull");
            var existsTest6 = storage.FileExists(fileName, "mobilethumbnail");

            var path = $@"E:\Storage\{storage.GetType().Name}\{DateTime.Now.ToString("yyyyMMddHHmmssfff")}\";
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            var pathLocalWithFile = Path.Combine(path, file.Name);
            File.WriteAllBytes(pathLocalWithFile, file.Data);
        }
    }
}
