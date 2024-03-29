﻿using System.IO;

namespace FileStorage.Extensions
{
    public static class StreamExtentions
    {
        public static byte[] ToByteArray(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.ReadAsync(buffer, 0, buffer.Length).GetAwaiter().GetResult()) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
