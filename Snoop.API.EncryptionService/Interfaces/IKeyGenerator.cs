namespace Snoop.API.EncryptionService.Services.Interfaces
{
    public interface IKeyGenerator
    {
        string GetUniqueKey(int size);
    }
}