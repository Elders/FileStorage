
using FileStorage.FileGenerator;

namespace FileStorage
{
    public interface IFileStorageSettings<T>
    {
        IFileGenerator Generator { get; }
        bool IsGenerationEnabled { get; }
        T UseFileGenerator(IFileGenerator generator);
    }
}