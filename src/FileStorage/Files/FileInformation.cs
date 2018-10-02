using System;
using System.Collections.Generic;

namespace FileStorage.Files
{
    public class FileInformation : IEquatable<FileInformation>
    {
        public FileInformation(string name)
        {
            //TODO null check

            Name = name;
            Meta = new HashSet<FileMeta>();
        }

        public FileInformation(string name, IEnumerable<FileMeta> meta)
        {
            //TODO null check

            Name = name;
            Meta = meta;
        }

        public string Name { get; private set; }
        public IEnumerable<FileMeta> Meta { get; private set; }

        /// <summary>
        /// A name unique composed from the name and the meta
        /// </summary>
        public string UniqueName { get { return GetHashCode().ToString(); } }

        public override bool Equals(object obj)
        {
            return Equals(obj as FileInformation);
        }

        public bool Equals(FileInformation other)
        {
            return other != null &&
                   Name == other.Name &&
                   EqualityComparer<IEnumerable<FileMeta>>.Default.Equals(Meta, other.Meta);
        }

        public override int GetHashCode()
        {
            var hashCode = 150949998;

            hashCode = hashCode * 31 + Name.GetHashCode();

            foreach (var meta in Meta)
            {
                hashCode = hashCode * 31 + meta.GetHashCode();
            }

            return hashCode;
        }
    }
}
