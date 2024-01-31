using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.PassengerInfo
{
    public class PassengerInfoEntryValidator
    {
        private TextBox _passgNameTxt = null; 
        private TextBox _icTxt = null; 
        private TextBox _contactTxt = null;
        private Dispatcher _pageDispatcher = null;
        private ResourceDictionary _currentRosLang = null;
        private ResourceDictionary _defaulttRosLang = null;

        public PassengerInfoEntryValidator(Dispatcher pageDispatcher, TextBox passgNameTxt, TextBox icTxt, TextBox contactTxt,
            ResourceDictionary defaultRosLang)
        {
            _passgNameTxt = passgNameTxt;
            _pageDispatcher = pageDispatcher; 
            _icTxt = icTxt;
            _contactTxt = contactTxt;
            _defaulttRosLang = defaultRosLang;
        }

        public void PageLoad(ResourceDictionary currentRosLang)
        {
            _currentRosLang = currentRosLang ?? _defaulttRosLang;
        }

        public bool ValidateAllEntry(out TextBox focusRequestTextBox, out string errorMsg)
        {
            focusRequestTextBox = null;
            errorMsg = null;

            // Validate Passenger's Name
            if (string.IsNullOrWhiteSpace(_passgNameTxt.Text.Trim()))
            {
                focusRequestTextBox = _passgNameTxt;

                string advStr = _currentRosLang["ADVICE_PASSENGER_NAME_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(advStr))
                    advStr = $@"Please enter name of Passenger";

                //SetErrorMsg(passengerErrMsg: advStr);
                errorMsg = advStr;
            }
            //-------------------------------------------------------------------------------------------
            // Validate IC
            else if (string.IsNullOrWhiteSpace(_icTxt.Text.Trim()))
            {
                if (focusRequestTextBox == null)
                    focusRequestTextBox = _icTxt;

                string advStr = _currentRosLang["ADVICE_IC_NO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(advStr))
                    advStr = $@"Please enter IC(MyKad)/Passport";

                // SetErrorMsg(icPassErrMsg: advStr);
                errorMsg = advStr;
            }
            else if (_icTxt.Text.Trim().Length < 8)
            {
                if (focusRequestTextBox == null)
                    focusRequestTextBox = _icTxt;

                string errMsg = _currentRosLang["CUST_ERROR_IC_NO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(errMsg))
                    errMsg = $@"Please enter a minimum of 8 alphanumeric for IC(MyKad)/Passport";

                // SetErrorMsg(icPassErrMsg: errMsg);
                errorMsg = errMsg;
            }
            //-------------------------------------------------------------------------------------------
            // Validate Contact Number
            else if (string.IsNullOrWhiteSpace(_contactTxt.Text.Trim()))
            {
                if (focusRequestTextBox == null)
                    focusRequestTextBox = _contactTxt;

                string advStr = _currentRosLang["ADVICE_MOBILE_NO_Label"]?.ToString();

                if (string.IsNullOrWhiteSpace(advStr))
                    advStr = $@"Please enter a minimum of 10 numeric characters without space for Mobile No.";

                //SetErrorMsg(contactErrMsg: advStr);
                errorMsg = advStr;
            }
            else
            {
                _contactTxt.Text = _contactTxt.Text.Trim();
                System.Windows.Forms.Application.DoEvents();

                int intRes = 0;
                string chr = "";
                string contactNo = "";

                for (int chrInx = 0; chrInx < _contactTxt.Text.Length; chrInx++)
                {
                    chr = _contactTxt.Text.Substring(chrInx, 1);
                    if (int.TryParse(chr, out intRes) == true)
                        contactNo += chr;
                }

                if (contactNo.Trim().Length < 10)
                {
                    if (focusRequestTextBox == null)
                        focusRequestTextBox = _contactTxt;

                    string errMsg = _currentRosLang["CUST_ERROR_MOBILE_NO_Label"]?.ToString();
                        
                    if (string.IsNullOrWhiteSpace(errMsg))
                        errMsg = $@"Mobile No. must be 10 numeric characters and without space";

                    //SetErrorMsg(contactErrMsg: errMsg);
                    errorMsg = errMsg;
                }
            }

            if (focusRequestTextBox != null)
            {
                if (string.IsNullOrWhiteSpace(errorMsg))
                    errorMsg = "Please check the passenger info again";

                return false;
            }
            return true;
        }
    }
}
