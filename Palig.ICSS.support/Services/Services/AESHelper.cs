

using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Palig.ICSS.support.Services.Interfaces;


namespace Palig.ICSS.support.Services.Services
{
    public class AESHelper : IAESHelper
    {
        
    private readonly string _secretKey;

        public AESHelper(IConfiguration configuration)
        {
            _secretKey = configuration["SECURITY:SecretAES"];
            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new ArgumentNullException("The SecretAES key is missing in appsettings.json.");
            }
        }

        // Generate a 256-bit (32-byte) AES key and return it as a Base64 string
        public static string GenerateKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }

        // Encrypt a string using the AES key from appsettings.json
        public string Encrypt(string plainText)
        {
            byte[] keyBytes = Convert.FromBase64String(_secretKey);
            byte[] ivBytes = GenerateIV(); // Generate a new IV for each encryption
            byte[] encryptedBytes;

            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    ms.Write(ivBytes, 0, ivBytes.Length); // Prepend IV to the ciphertext
                    using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cryptoStream))
                    {
                        writer.Write(plainText);
                    }

                    encryptedBytes = ms.ToArray();
                }
            }

            return Convert.ToBase64String(encryptedBytes);
        }

        // Decrypt a string using the AES key from appsettings.json
        public string Decrypt(string cipherText)
        {
            byte[] keyBytes = Convert.FromBase64String(_secretKey);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes;

                // Extract the IV from the beginning of the cipherText
                byte[] ivBytes = new byte[16];
                Array.Copy(cipherBytes, 0, ivBytes, 0, ivBytes.Length);
                aes.IV = ivBytes;

                byte[] actualCipherBytes = new byte[cipherBytes.Length - ivBytes.Length];
                Array.Copy(cipherBytes, ivBytes.Length, actualCipherBytes, 0, actualCipherBytes.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(actualCipherBytes))
                using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        // Generate a 128-bit (16-byte) IV
        private static byte[] GenerateIV()
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateIV();
                return aes.IV;
            }
        }
    }


}
