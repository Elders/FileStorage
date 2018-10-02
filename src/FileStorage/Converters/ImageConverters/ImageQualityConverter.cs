using ImageProcessor;
using System.IO;

namespace FileStorage.Converters.ImageProcessor
{
    public class ImageQualityConverter : IConverter
    {
        public bool CanConvert(ConverterContext converterParams)
        {
            return converterParams.HasParameter("quality");
        }

        public byte[] Convert(byte[] data, ConverterContext converterParams)
        {
            var quality = int.Parse(converterParams.GetParameter("quality").ToString());


            using (MemoryStream inStream = new MemoryStream(data))
            using (MemoryStream outStream = new MemoryStream())
            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
            {
                imageFactory.Load(inStream)
                            .Quality(quality)
                            .Save(outStream);


                return outStream.ToArray();
            }
        }
    }
}


