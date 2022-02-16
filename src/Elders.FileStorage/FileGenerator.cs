using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FileStorage.FileFormats;

namespace FileStorage
{
    public class FileGenerator : IFileGenerator
    {
        readonly ConcurrentDictionary<string, IFileFormat> formats;

        public IEnumerable<IFileFormat> Formats => formats.Values;

        public FileGenerator()
        {
            formats = new ConcurrentDictionary<string, IFileFormat>();
            RegisterFormat(new Original());
        }

        public FileGenerator(IEnumerable<IFileFormat> formats)
            : this()
        {
            if (ReferenceEquals(formats, null) == true) throw new ArgumentNullException(nameof(formats));

            foreach (var format in formats)
            {
                this.formats.TryAdd(format.Name, format);
            }
        }

        public FIleGenerateResponse Generate(byte[] data, string format)
        {
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (formats.ContainsKey(format) == false) throw new NotSupportedException($"This file format is not supported. {format}");

            var formatInstance = formats[format];
            var newData = formatInstance.Generate(data);

            return newData;
        }

        public IFileGenerator RegisterFormat(IFileFormat format)
        {
            formats.TryAdd(format.Name, format);

            return this;
        }
    }
}
