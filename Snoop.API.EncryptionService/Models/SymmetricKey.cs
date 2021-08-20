namespace Snoop.API.EncryptionService.Models
{
    public class SymmetricKey : Key
    {
        public string InitializationVector { get; set; }
        public string Key { get; set; }
    }
}
