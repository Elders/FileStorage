namespace FileStorage.Validations
{
    public interface IValidation
    {
        bool IsValid(byte[] data);
        string GetErrorMessage();
    }
}
