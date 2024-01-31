using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.PassengerInfo
{
    /// <summary>
    /// ClassCode:EXIT80.21; Interaction logic for pgPassengerInfo.xaml
    /// </summary>
    public partial class pgPassengerInfo : Page, ICTPassengerInfo, IKioskViewPage
    {
        private string _logChannel = "ViewPage";

        private CultureInfo _culProvider = CultureInfo.InvariantCulture;
        private string _dateFormat = "dd/MM/yyyy";

        private bool _pageLoaded = false;

        private static Brush _enableButtonColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x43, 0xD8, 0x2C));
        private static Brush _disableButtonColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));

        private Brush _focusBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
        private Brush _normalBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xE4, 0xE4, 0xE4));

        private LanguageCode _language = LanguageCode.English;

        private ResourceDictionary _currentRosLang = null;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;
        private NetServiceAnswerMan _netClientSvcAnswerMan = null;
        private PassengerInfoEntryProxy _keyboardHelper = null;
        private PassengerInfoEntryValidator _passgChecker = null;

        private BoardingTicket _boardingTicket = null;
        private pgCTMyKad _myKadPage = null;
        //private TextBox _lastFocusTextBox = null;
        private bool _showMyKadScanner = false;

        private DbLog Log { get; set; }

        /// <summary>
		/// FuncCode:EXIT80.2101
		/// </summary>
        public pgPassengerInfo()
        {
            InitializeComponent();

            _showMyKadScanner = App.SysParam.PrmMyKadScanner;
            Log = DbLog.GetDbLog();
            _langMal = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\PassengerInfo\rosPassengerInfoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\PassengerInfo\rosPassengerInfoEnglish.xaml");

            _myKadPage = new pgCTMyKad();
            _keyboardHelper = new PassengerInfoEntryProxy(KbKeys, this.Dispatcher,
                BdPassNameWorkLocation, TxtPassgNameWatermark, GrdPassgNameSection, TxtPassgName, BdClearPassgName, 
                BdICPassWorkLocation, TxtICPassWatermark, GrdICPassSection, TxtICPass, BdClearICPass, 
                BdContactNoWorkLocation, TxtContactNoWatermark, GrdContactNoSection, TxtContactNo, BdClearContactNo);
            _passgChecker = new PassengerInfoEntryValidator(this.Dispatcher, TxtPassgName, TxtICPass, TxtContactNo, _langEng);

            _myKadPage.OnEndScan += _myKadPage_OnEndScan;
        }

        /// <summary>
        /// FuncCode:EXIT80.2105
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UnShieldPage();
                _netClientSvcAnswerMan?.Dispose();

                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_currentRosLang);

                EnableConfirm = true;

                TxtTicketNo.Text = _boardingTicket.TicketNo ?? "";
                TxtBusCompanyName.Text = _boardingTicket.BusCompanyName ?? "";
                _passgChecker.PageLoad(_currentRosLang);
                TxtPageError.Text = "";
                TxtTripNo.Text = _boardingTicket.TripNo ?? "*";

                try
                {
                    if (_boardingTicket.PassengerDepartDate.HasValue)
                        TxtDepartureDate.Text = DateTimeCulture.GetCultureString(_boardingTicket.PassengerDepartDate.Value, "dd MMMM yyyy", _language);
                    else
                        TxtDepartureDate.Text = "????? Unknown ?????";
                }
                catch { }

                TxtFrom.Text = $@"{_boardingTicket.Fromdesn}";
                TxtTo.Text = $@"{_boardingTicket.ToDesn}";
                TxtSeat.Text = $@"{_boardingTicket.SeatNo}     RM {_boardingTicket.SellingPrice:##,##0.00}     ({TicketTypeTranslator.GetTypeDescription(_language, _boardingTicket.SeatType)})";

                TxtFacilityCharges.Text = $@"RM {_boardingTicket.FacilityCharge:##,##0.00}";
                TxtQRCharges.Text = $@"RM {_boardingTicket.QRCharge:##,##0.00}";

                TxtPassgName.Text = (_boardingTicket.ExistingName ?? "").Trim();
                TxtICPass.Text = (_boardingTicket.ExistingIC ?? "").Trim();
                TxtContactNo.Text = (_boardingTicket.ExistingContact ?? "").Trim();

                FrmIdentityEntry.Content = null;
                FrmIdentityEntry.NavigationService.RemoveBackEntry();
                GrdPopUp.Visibility = Visibility.Collapsed;

                if (_showMyKadScanner)
                    BtnMyKad.Visibility = Visibility.Visible;
                else
                    BtnMyKad.Visibility = Visibility.Collapsed;

                UscNav.LoadControl(_language);

                LoadCompanyLogo();

                _keyboardHelper.LoadPage();
                _pageLoaded = true;
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1605.EX01); pgTrip.Page_Loaded");
                App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.1605.EX01)", ex), "EX01", classNMethodName: "pgBusCompany.Page_Loaded");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1605.EX01)");
            }


            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            async Task LoadCompanyLogo()
            {
                if (_boardingTicket.BusCompanyLogoURL != null)
                    ImgBusComapnyLogo.Source = await App.CTCompanyLogoCache.GetImage(_boardingTicket.BusCompanyLogoURL);
                else
                    ImgBusComapnyLogo.Source = null;
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2110
		/// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = false;
        }

        /// <summary>
		/// FuncCode:EXIT80.2115
		/// </summary>
        private void ScanMyKad_Click(object sender, RoutedEventArgs e)
        {
            if (App.CollectBoardingPassCountDown.ReNewCountdownAndWaitForResult())
            {
                try
                {
                    GrdPopUp.Visibility = Visibility.Visible;

                    System.Windows.Forms.Application.DoEvents();

                    _myKadPage.InitPageData(_language, 1);

                    FrmIdentityEntry.Content = null;
                    FrmIdentityEntry.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    FrmIdentityEntry.NavigationService.Navigate(_myKadPage);
                    System.Windows.Forms.Application.DoEvents();
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg("Error on pgPassengerInfo.ScanMyKad_Click");
                    App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPassengerInfo.ScanMyKad_Click");
                }
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2116
		/// </summary>
        private void _myKadPage_OnEndScan(object sender, CustInfo.EndOfMyKadScanEventArgs e)
        {
            try
            {
                ReNewCountdown("_myKadPage_OnEndScan");

                PassengerIdentity pssgId = null;
                pssgId = e.Identity;
                if (pssgId is null)
                {
                    App.ShowDebugMsg("pgCustInfo.ScanMyKad_Click : Unable to read Identity");
                }
                else
                {
                    if (pssgId.IsIDReadSuccess)
                    {
                        this.Dispatcher.Invoke(new Action(() => 
                        {
                            TxtPassgName.Text = pssgId.Name;
                            TxtICPass.Text = pssgId.IdNumber;
                        }));

                        App.ShowDebugMsg($@"pgPassengerInfo.ScanMyKad_Click : IC: {pssgId.IdNumber}; Name: {pssgId.Name}");
                    }
                    else if (string.IsNullOrWhiteSpace(pssgId.Message) == false)
                    {
                        App.ShowDebugMsg($@"pgPassengerInfo.ScanMyKad_Click : Error: {pssgId.Message}; ");
                    }
                    else
                    {
                        App.ShowDebugMsg($@"pgPassengerInfo.ScanMyKad_Click : Not response");
                    }
                }

                App.ShowDebugMsg("pgPassengerInfo.ScanMyKad_Click : End of MyKad Scanning");

                this.Dispatcher.Invoke(new Action(() => {
                    FrmIdentityEntry.Content = null;
                    FrmIdentityEntry.NavigationService.RemoveBackEntry();
                    GrdPopUp.Visibility = Visibility.Collapsed;

                    if ((pssgId != null) && (pssgId.IsIDReadSuccess))
                    {
                        _keyboardHelper.FocusedTextBox(TxtContactNo, isEnableBorderEffect: true);
                    }
                    else 
                        _keyboardHelper.FocusAEntryTextBox();
                }));

            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgPassengerInfo._myKadPage_OnEndScan");
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPassengerInfo._myKadPage_OnEndScan");
            }
            finally
            {
                App.MainScreenControl.MainFormDispatcher.Invoke(new Action(() => {
                    App.MainScreenControl.ExecMenu.UnShieldMenu();
                }));
                System.Windows.Forms.Application.DoEvents();
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2120
		/// </summary>
        private void BdConfirm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TxtPageError.Text = "";

                System.Windows.Forms.Application.DoEvents();

                if (_passgChecker.ValidateAllEntry(out TextBox requestFocusTxt, out string errorMsg) == false)
                {
                    //if (string.IsNullOrWhiteSpace(_currentRosLang["CORRECT_PASSENGER_INFO_Label"]?.ToString()) == false)
                    //    TxtPageError.Text = _currentRosLang["CORRECT_PASSENGER_INFO_Label"].ToString();

                    //else
                    //    TxtPageError.Text = "";

                    TxtPageError.Text = errorMsg;

                    if (requestFocusTxt != null)
                    {
                        _keyboardHelper.FocusedTextBox(requestFocusTxt, isEnableBorderEffect: true);
                    }
                }
                else
                {
                    Submit();
                }
            }
            catch (Exception ex)
            {
                if (string.IsNullOrWhiteSpace(ex?.Message) == false)
                    TxtPageError.Text = ex.Message;

                App.ShowDebugMsg($@"{ex.Message}; (EXIT80.2120.EX01); pgTrip.Page_Loaded");
                App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.2120.EX01)", ex), "EX01", classNMethodName: "pgPassengerInfo.BdConfirm_MouseLeftButtonUp");
                //App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1605.EX01)");
            }
        }
        
        private bool _isConfirmEnabled = false;
        private bool EnableConfirm
        {
            get => _isConfirmEnabled;
            set
            {
                if (value == true)
                {
                    _isConfirmEnabled = true;
                    BdComfirm.Background = _enableButtonColor;
                }
                else
                {
                    _isConfirmEnabled = false;
                    BdComfirm.Background = _disableButtonColor;
                }
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2145
		/// </summary>
        public void InitData(IKioskMsg kioskMsg)
        {
            if ((kioskMsg is IUserSession usrSession) && (usrSession.Session != null))
            {
                _boardingTicket = usrSession.Session.TicketCollection.Duplicate();

                _language = usrSession.Session.Language;

                if (_language == LanguageCode.Malay)
                    _currentRosLang = _langMal;
                else
                    _currentRosLang = _langEng;
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2146
		/// </summary>
		private void Submit()
        {
            ShieldPage();
            System.Windows.Forms.Application.DoEvents();

            try
            {
                _netClientSvcAnswerMan?.Dispose();

                _netClientSvcAnswerMan = App.NetClientSvc.CollectTicketService.SubmitPassengerInfo(TxtTicketNo.Text, TxtPassgName.Text, TxtICPass.Text, TxtContactNo.Text,
                    "Local Server not responding (EXIT80.2146.X1)",
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

        /// <summary>
		/// FuncCode:EXIT80.2150
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
                    Log.LogError(_logChannel, "-", new Exception($@"Tag: {tag}", ex), "EX01", "pgPassengerInfo.ReNewCountdown");
                }
            }
        }

        /// <summary>
		/// FuncCode:EXIT80.2155
		/// </summary>
		public void ShieldPage()
        {
            TxtShieldInProgress.Visibility = Visibility.Visible;
            GrdScreenShield.Visibility = Visibility.Visible;
        }

        /// <summary>
		/// FuncCode:EXIT80.2160
		/// </summary>
        private void UnShieldPage()
        {
            GrdScreenShield.Visibility = Visibility.Collapsed;
        }
    }
}
