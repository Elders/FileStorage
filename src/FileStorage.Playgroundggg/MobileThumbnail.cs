using System.IO;
using FileStorage.Extensions;
using ImageResizer;
using FileGenerator.FileFormats;
using FileGenerator;

namespace FileStorage.Playground
{
    public class MobileThumbnail : IFileFormat
    {
        public static string FormatName = typeof(MobileThumbnail).Name.ToLowerInvariant();

        public string Name { get { return FormatName; } }



        public FIleGenerateResponse Generate(byte[] data)
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
            catch (ImageCorruptedException)
            {
                //log.Error(ex);

                return null;
            }

            if (newStream.Length > 0)
            {
                newStream.Position = 0;
            }

            return new FIleGenerateResponse(true, newStream.ToByteArray(), string.Empty);
        }
    }
}
