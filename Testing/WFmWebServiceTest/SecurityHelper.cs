using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WFmWebServiceTest
{
    public class SecurityHelper
    {
        //private static string hashSecretKey = System.Configuration.ConfigurationManager.AppSettings["TOSHashSecretKey"];
        //static string key = "fqwe0eqw_uN12&Ufnq_023E12";
        //static string key = "dlfjhdkfhsd 9823754893247b dskfjhsdfkhds wkiherewjhr";

        //public static string Key
        //{
        //    get
        //    {
        //        return "e10adc3949ba59abbe56e057f20f883e";
        //    }
        //}

        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public static string PassEncrypt(string originalPassword)
        {
            //Declarations
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;
            //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)
            md5 = new MD5CryptoServiceProvider();
            originalBytes = ASCIIEncoding.Default.GetBytes(originalPassword);
            encodedBytes = md5.ComputeHash(originalBytes);

            //Convert encoded bytes back to a 'readable' string
            return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
        }


        //public static string Encrypt(string toEncrypt, string key, bool useHashing)
        //{
        //    byte[] keyArray;
        //    byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

        //    //System.Windows.Forms.MessageBox.Show(key);
        //    if (useHashing)
        //    {
        //        MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
        //        keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
        //        hashmd5.Clear();
        //    }
        //    else
        //        keyArray = UTF8Encoding.UTF8.GetBytes(key);

        //    TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
        //    tdes.Key = keyArray;
        //    tdes.Mode = CipherMode.ECB;
        //    tdes.Padding = PaddingMode.PKCS7;

        //    ICryptoTransform cTransform = tdes.CreateEncryptor();
        //    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        //    tdes.Clear();
        //    return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        //}

        public static string Encrypt(string toEncrypt, string hashedPassword)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hashedPassword));
            hashmd5.Clear();

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        /// <summary>
        /// DeCrypt a string using dual encryption method. Return a DeCrypted clear string
        /// </summary>
        /// <param name="cipherString">encrypted string</param>
        /// <param name="useHashing">Did you use hashing to encrypt this data? pass true is yes</param>
        /// <returns></returns>
        public static string Decrypt(string cipherString, string hashedPassword)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hashedPassword));
            hashmd5.Clear();

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }


        public static string HMACSHA512(string text, string secretkey)
        {
            byte[] keyInBytes = Encoding.UTF8.GetBytes(secretkey);
            byte[] payloadInBytes = Encoding.UTF8.GetBytes(text);

            var md5 = new HMACSHA512(keyInBytes);
            byte[] hash = md5.ComputeHash(payloadInBytes);

            return BitConverter.ToString(hash).Replace("-", string.Empty);


        }

        //public static string AESEncrypt(string toEncrypt, string key, bool checkHashValue)
        //{


        //    if (checkHashValue == true)
        //    {
        //        toEncrypt = toEncrypt + "\r\n\r\n\r\n" + HMACSHA512(toEncrypt, hashSecretKey);
        //    }


        //    byte[] encrypted;
        //    byte[] IV;

        //    using (Aes aesAlg = Aes.Create())
        //    {
        //        aesAlg.KeySize = 256;
        //        aesAlg.BlockSize = 128;

        //        aesAlg.Key = UTF8Encoding.UTF8.GetBytes(key);

        //        aesAlg.GenerateIV();
        //        IV = aesAlg.IV;

        //        aesAlg.Mode = CipherMode.CBC;

        //        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        //        // Create the streams used for encryption. 
        //        using (var msEncrypt = new MemoryStream())
        //        {
        //            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (var swEncrypt = new StreamWriter(csEncrypt))
        //                {
        //                    //Write all data to the stream.
        //                    swEncrypt.Write(toEncrypt);
        //                }
        //                encrypted = msEncrypt.ToArray();
        //            }
        //        }
        //    }

        //    var combinedIvCt = new byte[IV.Length + encrypted.Length];
        //    Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
        //    Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

        //    // Return the encrypted bytes from the memory stream. 

        //    return Convert.ToBase64String(combinedIvCt);
        //}

        //public static string AESDecrypt(string cipherString, string key, bool checkHashValue)
        //{

        //    byte[] cipherTextCombined = Convert.FromBase64String(cipherString);
        //    string plaintext = null;

        //    // Create an Aes object 
        //    // with the specified key and IV. 
        //    using (Aes aesAlg = Aes.Create())
        //    {
        //        aesAlg.KeySize = 256;
        //        aesAlg.BlockSize = 128;

        //        aesAlg.Key = UTF8Encoding.UTF8.GetBytes(key); ;

        //        byte[] IV = new byte[aesAlg.BlockSize / 8];
        //        byte[] cipherText = new byte[cipherTextCombined.Length - IV.Length];

        //        Array.Copy(cipherTextCombined, IV, IV.Length);
        //        Array.Copy(cipherTextCombined, IV.Length, cipherText, 0, cipherText.Length);

        //        aesAlg.IV = IV;

        //        aesAlg.Mode = CipherMode.CBC;

        //        // Create a decrytor to perform the stream transform.
        //        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        //        // Create the streams used for decryption. 
        //        using (var msDecrypt = new MemoryStream(cipherText))
        //        {
        //            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //            {
        //                using (var srDecrypt = new StreamReader(csDecrypt))
        //                {

        //                    // Read the decrypted bytes from the decrypting stream
        //                    // and place them in a string.
        //                    plaintext = srDecrypt.ReadToEnd();
        //                }
        //            }
        //        }

        //    }


        //    string[] arrayPlaintext = Regex.Split(plaintext, "\r\n\r\n\r\n");

        //    if (checkHashValue == true && (arrayPlaintext.Length != 2 || arrayPlaintext[1] != HMACSHA512(arrayPlaintext[0], hashSecretKey)))
        //    {
        //        plaintext = "";
        //        throw new Exception("AESDecrypt - Hash value is not match");
        //    }
        //    else
        //    {
        //        plaintext = arrayPlaintext[0];
        //    }

        //    return plaintext;

        //}
    }
}
