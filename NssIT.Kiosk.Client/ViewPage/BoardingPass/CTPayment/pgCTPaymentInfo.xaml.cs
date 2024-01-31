using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
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

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.CTPayment
{
    /// <summary>
    /// ClassCode:EXIT80.23; Interaction logic for pgCTPaymentInfo.xaml
    /// </summary>
    public partial class pgCTPaymentInfo : Page
    {
        private string _logChannel = "Payment";

        private BoardingTicket _boardingTicket = null;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _currentRosLang = null;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private bool _pageLoaded = false;
        private NetServiceAnswerMan _netClientSvcAnswerMan = null;

        /// <summary>
		/// FuncCode:EXIT80.2301
		/// </summary>
        public pgCTPaymentInfo()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\CTPayment\rosCTPaymentMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\CTPayment\rosCTPaymentEnglish.xaml");
            _currentRosLang = _langEng;
        }

        /// <summary>
		/// FuncCode:EXIT80.2305
		/// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //UnShieldPage();
                _netClientSvcAnswerMan?.Dispose();

                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_currentRosLang);

                LoadCompanyLogo();

                TxtTicketNo.Text = _boardingTicket.TicketNo ?? "";
                TxtTripNo.Text = _boardingTicket.TripNo ?? "";
                TxtPassgName.Text = (_boardingTicket.PassengerName ?? "").Trim();
                TxtICPass.Text = (_boardingTicket.PassengerIC ?? "").Trim();
                TxtContactNo.Text = (_boardingTicket.PassengerContact ?? "").Trim();
                TxtBusCompanyName.Text = _boardingTicket.BusCompanyName ?? "";

                if (_boardingTicket.PassengerDepartDate.HasValue)
                    TxtDepartureDate.Text = DateTimeCulture.GetCultureString(_boardingTicket.PassengerDepartDate.Value, "dd MMMM yyyy", _language);
                else
                    TxtDepartureDate.Text = "????? Unknown ?????";

                TxtFrom.Text = $@"{_boardingTicket.Fromdesn}";
                TxtTo.Text = $@"{_boardingTicket.ToDesn}";
                TxtSeat.Text = $@"{_boardingTicket.SeatNo}     RM {_boardingTicket.SellingPrice:##,##0.00}     ({TicketTypeTranslator.GetTypeDescription(_language, _boardingTicket.SeatType)})";

                TxtFasilityCharges.Text = $@"RM {_boardingTicket.FacilityCharge:##,##0.00}";
                TxtQRCharges.Text = $@"RM {_boardingTicket.QRCharge:##,##0.00}";
                TxtTotalAmount.Text = $@"RM {_boardingTicket.TotalChargableAmount:##,##0.00}";

                _pageLoaded = true;
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT80.2305.EX01); pgTrip.Page_Loaded");
                App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.2305.EX01)", ex), "EX01", classNMethodName: "pgCTPaymentInfo.Page_Loaded");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.2305.EX01)");
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

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = false;
        }

        public void InitPaymentInfo(BoardingTicket boardingTicket, LanguageCode language)
        {
            _boardingTicket = boardingTicket;
            _language = language;
        }
    }
}
