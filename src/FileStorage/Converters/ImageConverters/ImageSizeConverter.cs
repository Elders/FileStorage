using FileStorage.Converters.Extensions;
using ImageProcessor;
using ImageProcessor.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;

namespace FileStorage.Converters.ImageProcessor
{

    public class ImageSizeConverter : IConverter
    {
        readonly string[] _allowedParams = new string[] { "width", "height" };

        public bool CanConvert(ConverterContext converterParams)
        {
            return converterParams.Parameters.Intersect(_allowedParams).Any();
        }

        public byte[] Convert(byte[] data, ConverterContext context)
        {
            Size size = default(Size);
            int width, height;
            if (context.TryGetParameter("width", out width) && context.TryGetParameter("height", out height))
                size = new Size(width, height);

            var mode = context.ParseEnum<ResizeMode>("mode");
            var position = context.ParseEnum<AnchorPosition>("anchor");

            bool upscale;
            context.TryGetParameter("upscale", out upscale);

            float[] center;
            if (context.TryGetParameter("center", out center) == false)
                center = new float[] { };


            var layer = new ResizeLayer(size)
            {
                ResizeMode = mode,
                AnchorPosition = position,
                Upscale = upscale,
                CenterCoordinates = center
            };


            using (MemoryStream inStream = new MemoryStream(data))
            using (MemoryStream outStream = new MemoryStream())
            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
            {
                imageFactory.Load(inStream)
                            .Resize(layer)
                            .Save(outStream);


                return outStream.ToArray();
            }
        }
    }
}


