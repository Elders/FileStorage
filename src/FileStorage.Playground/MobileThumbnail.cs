//using System.IO;
//using FileStorage.Extensions;
//using FileStorage.FileFormats;
//using System;

//namespace FileStorage.Playground
//{
//    public class MobileThumbnail : IFileFormat
//    {
//        public static string FormatName = typeof(MobileThumbnail).Name.ToLowerInvariant();

//        public string Name { get { return FormatName; } }



//        public FileGenerateResponse Generate(byte[] data)
//        {
//            var newStream = new MemoryStream();

//            var instr = new Instructions();
//            instr.Mode = FitMode.Max;
//            instr.Width = 384;
//            instr.Height = 216;

//            var job = new ImageJob(new MemoryStream(data), newStream, instr);

//            try
//            {
//                ImageBuilder.Current.Build(job);
//            }
//            catch (Exception ex)
//            {
//                return new FileGenerateResponse(false, null, ex.Message);
//            }

//            if (newStream.Length > 0)
//            {
//                newStream.Position = 0;
//            }

//            return new FileGenerateResponse(true, newStream.ToByteArray(), string.Empty);
//        }
//    }
//}
