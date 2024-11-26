using System.Security.Cryptography;
using System.Text;

namespace BumbleBeeFoundation_Client
{
    // Encryption helper to store ID and tax numbers securely in the database
    public static class EncryptionHelper
    {
        private static readonly string dataSec = "BumbleBee123!"; 

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using (var aes = Aes.Create())
            {
                var key = Encoding.UTF8.GetBytes(dataSec);
                Array.Resize(ref key, 32); 
                aes.Key = key;
                aes.IV = new byte[16]; 

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
}
