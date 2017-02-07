using System;
using System.Linq;

namespace FileStorage.MimeTypes
{
    public class MimeTypePattern
    {
        public MimeTypePattern(byte[] pattern, ushort offset = 0)
        {
            Bytes = pattern;
            Offset = offset;
        }

        public MimeTypePattern(string hexPattern, ushort offset = 0)
            : this(StringToByteArray(hexPattern), offset)
        { }

        public byte[] Bytes { get; private set; }
        public ushort Offset { get; private set; }

        static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public bool IsMatch(byte[] data)
        {
            if (data.Length >= Bytes.Length + Offset &&
                data.Skip(Offset).Take(Bytes.Length).SequenceEqual(Bytes))
                return true;

            return false;
        }
    }
}
