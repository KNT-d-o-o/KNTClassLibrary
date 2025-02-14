using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KNTCommon.Data.Models
{
    public class PManager
    {
        private static readonly string EncryptionKey = "VasaZeloMocnaKljuc123";

        public static string EncryptPassword(string plainText)
        {
            using (var aes = Aes.Create())
            {
                var key = GetValidKey(EncryptionKey);
                aes.Key = key;
                aes.IV = new byte[16];

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private static byte[] GetValidKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Encryption key cannot be null or empty.");

            key = key.PadRight(32).Substring(0, 32);
            return Encoding.UTF8.GetBytes(key);
        }

        public static string DecryptPassword(string encryptedText)
        {
            using (var aes = Aes.Create())
            {
                var key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32));
                aes.Key = key;
                aes.IV = new byte[16]; // Mora biti enako kot pri šifriranju
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(Convert.FromBase64String(encryptedText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }
}
