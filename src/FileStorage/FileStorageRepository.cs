using FileStorage.Cache;
using FileStorage.Converters;
using FileStorage.Converters.ImageProcessor;
using FileStorage.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage
{
    /// <summary>
    /// Abstraction for the FileStorageRepository
    /// </summary>
    public abstract class FileStorageRepository<T> : IFileStorageRepository
        where T : FileStorageSettings
    {
        protected readonly T _storageSettings;

        public FileStorageRepository(T storageSettings)
        {
            if (ReferenceEquals(storageSettings, null) == true) throw new ArgumentNullException(nameof(storageSettings));

            _storageSettings = storageSettings;
        }

        public abstract void Delete(string fileName);
        public abstract bool FileExists(string fileName);
        public virtual SaveResult Save(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo)
        {
            var validationResult = Validate(data);

            if (validationResult.Success == false)
                return new SaveResult(false, validationResult.Error);
            else
                return SaveResult.Successfull;
        }

        public abstract IFile Get(string fileName);
        public abstract string GetFileUri(string fileName);
        public abstract Stream GetStream(string fileName, IEnumerable<FileMeta> metaInfo);

        private ValidationResult Validate(byte[] data)
        {
            foreach (var validation in _storageSettings.Validations)
            {
                if (validation.IsValid(data) == false)
                    return new ValidationResult(false, validation.GetErrorMessage());
            }

            return ValidationResult.Successfull;
        }

        public byte[] GetConverted(string fileName, ConverterContext converterContext)
        {
            if (_storageSettings.HasCache && _storageSettings.Cache.HasCachedFile(new FileInformation(fileName, converterContext.GetAsFileMeta())))
                return _storageSettings.Cache.GetCachedFile(new FileInformation(fileName, converterContext.GetAsFileMeta())).Data;


            var data = Get(Path.GetFileNameWithoutExtension(fileName)).Data;
            var convertersToUse = _storageSettings.GetMatchingConverters(converterContext);

            foreach (var converter in convertersToUse)
            {
                data = converter.Convert(data, converterContext);
            }

            if (_storageSettings.HasCache)
                _storageSettings.Cache.CacheFile(new FileStorageFile(new FileInformation(fileName, converterContext.GetAsFileMeta()), data));

            return data;
        }
    }
}
