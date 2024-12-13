using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KNTCommon.Business.Repositories
{
    public class Encryption : IEncryption
    {
        private readonly IConfiguration Config;

        public Encryption(IConfiguration config)
        {
            Config = config;
        }
        public Task<byte[]> Encrypt(string plaintext, byte[] iv)
        {
            //iv is generated randomly for every user 
            //key is universal in config 
            var key = GetKeyFromConfigAsByteArray();

            // Convert the plaintext to bytes
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            // Create AES cipher with CBC mode and PKCS7 padding
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CBC/PKCS7Padding");

            // Initialize the cipher with encryption mode and the key/IV
            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            // Encrypt the plaintext
            byte[] encryptedBytes = cipher.DoFinal(plaintextBytes);

            //var resultString = Encoding.UTF8.GetString(encryptedBytes);

            return Task.FromResult(encryptedBytes);
        }

        public Task<string> Decrypt(byte[] encryptedData, byte[] iv)
        {
            //iv is generated randomly for every user 
            //key is universal in config 
            var key = GetKeyFromConfigAsByteArray();

            //byte[] encryptedData = Encoding.UTF8.GetBytes(encryptedDataString);

            // Create AES cipher with CBC mode and PKCS7 padding
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CBC/PKCS7Padding");

            // Initialize the cipher with decryption mode and the key/IV
            cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            // Decrypt the encrypted data
            byte[] decryptedBytes = cipher.DoFinal(encryptedData);

            // Convert the decrypted bytes to plaintext
            string plaintext = Encoding.UTF8.GetString(decryptedBytes);

            return Task.FromResult(plaintext);
        }

        public byte[] GenerateRandomIV()
        {
            string? ivSizeString = Config.GetSection("CustomSettings:IvSize").Value;
            int ivSize = string.IsNullOrEmpty(ivSizeString) ? 0 : int.Parse(ivSizeString);
            byte[] iv = new byte[ivSize / 8]; // Convert bits to bytes
            SecureRandom random = new SecureRandom();
            random.NextBytes(iv);
            return iv;
        }

        private byte[] GetKeyFromConfigAsByteArray()
        {
            var key = Config.GetSection("CustomSettings:Key").Value;
            if(string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("The configuration key 'CustomSettings:Key' is missing or empty.");
            }
            return Encoding.UTF8.GetBytes(key);
        }

    }
}
