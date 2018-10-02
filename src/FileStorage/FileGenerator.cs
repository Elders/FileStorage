using System;
using System.Collections.Generic;
using FileStorage.FileFormats;

namespace FileStorage.FileGenerator
{
    public class FileGenerator : IFileGenerator
    {
        readonly Dictionary<string, IFileFormat> formats;

        public IEnumerable<IFileFormat> Formats => formats.Values;

        public FileGenerator()
        {
            formats = new Dictionary<string, IFileFormat>();
            RegisterFormat(new Original());
        }

        public FileGenerator(IEnumerable<IFileFormat> formats)
            : this()
        {
            if (ReferenceEquals(formats, null) == true) throw new ArgumentNullException(nameof(formats));

            foreach (var format in formats)
            {
                if (this.formats.ContainsKey(format.Name) == false)
                    this.formats.Add(format.Name, format);

            }
        }

        public FileGenerateResponse Generate(byte[] data, string format)
        {
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (formats.ContainsKey(format) == false) throw new NotSupportedException($"This file format is not supported. {format}");

            var formatInstance = formats[format];
            var newData = formatInstance.Generate(data);

            return newData;
        }

        public IFileGenerator RegisterFormat(IFileFormat format)
        {
            if (formats.ContainsKey(format.Name) == false)
                formats.Add(format.Name, format);
            return this;
        }
    }
}
