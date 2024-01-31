using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class PassengerInfoManager
    {
        private const int _maxPassengerCount = 20;
        private Dispatcher _pageDispatcher = null;

        private System.Windows.Media.Brush _focusBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
        private System.Windows.Media.Brush _normalBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xE4, 0xE4, 0xE4));

        //_focusBorderEffectColor  _normalBorderEffectColor

        private string _passengerContainerPrefix = "BdPass";
        private string _namePrefix = "TxtPassName";
        private string _idPrefix = "TxtPassId";
        private string _contactPrefix = "TxtContact";
        private string _seatPrefix = "TxtSeat";
        private string _seatIdPrefix = "TxtSeatId";
        private string _seatTypePrefix = "TxtSeatType";
        private string _myKadPrefix = "BtnMyKad";

        private TextBlock _txtError = null;
        private TextBlock _txtConfirmWait = null;

        private bool _hasConfirm = false;
        private int _passengerCount = 1;

        private Grid _grdPassengerList = null;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public PassengerInfoManager(Page page, TextBlock txtError, TextBlock txtConfirmWait, Grid grdPassengerList)
        {
            _langMal = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoEnglish.xaml");

            _pageDispatcher = page.Dispatcher;
            _grdPassengerList = grdPassengerList;

            _txtError = txtError;
            _txtConfirmWait = txtConfirmWait;

            string controlName = "";

            object obj = null;

            //Reat all passenger border (container) into list
            for (int inx = 0; inx < _maxPassengerCount; inx++)
            {
                controlName = $@"{_passengerContainerPrefix}{inx.ToString().Trim()}";

                obj = _grdPassengerList.FindName(controlName);

                if (obj is Border container)
                {
                    //---------------------------------------------------------------------------------------------------------------------
                    //Check Control ; Read name textbox into list
                    {
                        controlName = $@"{_namePrefix}{inx.ToString().Trim()}";
                        obj = container.FindName(controlName);
                        if (obj is TextBox nameBox) { }
                        else
                            throw new Exception($@"Unable to read name {controlName} Text Box; PassengerInfoChecker.constructor");
                    }
                    //---------------------------------------------------------------------------------------------------------------------
                    // Check Control; / Read id textbox into list
                    {
                        controlName = $@"{_idPrefix}{inx.ToString().Trim()}";
                        obj = container.FindName(controlName);
                        if (obj is TextBox idBox) { }
                        else
                            throw new Exception($@"Unable to read id {controlName} Text Box; PassengerInfoChecker.constructor");
                    }
                    //---------------------------------------------------------------------------------------------------------------------
                    // Check Control ; Read name textbox into list
                    {
                        controlName = $@"{_contactPrefix}{inx.ToString().Trim()}";
                        obj = container.FindName(controlName);
                        if (obj is TextBox contactBox) { }
                        else
                            throw new Exception($@"Unable to read contact {controlName} Text Box; PassengerInfoChecker.constructor");
                    }
                    //---------------------------------------------------------------------------------------------------------------------
                    // Check Control ; Read SeatNo Ducument.Run into list
                    {
                        controlName = $@"{_seatPrefix}{inx.ToString().Trim()}";
                        obj = container.FindName(controlName);
                        if (obj is Run runText) { }
                        else
                            throw new Exception($@"Unable to read seat {controlName} Run Text; PassengerInfoChecker.constructor");
                    }
                    //---------------------------------------------------------------------------------------------------------------------
                    // Check Control ; Read Seat Id textbox into list
                    {
                        controlName = $@"{_seatIdPrefix}{inx.ToString().Trim()}";
                        obj = container.FindName(controlName);
                        if (obj is TextBox contactBox) { }
                        else
                            throw new Exception($@"Unable to read seat id {controlName} Text Box; PassengerInfoChecker.constructor");
                    }
                    //---------------------------------------------------------------------------------------------------------------------
                    //Check Control ; Read Seat Type textbox into list
                    {
                        controlName = $@"{_seatTypePrefix}{inx.ToString().Trim()}";
                        obj = container.FindName(controlName);
                        if (obj is TextBox seatTypeBox) { }
                        else
                            throw new Exception($@"Unable to read seat type {controlName} Text Box; PassengerInfoChecker.constructor");
                    }
                    //---------------------------------------------------------------------------------------------------------------------
                    //Check Control ; Read MyKad Button into list
                    {
                        controlName = $@"{_myKadPrefix}{inx.ToString().Trim()}";
                        obj = container.FindName(controlName);
                        if (obj is Button myKadBtn) { }
                        else
                            throw new Exception($@"Unable to read MyKad {controlName} Button; PassengerInfoChecker.constructor");
                    }
                    //---------------------------------------------------------------------------------------------------------------------
                }
                else
                    throw new Exception($@"Unable to read passenger {controlName} Border (container); PassengerInfoChecker.constructor");
            }
        }

        public void InitPassengerContainer(PassengerSeatNo[] seatNoList, LanguageCode language)
        {
            _language = language;

            if (seatNoList.Length > _maxPassengerCount)
                _passengerCount = _maxPassengerCount;
            else if (seatNoList.Length <= 0)
                _passengerCount = 1;
            else
                _passengerCount = seatNoList.Length;

            _hasConfirm = false;
            _txtError.Text = "";
            _txtError.Visibility = System.Windows.Visibility.Collapsed;
            _txtConfirmWait.Visibility = System.Windows.Visibility.Collapsed;

            string controlName = "";
            object obj = null;

            bool showMyKadScanner = App.SysParam.PrmMyKadScanner;

            //Reat all passenger border (container) into list
            for (int inx = 0; inx < _maxPassengerCount; inx++)
            {
                controlName = $@"{_passengerContainerPrefix}{inx.ToString().ToString()}";
                obj = _grdPassengerList.FindName(controlName);

                if (obj is Border container)
                {
                    if (inx < _passengerCount)
                    {
                        container.Visibility = System.Windows.Visibility.Visible;

                        Run seatRunText = GetSeatRunText(inx);
                        seatRunText.Text = seatNoList[inx].SeatDesn;

                        TextBox seatIdBox = GetInputTextBox(inx, _seatIdPrefix);
                        seatIdBox.Text = seatNoList[inx].SeatId;

                        TextBox seatTypedBox = GetInputTextBox(inx, _seatTypePrefix);
                        seatTypedBox.Text = seatNoList[inx].SeatType;

                        Button myKadBtn = GetButton(inx, _myKadPrefix);
                        if (showMyKadScanner)
                            myKadBtn.Visibility = Visibility.Visible;
                        else
                            myKadBtn.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        container.Visibility = System.Windows.Visibility.Collapsed;

                        Run seatRunText = GetSeatRunText(inx);
                        seatRunText.Text = "";

                        TextBox seatIdBox = GetInputTextBox(inx, _seatIdPrefix);
                        seatIdBox.Text = "";

                        TextBox seatTypedBox = GetInputTextBox(inx, _seatTypePrefix);
                        seatTypedBox.Text = "";
                    }

                    TextBox nameBox = GetInputTextBox(inx, _namePrefix);
                    nameBox.Text = "";

                    TextBox idBox = GetInputTextBox(inx, _idPrefix);
                    idBox.Text = "";

                    TextBox contactBox = GetInputTextBox(inx, _contactPrefix);
                    contactBox.Text = "";
                }
                else
                    throw new Exception($@"Unable to read passenger {controlName} Border (container); PassengerInfoChecker.InitPassengerContainer");
            }
        }

        private TextBox GetInputTextBox(int borderContainerInx, string textBoxNamePrefix)
        {
            string controlName = $@"{_passengerContainerPrefix}{borderContainerInx.ToString().Trim()}";
            object obj = _grdPassengerList.FindName(controlName);

            if (obj is Border container)
            {
                //---------------------------------------------------------------------------------------------------------------------
                //Read textbox into list
                {
                    controlName = $@"{textBoxNamePrefix}{borderContainerInx.ToString().ToString()}";
                    obj = container.FindName(controlName);
                    if (obj is TextBox tBox)
                    {
                        return tBox;
                    }
                    else
                        throw new Exception($@"Unable get control {controlName} Text Box; Prefix: {textBoxNamePrefix}; PassengerInfoChecker.GetInputTextBox");
                }
                //---------------------------------------------------------------------------------------------------------------------
            }
            else
                throw new Exception($@"Unable to read {controlName} Border; Border Inx: {borderContainerInx}; PassengerInfoChecker.GetInputTextBox");
        }

        private Button GetButton(int borderContainerInx, string buttonPrefix)
        {
            string controlName = $@"{_passengerContainerPrefix}{borderContainerInx.ToString().Trim()}";
            object obj = _grdPassengerList.FindName(controlName);

            if (obj is Border container)
            {
                //---------------------------------------------------------------------------------------------------------------------
                //Read textbox into list
                {
                    controlName = $@"{_myKadPrefix}{borderContainerInx.ToString().Trim()}";
                    obj = container.FindName(controlName);
                    if (obj is Button tBtn)
                    {
                        return tBtn;
                    }
                    else
                        throw new Exception($@"Unable get control {controlName} Button; Prefix: {_myKadPrefix}; PassengerInfoChecker.GetButton");
                }
                //---------------------------------------------------------------------------------------------------------------------
            }
            else
                throw new Exception($@"Unable to read {controlName} Border; Border Inx: {borderContainerInx}; PassengerInfoChecker.GetButton");
        }

        private Run GetSeatRunText(int borderContainerInx)
        {
            string controlName = $@"{_passengerContainerPrefix}{borderContainerInx.ToString().ToString()}";
            object obj = _grdPassengerList.FindName(controlName);

            if (obj is Border container)
            {
                //---------------------------------------------------------------------------------------------------------------------
                //Read textbox into list
                {
                    controlName = $@"{_seatPrefix}{borderContainerInx.ToString().ToString()}";
                    obj = container.FindName(controlName);
                    if (obj is Run runText)
                    {
                        return runText;
                    }
                    else
                        throw new Exception($@"Unable get seat {controlName} Run Control; PassengerInfoChecker.GetSeatRunText");
                }
                //---------------------------------------------------------------------------------------------------------------------
            }
            else
                throw new Exception($@"Unable to read {controlName} Border; Border Inx: {borderContainerInx}; PassengerInfoChecker.GetSeatRunText");
        }

        /// <summary>
        /// Return true when all passenger info are valid
        /// </summary>
        /// <param name="passengerInfoList">Only return PassengerInfo array length more than 0 when this function is return true; Else null is return</param>
        /// <returns></returns>
        private bool CheckNReadAllPassengerInfo(out PassengerInfo[] passengerInfoList, out TextBox focusRequestTextBox)
        {
            focusRequestTextBox = null;
            passengerInfoList = null;
            List<PassengerInfo> passgList = new List<PassengerInfo>();

            for (int inx = 0; inx < _passengerCount; inx++)
            {

                try
                {
                    TextBox nameBox = GetInputTextBox(inx, _namePrefix);
                    if (string.IsNullOrWhiteSpace(nameBox.Text.Trim()))
                    {
                        focusRequestTextBox = nameBox;
                        string advStr = "";

                        if (_language == LanguageCode.Malay)
                            advStr = string.Format(_langMal["ADVICE_PASSENGER_NAME_Label"]?.ToString(), (inx + 1).ToString());
                        else
                            advStr = string.Format(_langEng["ADVICE_PASSENGER_NAME_Label"]?.ToString(), (inx + 1).ToString());

                        if (string.IsNullOrWhiteSpace(advStr))
                            advStr = $@"Please enter name of Passenger {inx + 1}";

                        throw new Exception(advStr);
                    }

                    TextBox idBox = GetInputTextBox(inx, _idPrefix);
                    if (string.IsNullOrWhiteSpace(idBox.Text.Trim()))
                    {
                        focusRequestTextBox = idBox;
                        string advStr = "";

                        if (_language == LanguageCode.Malay)
                            advStr = string.Format(_langMal["ADVICE_IC_NO_Label"]?.ToString(), (inx + 1).ToString());
                        else
                            advStr = string.Format(_langEng["ADVICE_IC_NO_Label"]?.ToString(), (inx + 1).ToString());

                        if (string.IsNullOrWhiteSpace(advStr))
                            advStr = $@"Please enter IC(MyKad)/Passport for Passenger {inx + 1}";

                        throw new Exception(advStr);
                    }
                    else if (idBox.Text.Trim().Length < 8)
                    {
                        focusRequestTextBox = idBox;
                        string errMsg = "";

                        if (_language == LanguageCode.Malay)
                            errMsg = string.Format(_langMal["CUST_ERROR_IC_NO_Label"]?.ToString(), (inx + 1).ToString());
                        else
                            errMsg = string.Format(_langEng["CUST_ERROR_IC_NO_Label"]?.ToString(), (inx + 1).ToString());

                        if (string.IsNullOrWhiteSpace(errMsg))
                            errMsg = $@"Please enter a minimum of 8 alphanumeric for IC(MyKad)/Passport Passenger {inx + 1}";

                        throw new Exception(errMsg);
                    }

                    TextBox contactBox = GetInputTextBox(inx, _contactPrefix);
                    if (string.IsNullOrWhiteSpace(contactBox.Text.Trim()))
                    {
                        focusRequestTextBox = contactBox;

                        string advStr = "";

                        if (_language == LanguageCode.Malay)
                            advStr = string.Format(_langMal["ADVICE_MOBILE_NO_Label"]?.ToString(), (inx + 1).ToString());
                        else
                            advStr = string.Format(_langEng["ADVICE_MOBILE_NO_Label"]?.ToString(), (inx + 1).ToString());

                        if (string.IsNullOrWhiteSpace(advStr))
                            advStr = $@"Please enter a minimum of 10 numeric characters without space for Mobile No. Passenger {inx + 1}";

                        throw new Exception(advStr);
                    }
                    else
                    {
                        contactBox.Text = contactBox.Text.Trim();
                        System.Windows.Forms.Application.DoEvents();

                        int intRes = 0;
                        string chr = "";
                        string contactNo = "";

                        for (int chrInx = 0; chrInx < contactBox.Text.Length; chrInx++)
                        {
                            chr = contactBox.Text.Substring(chrInx, 1);
                            if (int.TryParse(chr, out intRes) == true)
                                contactNo += chr;
                        }

                        if (contactNo.Trim().Length < 10)
                        {
                            focusRequestTextBox = contactBox;

                            string errMsg = "";

                            if (_language == LanguageCode.Malay)
                                errMsg = string.Format(_langMal["CUST_ERROR_MOBILE_NO_Label"]?.ToString(), (inx + 1).ToString());
                            else
                                errMsg = string.Format(_langEng["CUST_ERROR_MOBILE_NO_Label"]?.ToString(), (inx + 1).ToString());

                            if (string.IsNullOrWhiteSpace(errMsg))
                                errMsg = $@"Passenger {inx + 1} Mobile No. must be 10 numeric characters and without space";

                            throw new Exception(errMsg);
                        }
                    }

                    TextBox seatIdBox = GetInputTextBox(inx, _seatIdPrefix);
                    TextBox seatTypeBox = GetInputTextBox(inx, _seatTypePrefix);
                    Run seatRunText = GetSeatRunText(inx);
                    passgList.Add(new PassengerInfo(nameBox.Text, idBox.Text, contactBox.Text, seatRunText.Text, seatIdBox.Text, seatTypeBox.Text));
                }
                catch (Exception ex)
                {
                    _txtError.Text = ex.Message;
                    _txtError.Visibility = System.Windows.Visibility.Visible;
                    _txtConfirmWait.Visibility = System.Windows.Visibility.Collapsed;
                    return false;
                }
            }

            _txtError.Text = "";
            _txtError.Visibility = System.Windows.Visibility.Collapsed;
            _txtConfirmWait.Visibility = System.Windows.Visibility.Visible;

            passengerInfoList = passgList.ToArray();
            if (passengerInfoList.Length == 0)
                throw new Exception($@"Unknown error when reading passenger info; (EXIT10000810)");

            return true;
        }

        /// <summary>
        /// Return true if successfully confirm.
        /// </summary>
        /// <param name="passengerInfoList"></param>
        /// <returns></returns>
        public bool ConfirmPassengerInfo(out PassengerInfo[] passengerInfoList)
        {
            passengerInfoList = null;
            if (_hasConfirm == false)
            {
                bool isCheckingSuccess = CheckNReadAllPassengerInfo(out PassengerInfo[] passengerInfoListX, out TextBox focusRequestTextBox);

                if (isCheckingSuccess)
                {
                    _hasConfirm = true;
                    passengerInfoList = passengerInfoListX;
                    App.ShowDebugMsg($@"ConfirmPassengerInfo -- Success; Data Count: {passengerInfoListX.Length}");
                    return true;
                }
                else
                {
                    if (focusRequestTextBox != null)
                    {
                        SetBorderFocusEffect(focusRequestTextBox);
                        focusRequestTextBox.Focus();
                    }
                }
            }
            else
                App.ShowDebugMsg("ConfirmPassengerInfo -- Already Confirm");

            return false;
        }

        public void UpdateCustomerInfo(int recordInx, string id, string name)
        {
            _pageDispatcher.Invoke(new Action(() => {
                TextBox nameBox = GetInputTextBox(recordInx, _namePrefix);
                TextBox idBox = GetInputTextBox(recordInx, _idPrefix);

                nameBox.Text = name;
                idBox.Text = id;
            }));
        }

        public void SetMobileFocus(int recordInx)
        {
            _pageDispatcher.Invoke(new Action(() => {
                TextBox mobileBox = GetInputTextBox(recordInx, _contactPrefix);
                mobileBox.Focus();
            }));
        }

        public void SetNameFocus(int recordInx)
        {
            _pageDispatcher.Invoke(new Action(() => {
                TextBox nameBox = GetInputTextBox(recordInx, _namePrefix);
                nameBox.Focus();
            }));
        }

        public void SetMobileBorderFocusEffect(int recordInx)
        {
            _pageDispatcher.Invoke(new Action(() => {
                TextBox mobileBox = GetInputTextBox(recordInx, _contactPrefix);
                
                if (mobileBox != null)
                {
                    if (mobileBox.Parent is Border bd)
                    {
                        bd.BorderBrush = _focusBorderEffectColor;
                        bd.BorderThickness = new Thickness(2, 2, 2, 2);
                    }
                }

            }));
        }

        public void SetBorderFocusEffect(TextBox textbox)
        {
            _pageDispatcher.Invoke(new Action(() =>
            {
                if (textbox.Parent is Border bd)
                {
                    bd.BorderBrush = _focusBorderEffectColor;
                    bd.BorderThickness = new Thickness(2, 2, 2, 2);
                }
            }));
        }

        public void ResetBorderFocusEffect(TextBox textbox)
        {
            _pageDispatcher.Invoke(new Action(() => 
            {
                if (textbox.Parent is Border bd)
                {
                    bd.BorderBrush = _normalBorderEffectColor;
                    bd.BorderThickness = new Thickness(1, 1, 1, 1);
                }
            }));
        }

    }
}
