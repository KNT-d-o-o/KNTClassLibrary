namespace KNTCommon.Business.Repositories
{
    public interface IEncryption
    {
        Task<byte[]> Encrypt(string plaintext, byte[] iv);
        Task<string> Decrypt(byte[] encryptedData, byte[] iv);
        byte[] GenerateRandomIV();
    }
}