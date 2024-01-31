using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NssIT.Kiosk.AppDecorator.Config;

namespace NssIT.Kiosk.Network.SignalRClient.API.Base
{
    public class PaymentGuard
    {
        private static SemaphoreSlim _lock = new SemaphoreSlim(1);

        /// <summary>
        /// To encrypt for (/api/Sales/create_sale).signature
        /// </summary>
        /// <param name="hashText">Refer to specifix data string string format; Check design document</param>
        /// <returns>Return an encrypted string</returns>
        public static string HashRSAKey(string hashText)
        {
            string privateKey = PrivateKey;
            // Create a new instance of RSACryptoServiceProvider using the
            // key from RSAParameters.
            RSACryptoServiceProvider RSAalg = PemKeyUtils.GetRSAProviderFromText(privateKey);

            var hashByte = Encoding.UTF8.GetBytes(hashText);
            // Hash and sign the data. Pass a new instance of SHA256
            // to specify the hashing algorithm.
            byte[] signedByteArray = RSAalg.SignData(hashByte, SHA256.Create());

            return Convert.ToBase64String(signedByteArray);
        }


        /// <summary>
        /// Decrypt and check refer to response of (/api/Sales/create_sale)-> (Response).signature
        /// </summary>
        /// <param name="hashText">Refer to specifix data string string format; Check design document</param>
        /// <param name="signatureText">An encrypted string refer to hashText parameteter</param>
        /// <returns></returns>
        public static bool VerifyRSASignedHash(string hashText, string signatureText)
        {
            string publicKey = PublicKey;
            // Create a new instance of RSACryptoServiceProvider using the
            // key from RSAParameters.
            RSACryptoServiceProvider RSAalg = PemKeyUtils.GetRSAProviderFromText(publicKey);

            var hashByte = Encoding.UTF8.GetBytes(hashText);
            var signatureByte = Convert.FromBase64String(signatureText);

            return RSAalg.VerifyData(hashByte, SHA256.Create(), signatureByte);
        }

        private static string _privateKey = null;

        private static string PrivateKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_privateKey))
                {
                    ReadRegistry();
                }

                return _privateKey;
            }
        }

        private static string _publicKey = null;
        private static string PublicKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_publicKey))
                {
                    ReadRegistry();
                }

                return _publicKey;
            }
        }

        private static string _sectionVersion = null;
        /// <summary>
        /// Return Live, Development, Staging, or Training.
        /// Value refer to NssIT.Kiosk.AppDecorator.Config.ConfigConstant.WebAPISiteCode
        /// </summary>
        public static string SectionVersion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_sectionVersion))
                {
                    ReadRegistry();
                }

                return _sectionVersion;
            }
        }

        private static void ReadRegistry()
        {
            try
            {
                _lock.WaitAsync().Wait();

                if (string.IsNullOrWhiteSpace(_sectionVersion))
                {
                    RegistrySetup regEdit = new RegistrySetup();
                    _sectionVersion = regEdit.ReadValue(RegistrySetup.BTnGSectionTag);
                    _publicKey = regEdit.ReadValue(RegistrySetup.BTnGCommonCodeTag);
                    _privateKey = regEdit.ReadValue(RegistrySetup.BTnGSpecialCodeTag);
                }
            }
            catch { }
            finally
            {
                if (_lock.CurrentCount == 0)
                    _lock.Release();
            }
        }
    }
}
