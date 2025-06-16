using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Crypto
{
    public static class CryptoHelper
    {
        private static readonly string Key = "this-is-your-32-char-secret-key!";

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);

            var iv = fullCipher.Take(16).ToArray();
            var cipherBytes = fullCipher.Skip(16).ToArray();

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}
