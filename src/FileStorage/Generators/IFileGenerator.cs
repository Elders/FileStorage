using System.Collections.Generic;
using FileStorage.FileFormats;

namespace FileStorage.Generators
{
    public interface IFileGenerator
    {
        /// <summary>
        /// Generates a file to specific format
        /// </summary>
        /// <param name="data"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        FIleGenerateResponse Generate(byte[] data, string format);

        /// <summary>
        /// Registers Format
        /// </summary>
        /// <param name="format"></param>
        IFileGenerator RegisterFormat(IFileFormat format);

        /// <summary>
        /// All registered formats
        /// </summary>
        IEnumerable<IFileFormat> Formats { get; }
    }
}