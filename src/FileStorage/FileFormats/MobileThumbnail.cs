using FileStorage.Extensions;
using ImageResizer;
using System.Collections.Generic;
using System.IO;

namespace FileStorage.FileFormats
{
    public class MobileThumbnail : IFileFormat
    {
        public IFileStorageRepository FileStorageRepository { get; private set; }

        public MobileThumbnail(IFileStorageRepository repo)
        {
            FileStorageRepository = repo;
        }

        public static string FormatName = typeof(MobileThumbnail).Name.ToLowerInvariant();

        public string Name { get { return FormatName; } }

        public bool FindFile(string filename)
        {
            return FileStorageRepository.FileExists(filename, Name);
        }

        public bool Generate(string sourceName)
        {
            var sourceFile = FileStorageRepository.Download(sourceName);

            var newStream = new MemoryStream();

            if (sourceFile != null)
            {
                var instr = new Instructions();
                instr.Mode = FitMode.Max;
                instr.Width = 384;
                instr.Height = 216;

                var job = new ImageJob(new MemoryStream(sourceFile.Data), newStream, instr);

                try
                {
                    var result = ImageBuilder.Current.Build(job);
                }
                catch (ImageCorruptedException ex)
                {
                    //log.Error(ex);

                    return false;
                }
            }

            if (newStream.Length > 0)
            {
                newStream.Position = 0;

                var data = newStream.ToByteArray();

                var metaData = new List<FileMeta>();
                metaData.Add(new FileMeta("SourceName", sourceName));

                FileStorageRepository.Upload(sourceName, data, metaData, Name);

                return true;
            }

            return false;
        }

        public byte[] Generate(byte[] data)
        {
            var newStream = new MemoryStream();

            var instr = new Instructions();
            instr.Mode = FitMode.Max;
            instr.Width = 384;
            instr.Height = 216;

            var job = new ImageJob(new MemoryStream(data), newStream, instr);

            try
            {
                var result = ImageResizer.ImageBuilder.Current.Build(job);
            }
            catch (ImageCorruptedException ex)
            {
                //log.Error(ex);

                return null;
            }

            if (newStream.Length > 0)
            {
                newStream.Position = 0;
            }

            return newStream.ToByteArray();
        }
    }
}
