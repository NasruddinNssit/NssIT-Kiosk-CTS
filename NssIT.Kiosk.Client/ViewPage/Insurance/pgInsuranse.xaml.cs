using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
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

namespace NssIT.Kiosk.Client.ViewPage.Insurance
{
    /// <summary>
    /// Interaction logic for pgInsuranse.xaml
    /// </summary>
    public partial class pgInsuranse : Page, IInsuranse
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
		private DateTime _departPassengerDepartDateTime = DateTime.MinValue;
		private CustSeatDetail[] _departCustSeatDetail = null;
		private DateTime _departPassengerDate = DateTime.MinValue;

		private SeatDetailViewList _seatDetailList = new SeatDetailViewList();

		private CultureInfo _provider = CultureInfo.InvariantCulture;
		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private LanguageCode _language = LanguageCode.English;


		public pgInsuranse()
        {
            InitializeComponent();

			LstSeatList.DataContext = _seatDetailList;
			_langMal = CommonFunc.GetXamlResource(@"ViewPage\Insurance\rosInsuranceMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\Insurance\rosInsuranceEnglish.xaml");
		}

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
				LoadInsuranceMsg();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"Error: {ex.Message}; pgInsuranse.Page_Loaded");
				App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsuranse.Page_Loaded");
			}

			void LoadInsuranceMsg()
            {
				using (var d = this.Dispatcher.DisableProcessing())
				{
					_seatDetailList.Clear();

					foreach (CustSeatDetail st in _departCustSeatDetail)
                    {
						_seatDetailList.Add(new SeatDetailViewRow() 
						{ 
							Currency = $@"{_departCurrency}", SeatDesn = $@"{st.Desn}",
							Price = (_departAdultPrice + _departInsurance + _departTerminalCharge + _departOnlineQrCharge)
						});
					}

					TxtCurrency.Text = _departCurrency;
					TxtAmount.Text = $@"{_departTotalAmount:#,###.00}";

					string insName = GetInsuranceName();
					//TxtInsuranceA1.Text = $@"Please include PIP (Passenger Insurance Protection) and admin fees({_departCurrency} {_departInsurance:0.00} per person) in my ticket(s).";
					TxtInsuranceA1.Text = string.Format(currLanguage["INSURENCE_A_1_Label"].ToString(), insName, _departCurrency, _departInsurance);

					TxtOperatorName.Text = _departCompanyDesc;
					TxtOriginDesc.Text = _originStationName;
					TxtDestDesc.Text = _destinationStationName;
					TxtDepartDate.Text = _departPassengerDepartDateTime.ToString("dd/MM/yyyy");
					TxtDepartTime.Text = _departPassengerDepartDateTime.ToString("hh:mm tt");

					BitmapImage bip = new BitmapImage();
					bip.BeginInit();
					bip.UriSource = new Uri(_departCompanyLogoUrl, UriKind.Absolute);
					bip.EndInit();

					BdOperatorLogo.Background = new ImageBrush(bip) { Stretch = Stretch.Uniform };
				}
				System.Windows.Forms.Application.DoEvents();
			}

			string GetInsuranceName()
            {
				if (_language == LanguageCode.Malay)
				{
					if (App.SysParam.PrmAppGroup == AppGroup.Larkin)
						return "LPIP (Larkin Perlindungan Insurans Penumpang)";
					else
						return "PIP (Perlindungan Insurans Penumpang)";
				}
				else
				{
					if (App.SysParam.PrmAppGroup == AppGroup.Larkin)
						return "LPIP (Larkin Passenger Insurance Protection)";
					else
						return "PIP (Passenger Insurance Protection)";
				}
			}
		}

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

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

					Submit(isIncludeInsurance: false);
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

					Submit(isIncludeInsurance: true);
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

		public void InitInsurance(UIInsuranceAck uIInsurance)
        {
			if (uIInsurance.Session != null)
			{
				string tickDateTime = $@"{uIInsurance.Session.DepartPassengerDate.Value.ToString("dd-MM-yyyy")}_{uIInsurance.Session.DepartPassengerDepartTime}";
				_departPassengerDepartDateTime = DateTime.ParseExact(tickDateTime, "dd-MM-yyyy_HHmmss", _provider);

				_originStationName = uIInsurance.Session.OriginStationName;
				_destinationStationName = uIInsurance.Session.DestinationStationName;
				_departCompanyDesc = uIInsurance.Session.DepartCompanyDesc;
				_departCompanyLogoUrl = uIInsurance.Session.DepartCompanyLogoUrl;
				_departCustSeatDetail = uIInsurance.Session.PassengerSeatDetailList;
				_departCurrency = uIInsurance.Session.DepartCurrency;
				_departAdultPrice = uIInsurance.Session.DepartAdultPrice;
				_departInsurance = uIInsurance.Session.DepartInsurance;
				_departTerminalCharge = uIInsurance.Session.DepartTerminalCharge;
				_departOnlineQrCharge = uIInsurance.Session.DepartOnlineQrCharge;
				_departTotalAmount = uIInsurance.Session.DepartTotalAmount;

				_language = uIInsurance.Session.Language;
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

		private void Submit(bool isIncludeInsurance)
		{
			SetShield(isOn: true);
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitInsurance(isIncludeInsurance, out bool isServerResponded);

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
