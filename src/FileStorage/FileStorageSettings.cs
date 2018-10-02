using FileStorage.Cache;
using FileStorage.Converters;
using FileStorage.Validations;
using System.Collections.Generic;
using System.Linq;

namespace FileStorage
{
    public abstract class FileStorageSettings
    {
        public FileStorageSettings()
        {
            Validations = new List<IValidation>();
            Converters = new List<IConverter>();
        }



        public IList<IValidation> Validations { get; private set; }
        public IList<IConverter> Converters { get; private set; }

        internal bool HasCache { get { return ReferenceEquals(null, Cache) == false; } }
        internal IFileStorageCache Cache { get; private set; }

        public void AddValidation(IValidation validation)
        {
            Validations.Add(validation);
        }

        public void AddConverter(IConverter converter)
        {
            Converters.Add(converter);
        }

        public void UseCache(IFileStorageCache fileStorageCache)
        {
            Cache = fileStorageCache;
        }

        public IConverter[] GetMatchingConverters(ConverterContext converterContext)
        {
            return Converters.Where(x => x.CanConvert(converterContext)).ToArray();
        }
    }
}
