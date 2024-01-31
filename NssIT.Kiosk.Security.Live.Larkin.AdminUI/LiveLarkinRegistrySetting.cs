using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Security.Live.Larkin.AdminUI
{
    public class LiveLarkinRegistrySetting
    {
        private string CustomMsgTag = "##CUSTOM_MSG##";
        public WebAPISiteCode WebAPICode => WebAPISiteCode.Live;

        public bool VerifyRegistry(out string resultMessage)
        {
            bool retVal = true;
            resultMessage = "Verify Registry : Success";

            try
            {
                VerifyBTnGRegistry();
            }
            catch (Exception ex)
            {
                retVal = false;

                if (ex.Message?.Contains(CustomMsgTag) == true)
                {
                    resultMessage = ex.Message.Replace(CustomMsgTag, "");
                }
                else
                    resultMessage = ex.ToString();
            }
            return retVal;
        }

        public bool WriteRegistry(out string resultMessage)
        {
            bool retVal = true;
            resultMessage = "Write Registry : DONE";

            try
            {
                WriteBTnGRegistry();
            }
            catch (Exception ex)
            {
                retVal = false;
                resultMessage = ex.ToString();
            }

            return retVal;
        }

        public void ExportToTextFile()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void VerifyBTnGRegistry()
        {
            RegistrySetup regEdit = new RegistrySetup();

            string bTnGMerchantIdValue = regEdit.ReadValue(RegistrySetup.BTnGMerchantIdTag);
            string bTnGSectionValue = regEdit.ReadValue(RegistrySetup.BTnGSectionTag);
            string bTnGCommonCodeValue = regEdit.ReadValue(RegistrySetup.BTnGCommonCodeTag);
            string bTnGSpecialCodeValue = regEdit.ReadValue(RegistrySetup.BTnGSpecialCodeTag);

            string webApiUrl = regEdit.ReadValue(RegistrySetup.BTnGWebApiUrlTag);
            string hashSecretKey = regEdit.ReadValue(RegistrySetup.HashSecretKeyTag);
            string tvmKey = regEdit.ReadValue(RegistrySetup.TVMKeyTag);

            if (bTnGSectionValue is null)
            {
                throw new Exception($@"{CustomMsgTag}Value not found for Payment Gateway Section");
            }
            else if (bTnGMerchantIdValue is null)
            {
                throw new Exception($@"{CustomMsgTag}Value not found for Merchant Id");
            }
            else if (bTnGCommonCodeValue is null)
            {
                throw new Exception($@"{CustomMsgTag}Value not found for Payment Gateway Common Code");
            }
            else if (bTnGSpecialCodeValue is null)
            {
                throw new Exception($@"{CustomMsgTag}Value not found for Payment Gateway Special Code");
            }
            else if (webApiUrl is null)
            {
                throw new Exception($@"{CustomMsgTag}Value not found for WebApiUrl");
            }
            else if (hashSecretKey is null)
            {
                throw new Exception($@"{CustomMsgTag}Value not found for HashSecretKey");
            }
            else if (tvmKey is null)
            {
                throw new Exception($@"{CustomMsgTag}Value not found for TVMKey");
            }

            IBTnGGuardInfo guard = BTnGLiveLarkinGuard.GuardInfo;

            if (bTnGSectionValue.Equals(WebAPICode.ToString(), StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception($@"{CustomMsgTag}Value not matched for Payment Gateway Section; Current Section : {bTnGSectionValue}");
            }

            else if (bTnGMerchantIdValue.Equals(guard.MerchantId, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception($@"{CustomMsgTag}Value not matched for Merchant Id; Current Merchant Id : {bTnGMerchantIdValue}");
            }

            else if (bTnGCommonCodeValue.Equals(guard.PublicKey, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception($@"{CustomMsgTag}Value not matched for Payment Gateway Common Code");
            }

            else if (bTnGSpecialCodeValue.Equals(guard.PrivateKey, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception($@"{CustomMsgTag}Value not matched for Payment Gateway Special Code");
            }

            else if (webApiUrl.Equals(guard.WebApiUrl, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception($@"{CustomMsgTag}Value not matched for WebApiUrl; WebApiUrl : {webApiUrl}");
            }

            else if (hashSecretKey.Equals(guard.HashSecretKey, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception($@"{CustomMsgTag}Value not matched for HashSecretKey");
            }

            else if (tvmKey.Equals(guard.TVMKey, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception($@"{CustomMsgTag}Value not matched for TVMKey");
            }
        }

        private void WriteBTnGRegistry()
        {
            RegistrySetup regEdit = new RegistrySetup();
            IBTnGGuardInfo guard = BTnGLiveLarkinGuard.GuardInfo;

            regEdit.WriteValue(RegistrySetup.BTnGMerchantIdTag, guard.MerchantId);
            regEdit.WriteValue(RegistrySetup.BTnGSectionTag, WebAPICode.ToString());
            regEdit.WriteValue(RegistrySetup.BTnGCommonCodeTag, guard.PublicKey);
            regEdit.WriteValue(RegistrySetup.BTnGSpecialCodeTag, guard.PrivateKey);
            regEdit.WriteValue(RegistrySetup.BTnGWebApiUrlTag, guard.WebApiUrl);
            regEdit.WriteValue(RegistrySetup.HashSecretKeyTag, guard.HashSecretKey);
            regEdit.WriteValue(RegistrySetup.TVMKeyTag, guard.TVMKey);
        }
    }
}