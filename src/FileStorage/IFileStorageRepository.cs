using FileStorage.FileFormats;
using System.Collections.Generic;

namespace FileStorage
{
    public interface IFileStorageRepository
    {
        /// <summary>
        /// Stores a file.
        /// </summary>
        /// <param name="fileName">The name of the file where it is going to be stored.</param>
        /// <param name="data">The file represented as <see cref="byte[]"/></param>
        /// <param name="metaInfo">Meta information attached to the file. ex: geo coordinates of a photo.</param>
        /// <param name="format">The file format</param>
        void Upload(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original");

        /// <summary>
        /// Retreives a file from.
        /// </summary>
        /// <param name="fileName">The name of the file how it is stored.</param>
        /// <param name="format">original, mobilefull, mobilethumbnail</param>
        /// <returns>Returns the file and optionally IF IT IS A PICTURE YOU CAN USE THE LAST PARAMETER WHICH IS SPECIFICALLY AND ONLY FOR PICTURE.</returns>
        IFile Download(string fileName, string format = "original");

        /// <summary>
        /// Gets the file location based on SOMEWHERE
        /// </summary>
        /// <param name="fileName">The fileName</param>
        /// <param name="format">original, mobilefull, mobilethumbnail</param>
        /// <returns>Returns the file location.</returns>
        string GetFileUri(string fileName, string format = "original");

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="fileName">The file to check.</param>
        /// <param name="format">The file format to check.</param>
        /// <returns>true if file</returns>
        bool FileExists(string fileName, string format = "original");
    }
}