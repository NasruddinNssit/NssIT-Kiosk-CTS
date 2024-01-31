using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Config
{
    /// <summary>
    /// ClassCode:EXIT01.02
    /// </summary>
    public class RegistrySetup
    {
        private string _subKeyName = "CTS_Kiosk";

        private string _deviceIdTag = "DeviceId";

        // Registry Name ----------------------------------------------
        // -- GoPayment Website setting --
        public static string BTnGMerchantIdTag => "BTnGMerchantId";
        public static string BTnGSectionTag => "BTnGSection";
        public static string BTnGCommonCodeTag => "BTnGCommonCode";
        public static string BTnGSpecialCodeTag => "BTnGSpecialCode";
        //-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
        // -- Payment Gateway Website --
        public static string BTnGWebApiUrlTag => "BTnGWebApiUrl";
        public static string HashSecretKeyTag => "HashSecretKey";
        public static string TVMKeyTag => "TVMKey";
        //-------------------------------------------------------------

        // Registry Values
        private static string _deviceId = null;

        public string DeviceId { get; private set; } = null;
        public string BTnGMerchantId { get; private set; } = null;
        public string BTnGSection { get; private set; } = null;
        public string BTnGCommonCode { get; private set; } = null;
        public string BTnGSpecialCode { get; private set; } = null;
        public string WebApiUrl { get; private set; } = null;
        public string HashSecretKey { get; private set; } = null;
        public string TVMKey { get; private set; } = null;
        //-------------------------------------------------------------

        private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
        private static RegistrySetup _setting = null;

        public RegistrySetup()
        { }

        public static RegistrySetup GetRegistrySetting()
        {
            if (_setting == null)
            {
                try
                {
                    _manLock.WaitAsync().Wait();
                    if (_setting == null)
                    {
                        _setting = new RegistrySetup();
                    }
                    return _setting;
                }
                finally
                {
                    if (_manLock.CurrentCount == 0)
                        _manLock.Release();
                }
            }
            else
                return _setting;
        }

        private bool _valuesHaveBeenRead = false;
        public void ReadAllRegistryValues(out string errorMessage)
        {
            errorMessage = null;

            if (_valuesHaveBeenRead)
                return;

            try
            {
                DeviceId = GetDeviceId();
            }
            catch (Exception ex)
            {
                errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? ex.Message : errorMessage;
            }

            try
            {
                BTnGMerchantId = ReadValue(BTnGMerchantIdTag);
            }
            catch (Exception ex)
            {
                errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? ex.Message : errorMessage;
            }

            try
            {
                WebApiUrl = ReadValue(BTnGWebApiUrlTag);
            }
            catch (Exception ex)
            {
                errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? ex.Message : errorMessage;
            }

            try
            {
                HashSecretKey = ReadValue(HashSecretKeyTag);
            }
            catch (Exception ex)
            {
                errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? ex.Message : errorMessage;
            }

            try
            {
                TVMKey = ReadValue(TVMKeyTag);
            }
            catch (Exception ex)
            {
                errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? ex.Message : errorMessage;
            }

            try
            {
                BTnGSection = ReadValue(BTnGSectionTag);
            }
            catch (Exception ex)
            {
                errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? ex.Message : errorMessage;
            }

            try
            {
                BTnGCommonCode = ReadValue(BTnGCommonCodeTag);
            }
            catch (Exception ex)
            {
                errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? ex.Message : errorMessage;
            }

            try
            {
                BTnGSpecialCode = ReadValue(BTnGSpecialCodeTag);
            }
            catch (Exception ex)
            {
                errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? ex.Message : errorMessage;
            }

            _valuesHaveBeenRead = true;
        }

        private Microsoft.Win32.RegistryKey GetKioskRegisterKey()
        {
            Microsoft.Win32.RegistryKey key;

            //Note : 64 bits Windows will be re-routed to HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\<_subKeyName>

            string keyN = $@"SOFTWARE\{_subKeyName}";
            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyN, true);

            if (key is null)
            {
                key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyN);
            }

            return key;
        }

        /// <summary>
        /// FuncCode:EXIT01.0202
        /// </summary>
        /// <returns></returns>
        private string GetDeviceId()
        {
            Microsoft.Win32.RegistryKey key = null;
            string retValue = null;

            if (string.IsNullOrWhiteSpace(_deviceId) == false)
                return _deviceId;

            try
            {
                key = GetKioskRegisterKey();

                if (key is null)
                {
                    throw new Exception($@"Unable to read Kiosk System Value. The key is missing; (EXIT01.0202.X01)");
                }
                else
                {
                    retValue = key.GetValue(_deviceIdTag)?.ToString();

                    if (string.IsNullOrWhiteSpace(retValue))
                    {
                        retValue = Guid.NewGuid().ToString();
                        key.SetValue(_deviceIdTag, retValue);
                    }

                    _deviceId = retValue;
                }
            }
            finally
            {
                key?.Close();
                key?.Dispose();
            }

            return retValue;
        }

        /// <summary>
        /// FuncCode:EXIT01.0203
        /// </summary>
        /// <param name="valueTag"></param>
        /// <returns></returns>
        public string ReadValue(string valueTag)
        {
            Microsoft.Win32.RegistryKey key = null;
            string retValue = null;

            key = GetKioskRegisterKey();

            if (key is null)
            {
                throw new Exception($@"Unable to read Internal Kiosk Value. The key is missing; (EXIT01.0203.X01)");
            }
            else
            {
                try
                {
                    retValue = key.GetValue(valueTag)?.ToString();

                    if (string.IsNullOrWhiteSpace(retValue))
                    {
                        retValue = null;
                    }
                }
                finally
                {
                    key?.Close();
                    key?.Dispose();
                }
            }
            return retValue;
        }

        /// <summary>
        /// FuncCode:EXIT01.0204
        /// </summary>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        public void WriteValue(string valueName, string value)
        {
            Microsoft.Win32.RegistryKey key = null;
            string retValue = null;

            key = GetKioskRegisterKey();

            if (key is null)
            {
                throw new Exception($@"Unable to write Internal Kiosk Value. The key is missing; (EXIT01.0204.X01)");
            }
            else
            {
                bool isProceed = true;

                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        key.SetValue(valueName, "");
                    }
                    else
                    {
                        key.SetValue(valueName, value);
                    }
                }
                catch (Exception ex)
                {
                    isProceed = false;
                    throw new Exception(ex.Message, ex);
                }
                finally
                {
                    key?.Close();
                    key?.Dispose();
                }

                if (isProceed)
                {
                    if (VerifyWriting(valueName, value) == false)
                    {
                        throw new Exception($@"Fail to write Internal Kiosk Value; (EXIT01.0204.X02)");
                    }
                }
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            /// <summary>
            /// FuncCode:EXIT01.021A
            /// </summary>
            bool VerifyWriting(string valueNameX, string refValueX)
            {
                string regVal = ReadValue(valueNameX);

                if (string.IsNullOrWhiteSpace(regVal) && string.IsNullOrWhiteSpace(refValueX))
                {
                    return true;
                }
                else if (string.IsNullOrWhiteSpace(regVal) || string.IsNullOrWhiteSpace(refValueX))
                {
                    return false;
                }
                else
                {
                    return regVal.Equals(refValueX);
                }
            }
        }
    }
}