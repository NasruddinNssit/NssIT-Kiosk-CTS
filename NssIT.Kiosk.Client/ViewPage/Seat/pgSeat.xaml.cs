using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.ViewPage.Trip;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    /// <summary>
    /// Interaction logic for pgSeat.xaml
    /// </summary>
    public partial class pgSeat : Page, ISeat, IKioskViewPage
	{
		private const string LogChannel = "ViewPage";

		private Brush _activateConfirmButtomColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x43, 0xD8, 0x2C));
		private Brush _deactivateConfirmButtomColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));

		private Brush _activateConfirmButtomTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _deactivateConfirmButtomTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));

		private SeatViewVerticalMover _touchVerticalMover;
		private SeatCalibrator _seatCalibrator = null;

		//private string _currencyCode = "RM";

		//private bool _insuranceRequested = true;
		//private decimal _insurance = 0M;

		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private LanguageCode _language = LanguageCode.English;
		private string _passengerDate = null;
		private string _busType = null;
		private decimal _terminalCharge = 0M;
		private int _tripCode = -1;
		private decimal _adultPrice = 0M;
		private string _adultExtra = null;
		private decimal _adultDisc = 0M;
		private decimal _onlineQrCharge = 0M;

		private PickDetail[] _pickDetail = null;
		private DropDetail[] _dropDetail = null;

		public pgSeat()
		{
			InitializeComponent();

			_touchVerticalMover = new SeatViewVerticalMover(this, SvSeatSelection);
			_touchVerticalMover.OnControlSelected += _touchVerticalMover_OnControlSelected;

			_seatCalibrator = new SeatCalibrator(this, this.Dispatcher, StkLowerDeck, StkUpperDeck, TxtMsg);

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\Seat\rosSeatMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\Seat\rosSeatEnglish.xaml");
		}

		private TripMode _travalMode = TripMode.Depart;
		public TripMode TravalMode
		{
			get
			{
				return _travalMode;
			}
			private set
			{
				_travalMode = value;

				this.Dispatcher.Invoke(new Action(() => {
					if (_travalMode == TripMode.Return)
					{
						if (_language == LanguageCode.Malay)
						{
							TxtMaximumSeatAdvice.Text = string.Format(_langMal["MANDATORY_SEAT_ADVICE_Label"]?.ToString(), MaximumSeatPerTrip.ToString());
						}
						else
						{
							TxtMaximumSeatAdvice.Text = string.Format(_langEng["MANDATORY_SEAT_ADVICE_Label"]?.ToString(), MaximumSeatPerTrip.ToString());
						}

						if (string.IsNullOrWhiteSpace(TxtMaximumSeatAdvice.Text))
							TxtMaximumSeatAdvice.Text = string.Format("Please select {0} seat/s(*)", MaximumSeatPerTrip.ToString());
					}
					else
					{
						if (_language == LanguageCode.Malay)
							TxtMaximumSeatAdvice.Text = string.Format(_langMal["MAXIMUM_SEAT_ADVICE_Label"]?.ToString(), MaximumSeatPerTrip.ToString());
						else
							TxtMaximumSeatAdvice.Text = string.Format(_langEng["MAXIMUM_SEAT_ADVICE_Label"]?.ToString(), MaximumSeatPerTrip.ToString());

						if (string.IsNullOrWhiteSpace(TxtMaximumSeatAdvice.Text))
							TxtMaximumSeatAdvice.Text = string.Format("Maximum {0} seats per trip(*)", MaximumSeatPerTrip.ToString());
					}
				}));				
			}
		}

		public SeatInfo[] SelectedSeatList
		{
			get
			{
				return _seatCalibrator.SelectedSeats.ToArray();
			}
		}

		/// <summary>
		/// When TravalMode is TripMode.Return, user must select seats count equal to this value. Else User must select at least 1 seat or up to this limited seat count value. 
		/// </summary>
		private int _maximumSeatPerTrip = 1;
		private int MaximumSeatPerTrip
		{
			get
			{
				return _maximumSeatPerTrip;
			}
			set
			{
				_maximumSeatPerTrip = (value > 0) ? value : 1;
			}
		}

		public string Currency
		{
			get
			{
				string retCurr = null;

				this.Dispatcher.Invoke(new Action(() => {
					retCurr = string.IsNullOrWhiteSpace(TxtCurrency.Text) ? null : TxtCurrency.Text;
				}));

				return retCurr;
			}
			private set
			{
				TxtCurrency.Text = value ?? "";
				//TxtCurrency2.Text = value ?? "";
			}
		}

		//public bool InsuranceRequested
		//{
		//	get
		//	{
		//		bool retReq = false;
		//		this.Dispatcher.Invoke(new Action(() => {
		//			if (ChkPIP.IsChecked.HasValue && (ChkPIP.IsChecked.Value == true))
		//				retReq = true;
		//			else
		//				retReq = false;
		//		}));
		//		return retReq;
		//	}
		//	private set
		//	{
		//		ChkPIP.IsChecked = value;
		//	}
		//}

		public decimal _insurancePrice = 0.0m;
		public decimal InsurancePrice
		{
			get
			{
				//decimal retAmt = 0.0M;
				//this.Dispatcher.Invoke(new Action(() => {
				//	if (decimal.TryParse(TxtPIPAmount.Text, out decimal pipAmount))
				//		retAmt = pipAmount;
				//	else
				//		retAmt = 0M;
				//}));

				return _insurancePrice;
			}
			private set
			{
				_insurancePrice = value;
			}
		}

		private bool _pageDataLoaded = false;
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			GrdScreenShield.Visibility = Visibility.Collapsed;

			this.Resources.MergedDictionaries.Clear();

			if (_language == LanguageCode.Malay)
				this.Resources.MergedDictionaries.Add(_langMal);
			else
				this.Resources.MergedDictionaries.Add(_langEng);

			TxtMsg.Text = "";

			_touchVerticalMover.ResetScrollIndex();

			RefreshSeatLayout();
			ChangeScreenSize();
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			_pageDataLoaded = false;
		}

		private bool _hasConfirmed = false;
		private void Button_ConfirmSeat(object sender, MouseButtonEventArgs e)
		{
			PickupNDropList pndList = null;

			if (_hasConfirmed == false)
			{
				if (SelectedSeatList.Length > 0)
				{
					App.MainScreenControl.ExecMenu.ShieldMenu();

					SeatInfo[] selectedSeatArr = SelectedSeatList;
					CustSeatDetail[] submitSeatList = new CustSeatDetail[selectedSeatArr.Length];

					int arrInx = -1;
					foreach(SeatInfo seat in selectedSeatArr)
					{
						arrInx++;
						submitSeatList[arrInx] = new CustSeatDetail() { SeatId = long.Parse(seat.ActualId), Desn = seat.SeatDesc, SeatType = seat.SeatType };
					}

					if ((_pickDetail != null) || (_dropDetail != null))
					{
						pndList = new PickupNDropList() { PickDetailList = _pickDetail, DropDetailList = _dropDetail };
					}

					_hasConfirmed = true;

					Submit(submitSeatList, pndList, _passengerDate, _busType, InsurancePrice,
						_terminalCharge, _tripCode, _adultPrice, _adultExtra, _adultDisc, _onlineQrCharge);

					//Submit(submitSeatList, pndList, _passengerDate, _busType, (InsuranceRequested ? InsurancePrice : 0M),
					//	_terminalCharge, _tripCode, _adultPrice, _adultExtra, _adultDisc, _onlineQrCharge);

					//App.NetClientSvc.SalesService.SubmitSeatList(submitSeatList, pndList, _passengerDate, _busType, (InsuranceRequested ? InsurancePrice : 0M), 
					//	_terminalCharge, _tripCode, _adultPrice, _adultExtra, _adultDisc, _onlineQrCharge, 
					//	out bool isServerResponded);

					//if (isServerResponded == false)
					//	App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000515)");
				}
			}
		}

		private void Submit(CustSeatDetail[] custSeatDetailList, PickupNDropList pickupAndDropList,
			string departDate, string departBusType,
			decimal departInsurance, decimal departTerminalCharge,
			int departTripCode, decimal departAdultPrice,
			string departAdultExtra, decimal departAdultDisc,
			decimal departOnlineQrCharge)
		{
			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitSeatList(custSeatDetailList, pickupAndDropList, departDate, departBusType, departInsurance,
						departTerminalCharge, departTripCode, departAdultPrice, departAdultExtra, departAdultDisc, departOnlineQrCharge,
						out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000515)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000516)");
					App.Log.LogError(LogChannel, "", new Exception("(EXIT10000516)", ex), "EX01", "pgSeat.Submit");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		public void ShieldPage()
		{
			GrdScreenShield.Visibility = Visibility.Visible;
		}

		//private void ChkPIP_ValueChange(object sender, RoutedEventArgs e)
		//{
		//	InsuranceRequested = ChkPIP.IsChecked.HasValue ? ChkPIP.IsChecked.Value : false;
		//	CountTotalTicketAmount();
		//}

		private void _touchVerticalMover_OnControlSelected(object sender, EventArgs e)
		{
			if ((sender is Border) || (sender is TextBlock))
			{
				Border bdSource = (sender is Border) ? (Border)sender : (Border)((TextBlock)sender).Parent;

				if (bdSource.Tag is string)
				{
					TriggerSeatBorder(bdSource);
					//DEBUG-Testing -- ShowMsg($@"StkLowerDeck_MouseDown : {bdSource.Tag.ToString()}");

					//if (_seatCalibrator != null)
					//{
					//	_seatCalibrator.SeatTriggerSelection(bdSource);

					//	CountTotalTicketAmount();
					//	RefreshConfirmButtom();
					//}
				}
			}
		}

		public void InitDepartSeatData(UIDepartSeatListAck uiDepartSeatList, UserSession session)
		{
			if (session is null)
				throw new Exception("Invalid session data;  (EXIT10000516)");

			//void InitSeatData ( SeatDeckCollection[] seatDeckArray, ..)
			_hasConfirmed = false;

			_language = session.Language;

			seat_status seatState = (seat_status)uiDepartSeatList.MessageData;

			MaximumSeatPerTrip = (App.AppMaxSeatPerTrip > seatState.MaxSeatAllow) ? (int)seatState.MaxSeatAllow : App.AppMaxSeatPerTrip;
			TravalMode = (session.TravelMode == AppDecorator.Common.TravelMode.DepartOrReturn) ? TripMode.Return : TripMode.Depart;
			
			Currency = string.IsNullOrWhiteSpace(session.DepartCurrency) ? "RM" : session.DepartCurrency.Trim();
			//InsuranceRequested = true;
			InsurancePrice = seatState.insurance;

			if (_travalMode == TripMode.Return)
				_passengerDate = session.ReturnPassengerDate.Value.ToString("dd/MM/yyyy");
			else
				_passengerDate = session.DepartPassengerDate.Value.ToString("dd/MM/yyyy");

			
			_busType = seatState.bustype;
			_terminalCharge = seatState.terminalcharge;
			_tripCode = seatState.tripcode;
			_adultPrice = seatState.adultprice;
			_adultExtra = seatState.adultextra;
			_adultDisc = seatState.adultdisc;
			_onlineQrCharge = seatState.onlineqrcharge;

			if (App.SysParam.PrmIsDebugMode)
			{
				if ((session.DepartEmbed?.Trim().Equals("1") == true))
				{
					//seatState.pick_details
					//seatState.drop_details
					//CYA-DEBUG - Test With Embed
					seatState.pick_details = new pick_detail[8];
					seatState.pick_details[0] = new pick_detail() { pick = Guid.NewGuid().ToString("D"), desn = "Melaka Sentral xxxxx ddddd fffff hhhhh kkkkk", time = "021500" };
					seatState.pick_details[1] = new pick_detail() { pick = Guid.NewGuid().ToString("D"), desn = "TBS", time = "031500" };
					seatState.pick_details[2] = new pick_detail() { pick = Guid.NewGuid().ToString("D"), desn = "Lar Kin", time = "041500" };
					seatState.pick_details[3] = new pick_detail() { pick = Guid.NewGuid().ToString("D"), desn = "Ipoh", time = "051500" };
					seatState.pick_details[4] = new pick_detail() { pick = Guid.NewGuid().ToString("D"), desn = "Rawang", time = "061500" };
					seatState.pick_details[5] = new pick_detail() { pick = Guid.NewGuid().ToString("D"), desn = "Sungai Buluh", time = "071500" };
					seatState.pick_details[6] = new pick_detail() { pick = Guid.NewGuid().ToString("D"), desn = "Batu Pahat", time = "081500" };
					seatState.pick_details[7] = new pick_detail() { pick = Guid.NewGuid().ToString("D"), desn = "Klang", time = "091500" };

					seatState.drop_details = new drop_detail[8];
					seatState.drop_details[0] = new drop_detail() { drop = Guid.NewGuid().ToString("D"), desn = "@Melaka Sentral xxxxx ddddd fffff hhhhh kkkkk" };
					seatState.drop_details[1] = new drop_detail() { drop = Guid.NewGuid().ToString("D"), desn = "@TBS" };
					seatState.drop_details[2] = new drop_detail() { drop = Guid.NewGuid().ToString("D"), desn = "@Lar Kin" };
					seatState.drop_details[3] = new drop_detail() { drop = Guid.NewGuid().ToString("D"), desn = "@Ipoh" };
					seatState.drop_details[4] = new drop_detail() { drop = Guid.NewGuid().ToString("D"), desn = "@Rawang" };
					seatState.drop_details[5] = new drop_detail() { drop = Guid.NewGuid().ToString("D"), desn = "@Sungai Buluh" };
					seatState.drop_details[6] = new drop_detail() { drop = Guid.NewGuid().ToString("D"), desn = "@Batu Pahat" };
					seatState.drop_details[7] = new drop_detail() { drop = Guid.NewGuid().ToString("D"), desn = "@Klang" };
				}
			}

			if ((session.DepartEmbed?.Trim().Equals("1") == true))
			{
				if (seatState.pick_details?.Length > 0)
				{
					_pickDetail = new PickDetail[seatState.pick_details.Length];

					int inx = -1;
					foreach (pick_detail row in seatState.pick_details)
					{
						inx++;
						_pickDetail[inx] = new PickDetail() { Pick = row.pick, PickDesn = row.desn, PickTime = row.time };
					}
				}
				else
					_pickDetail = null;

				if (seatState.drop_details?.Length > 0)
				{
					_dropDetail = new DropDetail[seatState.drop_details.Length];

					int inx = -1;
					foreach (drop_detail row in seatState.drop_details)
					{
						inx++;
						_dropDetail[inx] = new DropDetail() { Drop = row.drop, DropDesn = row.desn };
					}
				}
				else
					_dropDetail = null;
			}
			else
			{
				_pickDetail = null;
				_dropDetail = null;
			}

			decimal defaultSeatPrice = _adultPrice + InsurancePrice + _terminalCharge + _onlineQrCharge;

			SeatDeckCollection seatDeckColl = _seatCalibrator.GenerateSeatDeckCollection(seatState.col1, seatState.col2, seatState.row, defaultSeatPrice, seatState.details);

			//seatState.pick_details
			//_pickDetail
			//seatState.drop_details

			this.Dispatcher.Invoke(new Action(() => {

				_seatCalibrator.InitCalibration(new SeatDeckCollection[1] { seatDeckColl }, MaximumSeatPerTrip, defaultSeatPrice, _language);
				_pageDataLoaded = true;

				CountTotalTicketAmount();
				RefreshConfirmButtom();
			}));


			////////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			////////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			//_debugSeatPage.Debug_GetTestData(out TripMode tripMode, out int maxSeatPerTrip, out SeatDeckCollection[] seatDeckArray, out decimal defaultSeatPrice);
			//ISeat seatPage = (ISeat)_debugSeatPage;

			//seat_status seatState = (seat_status)uiDepartSeatList.MessageData;
			//int maxSeat = (App.AppMaxSeatPerTrip > seatState.MaxSeatAllow) ? (int)seatState.MaxSeatAllow : App.AppMaxSeatPerTrip;
			//decimal defaultTicketPrice = seatState.adultprice + seatState.insurance + seatState.terminalcharge + seatState.onlineqrcharge;
			//string currencyCode = string.IsNullOrWhiteSpace(session.DepartCurrency) ? "RM" : session.DepartCurrency.Trim();

			//seatPage.InitSeatData(TripMode.Depart, seatDeckArray, maxSeat, defaultSeatPrice, currencyCode, true, seatState.insurance);
			//frmWorkDetail.NavigationService.Navigate(SeatPage);

			////////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			////////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		}

		public void TriggerSeatBorder(Border bdSeat)
		{
			if (_seatCalibrator != null)
			{
				App.TimeoutManager.ResetTimeout();

				_seatCalibrator.SeatTriggerSelection(bdSeat);

				CountTotalTicketAmount();
				RefreshConfirmButtom();
			}
		}

		private void CountTotalTicketAmount()
		{
			if (!_pageDataLoaded)
				return;

			// Show Ticket Total Summary
			SeatInfo[] selectedSeats = SelectedSeatList;

			string[] seatDescArr = (from seatInfo in selectedSeats
									select seatInfo.SeatDesc).ToArray();

			TxtSelectedSeats.Text = "";
			foreach (string seatDesc in seatDescArr)
				TxtSelectedSeats.Text += seatDesc + ", ";

			if (TxtSelectedSeats.Text.Length > 0)
				TxtSelectedSeats.Text = TxtSelectedSeats.Text.Substring(0, TxtSelectedSeats.Text.Length - 2);

			int ticketCount = selectedSeats.Length;

			decimal totalTickAmt = (from seatInfo in selectedSeats
									select seatInfo.Price).Sum();

			//if (InsuranceRequested)
			//{
			decimal totalAmt = totalTickAmt;
			TxtTotal.Text = $@"{totalAmt:#,##0.00}";
			//}
			//else
			//{
			//	totalTickAmt -= (ticketCount * InsurancePrice);
			//	TxtTotal.Text = $@"{totalTickAmt:#,##0.00}";
			//}
		}

		///// <summary>
		///// </summary>
		///// <param name="travalMode"></param>
		///// <param name="seatDeckArray">Array[0] away refer to lower deck for bus.</param>
		///// <param name="maximumSeatPerTrip"></param>
		///// <param name="defaultSeatPrice"></param>
		///// <param name="currency"></param>
		///// <param name="insuranceRequested"></param>
		///// <param name="insurancePrice">This price is separated from ticket price</param>
		//public void InitSeatData(TripMode travalMode, SeatDeckCollection[] seatDeckArray, int maximumSeatPerTrip, decimal defaultSeatPrice, string currency, bool insuranceRequested, decimal insurancePrice)
		//{
		//	this.Dispatcher.Invoke(new Action(() => {
		//		TravalMode = travalMode;
		//		Currency = currency;

		//		MaximumSeatPerTrip = maximumSeatPerTrip;
		//		InsuranceRequested = insuranceRequested;
		//		InsurancePrice = insurancePrice;

		//		_seatCalibrator.InitCalibration(seatDeckArray, maximumSeatPerTrip, defaultSeatPrice);
		//		_pageDataLoaded = true;

		//		CountTotalTicketAmount();
		//		RefreshConfirmButtom();
		//	}));
		//}

		private void RefreshConfirmButtom()
		{
			if (!_pageDataLoaded)
				return;

			bool enableConfirm = false;
			int selectedSeatCount = SelectedSeatList.Length;

			if (TravalMode == TripMode.Return)
			{
				//DEBUG-Testing -- MainWindow.ShowMessage("RefreshConfirmButtom - TripMode.Return");

				if (selectedSeatCount == MaximumSeatPerTrip)
					enableConfirm = true;
			}
			else
			{
				//DEBUG-Testing -- MainWindow.ShowMessage($@"RefreshConfirmButtom - TripMode.Depart; selectedSeatCount : {selectedSeatCount}");

				if (selectedSeatCount > 0)
					enableConfirm = true;
			}

			if (enableConfirm)
			{
				BdComfirmSeat.Background = _activateConfirmButtomColor;
				TxtComfirmSeat.Foreground = _activateConfirmButtomTextColor;
			}
			else
			{
				BdComfirmSeat.Background = _deactivateConfirmButtomColor;
				TxtComfirmSeat.Foreground = _deactivateConfirmButtomTextColor;
			}
		}

		private void RefreshSeatLayout()
		{
			DispatcherProcessingDisabled dispDisabler;

			try
			{
				dispDisabler = Dispatcher.DisableProcessing();

				_seatCalibrator.RefreshAllContainerSeat(out int lowerDeckColCount, out int upperDeckColCount);

				int totalColumns = lowerDeckColCount + upperDeckColCount;

				// One Center Line View 
				if ((totalColumns > 12) || (upperDeckColCount == 0))
				{
					if ((int)TxtUpperDeckTag.GetValue(Grid.ColumnProperty) == 3)
					{
						TxtUpperDeckTag.SetValue(Grid.ColumnProperty, 1);
						TxtUpperDeckTag.SetValue(Grid.RowProperty, 3);

						StkUpperDeck.SetValue(Grid.ColumnProperty, 1);
						StkUpperDeck.SetValue(Grid.RowProperty, 4);

						BdSplitter.Visibility = Visibility.Collapsed;
					}

					if (upperDeckColCount == 0)
					{
						TxtUpperDeckTag.Visibility = Visibility.Collapsed;
					}
				}

				// Two (Left & Right) Lines View 
				else
				{
					if ((int)TxtUpperDeckTag.GetValue(Grid.ColumnProperty) == 1)
					{
						TxtUpperDeckTag.SetValue(Grid.ColumnProperty, 3);
						TxtUpperDeckTag.SetValue(Grid.RowProperty, 0);

						StkUpperDeck.SetValue(Grid.ColumnProperty, 3);
						StkUpperDeck.SetValue(Grid.RowProperty, 1);

						BdSplitter.Visibility = Visibility.Visible;
					}

					if (upperDeckColCount > 0)
					{
						TxtUpperDeckTag.Visibility = Visibility.Visible;
					}
				}

			}
			catch (Exception ex)
			{
				if (dispDisabler != null)
					dispDisabler.Dispose();

				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000511); pgSeatSelection.RefreshSeatLayout");
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000511)", ex), "EX01", classNMethodName: "pgSeatSelection.RefreshSeatLayout");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000501)");
			}
			finally
			{
				if (dispDisabler != null)
					dispDisabler.Dispose();
			}
		}

		public void Debug_GetTestData(out TripMode travalMode, out int maxSeatPerTrip, out SeatDeckCollection[] seatDeckArr, out decimal defaultSeatPrice)
		{
			defaultSeatPrice = 1.0M;
			travalMode = TripMode.Depart;
			maxSeatPerTrip = 1;
			seatDeckArr = new SeatDeckCollection[0];

			Random rand = new Random();
			travalMode = ((rand.Next(5, 45) % 3) == 0) ? TripMode.Return : TripMode.Depart;

			_seatCalibrator.DebugTest_XX_GetSeatsData(out maxSeatPerTrip, out seatDeckArr, out defaultSeatPrice);
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
						BdComfirmSeat.Height = 60;
						BdStacker1.Height = 600;
					}
					_changeScreenSizeCount++;
				}
			}
		}
	}
}