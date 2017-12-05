using System.Collections.Generic;
using System.IO;
using FileStorage.AmazonS3;
using FileStorage.MimeTypes;
using FileStorage.Azure;

namespace FileStorage.Playground
{
    class Program
    {
        static readonly string fileName = @"kv\m4a.m4a";
        static void Main(string[] args)
        {
            IFileGenerator generator = new FileGenerator.FileGenerator();
            generator.RegisterFormat(new MobileThumbnail());

            IMimeTypeResolver mimeTypeResolver = new DefaultMimeTypeResolver();
            IFileStorageRepository storage = FileSystem(generator, mimeTypeResolver);

            var bytes = File.ReadAllBytes($@"D:\{fileName}");
            var contentType = new DefaultMimeTypeResolver().GetMimeType(bytes);
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
            var connectionString = string.Empty;
            var containerName = string.Empty;

            var settings = new AzureStorageSettings(connectionString, containerName, 1024 * 4)
                .UseFileGenerator(generator)
                .UseUrlExpiration(new UrlExpiration(120))
                .UseMimeTypeResolver(mimeTypeResolver);
            var storage = new AzureFileStorageRepository(settings);

            return storage;
        }

        static IFileStorageRepository AmazonS3(IFileGenerator generator, IMimeTypeResolver mimeTypeResolver)
        {
            var accessKey = string.Empty;
            var secretKey = string.Empty;
            var region = string.Empty;
            var bucketName = string.Empty;

            var settings = new S3FileStorageSettings(accessKey, secretKey, region, bucketName)
                .UseFileGenerator(generator)
                .UseUrlExpiration(new UrlExpiration(120))
                .UseMimeTypeResolver(mimeTypeResolver);
            var storage = new S3FileStorageRepository(settings);

            return storage;
        }

        static IFileStorageRepository InMemory(IFileGenerator generator, IMimeTypeResolver mimeTypeResolver)
        {
            var settings = new InMemoryFileStorage.InMemoryFileStorageSettings()
                .UseFileGenerator(generator)
                .UseMimeTypeResolver(mimeTypeResolver);
            var storage = new InMemoryFileStorage.InMemoryFileStorageRepository(settings);

            return storage;
        }

        static IFileStorageRepository FileSystem(IFileGenerator generator, IMimeTypeResolver mimeTypeResolver)
        {
            var path = @"D:\kv";
            var settings = new FileSystem.FileSystemFileStorageSettings(path)
                .UseFileGenerator(generator)
                .UseMimeTypeResolver(mimeTypeResolver);
            var storage = new FileSystem.FileSystemFileStorageRepository(settings);

            return storage;
        }
    }

}
