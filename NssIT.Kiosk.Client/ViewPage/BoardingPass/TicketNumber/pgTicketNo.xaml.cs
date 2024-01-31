using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static NssIT.Kiosk.Client.ViewPage.KeyboardEventArgs;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.TicketNumber
{
    /// <summary>
	/// ClassCode:EXIT80.19; Interaction logic for pgTicketNo.xaml
	/// </summary>
    public partial class pgTicketNo : Page, ICTTicketNo, IKioskViewPage
    {
        private string _logChannel = "ViewPage";

        private bool _pageLoaded = false;

        private static Brush _enableButtonColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x43, 0xD8, 0x2C));
        private static Brush _disableButtonColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));

        private LanguageCode _language = LanguageCode.English;

        private ResourceDictionary _currentRosLang = null;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;
        private NetServiceAnswerMan _netClientSvcAnswerMan = null;

        private string _busCompanyName = "";
        private string _busCompanyLogoURL = null;
        private DateTime _passengerDepartDate = DateTime.MaxValue;

        private DbLog Log { get; set; }

        /// <summary>
		/// FuncCode:EXIT80.1901
		/// </summary>
        public pgTicketNo()
        {
            InitializeComponent();

            Log = DbLog.GetDbLog();
            KbKeys.OnKeyPressed += KbKeys_OnKeyPressed;
            _langMal = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\TicketNumber\rosTicketNoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\TicketNumber\rosTicketNoEnglish.xaml");
        }

        /// <summary>
        /// FuncCode:EXIT80.1902
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UnShieldPage();
                _netClientSvcAnswerMan?.Dispose();

                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_currentRosLang);

                EnableSearch = false;

                TxtTicketNoWatermark.Visibility = Visibility.Visible;
                GrdTicketNo.Visibility = Visibility.Collapsed;
                TxtTicketNo.Text = "";
                TxtTicketNoWatermark.CaretIndex = 0;
                TxtTicketNoWatermark.Focus();

                TxtBusCompanyName.Text = _busCompanyName;
                TxtDepartureDate.Text = DateTimeCulture.GetCultureString(_passengerDepartDate, "dd MMM yyyy", _language);
                UscNav.LoadControl(_language);
                LoadCompanyLogo();

                _pageLoaded = true;
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1902.EX01); pgTicketNo.Page_Loaded");
                App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.1902.EX01)", ex), "EX01", classNMethodName: "pgTicketNo.Page_Loaded");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1902.EX01)");
            }

            
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            async Task LoadCompanyLogo()
            {
                if (_busCompanyLogoURL != null)
                    ImgBusComapnyLogo.Source = await App.CTCompanyLogoCache.GetImage(_busCompanyLogoURL);
                else
                    ImgBusComapnyLogo.Source = null;
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.1903
		/// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = false;
        }

        private void KbKeys_OnKeyPressed(object sender, KeyboardEventArgs e)
        {
            try
            {
                ReNewCountdown("pgTicketNo.KbKeys_OnKeyPressed");

                bool isHitMaxLen = (TxtTicketNo.MaxLength <= TxtTicketNo.Text.Length);

                if ((e.KyCat == KeyCat.NormalChar) && (isHitMaxLen == false))
                {
                    TxtTicketNo.Text += e.KeyString;
                }
                else
                {
                    if (e.KyCat == KeyCat.Backspace)
                    {
                        if (TxtTicketNo.Text.Length > 0)
                        {
                            TxtTicketNo.Text = TxtTicketNo.Text.Substring(0, TxtTicketNo.Text.Length - 1);
                        }
                    }
                    else if (e.KyCat == KeyCat.Enter)
                    {

                    }
                    else if ((e.KyCat == KeyCat.Space) && (isHitMaxLen == false))
                    {
                        TxtTicketNo.Text += " ";
                    }
                }

                RefreshTicketNoArea();
                RefreshSearchButton();
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgTicketNo.KbKeys_OnKeyPressed");
            }
            finally
            {
                System.Windows.Forms.Application.DoEvents();
            }

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        }

        private void RefreshSearchButton()
        {
            if (TxtTicketNo.Text.Trim().Length > 0)
                EnableSearch = true;
            else
                EnableSearch = false;
        }

        private void RefreshTicketNoArea()
        {
            this.Dispatcher.Invoke(new Action(() => 
            {
                if (string.IsNullOrWhiteSpace(TxtTicketNo.Text))
                {
                    TxtTicketNoWatermark.Visibility = Visibility.Visible;
                    GrdTicketNo.Visibility = Visibility.Collapsed;

                    TxtTicketNo.Text = "";
                    TxtTicketNoWatermark.CaretIndex = 0;
                    TxtTicketNoWatermark.Focus();
                }
                else
                {
                    TxtTicketNoWatermark.Visibility = Visibility.Collapsed;
                    GrdTicketNo.Visibility = Visibility.Visible;

                    TxtTicketNo.CaretIndex = TxtTicketNo.Text.Length;
                    TxtTicketNo.Focus();
                }
            }));
        }

        /// <summary>
		/// FuncCode:EXIT80.1906
		/// </summary>
        private void Button_ClearTicketNo(object sender, MouseButtonEventArgs e)
        {
            ReNewCountdown("pgTicketNo.Button_ClearTicketNo");

            TxtTicketNoWatermark.Visibility = Visibility.Visible;
            GrdTicketNo.Visibility = Visibility.Collapsed;
            TxtTicketNo.Text = "";
            TxtTicketNoWatermark.CaretIndex = 0;

            RefreshSearchButton();

            TxtTicketNoWatermark.Focus();
        }

        /// <summary>
		/// FuncCode:EXIT80.1907
		/// </summary>
        private void TicketNoWatermark_GotFocus(object sender, RoutedEventArgs e)
        {
            ReNewCountdown("pgTicketNo.TicketNoWatermark_GotFocus");
            TxtTicketNoWatermark.CaretIndex = 0;
        }

        /// <summary>
		/// FuncCode:EXIT80.1908
		/// </summary>
        private void BdSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (EnableSearch)
            {
                try
                {
                    if ((_pageLoaded == false) || string.IsNullOrWhiteSpace(TxtTicketNo.Text))
                        return;

                    Submit();

                    EnableSearch = false;
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1850.EX01); pgDate.BdSearch_MouseLeftButtonDown");
                    App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.1850.EX01)", ex), "EX01", classNMethodName: "pgTicketNo.BdSearch_MouseLeftButtonDown");
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1850.EX01)");
                }
                
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.1909
		/// </summary>
        private void BdError_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReNewCountdown("pgTicketNo.BdError_MouseLeftButtonUp");
            UnShieldPage();
            EnableSearch = true;
        }

        /// <summary>
		/// FuncCode:EXIT80.1910
		/// </summary>
        public void InitData(IKioskMsg kioskMsg)
        {
            if ((kioskMsg is IUserSession usrSession) && (usrSession.Session != null))
            {
                if (usrSession.Session.TicketCollection.PassengerDepartDate.HasValue)
                    _passengerDepartDate = usrSession.Session.TicketCollection.PassengerDepartDate.Value;
                else
                    throw new Exception("Invalid passenger date specification; (EXIT80.1910.X01)");

                if (string.IsNullOrWhiteSpace(usrSession.Session.TicketCollection.BusCompanyName) == false)
                    _busCompanyName = usrSession.Session.TicketCollection.BusCompanyName;
                else
                    throw new Exception("Invalid bus company specification; (EXIT80.1910.X02)");

                _busCompanyLogoURL = usrSession.Session.TicketCollection.BusCompanyLogoURL;

                _language = usrSession.Session.Language;

                if (_language == LanguageCode.Malay)
                    _currentRosLang = _langMal;
                else
                    _currentRosLang = _langEng;
            }
        }

        public void ShowTicketNumberNotFound(IKioskMsg kioskMsg)
        {
            string errMsg = _currentRosLang["TICKET_NOT_FOUND_ERROR_ErrMsg"].ToString() + "(X)";
            string otherErrMsg = null;

            if  (kioskMsg.GetMsgData() is UIxCTTicketNoNotFoundAck ack5)
            {
                if (ack5.ErrorCode == 6)
                {
                    errMsg = _currentRosLang["TICKET_NOT_FOUND_ERROR_ErrMsg"].ToString();

                    if ((_language != LanguageCode.English) && (string.IsNullOrWhiteSpace(ack5.ErrorMessage) == false))
                    {
                        otherErrMsg = $@"({ack5.ErrorMessage.Trim()})";
                    }
                }
                else if (ack5.ErrorCode == 61)
                {
                    errMsg = _currentRosLang["ORIGIN_SERVER_NOT_FOUND_ErrMsg"].ToString();

                    if ((_language != LanguageCode.English) && (string.IsNullOrWhiteSpace(ack5.ErrorMessage) == false))
                    {
                        otherErrMsg = $@"({ack5.ErrorMessage.Trim()})";
                    }
                }
                else if (ack5.ErrorCode == 20)
                {
                    errMsg = _currentRosLang["INVALID_TOKEN_ErrMsg"].ToString();
                }
                else if (ack5.ErrorCode == 21)
                {
                    errMsg = _currentRosLang["TOKEN_EXPIRED_ErrMsg"].ToString();
                }
                else if (string.IsNullOrWhiteSpace(ack5.ErrorMessage) == false)
                {
                    if (_language != LanguageCode.English) 
                    {
                        errMsg = $@"({ack5.ErrorMessage.Trim()})";
                    }
                    else
                        errMsg = ack5.ErrorMessage.Trim();
                }
            }
            else
            {
                errMsg = string.IsNullOrWhiteSpace(errMsg) ? "Ticket not found from website (XX)" : errMsg;
            }
            
            ShieldPageError(errMsg, otherErrMsg);
        }

        /// <summary>
		/// FuncCode:EXIT80.1912
		/// </summary>
		private void Submit()
        {
            ShieldPage();
            System.Windows.Forms.Application.DoEvents();

            try
            {
                _netClientSvcAnswerMan?.Dispose();

                _netClientSvcAnswerMan = App.NetClientSvc.CollectTicketService.SubmitTicketNo(TxtTicketNo.Text.Trim(),
                    "Local Server not responding (EXIT80.1912.X1)",
                        new NetServiceAnswerMan.FailLocalServerResponseCallBackDelg(delegate (string failMessage)
                        {
                            App.MainScreenControl.Alert(detailMsg: failMessage);
                        }));

                ShieldPage();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1912.EX01); pgTicketNo.Submit");
                App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.1912.EX01)", ex), "EX01", classNMethodName: "pgTicketNo.Submit");
                //App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1912.EX01)");
            }
        }

        private bool _isSearchEnabled = false;
        private bool EnableSearch
        {
            get => _isSearchEnabled;
            set
            {
                if (value == true)
                {
                    _isSearchEnabled = true;
                    BdSearch.Background = _enableButtonColor;
                }
                else
                {
                    _isSearchEnabled = false;
                    BdSearch.Background = _disableButtonColor;
                }
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.1915
		/// </summary>
        private void ReNewCountdown(string tag)
        {
            if (_pageLoaded)
            {
                try
                {
                    App.CollectBoardingPassCountDown?.ReNewCountdown();
                }
                catch (Exception ex)
                {
                    Log.LogError(_logChannel, "-", new Exception($@"Tag: {tag}", ex), "EX01", "pgTicketNo.ReNewCountdown");
                }
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.1920
		/// </summary>
		public void ShieldPage()
        {
            TxtShieldInProgress.Visibility = Visibility.Visible;
            StpError.Visibility = Visibility.Collapsed;
            GrdScreenShield.Visibility = Visibility.Visible;
        }

        /// <summary>
		/// FuncCode:EXIT80.1925
		/// </summary>
        private void UnShieldPage()
        {
            GrdScreenShield.Visibility = Visibility.Collapsed;
        }

        /// <summary>
		/// FuncCode:EXIT80.1930
		/// </summary>
		public void ShieldPageError(string errorMsg, string otherLangErrorMsg)
        {
            TxtShieldInProgress.Visibility = Visibility.Collapsed;
            StpError.Visibility = Visibility.Visible;
            TxtShieldError.Text = errorMsg ?? "*Error*";

            if (string.IsNullOrWhiteSpace(otherLangErrorMsg))
                TxtShieldOtherLangError.Visibility = Visibility.Collapsed;
            else
            {
                TxtShieldOtherLangError.Text = otherLangErrorMsg;
                TxtShieldOtherLangError.Visibility = Visibility.Visible;
            }
            GrdScreenShield.Visibility = Visibility.Visible;
        }
    }
}
