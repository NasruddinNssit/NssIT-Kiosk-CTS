using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NssIT.Kiosk.PaymentGateway.SecurityProperty.SecureTools
{
    public static class SecurityHelper
    {
        public static string getSignature(string hashSecretKey, string tvmKey)
        {
            string currentdatetime = ToIso8601DateTime(GetUTCNow());
            return AESEncrypt(currentdatetime.ToString(), hashSecretKey, tvmKey);
        }

        public static string HMACSHA512(string text, string secretkey)
        {
            byte[] keyInBytes = Encoding.UTF8.GetBytes(secretkey);
            byte[] payloadInBytes = Encoding.UTF8.GetBytes(text);

            var md5 = new HMACSHA512(keyInBytes);
            byte[] hash = md5.ComputeHash(payloadInBytes);

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        public static string AESEncrypt(string toEncrypt, string hashSecretKey, string key)
        {
            bool checkHashValue = true;

            if (checkHashValue == true)
            {
                toEncrypt = toEncrypt + "\r\n\r\n\r\n" + HMACSHA512(toEncrypt, hashSecretKey);
            }

            byte[] encrypted;
            byte[] IV;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;

                aesAlg.Key = UTF8Encoding.UTF8.GetBytes(key);

                aesAlg.GenerateIV();
                IV = aesAlg.IV;

                aesAlg.Mode = CipherMode.CBC;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(toEncrypt);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            var combinedIvCt = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
            Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

            // Return the encrypted bytes from the memory stream. 

            return Convert.ToBase64String(combinedIvCt);
        }

        public static string AESDecrypt(string cipherString, string hashSecretKey, string key)
        {
            bool checkHashValue = true;

            byte[] cipherTextCombined = Convert.FromBase64String(cipherString);
            string plaintext = null;

            // Create an Aes object 
            // with the specified key and IV. 
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;

                aesAlg.Key = UTF8Encoding.UTF8.GetBytes(key); ;

                byte[] IV = new byte[aesAlg.BlockSize / 8];
                byte[] cipherText = new byte[cipherTextCombined.Length - IV.Length];

                Array.Copy(cipherTextCombined, IV, IV.Length);
                Array.Copy(cipherTextCombined, IV.Length, cipherText, 0, cipherText.Length);

                aesAlg.IV = IV;

                aesAlg.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption. 
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            if (checkHashValue == true)
            {
                string[] arrayPlaintext = Regex.Split(plaintext, "\r\n\r\n\r\n");

                if (arrayPlaintext.Length != 2 || arrayPlaintext[1] != HMACSHA512(arrayPlaintext[0], hashSecretKey))
                {
                    plaintext = "";
                    throw new Exception("AESDecrypt - Hash value is not match");
                }
                else
                {
                    plaintext = arrayPlaintext[0];
                }
            }

            return plaintext;
        }

        public static DateTime GetUTCNow()
        {
            return DateTime.UtcNow;
        }

        public static DateTime ParseIso8601DateTime(string dateToConvert)
        {
            return DateTime.ParseExact(dateToConvert, DateTimeFormat.Iso8601DateTimeFormat, CultureInfo.InvariantCulture);
        }

        public static string ToIso8601DateTime(this DateTime timeForConvert)
        {
            return timeForConvert.ToString(DateTimeFormat.Iso8601DateTimeFormat);
        }
    }
}