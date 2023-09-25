using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RomanaWeb.Classes
{                                         
    public static class Encyptmethod
    {
        public static byte[] IV = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        public static int BlockSize = 128;
        public static string key = "ShcoolApi";

        #region Encrypt string 
        public static string EncryptStringToBytes_Aes(string plainText)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(plainText);

            SymmetricAlgorithm crypt = Aes.Create();
            HashAlgorithm hash = MD5.Create();
            crypt.BlockSize = BlockSize;
            crypt.Key = hash.ComputeHash(Encoding.Unicode.GetBytes(key));
            crypt.IV = IV;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream =
                   new CryptoStream(memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                }

                plainText = Convert.ToBase64String(memoryStream.ToArray());
            }
            // Return the encrypted bytes from the memory stream.
            return plainText;
        }
        #endregion

        #region Decrypt string
        public static string DecryptStringFromBytes_Aes(string plainText)
        {

            // Declare the string used to hold
            // the decrypted text.
            byte[] bytes = Convert.FromBase64String(plainText);
            SymmetricAlgorithm crypt = Aes.Create();
            HashAlgorithm hash = MD5.Create();
            crypt.Key = hash.ComputeHash(Encoding.Unicode.GetBytes(key));
            crypt.IV = IV;
            // Create an Aes object
            // with the specified key and IV.
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (CryptoStream cryptoStream =
                   new CryptoStream(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    byte[] decryptedBytes = new byte[bytes.Length];
                    cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                    plainText = Encoding.Unicode.GetString(decryptedBytes);
                }
            }

            return plainText.Replace("\u0000","");
        }
        #endregion
    }
}
