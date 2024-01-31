using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Common.WebService.KioskWebService;
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

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.BusCompany
{
    /// <summary>
	/// ClassCode:EXIT80.16; Interaction logic for pgBusCompany.xaml
	/// </summary>
    public partial class pgBusCompany : Page, ICTBusCompany, IKioskViewPage
    {
        private string _logChannel = "ViewPage";

        private LanguageCode _language = LanguageCode.English;

        private bool _pageLoaded = false;

        private ResourceDictionary _currentRosLang = null;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private BusCompanyViewListHelper _busCompanyListHelper = null;
        private BusCompanyViewSearch _busCompanyViewSearch = null;

        private NetServiceAnswerMan _netClientSvcAnswerMan = null;

        private DbLog Log { get; set; }
        /// <summary>
        /// FuncCode:EXIT80.1601
        /// </summary>
        public pgBusCompany()
        {
            InitializeComponent();

            Log = DbLog.GetDbLog();
            _langMal = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\BusCompany\rosBusCompanyMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\BusCompany\rosBusCompanyEnglish.xaml");
            _busCompanyListHelper = new BusCompanyViewListHelper(this.Dispatcher, LstBusCompany);
            _busCompanyViewSearch = new BusCompanyViewSearch(this.Dispatcher, _busCompanyListHelper.BusCompanyViewList, LstBusCompany);
            KbKeys.OnKeyPressed += KbKeys_OnKeyPressed;
        }

        /// <summary>
        /// FuncCode:EXIT80.1605
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GrdScreenShield.Visibility = Visibility.Collapsed;

                _isBusCompanySelected = false;
                BusCompanyViewRow[] vRow = _busCompanyListHelper.PageLoaded();
                _busCompanyViewSearch.PageLoaded(vRow);

                TxtCompanyNameFilter.Text = "";
                TxtCompanyNameFilterWatermark.Visibility = Visibility.Visible;
                GrdCompanyNameFilter.Visibility = Visibility.Collapsed;
                UscNav.LoadControl(_language);

                System.Windows.Forms.Application.DoEvents();

                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_currentRosLang);

                _pageLoaded = true;
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1605.EX01); pgTrip.Page_Loaded");
                App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.1605.EX01)", ex), "EX01", classNMethodName: "pgBusCompany.Page_Loaded");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1605.EX01)");
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.1610
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _pageLoaded = false;
                _netClientSvcAnswerMan?.Dispose();
                _busCompanyViewSearch.PageUnLoaded();
                _busCompanyListHelper.PageUnLoaded();
                App.ShowDebugMsg("pgBusCompany Page_Unloaded");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1610.EX01); pgTrip.Page_Unloaded");
                App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.1610.EX01)", ex), "EX01", classNMethodName: "pgBusCompany.Page_Unloaded");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1610.EX01)");
            }
        }

        private bool _isBusCompanySelected = false;
        /// <summary>
        /// FuncCode:EXIT80.1615
        /// </summary>
        private void LstCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pageLoaded && (_isBusCompanySelected == false))
            {
                try
                {
                    if (LstBusCompany.SelectedItem is BusCompanyViewRow viewRow)
                    {
                        _isBusCompanySelected = true;
                        _netClientSvcAnswerMan?.Dispose();

                        _netClientSvcAnswerMan = App.NetClientSvc.CollectTicketService.SubmitBusCompany(viewRow.CompanyLogoURL, viewRow.CompanyCode, viewRow.CompanyName,
                            "Local Server not responding (EXIT80.1615.X1)",
                                new NetServiceAnswerMan.FailLocalServerResponseCallBackDelg(delegate (string failMessage)
                                {
                                    App.MainScreenControl.Alert(detailMsg: failMessage);
                                }));

                        ShieldPage();
                    }
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1615.EX01); pgBusCompany.LstCompany_SelectionChanged");
                    App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.1615.EX01)", ex), "EX01", classNMethodName: "pgBusCompany.LstCompany_SelectionChanged");
                    //App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1615.EX01)");
                }
            }
        }

        private void LstCompany_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ReNewCountdown("LstCompany_ScrollChanged");
        }

        private void KbKeys_OnKeyPressed(object sender, KeyboardEventArgs e)
        {
            try
            {
                ReNewCountdown("pgBusCompany.KbKeys_OnKeyPressed");

                if (e.KyCat == KeyCat.NormalChar)
                {
                    TxtCompanyNameFilter.Text += e.KeyString;
                }
                else
                {
                    if (e.KyCat == KeyCat.Backspace)
                    {
                        if (TxtCompanyNameFilter.Text.Length > 0)
                        {
                            TxtCompanyNameFilter.Text = TxtCompanyNameFilter.Text.Substring(0, TxtCompanyNameFilter.Text.Length - 1);
                        }
                    }
                    else if (e.KyCat == KeyCat.Enter)
                    {

                    }
                    else if (e.KyCat == KeyCat.Space)
                    {
                        TxtCompanyNameFilter.Text += " ";
                    }
                }

                if (string.IsNullOrWhiteSpace(TxtCompanyNameFilter.Text))
                {
                    Visibility watermarkHistoryVisibility = TxtCompanyNameFilterWatermark.Visibility;

                    TxtCompanyNameFilterWatermark.Visibility = Visibility.Visible;
                    GrdCompanyNameFilter.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TxtCompanyNameFilterWatermark.Visibility = Visibility.Collapsed;
                    GrdCompanyNameFilter.Visibility = Visibility.Visible;
                    TxtCompanyNameFilter.Focus();
                    TxtCompanyNameFilter.CaretIndex = TxtCompanyNameFilter.Text.Length;
                }

                System.Windows.Forms.Application.DoEvents();
                _busCompanyViewSearch.FilterListByTime(TxtCompanyNameFilter.Text);
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgBusCompany.KbKeys_OnKeyPressed");
            }
            finally
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        private void TxtCompanyNameFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtCompanyNameFilter.Text))
                {
                    TxtCompanyNameFilterWatermark.Visibility = Visibility.Visible;
                    GrdCompanyNameFilter.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, "-", ex, "EX01", "pgBusCompany.TxtCompanyNameFilter_LostFocus");
            }
        }

        private void TxtCompanyNameFilterWatermark_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtCompanyNameFilterWatermark.Visibility = Visibility.Collapsed;
                GrdCompanyNameFilter.Visibility = Visibility.Visible;
                TxtCompanyNameFilter.Focus();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "", ex, "EX01", "pgStation.TxtStationFilterWatermark_GotFocus");
            }
        }

        private void Button_ClearCompanyNameFilter(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ReNewCountdown("KbKeys_OnKeyPressed");
                TxtCompanyNameFilter.Text = "";
                TxtCompanyNameFilterWatermark.Visibility = Visibility.Visible;
                GrdCompanyNameFilter.Visibility = Visibility.Collapsed;
                _busCompanyViewSearch.FilterListByTime(TxtCompanyNameFilter.Text);
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.Button_ClearStationFilter");
            }
        }

        public void InitData(IKioskMsg kioskMsg)
        {
            if ((kioskMsg is IUserSession usrSession) && (usrSession.Session != null))
            {
                _language = usrSession.Session.Language;

                if (_language == LanguageCode.Malay)
                    _currentRosLang = _langMal;
                else
                    _currentRosLang = _langEng;

                if (kioskMsg.GetMsgData() is UIxGnBTnGAck<boardingcompany_status> ackData)
                {
                    _busCompanyListHelper.InitData(ackData.Data);
                }
            }
        }

        public void ShieldPage()
        {
            GrdScreenShield.Visibility = Visibility.Visible;
        }

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
                    Log.LogError(_logChannel, "-", new Exception($@"Tag: {tag}", ex), "EX01", "pgBusCompany.ReNewCountdown");
                }
            }
        }
    }
}
