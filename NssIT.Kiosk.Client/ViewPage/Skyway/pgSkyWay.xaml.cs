using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.ViewPage.Insurance;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace NssIT.Kiosk.Client.ViewPage.Skyway
{
    /// <summary>
    /// Interaction logic for pgInsuranse.xaml
    /// </summary>
    public partial class pgSkyWay : Page
	{
		private Brush _activateButtomColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x43, 0xD8, 0x2C));
		private Brush _deactivateButtomColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));

		private const string LogChannel = "ViewPage";

        private string _originStationName = "";
        private string _destinationStationName = "";
        private string _departCompanyDesc = "";
        private string _departCompanyLogoUrl = "";
        private string _departCurrency = "RM";
        private decimal _departAdultPrice = 0M;
        private decimal _departInsurance = 0M;
        private decimal _departTerminalCharge = 0M;
        private decimal _departOnlineQrCharge = 0M;
        private decimal _departTotalAmount = 0M;
		private decimal _skyWayPrice = 0M;
        private DateTime _departPassengerDepartDateTime = DateTime.MinValue;
        private CustSeatDetail[] _departCustSeatDetail = null;
        private DateTime _departPassengerDate = DateTime.MinValue;

        private CultureInfo _provider = CultureInfo.InvariantCulture;
		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private LanguageCode _language = LanguageCode.English;


		public pgSkyWay()
        {
            InitializeComponent();
			
			_langMal = CommonFunc.GetXamlResource(@"ViewPage\Skyway\rosInsuranceMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\Skyway\rosInsuranceEnglish.xaml");
		}


		//public void InitSkyWayData(UserSession session)
		//{
		//	_language = session.Language;
		//}
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			ResourceDictionary currLanguage = _langEng;
			try
			{
				_hasResult = false;
				if (_resultLock.CurrentCount == 0)
					_resultLock.Release();

				this.Resources.MergedDictionaries.Clear();
				if (_language == LanguageCode.Malay)
					currLanguage = _langMal;
				else
					currLanguage = _langEng;

				this.Resources.MergedDictionaries.Add(currLanguage);

				ChangeScreenSize();
				SetShield(isOn: false);
				LoadSkyWayMsg();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"Error: {ex.Message}; pgInsuranse.Page_Loaded");
				App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsuranse.Page_Loaded");
			}

			void LoadSkyWayMsg()
			{
				//TxtLabel.Text = string.Format(currLanguage["Buy_Label"].ToString());

				using(var d = this.Dispatcher.DisableProcessing())
				{
					//_seatDetailList.Clear();
					//foreach(CustSeatDetail st in _departCustSeatDetail)
					//{
					//	_seatDetailList.Add(new SeatDetailViewRow()
					//	{
					//		Currency = $@"{_departCurrency}",SeatDesn = $@"{st.Desn}",
     //                       Price = (_departAdultPrice + _departInsurance + _departTerminalCharge + _departOnlineQrCharge)
     //                   });
					//}

                   

                    TxtOperatorName.Text = _departCompanyDesc;
                    TxtOriginDesc.Text = _originStationName;
                    TxtDestDesc.Text = _destinationStationName;
                    TxtDepartDate.Text = _departPassengerDepartDateTime.ToString("dd/MM/yyyy");
                    TxtDepartTime.Text = _departPassengerDepartDateTime.ToString("hh:mm tt");

					if(_language == LanguageCode.Malay)
					{
						TxtTotalTicket.Text = string.Format(_langMal["TOTAL_TICKET_Label"]?.ToString(), _departCustSeatDetail.Length);
                        

                    }
					else
					{
                        TxtTotalTicket.Text = string.Format(_langEng["TOTAL_TICKET_Label"]?.ToString(), _departCustSeatDetail.Length);

                    }
					var skywayTotalPrice = _skyWayPrice * _departCustSeatDetail.Length;

					TxtSkywayPrice.Text = $@"{_departCurrency} {_skyWayPrice: #,###.00} x {_departCustSeatDetail.Length}";
                    TxtSkywayTotalPrice.Text = $@"{_departCurrency} {skywayTotalPrice: #,###.00} ";
                    BitmapImage bip = new BitmapImage();
                    bip.BeginInit();
                    bip.UriSource = new Uri(_departCompanyLogoUrl, UriKind.Absolute);
                    bip.EndInit();

                    BdOperatorLogo.Background = new ImageBrush(bip) { Stretch = Stretch.Uniform };

                }

                System.Windows.Forms.Application.DoEvents();

            }

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

		public void InitSkyWay(UISkyWayAck uISkyWay)
		{
			if(uISkyWay.Session != null)
			{
				string ticketDateTime = $@"{uISkyWay.Session.DepartPassengerDate.Value.ToString("dd-MM-yyyy")}_{uISkyWay.Session.DepartPassengerDepartTime}";
				_departPassengerDepartDateTime = DateTime.ParseExact(ticketDateTime, "dd-MM-yyyy_HHmmss", _provider);

				_originStationName = uISkyWay.Session.OriginStationName;
				_destinationStationName = uISkyWay.Session.DestinationStationName;
				_departCompanyDesc = uISkyWay.Session.DepartCompanyDesc;
				_departCompanyLogoUrl = uISkyWay.Session.DepartCompanyLogoUrl;
				_departCustSeatDetail = uISkyWay.Session.PassengerSeatDetailList;
				_departCurrency = uISkyWay.Session.DepartCurrency;
                _departAdultPrice = uISkyWay.Session.DepartAdultPrice;
                _departInsurance = uISkyWay.Session.DepartInsurance;
                _departTerminalCharge = uISkyWay.Session.DepartTerminalCharge;
                _departOnlineQrCharge = uISkyWay.Session.DepartOnlineQrCharge;
                _departTotalAmount = uISkyWay.Session.DepartTotalAmount;
				_skyWayPrice = uISkyWay.Session.DepartSkyWayAmount;
				_language = uISkyWay.Session.Language;
			}
			else
			{
                _departPassengerDepartDateTime = DateTime.MinValue;
                _originStationName = "";
                _destinationStationName = "";
                _departCompanyDesc = "";
                _departCompanyLogoUrl = "";
                _departCustSeatDetail = null;
                _departCurrency = "";
                _departAdultPrice = 0.0M;
                _departInsurance = 0.0M;
                _departTerminalCharge = 0.0M;
                _departOnlineQrCharge = 0.0M;
                _departTotalAmount = 0.0M;

                _language = LanguageCode.English;
            }
		}

		private bool _hasResult = false;
		private SemaphoreSlim _resultLock = new SemaphoreSlim(1);
		private async void BdNo_Click(object sender, MouseButtonEventArgs e)
		{
			if ((await _resultLock.WaitAsync(3 * 1000)) == true)
            {
				try
				{
					if (_hasResult)
						return;

					Submit(isIncludeSkyWay: false);
					_hasResult = true;
				}
				catch (Exception ex)
				{
					App.ShowDebugMsg($@"Error: {ex.Message}; pgInsuranse.BdNo_Click");
					App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsuranse.BdNo_Click");
				}
				finally
				{
					if (_resultLock.CurrentCount == 0)
						_resultLock.Release();
				}
			}			
		}

		private async void BdYes_Click(object sender, MouseButtonEventArgs e)
		{
			if ((await _resultLock.WaitAsync(3 * 1000)) == true)
			{
				try
				{
					if (_hasResult)
						return;

					Submit(isIncludeSkyWay: true);
					_hasResult = true;
				}
				catch (Exception ex)
				{
					App.ShowDebugMsg($@"Error: {ex.Message}; pgInsuranse.BdYes_Click");
					App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsuranse.BdYes_Click");
				}
				finally
				{
					if (_resultLock.CurrentCount == 0)
						_resultLock.Release();
				}
			}
		}
		private void Submit(bool isIncludeSkyWay)
		{
			SetShield(isOn: true);
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitSkyWay(isIncludeSkyWay, out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000111)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000112)");
					App.Log.LogError(LogChannel, "", new Exception("(EXIT10000112)", ex), "EX01", "pgInsuranse.Submit");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		private void SetShield(bool isOn)
        {
			if (isOn)
            {
				BdNo.Background = _deactivateButtomColor;
				BdYes.Background = _deactivateButtomColor;
			}
			else
            {
				BdNo.Background = _activateButtomColor;
				BdYes.Background = _activateButtomColor;
			}
        }

		private int _changeScreenSizeCount = 0;
		private void ChangeScreenSize()
		{
			if (App.MainScreenControl.QueryWindowSize
				(out double winWidth, out double winHeight) == true)
			{
				if (_changeScreenSizeCount <= 3)
				{
					if (winHeight > 1500)
					{
						BdNo.Height = 60;
						BdYes.Height = 60;
					}
					_changeScreenSizeCount++;
				}
			}
		}
	}
}
