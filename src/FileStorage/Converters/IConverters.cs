using FileStorage.Converters.ImageProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Converters
{
    public interface IConverter
    {
        byte[] Convert(byte[] data, ConverterContext converterParams);
        bool CanConvert(ConverterContext converterParams);
    }
}
