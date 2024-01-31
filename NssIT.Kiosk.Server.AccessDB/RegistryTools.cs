using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB
{
    public class RegistryTools
    {
        /// <summary>
        /// Return true when value is found. Else return false.
        /// </summary>
        /// <param name="rgKey">Like Registry.CurrentUser or Registry.LocalMachine</param>
        /// <param name="subKey">Like "SOFTWARE\eTicketing"</param>
        /// <param name="valueName">Like "UserID" or "UserPass"</param>
        /// <param name="isSubKeyFound">Return true if subKey is found. Else return false</param>
        /// <returns></returns>
        public static bool GetValue(RegistryKey rgKey, string subKey, string valueName, out bool isSubKeyFound, out string resultValue)
        {
            bool valueFound = false;

            isSubKeyFound = false;
            resultValue = null;

            if (rgKey is null)
                throw new Exception("Registry key cannot be null");

            if (string.IsNullOrWhiteSpace(valueName))
                throw new Exception("Invalid Registry's value name");

            //Validate the subKey
            isSubKeyFound = ValidateSubKey(rgKey, subKey);

            //Read Value
            if (isSubKeyFound)
            {
                string fullKeyName = $@"{rgKey.Name}\{subKey}";
                resultValue = (string)Registry.GetValue($@"{fullKeyName}", valueName, null);
                valueFound = (resultValue is null) ? false : true;
            }

            return valueFound;

            bool ValidateSubKey(RegistryKey rkey, string subKeyX)
            {
                RegistryKey chkKey = rkey.OpenSubKey(subKeyX);
                return (chkKey is null) ? false : true;
            }
        }
    }
}
