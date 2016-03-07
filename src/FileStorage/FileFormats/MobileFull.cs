using System.Collections.Generic;
using System.IO;
using ImageResizer;
using FileStorage.Extensions;

namespace FileStorage.FileFormats
{
    public class MobileFull : IFileFormat
    {
        //private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(MobileFull));

        public IFileStorageRepository FileStorageRepository { get; private set; }

        public MobileFull(IFileStorageRepository repo)
        {
            FileStorageRepository = repo;
        }

        public static string FormatName = typeof(MobileFull).Name.ToLowerInvariant();

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
                instr.Width = 1920;
                instr.Height = 1080;

                var job = new ImageJob(new MemoryStream(sourceFile.Data), newStream, instr);

                try
                {
                    var result = ImageResizer.ImageBuilder.Current.Build(job);
                }
                catch (ImageResizer.ImageCorruptedException ex)
                {
                    // log.Error(ex);

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
            instr.Width = 1920;
            instr.Height = 1080;

            var job = new ImageJob(new MemoryStream(data), newStream, instr);

            try
            {
                var result = ImageResizer.ImageBuilder.Current.Build(job);
            }
            catch (ImageResizer.ImageCorruptedException ex)
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
