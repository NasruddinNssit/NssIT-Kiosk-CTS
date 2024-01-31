using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.PaymentGatewaySchedule.SecurityProperty
{
    public class AuthenticationSet
    {
        private string _webApiUrl = "";
        private bool _isLiveAccess = false;

        public AuthenticationSet(string webApiUrl)
        {
            _webApiUrl = string.IsNullOrWhiteSpace(webApiUrl?.Trim()) ? "" : webApiUrl.Trim();

            if (_webApiUrl.Contains(SecureCode.LIVE_DomainUrl.Trim()))
            {
                _isLiveAccess = true;
            }
        }

        string _tvmKey = null;
        public string TVMKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_tvmKey) == false)
                    return _tvmKey;

                if (_isLiveAccess == true)
                {
                    _tvmKey = SecureCode.TVMKeyLIVE.Trim();
                }
                else
                {
                    _tvmKey = SecureCode.TVMKeyDEV.Trim();
                }
                return _tvmKey;
            }
        }

        private string _hashSecretKey = null;
        public string HashSecretKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_hashSecretKey) == false)
                    return _hashSecretKey;

                if (_isLiveAccess == true)
                {
                    _hashSecretKey = SecureCode.HashSecretKeyLIVE.Trim();
                }
                else
                {
                    _hashSecretKey = SecureCode.HashSecretKeyDEV.Trim();
                }
                return _hashSecretKey;
            }
        }
    }
}
