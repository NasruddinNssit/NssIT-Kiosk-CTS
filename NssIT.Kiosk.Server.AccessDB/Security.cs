using Microsoft.Win32;
using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB
{
    public class Security
    {
        private const string LogChannel = "Security";

        private static bool _accessRight = false;

        private static string _accessToken = null;
        public static string AccessToken
        {
            get
            {
                if ((_accessToken is null) || (_accessRight == false))
                {
                    bool x = ReDoLogin(out bool networkTimeout, out bool isValidAuthentication);
                }
                return _accessToken;
            }
        }

        private static DbLog _log = null;
        private static DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private static ServiceSoapClient _soap = null;
        private static ServiceSoapClient Soap
        {
            get
            {
                if (_soap == null)
                {
                    BasicHttpBinding binding = new BasicHttpBinding();
                    EndpointAddress address = new EndpointAddress(AppDecorator.Config.Setting.GetSetting().WebServiceURL);
                    _soap = new ServiceSoapClient(binding, address);
                }

                return _soap;
            }
        }

        public static bool ReDoLogin(out bool networkTimeout, out bool isValidAuthentication)
        {
            networkTimeout = false;
            isValidAuthentication = false;
            string accessToken = null;

            _accessRight = LogonWebService(out accessToken, out bool networkTimeoutX, out bool isValidAuthenticationX);

            if (_accessRight == true)
            {
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    _accessRight = false;
                    _accessToken = null;
                }
                else
                    _accessToken = accessToken;
            }
            else
            {
                _accessToken = null;
            }

            networkTimeout = networkTimeoutX;
            isValidAuthentication = isValidAuthenticationX;

            return _accessRight;
        }

        public static bool GetTimeStamp(out string timeStampStr)
        {
            timeStampStr = null;

            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    timestamp_status timSt = Soap.Timestamp();
                    if (timSt.code == 0)
                    {
                        timeStampStr = timSt.expdatetime;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, classNMethodName: "Security.GetTimeStamp");
                }
            }

            return false;
        }

        /// <summary>
        /// Return true if logon success.
        /// </summary>
        /// <returns></returns>
        private static bool LogonWebService(out string accessToken, out bool networkTimeout, out bool isValidAuthentication)
        {
            accessToken = null;
            networkTimeout = false;
            isValidAuthentication = false;

            int maxRetryTimes = 3;
            int waitSec = 60;

            bool isValidAuthenticationX = false;
            bool networkTimeoutX = false;
            string accessTokenX = null;
            bool retResult = false;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                try
                {
                    isValidAuthenticationX = false;
                    networkTimeoutX = false;
                    accessTokenX = null;

                    Thread threadWorker = new Thread(new ThreadStart(LogonThreadWorking));
                    threadWorker.IsBackground = true;

                    DateTime timeOut = DateTime.Now.AddSeconds(waitSec);
                    threadWorker.Start();

                    while ((timeOut.Subtract(DateTime.Now).TotalMilliseconds > 0) && (accessTokenX is null) && (threadWorker.ThreadState.IsState(ThreadState.Stopped) == false))
                    {
                        Thread.Sleep(300);
                    }

                    if (string.IsNullOrWhiteSpace(accessTokenX))
                    {
                        try
                        {
                            threadWorker.Abort();
                            Thread.Sleep(500);
                        }
                        catch { }

                        accessTokenX = null;
                        retResult = false;
                    }
                    else
                    {
                        retResult = true;
                        break;
                    }

                    Thread.Sleep(ServerAccess.RetryIntervalSec * 1000);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, classNMethodName: "Security.LogonWebService");
                }
            }

            networkTimeout = networkTimeoutX;
            accessToken = accessTokenX;
            isValidAuthentication = isValidAuthenticationX;

            return retResult;

            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void LogonThreadWorking()
            {
                //For Data Execution Code => 20: Invalid Token; 21: Token Expired

                try
                {
                    string usrId = KioskID;
                    string passW = Password;

                    if (usrId is null)
                        throw new Exception("Unable to read logon kiosk/user id & password from local PC");

                    //GetUserIdentity(out string usrId, out string passW);

                    if (GetTimeStamp(out string timeStampStr) == false)
                    {
                        networkTimeoutX = true;
                        return;
                    }

                    string passWordHashed = SecurityCommon.PassEncrypt(passW);
                    string strToBeEncr = $@"{usrId},{timeStampStr}";

                    string encStr = SecurityCommon.Encrypt(strToBeEncr, passWordHashed);

                    login_status lgn = _soap.Login(usrId, encStr, AppDecorator.Config.Setting.GetSetting().IPAddress);

                    // lgn.Code => 0: success, 1: Blank Parameter found; 2: No such User Id; 3: expired, 4: Inactive User Id; 5: Invalid IP; 6: User Id not match with encrypted data; 99 is other error

                    if (lgn is null)
                    {
                        networkTimeoutX = true;
                        isValidAuthenticationX = false;
                    }
                    else if (lgn.code != 0)
                    {
                        string errMsg = null;

                        switch (lgn.code)
                        {
                            case 1:
                                networkTimeoutX = false;
                                isValidAuthenticationX = false;
                                errMsg = "Error : Blank parameter found";
                                break;

                            case 2:
                                networkTimeoutX = false;
                                isValidAuthenticationX = false;
                                errMsg = "Error : No such User Id";
                                break;

                            case 3:
                                networkTimeoutX = true;
                                isValidAuthenticationX = true;
                                errMsg = "Error : Time expired";
                                break;

                            case 4:
                                networkTimeoutX = false;
                                isValidAuthenticationX = false;
                                errMsg = "Error : Inactive User Id";
                                break;

                            case 5:
                                networkTimeoutX = false;
                                isValidAuthenticationX = false;
                                errMsg = "Error : Invalid IP";
                                break;

                            case 6:
                                networkTimeoutX = false;
                                isValidAuthenticationX = false;
                                errMsg = "Error : User Id not match with encrypted data";
                                break;

                            default:
                                networkTimeoutX = false;
                                isValidAuthenticationX = false;
                                errMsg = $@"Error : {lgn.msg ?? "Fail login web service"}; Code : {lgn.code}";
                                break;
                        }


                        Log.LogText(LogChannel, "-", errMsg, "A10", "Security:LogonWebService", AppDecorator.Log.MessageType.Error);
                    }
                    else
                    {
                        accessTokenX = lgn.kiosktoken;
                        networkTimeoutX = false;
                        isValidAuthenticationX = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, "EX01", "Security:LogonWebService");
                }
            }

            //void GetUserIdentity(out string userId, out string password)
            //{
            //    userId = null;
            //    password = null;

            //    userId = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\eTicketing", "UserID", null);
            //    password = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\eTicketing", "UserPass", null);

            //    //Note : 64 bits Windows will be re-routed to HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\eTicketing

            //    if (userId == null)
            //    {
            //        userId = (string)Registry.GetValue(@"HKCU\SOFTWARE\eTicketing", "UserID", null);
            //        password = (string)Registry.GetValue(@"HKCU\SOFTWARE\eTicketing", "UserPass", null);
            //    }

            //    if (userId is null)
            //    {
            //        isValidAuthenticationX = false;
            //        Log.LogText(LogChannel, "-", "Invalid User Id", "E01", "Security.GetUserIdentity", AppDecorator.Log.MessageType.Error);
            //        throw new Exception("Unable to read logon user id from local PC");
            //    }

            //    if (password is null)
            //    {
            //        isValidAuthenticationX = false;
            //        Log.LogText(LogChannel, "-", "Invalid password Id", "E02", "Security.GetUserIdentity", AppDecorator.Log.MessageType.Error);
            //        throw new Exception("Unable to read logon password from local PC");
            //    }
            //}
        }

        private static string _kioskId = null;
        public static string KioskID
        {
            get
            {
                if (_kioskId is null)
                {
                    bool isValidId = GetKioskIdentity(out string userId, out string password);

                    if (isValidId)
                    {
                        _kioskId = userId;
                        _password = password;
                    }
                }
                return _kioskId;
            }
        }

        private static string _password = null;
        public static string Password
        {
            get
            {
                if (_kioskId is null)
                {
                    bool isValidId = GetKioskIdentity(out string userId, out string password);
                    if (isValidId)
                    {
                        _kioskId = userId;
                        _password = password;
                    }
                }
                return _password;
            }
        }

        private static bool GetKioskIdentity(out string userId, out string password)
        {
            userId = null; 
            password = null;
            bool retVal = false;

            try
            {
                //userId = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\eTicketing", "UserID", null);
                //password = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\eTicketing", "UserPass", null);

                Log.LogText(LogChannel, "-", $@"Start - GetKioskIdentity", "A01", "Security.GetKioskIdentity");

                if (RegistryTools.GetValue(Registry.LocalMachine, $@"SOFTWARE\eTicketing", "UserID", out bool isSubKeyFound1, out string resultValue1))
                {
                    Log.LogText(LogChannel, "-", $@"Key found at Registry.LocalMachine", "A05", "Security.GetKioskIdentity");

                    if (string.IsNullOrWhiteSpace(resultValue1))
                        throw new Exception("Invalid logon user id from local PC (A)");

                    userId = resultValue1.Trim();

                    if (RegistryTools.GetValue(Registry.LocalMachine, $@"SOFTWARE\eTicketing", "UserPass", out bool isSubKeyFound2, out string resultValue2))
                    {
                        password = resultValue2.Trim();
                    }
                    else
                    {
                        throw new Exception("Unable to read logon password from local PC (A)");
                    }
                }
                else
                {
                    Log.LogText(LogChannel, "-", $@"Key Not found at Registry.LocalMachine", "A10", "Security.GetKioskIdentity");

                    if (isSubKeyFound1 == true)
                    {
                        throw new Exception("Unable to read logon user id from local PC (A1)");
                    }
                }

                if (RegistryTools.GetValue(Registry.CurrentUser, $@"SOFTWARE\eTicketing", "UserID", out bool isSubKeyFound3, out string resultValue3))
                {
                    Log.LogText(LogChannel, "-", $@"Key found at Registry.CurrentUser", "A15", "Security.GetKioskIdentity");

                    if (string.IsNullOrWhiteSpace(resultValue3))
                        throw new Exception("Invalid logon user id from local PC (B)");

                    userId = resultValue3.Trim();

                    if (RegistryTools.GetValue(Registry.CurrentUser, $@"SOFTWARE\eTicketing", "UserPass", out bool isSubKeyFound4, out string resultValue4))
                    {
                        password = resultValue4.Trim();
                    }
                    else
                    {
                        throw new Exception("Unable to read logon password from local PC (B)");
                    }
                }
                else
                {
                    Log.LogText(LogChannel, "-", $@"Key NOT found at Registry.CurrentUser", "A20", "Security.GetKioskIdentity");

                    if (isSubKeyFound3 == true)
                    {
                        throw new Exception("Unable to read logon user id from local PC (B1)");
                    }
                }


                //Note : 64 bits Windows will be re-routed to HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\eTicketing

                if (userId == null)
                {
                    Log.LogText(LogChannel, "-", $@"Key NOT found at Registry.CurrentUser & Registry.LocalMachine", "A25", "Security.GetKioskIdentity");

                    userId = (string)Registry.GetValue(@"HKCU\SOFTWARE\eTicketing", "UserID", null);
                    password = (string)Registry.GetValue(@"HKCU\SOFTWARE\eTicketing", "UserPass", null);
                }

                if (userId is null)
                {
                    retVal = false;
                    Log.LogText(LogChannel, "-", "Invalid User Id", "EX01", "Security.GetKioskIdentity", AppDecorator.Log.MessageType.Error);
                    throw new Exception("Unable to read logon user id from local PC");
                }

                if (password is null)
                {
                    retVal = false;
                    Log.LogText(LogChannel, "-", "Invalid password Id", "EX02", "Security.GetKioskIdentity", AppDecorator.Log.MessageType.Error);
                    throw new Exception("Unable to read logon password from local PC");
                }

                Log.LogText(LogChannel, "-", $@"UID :{userId}; PS :{password}", "A50", "Security.GetKioskIdentity");

                retVal = true;
            }
            catch (Exception ex)
            {
                Log.LogError(LogChannel, "-", ex, "EX10", "Security.GetKioskIdentity");
            }

            return retVal;
        }

    }
}
