using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Client.ViewPage.Alert;
using NssIT.Kiosk.Client.ViewPage.Date;
using NssIT.Kiosk.Client.ViewPage.Info;
using NssIT.Kiosk.Client.ViewPage.Intro;
using NssIT.Kiosk.Client.ViewPage.Language;
using NssIT.Kiosk.Client.ViewPage.Menu;
using NssIT.Kiosk.Client.ViewPage.StationTerminal;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using NssIT.Kiosk.Client.ViewPage.Menu.Section;
using NssIT.Kiosk.Client.ViewPage.Trip;
using NssIT.Kiosk.Client.ViewPage.Seat;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System.Globalization;
using NssIT.Kiosk.Client.ViewPage.PickupNDrop;
using NssIT.Kiosk.Client.ViewPage.CustInfo;
using NssIT.Kiosk.Client.ViewPage.Payment;
using System.Windows.Threading;
using NssIT.Kiosk.Client.ViewPage;
using System.Threading.Tasks;
using NssIT.Kiosk.Client.ViewPage.Insurance;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Client.ViewPage.BoardingPass.BusCompany;
using NssIT.Kiosk.Client.ViewPage.BoardingPass.BoardingDate;
using NssIT.Kiosk.Client.ViewPage.BoardingPass.TicketNumber;
using NssIT.Kiosk.Client.ViewPage.BoardingPass.PassengerInfo;
using NssIT.Kiosk.Client.ViewPage.BoardingPass.CTPayment;

namespace NssIT.Kiosk.Client
{
	/// <summary>
	/// ClassCode:EXIT80.02; Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IMainScreenControl
	{
		private const string LogChannel = "MainWindow";

		private const double _infoHeightNormal = 50D;
		private const double _menuWidthNormal = 250D;
		private const double _menuWidthPayment = 500D;

		public IInfo UserInfo { get; set; }
		private IAlertPage AlertPage { get; set; }
		private IStation StationPage { get; set; }
		private ITravelDate DatePage { get; set; }
		private ITrip TripPage { get; set; }
		private ISeat SeatPage { get; set; }
		private IPickupNDrop PickupNDropPage { get; set; }
		private IInsuranse InsurancePage { get; set; }
		private ICustInfo PassengerInfoPage { get; set; }
		private IPayment PaymentPage { get; set; }
		private ITimeFilter TimeFilterPage { get; set; }
		private ILanguage LanguagePage { get; set; }
		private ICTBusCompany CTBusCompanyPage { get; set; }
		private ICTBoardingDate CTBoardingDatePage { get; set; }
		private ICTTicketNo CTTicketNoPage { get; set; }
		private ICTPassengerInfo CTPassengerInfoPage { get; set; }
		private ICTPayment CTPayment { get; set; }

		private pgIntro _pgIntro = null;
		private pgSeat _debugSeatPage = null;

		private CultureInfo _provider = CultureInfo.InvariantCulture;

		public IMenuExec ExecMenu { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			AlertPage = new pgOutofOrder();
			StationPage = new pgStation();

			_pgIntro = new pgIntro();
			LanguagePage = new pgLanguage();

			TimeFilterPage = new pgTimeFilter();

			ExecMenu = new pgMenu();
			ExecMenu.InitTimeFilterPage(TimeFilterPage);

			UserInfo = new pgInfo();
			DatePage = new pgDate();

			TripPage = new pgTrip();
			TripPage.InitTimeFilterPage(TimeFilterPage);

			InsurancePage = new pgInsuranse();
			PickupNDropPage = new pgPickupNDrop();
			PassengerInfoPage = new pgCustInfo();
			PaymentPage = new pgPayment();

			CTBusCompanyPage = new pgBusCompany();
			CTBoardingDatePage = new pgBoardingDate();
			CTTicketNoPage = new pgTicketNo();
			CTPassengerInfoPage = new pgPassengerInfo();
			CTPayment = new pgCTPayment();

			_debugSeatPage = new pgSeat();
			SeatPage = _debugSeatPage;

			ExecMenu.OnEditMenuItem += ExecMenu_OnEditMenuItem;
			ExecMenu.OnPageNavigateChanged += ExecMenu_OnPageNavigateChanged;
		}

		public ICash CashierPage
        {
            get
            {
				return PaymentPage?.GetCashierPage();
			}
		}

		public IBTnG BTnGCounter
		{
			get
			{
				return PaymentPage?.GetBTnGCounterPage();
			}
		}

		public void ToTopMostScreenLayer()
		{
			this.Dispatcher.Invoke(new Action(() => {
				this.Topmost = true;
			}));
			System.Windows.Forms.Application.DoEvents();
		}

		public void ToNormalScreenLayer()
		{
			this.Dispatcher.Invoke(new Action(() => {
				this.Topmost = false;
			}));
			System.Windows.Forms.Application.DoEvents();
		}

		public Dispatcher MainFormDispatcher { get => this.Dispatcher; }

		//private bool _editSalesDetailFlag = false;
		//private SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private void ExecMenu_OnEditMenuItem(object sender, MenuItemEditEventArgs e)
		{
			try
			{
				//await _manLock.WaitAsync();

				//if (_editSalesDetailFlag == false)
				//{
				ExecMenu.ShieldMenu();


				SubmitOnEditMenuItem(e.EditItemCode);
				//App.NetClientSvc.SalesService.EditSalesDetail(e.EditItemCode, out bool isServerResponded);

				//if (isServerResponded == false)
				//	App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000305)");

				//_editSalesDetailFlag = true;
				//}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000306)", ex), "EX01", classNMethodName: "MainWindow.ExecMenu_OnEditMenuItem");
				App.MainScreenControl.Alert(detailMsg: $@"Error on language selection; (EXIT10000306)");
			}
			finally
			{
				//_manLock.Release();
			}
		}

		private void SubmitOnEditMenuItem(TickSalesMenuItemCode editDetailItem)
		{
			if (frmWorkDetail.Content is IKioskViewPage kiospPage)
				kiospPage.ShieldPage();

			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.EditSalesDetail(editDetailItem, out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000305)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000315)");
					App.Log.LogError(LogChannel, "", new Exception("(EXIT10000315)", ex), "EX01", "MainWindow.SubmitOnEditMenuItem");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}


		private void ExecMenu_OnPageNavigateChanged(object sender, MenuItemPageNavigateEventArgs e)
		{
			try
			{
				//await _manLock.WaitAsync();

				//if (_editSalesDetailFlag == false)
				//{
				ExecMenu.ShieldMenu();

				SubmitOnPageNavigateChanged(e.PageNavigateDirection);
				//App.NetClientSvc.SalesService.NavigateToPage(e.PageNavigateDirection);

				//if (isServerResponded == false)
				//	App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000312)");

			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000313)", ex), "EX01", classNMethodName: "MainWindow.ExecMenu_OnPageNavigateChanged");
				App.MainScreenControl.Alert(detailMsg: $@"Error on language selection; (EXIT10000313)");
			}
			finally
			{
				//_manLock.Release();
			}
		}

		private void SubmitOnPageNavigateChanged(PageNavigateDirection navigateDirection)
		{
			if (frmWorkDetail.Content is IKioskViewPage kiospPage)
				kiospPage.ShieldPage();

			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.NavigateToPage(navigateDirection);
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000316)");
					App.Log.LogError(LogChannel, "", new Exception("(EXIT10000316)", ex), "EX01", "MainWindow.SubmitOnPageNavigateChanged");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e) { }

		private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
		{
			App.ShutdownX();
		}

		private void HideMenu()
		{
			if (frmMenu.Content != null)
			{
				try
				{
					frmMenu.Content = null;
				}
				catch { }
			}
		}



		private void HideInfo()
		{
			if (frmWorkHeader.Content != null)
			{
				try
				{
					frmWorkHeader.Content = null;
				}
				catch { }
			}
		}

		private void DisplayInfo(double infoHeight)
		{
			try
			{
				if (infoHeight < 0)
					infoHeight = _infoHeightNormal;

				if (frmWorkHeader.Content == null)
				{
					try
					{
						//_editSalesDetailFlag = false;

						this.Dispatcher.Invoke(new Action(() => {
							frmWorkHeader.NavigationService.Navigate(UserInfo);
						}));
					}
					catch { }
				}
				if (((Page)UserInfo).ActualHeight != infoHeight)
				{
					((Page)UserInfo).Height = infoHeight;
				}
			}
			catch (ThreadAbortException) { }
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000301)", ex), classNMethodName: "MainWindow.ShowWelcome");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000301)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.DisplayInfo;");
			}
		}

		public void ShowWelcome()
		{
			try
			{
				App.ResetCurrentSaleTransactionModule();
				App.CollectBoardingPassCountDown?.ForceResetCounter();
				App.NetClientSvc?.SalesService?.ResetUserSessionSendOnly("*");
				App.NetClientSvc?.CollectTicketService?.ResetUserSessionSendOnly("*");

				//_editSalesDetailFlag = false;

				App.Log.LogText(LogChannel, "-", "xxxxxxxxxxxxxxxxxxxxxxxxxx Welcome xxxxxxxxxxxxxxxxxxxxxxxxxx", classNMethodName: "MainWindow.ShowWelcome");

				this.Dispatcher.Invoke(new Action(() => {

					ExecMenu.ResetDepartDate();
					ExecMenu.ResetDepartOperator();
					ExecMenu.ResetDepartSeat();
					ExecMenu.ResetFromStation();
					ExecMenu.ResetPassenger();
					ExecMenu.ResetPayment();
					ExecMenu.ResetReturnDate();
					ExecMenu.ResetReturnOperator();
					ExecMenu.ResetReturnSeat();
					ExecMenu.ResetToStation();

					ExecMenu.UnShieldMenu();

					HideInfo();
					HideMenu();
					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					System.Windows.Forms.Application.DoEvents();
					frmWorkDetail.NavigationService.Navigate(_pgIntro);
					System.Windows.Forms.Application.DoEvents();
				}));

				App.PaymentScrImage.DeleteHistoryFile();
			}
			catch (ThreadAbortException) { }
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000301)", ex), classNMethodName: "MainWindow.ShowWelcome");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000301)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ShowWelcome;");
			}
		}

		public void ChooseLanguage(AppModule module)
		{
			try
			{
				App.ShowDebugMsg("ChoiseLanguage");

				this.Dispatcher.Invoke(new Action(() => {
					HideInfo();
					HideMenu();

					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					LanguagePage.InitData(module);
					frmWorkDetail.NavigationService.Navigate(LanguagePage);
					System.Windows.Forms.Application.DoEvents();
				}));

			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000302)", ex), classNMethodName: "MainWindow.ChooseLanguage");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000302)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseLanguage;");
			}
		}

		public void ChooseDestinationStation(UIDestinationListAck uiDest, UserSession session)
		{
			try
			{
				//_editSalesDetailFlag = false;
				App.ShowDebugMsg("ChooseDestinationStation");

				this.Dispatcher.Invoke(new Action(() =>
				{
					using (var d = this.Dispatcher.DisableProcessing())
					{
						this.Dispatcher.BeginInvoke(new Action(() => {
							//UserInfo.ShowInfo(InfoCode.DestinationInfo, session.Language);
							//DisplayInfo(_infoHeightNormal);
							//DisplayMenu(_menuWidthNormal, session, TickSalesMenuItemCode.ToStation);

							frmWorkDetail.Content = null;
							frmWorkDetail.NavigationService.RemoveBackEntry();
							System.Windows.Forms.Application.DoEvents();

							StationPage.InitDestStationData(uiDest);
							ExecMenu.UnShieldMenu();
							frmWorkDetail.NavigationService.Navigate(StationPage);

							UserInfo.ShowInfo(InfoCode.DestinationInfo, session.Language);
							DisplayInfo(_infoHeightNormal);
							DisplayMenu(_menuWidthNormal, session, TickSalesMenuItemCode.ToStation);

							//System.Windows.Forms.Application.DoEvents();
						}));
					}
				}));
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000303)", ex), classNMethodName: "MainWindow.ChooseDestinationStation");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000303)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseDestinationStation;");
			}
		}

		public void ChooseOriginStation(UIOriginListAck uiOrig, UserSession session)
		{
			try
			{
				//_editSalesDetailFlag = false;
				App.ShowDebugMsg("ChooseOriginStation");

				this.Dispatcher.Invoke(new Action(() =>
				{
					using (var d = this.Dispatcher.DisableProcessing())
					{
						this.Dispatcher.BeginInvoke(new Action(() => {
							//UserInfo.ShowInfo(InfoCode.OriginInfo, session.Language);
							//DisplayInfo(_infoHeightNormal);
							//DisplayMenu(_menuWidthNormal, session, TickSalesMenuItemCode.ToStation);

							frmWorkDetail.Content = null;
							frmWorkDetail.NavigationService.RemoveBackEntry();
							System.Windows.Forms.Application.DoEvents();

							StationPage.InitOriginStationData(uiOrig);
							ExecMenu.UnShieldMenu();
							frmWorkDetail.NavigationService.Navigate(StationPage);

							UserInfo.ShowInfo(InfoCode.OriginInfo, session.Language);
							DisplayInfo(_infoHeightNormal);
							DisplayMenu(_menuWidthNormal, session, TickSalesMenuItemCode.FromStation);

							//System.Windows.Forms.Application.DoEvents();
						}));
					}
				}));
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000303)", ex), classNMethodName: "MainWindow.ChooseOriginStation");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000303)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseOriginStation;");
			}
		}

		public void ChooseTravelDates(UITravelDatesEnteringAck uiTravelDate, UserSession session)
		{
			try
			{
				//_editSalesDetailFlag = false;
				App.ShowDebugMsg("ChooseDestinationStation");

				this.Dispatcher.Invoke(new Action(() => {

					UserInfo.ShowInfo(InfoCode.TravelDateInfo, session.Language);

					DisplayInfo(_infoHeightNormal);
					DisplayMenu(_menuWidthNormal, session, TickSalesMenuItemCode.DepartDate);

					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					DatePage.InitData(session);
					ExecMenu.UnShieldMenu();
					frmWorkDetail.NavigationService.Navigate(DatePage);

					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000304)", ex), classNMethodName: "MainWindow.ChooseTravelDates");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000304)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseTravelDates;");
			}
		}

		public void ShowInitDepartTrip(UIDepartTripInitAck tripInit, UserSession session)
		{
			try
			{
				//_editSalesDetailFlag = false;
				App.ShowDebugMsg("MainWindow.ChooseDepartureTrip");

				this.Dispatcher.Invoke(new Action(() => {

					UserInfo.ShowInfo(InfoCode.DepartTripInfo, session.Language);

					DisplayInfo(_infoHeightNormal);
					DisplayMenu(_menuWidthNormal, session, TickSalesMenuItemCode.DepartOperator);

					//TripPage.
					//DatePage.InitData(session);

					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					TripPage.InitData(session);
					ExecMenu.UnShieldMenu();
					frmWorkDetail.NavigationService.Navigate(TripPage);
					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000308)", ex), classNMethodName: "MainWindow.ShowInitDepartTrip");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000308)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ShowInitDepartTrip;");
			}
		}

		public void UpdateDepartTripList(UIDepartTripListAck uiDepartTripList, UserSession session)
		{
			try
			{
				//_editSalesDetailFlag = false;
				App.ShowDebugMsg("MainWindow.UpdateDepartTripList");

				this.Dispatcher.Invoke(new Action(() => {
					TripPage.UpdateDepartTripList(uiDepartTripList, session);
					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000309)", ex), classNMethodName: "MainWindow.UpdateDepartTripList");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000309)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateDepartTripList;");
			}
		}

		public void UpdateDepartTripSubmitError(UIDepartTripSubmissionErrorAck uiTripSubErr, UserSession session)
		{
			try
			{
				Thread thrWorker = new Thread(new ThreadStart(new Action(() => {
					try
					{
						this.Dispatcher.Invoke(new Action(() => {
							TripPage.UpdateShieldErrorMessage(uiTripSubErr.ErrorMessage);
							System.Windows.Forms.Application.DoEvents();
							App.TimeoutManager.ResetTimeout();

							//Task.Delay(1000 * 10).Wait();
							//TripPage.ResetPageAfterError();
							//System.Windows.Forms.Application.DoEvents();
						}));
						//App.TimeoutManager.ResetTimeout();
					}
					catch (Exception ex)
					{
						App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000317)", ex), classNMethodName: "MainWindow.UpdateDepartTripSubmitError");
						Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000317)");
						App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateDepartTripSubmitError; (EXIT10000317)");
					}
				})));
				thrWorker.IsBackground = true;
				thrWorker.Start();
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000318)", ex), classNMethodName: "MainWindow.UpdateDepartTripSubmitError");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000318)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateDepartTripSubmitError; (EXIT10000318)");
			}

		}

		public void ChooseDepartSeat(UIDepartSeatListAck uiDepartSeatList, UserSession session)
		{
			try
			{
				//_editSalesDetailFlag = false;
				App.ShowDebugMsg("ChooseDestinationStation");

				this.Dispatcher.Invoke(new Action(() => {

					UserInfo.ShowInfo(InfoCode.DepartSeatInfo, session.Language);

					DisplayInfo(_infoHeightNormal);
					DisplayMenu(_menuWidthNormal, session, TickSalesMenuItemCode.DepartSeat);

					// DEBUG-Testing -----------------------------------------
					//_debugSeatPage.Debug_GetTestData(out TripMode tripMode, out int maxSeatPerTrip, out SeatDeckCollection[] seatDeckArray, out decimal defaultSeatPrice);
					//ISeat seatPage = (ISeat)_debugSeatPage;
					//seatPage.InitSeatData(TripMode.Depart, seatDeckArray, App.AppMaxSeatPerTrip, defaultSeatPrice, "RM", true, 0.40M);
					//frmWorkDetail.NavigationService.Navigate(SeatPage);
					// DEBUG-Testing -----------------------------------------
					//_debugSeatPage.Debug_GetTestData(out TripMode tripMode, out int maxSeatPerTrip, out SeatDeckCollection[] seatDeckArray, out decimal defaultSeatPrice);
					//ISeat seatPage = (ISeat)_debugSeatPage;

					//seat_status seatState = (seat_status)uiDepartSeatList.MessageData;
					//int maxSeat = (App.AppMaxSeatPerTrip > seatState.MaxSeatAllow) ? (int)seatState.MaxSeatAllow : App.AppMaxSeatPerTrip;
					//decimal defaultTicketPrice = seatState.adultprice + seatState.insurance + seatState.terminalcharge + seatState.onlineqrcharge;
					//string currencyCode = string.IsNullOrWhiteSpace(session.DepartCurrency) ? "RM" : session.DepartCurrency.Trim();

					//seatPage.InitSeatData(TripMode.Depart, seatDeckArray, maxSeat, defaultSeatPrice, currencyCode, true, seatState.insurance);
					//frmWorkDetail.NavigationService.Navigate(SeatPage);
					// ---------------------------------------------------
					//Actual Codes
					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					SeatPage.InitDepartSeatData(uiDepartSeatList, session);
					ExecMenu.UnShieldMenu();
					frmWorkDetail.NavigationService.Navigate(SeatPage);
					// ---------------------------------------------------

					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000304)", ex), classNMethodName: "MainWindow.ChooseDepartSeat");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000304)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseDepartSeat;");
			}
		}

		public void ChoosePickupNDrop(UIDepartPickupNDropAck uiIDepartPickupNDrop)
		{
			try
			{
				//_editSalesDetailFlag = false;
				App.ShowDebugMsg("ChoosePickupNDrop");

				this.Dispatcher.Invoke(new Action(() => {

					UserInfo.ShowInfo(InfoCode.DepartPickupNDrop, uiIDepartPickupNDrop.Session.Language);

					DisplayInfo(_infoHeightNormal);
					DisplayMenu(_menuWidthNormal, uiIDepartPickupNDrop.Session, TickSalesMenuItemCode.DepartSeat);

					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					PickupNDropPage.InitPickupDropData(uiIDepartPickupNDrop);
					ExecMenu.UnShieldMenu();
					frmWorkDetail.NavigationService.Navigate(PickupNDropPage);
					// ---------------------------------------------------

					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000304)", ex), classNMethodName: "MainWindow.ChoosePickupNDrop");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000304)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChoosePickupNDrop;");
			}
		}

		public void ChooseInsurance(UIInsuranceAck uiInsurnace)
		{
			try
			{
				App.ShowDebugMsg("Insurance Page");

				this.Dispatcher.Invoke(new Action(() =>
				{
					UserInfo.ShowInfo("Your Selected Ticket");

					DisplayInfo(_infoHeightNormal);
					DisplayMenu(_menuWidthNormal, uiInsurnace.Session, TickSalesMenuItemCode.DepartSeat);

					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					ExecMenu.UnShieldMenu();
					InsurancePage.InitInsurance(uiInsurnace);
					frmWorkDetail.NavigationService.Navigate(InsurancePage);
					// ---------------------------------------------------

					System.Windows.Forms.Application.DoEvents();
				}));

				/////-----------------------------------------------------------------------------------------------

			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000321)", ex), classNMethodName: "MainWindow.ChooseInsurance");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000321)");
				App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT10000321); At MainWindow.ChooseInsurance;");
			}
		}

		public void EnterPassengerInfo(UICustInfoAck uiCustInfo)
		{
			try
			{
				App.ShowDebugMsg("EnterPassengerInfo");

				this.Dispatcher.Invoke(new Action(() =>
				{
					UserInfo.ShowInfo(InfoCode.PassengerInfo, uiCustInfo.Session.Language);

					DisplayInfo(_infoHeightNormal);
					DisplayMenu(_menuWidthNormal, uiCustInfo.Session, TickSalesMenuItemCode.Passenger);

					//CustomerInfoList custInfoList = (CustomerInfoList)uiCustInfo.MessageData;
					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					PassengerInfoPage.InitPassengerInfo(uiCustInfo);
					ExecMenu.UnShieldMenu();
					frmWorkDetail.NavigationService.Navigate(PassengerInfoPage);
					// ---------------------------------------------------

					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000310)", ex), classNMethodName: "MainWindow.EnterPassengerInfo");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000310)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.EnterPassengerInfo;");
			}
		}

		public void MakeTicketPayment(UISalesPaymentProceedAck uiSalesPayment)
		{
			string _transactionNo = null;

			try
			{
				App.SetCurrentSaleTransactionToTicketing();

				App.ShowDebugMsg("MakeTicketPayment");

				_transactionNo = uiSalesPayment.Session.DepartSeatConfirmTransNo;

				//if (App.SysParam.PrmNoPaymentNeed)
				//{
				//	App.ShowDebugMsg($@"Received UIMakeSalesPaymentAck but PrmNoPaymentNeed set to true");

				//	this.Dispatcher.Invoke(new Action(() =>
				//	{
				//		HideInfo();
				//		DisplayMenu(_menuWidthPayment, uiSalesPayment.Session, TickSalesMenuItemCode.Payment);
				//		ExecMenu.UnShieldMenu();

				//		// ---------------------------------------------------
				//		System.Windows.Forms.Application.DoEvents();

				//		MessageBox.Show("Demo -- Payment Done");
				//	}));

				//	ShowWelcome();
				//}
				//else
				//{
				//	if (App.SysParam.PrmNoPaymentNeed)
				//	{
				//		//By Pass
				//	}
				//	else
				{
					Thread machChkThreadWorker = new Thread(new ThreadStart(new Action(() =>
					{
						try
						{
							if ((App.CheckIsPaymentTypeAvailable(AppDecorator.Common.AppService.Sales.PaymentType.Cash)) && (App.SysParam.PrmNoPaymentNeed == false))
							{
								var testToBypass = App.NetClientSvc.CashPaymentService.CheckCashMachineIsReady("-", out bool isLowCoin, out bool isCoinMachRecoveryInProgressAfterDispense, out string errorMsg, 20);


                                if (!testToBypass)
								{
										//By Pass
								}
								else
								{
									string errMsg = "Cash Machine not ready";

									if (isCoinMachRecoveryInProgressAfterDispense)
										errMsg += "; Coin machine under a short maintenance. This may take 15 seconds to 3 minutes";
									if (isLowCoin)
										errMsg += "; Low coin encountered";
									if (string.IsNullOrEmpty(errorMsg) == false)
										errMsg += $@"; {errorMsg}";

									throw new Exception($@"{errMsg}; (EXIT10000319)");
								}
							}

							this.Dispatcher.Invoke(new Action(() =>
							{
								HideInfo();
								DisplayMenu(_menuWidthPayment, uiSalesPayment.Session, TickSalesMenuItemCode.Payment);

								frmWorkDetail.Content = null;
								frmWorkDetail.NavigationService.RemoveBackEntry();
								PaymentPage.InitPayment(uiSalesPayment.Session);
								ExecMenu.UnShieldMenu();
								frmWorkDetail.NavigationService.Navigate(PaymentPage);
									// ---------------------------------------------------

									System.Windows.Forms.Application.DoEvents();
							}));
						}
						catch (Exception ex)
						{
							Exception err2 = new Exception($@"{ex.Message};(EXIT10000322);", ex);
							ReleaseTicketOnError(err2);
						}
					})));

					machChkThreadWorker.IsBackground = true;
					machChkThreadWorker.Start();
				}
				//}
			}
			catch (Exception ex)
			{
				Exception err2 = new Exception($@"{ex.Message}; (EXIT10000311);", ex);
				ReleaseTicketOnError(err2);
			}

			void ReleaseTicketOnError(Exception ex5)
			{
				string errEx = "";

				if (string.IsNullOrWhiteSpace(_transactionNo) == false)
				{
					try
					{
						App.NetClientSvc.SalesService.RequestSeatRelease(_transactionNo);
					}
					catch (Exception ex2)
					{
						errEx = ex2.Message;
					}
					finally
					{
						if (string.IsNullOrWhiteSpace(errEx) == false)
							errEx = $@"Error When Request for Seat Release; {errEx}";
					}
				}

				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex5.Message}; {errEx};", ex5), "EX01", classNMethodName: "MainWindow.MakeTicketPayment->ReleaseTicketOnError");
				Alert(detailMsg: $@"Error: {ex5.Message}; {errEx}; ");
				App.ShowDebugMsg($@"Error: {ex5.Message}; {errEx}; At MainWindow.ReleaseTicketOnError;");
			}
		}

		public void UpdateTransactionCompleteStatus(UICompleteTransactionResult uiCompltResult)
		{
			try
			{
				//_editSalesDetailFlag = false;
				App.ShowDebugMsg("MainWindow.UpdateTransactionCompleteStatus");

				this.Dispatcher.Invoke(new Action(() => {
					DisplayMenu(_menuWidthPayment, uiCompltResult.Session, TickSalesMenuItemCode.AfterPayment);
					PaymentPage.UpdateTransCompleteStatus(uiCompltResult);
				}));

			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000309)", ex), classNMethodName: "MainWindow.UpdateTransactionCompleteStatus");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000309)");
				App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT10000309); At MainWindow.UpdateTransactionCompleteStatus;");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.0250
		/// </summary>
		/// <param name="kioskMsg"></param>
		public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
		{
			try
			{
				App.ShowDebugMsg("MainWindow.BTnGShowPaymentInfo");

				if (App.CurrentSaleTransactionModule == AppModule.UIKioskSales)
                {
					this.Dispatcher.Invoke(new Action(() => {
						PaymentPage.BTnGShowPaymentInfo(kioskMsg);
					}));
				}
				else if (App.CurrentSaleTransactionModule == AppModule.UICollectTicket)
				{
					this.Dispatcher.Invoke(new Action(() => {
						CTPayment.BTnGShowPaymentInfo(kioskMsg);
					}));
				}
				else
                {
					throw new Exception($@"Unrecognized Payment module ({App.CurrentSaleTransactionModule.ToString()}) specification;");
                }
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT80.0250.EX01)", ex), classNMethodName: "MainWindow.BTnGShowPaymentInfo");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT80.0250.EX01)");
				App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT80.0250.EX01); At MainWindow.BTnGShowPaymentInfo;");
			}
		}

		#region - Collect Ticket -

		/// <summary>
		/// FuncCode:EXIT80.0251
		/// </summary>
		public void CTChooseBusCompany(IKioskMsg kioskMsg)
        {
			try
			{
				App.Log.LogText(LogChannel, "-", "===== Select Bus Company for Boarding Pass Collection =====", "A01", classNMethodName: "MainWindow.CTChooseBusCompany");

				this.Dispatcher.Invoke(new Action(() => {
					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					System.Windows.Forms.Application.DoEvents();
					CTBusCompanyPage.InitData(kioskMsg);
					frmWorkDetail.NavigationService.Navigate(CTBusCompanyPage);
					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (ThreadAbortException) { }
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT80.0251.EX01)", ex), "EX01", classNMethodName: "MainWindow.CTChooseBusCompany");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT80.0251.EX01)");
				App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT80.0251.EX01); At MainWindow.CTChooseBusCompany;");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.0252
		/// </summary>
		public void CTChooseDepartureDate(IKioskMsg kioskMsg)
		{
			try
			{
				App.Log.LogText(LogChannel, "-", "===== Select Departure Date for Boarding Pass Collection =====", "A01", classNMethodName: "MainWindow.CTChooseDepartureDate");

				this.Dispatcher.Invoke(new Action(() => {
					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					System.Windows.Forms.Application.DoEvents();
					CTBoardingDatePage.InitData(kioskMsg);
					frmWorkDetail.NavigationService.Navigate(CTBoardingDatePage);
					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (ThreadAbortException) { }
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT80.0252.EX01)", ex), "EX01", classNMethodName: "MainWindow.CTChooseDepartureDate");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT80.0252.EX01)");
				App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT80.0252.EX01); At MainWindow.CTChooseDepartureDate;");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.0253
		/// </summary>
		public void CTEnterTicketNo(IKioskMsg kioskMsg)
		{
			try
			{
				App.Log.LogText(LogChannel, "-", "===== Enter Ticket No. for Boarding Pass Collection =====", "A01", classNMethodName: "MainWindow.CTEnterTicketNo");

				this.Dispatcher.Invoke(new Action(() => {
					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					System.Windows.Forms.Application.DoEvents();
					CTTicketNoPage.InitData(kioskMsg);
					frmWorkDetail.NavigationService.Navigate(CTTicketNoPage);
					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (ThreadAbortException) { }
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT80.0253.EX01)", ex), "EX01", classNMethodName: "MainWindow.CTEnterTicketNo");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT80.0253.EX01)");
				App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT80.0253.EX01); At MainWindow.CTEnterTicketNo;");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.0254
		/// </summary>
		public void CTShowTicketNumberNotFound(IKioskMsg kioskMsg)
		{
			try
			{
				App.Log.LogText(LogChannel, "-", "===== Show Ticket No. not found for Boarding Pass Collection =====", "A01", classNMethodName: "MainWindow.CTShowTicketNumberNotFound");

				this.Dispatcher.Invoke(new Action(() => {
					if (frmWorkDetail.Content is ICTTicketNo tickNoPage)
					{
						tickNoPage.ShowTicketNumberNotFound(kioskMsg);
						System.Windows.Forms.Application.DoEvents();
					}
				}));
			}
			catch (ThreadAbortException) { }
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT80.0254.EX01)", ex), "EX01", classNMethodName: "MainWindow.CTShowTicketNumberNotFound");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT80.0254.EX01)");
				App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT80.0254.EX01); At MainWindow.CTShowTicketNumberNotFound;");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.0255
		/// </summary>
		public void CTEnterPassengerInfo(IKioskMsg kioskMsg)
		{
			try
			{
				App.Log.LogText(LogChannel, "-", "===== Enter Passenger Info for Boarding Pass Collection =====", "A01", classNMethodName: "MainWindow.CTEnterTicketNo");

				this.Dispatcher.Invoke(new Action(() => {
					frmWorkDetail.Content = null;
					frmWorkDetail.NavigationService.RemoveBackEntry();
					System.Windows.Forms.Application.DoEvents();
					CTPassengerInfoPage.InitData(kioskMsg);
					frmWorkDetail.NavigationService.Navigate(CTPassengerInfoPage);
					System.Windows.Forms.Application.DoEvents();
				}));
			}
			catch (ThreadAbortException) { }
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT80.0255.EX01)", ex), "EX01", classNMethodName: "MainWindow.CTEnterPassengerInfo");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT80.0255.EX01)");
				App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT80.0255.EX01); At MainWindow.CTEnterPassengerInfo;");
			}
		}

		public void CTMakeTicketPayment(IKioskMsg kioskMsg)
		{
			try
			{
				App.SetCurrentSaleTransactionToCollectTicket();

				App.ShowDebugMsg("Collect Ticket - Make Payment");

				Thread machChkThreadWorker = new Thread(new ThreadStart(new Action(() =>
				{
					try
					{
						if ((App.CheckIsPaymentTypeAvailable(AppDecorator.Common.AppService.Sales.PaymentType.Cash)) && (App.SysParam.PrmNoPaymentNeed == false))
						{
							if (App.NetClientSvc.CashPaymentService.CheckCashMachineIsReady("-", out bool isLowCoin, out bool isCoinMachRecoveryInProgressAfterDispense, out string errorMsg, 20))
							{
								//By Pass
							}
							else
							{
								string errMsg = "Cash Machine not ready";

								if (isCoinMachRecoveryInProgressAfterDispense)
									errMsg += "; Coin machine under a short maintenance. This may take 15 seconds to 3 minutes";
								if (isLowCoin)
									errMsg += "; Low coin encountered";
								if (string.IsNullOrEmpty(errorMsg) == false)
									errMsg += $@"; {errorMsg}";

								throw new Exception($@"{errMsg}; (EXIT10000319)");
							}
						}

						this.Dispatcher.Invoke(new Action(() =>
						{
							frmWorkDetail.Content = null;
							frmWorkDetail.NavigationService.RemoveBackEntry();
							System.Windows.Forms.Application.DoEvents();
							CTPayment.InitData(kioskMsg);
							frmWorkDetail.NavigationService.Navigate(CTPayment);
							System.Windows.Forms.Application.DoEvents();
						}));
					}
					catch (Exception ex)
					{
						Exception err2 = new Exception($@"{ex.Message};(EXIT10000323);", ex);
					}
				})));

				machChkThreadWorker.IsBackground = true;
				machChkThreadWorker.Start();
			}
			catch (Exception ex)
			{
				Exception err2 = new Exception($@"{ex.Message}; (EXIT10000311);", ex);
			}
		}

		#endregion - End - Collect Ticket -

		public void Alert(string malayShortMsg = "TIDAK BERFUNGSI", string engShortMsg = "OUT OF ORDER", string detailMsg = "")
		{
			try
			{
				App.CollectBoardingPassCountDown.ForceResetCounter();

				AlertPage.ShowAlertMessage(malayShortMsg, engShortMsg, detailMsg);
				App.ShowDebugMsg($@"Show Alert Page..");

				this.Dispatcher.Invoke(new Action(() => {
					if (frmWorkDetail.Content.GetType().IsEquivalentTo(AlertPage.GetType()) == false)
					{
						HideInfo();
						HideMenu();
						frmWorkDetail.Content = null;
						frmWorkDetail.NavigationService.RemoveBackEntry();
						System.Windows.Forms.Application.DoEvents();
						frmWorkDetail.NavigationService.Navigate(AlertPage);
					}
				}));

				//_editSalesDetailFlag = false;

				System.Windows.Forms.Application.DoEvents();
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, classNMethodName: "MainWindow.StartSelling");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.Alert;");
			}
		}

		private void DisplayMenu(double menuWidth, UserSession session, TickSalesMenuItemCode currentEditItemCode)
		{
			try
			{
				if (menuWidth < 0)
					menuWidth = _menuWidthNormal;

				
				this.Dispatcher.Invoke(new Action(() => 
				{
					if (frmMenu.Content == null)
					{
						frmMenu.NavigationService.Navigate(ExecMenu);
						System.Windows.Forms.Application.DoEvents();
					}

					if (session != null)
					{
						ExecMenu.SetLanguage(session.Language);
						ExecMenu.ActivateEditMenuItem(currentEditItemCode);

						// Set all Menu info here
						if (currentEditItemCode != TickSalesMenuItemCode.FromStation)
						{
							if (string.IsNullOrWhiteSpace(session.OriginStationCode) == false)
							{
								ExecMenu.SetFromStationData(session.OriginStationName);
								ExecMenu.ShowFromStationData();

								if ((App.SysParam.PrmAppGroup == AppGroup.Larkin) || (App.SysParam.PrmAppGroup == AppGroup.Klang))
									ExecMenu.IsEditAllowedFromStation = true;
								else
									ExecMenu.IsEditAllowedFromStation = false;
							}
							else
							{
								ExecMenu.ResetToStation();
							}
						}

						if (currentEditItemCode != TickSalesMenuItemCode.ToStation)
						{
							if (string.IsNullOrWhiteSpace(session.DestinationStationCode) == false)
							{
								ExecMenu.SetToStationData(session.DestinationStationName);
								ExecMenu.ShowToStationData();
								ExecMenu.IsEditAllowedToStation = true;
							}
							else
							{
								ExecMenu.ResetToStation();
							}
						}

						if (currentEditItemCode != TickSalesMenuItemCode.DepartDate)
						{
							if (session.DepartPassengerDate.HasValue)
							{
								if (string.IsNullOrWhiteSpace(session.DepartPassengerDepartTime) == false)
								{
									string tickDateTime = $@"{session.DepartPassengerDate.Value.ToString("dd-MM-yyyy")}_{session.DepartPassengerDepartTime}";
									DateTime result = DateTime.ParseExact(tickDateTime, "dd-MM-yyyy_HHmmss", _provider);
									ExecMenu.SetDepartDateData(result.ToString("dd MMM yyyy hh:mm tt"));
								}
								else
									ExecMenu.SetDepartDateData(session.DepartPassengerDate.Value.ToString("dd MMM yyyy"));

								ExecMenu.ShowDepartDateData();
								ExecMenu.IsEditAllowedDepartDate = true;
							}
							else
							{
								ExecMenu.ResetDepartDate();
							}
						}

						if (currentEditItemCode != TickSalesMenuItemCode.DepartOperator)
						{
							if (string.IsNullOrWhiteSpace(session.DepartCompanyDesc) == false)
							{
								ExecMenu.SetDepartOperatorData(session.DepartCompanyDesc);
								ExecMenu.ShowDepartOperatorData();
								ExecMenu.IsEditAllowedDepartOperator = true;
							}
							else
							{
								ExecMenu.ResetDepartOperator();
							}
						}

						if (currentEditItemCode != TickSalesMenuItemCode.DepartSeat)
						{
							if (session.PassengerSeatDetailList?.Length > 0)
							{
								string seatStr = "";
								foreach(CustSeatDetail seat in session.PassengerSeatDetailList)
								{
									if (string.IsNullOrWhiteSpace(seat.Desn) == false)
										seatStr += $@",{seat.Desn}";
								}

								if (seatStr.Length > 0)
								{
									seatStr = seatStr.Substring(1);
									ExecMenu.SetDepartSeatData(seatStr);
									ExecMenu.ShowDepartSeatData();
									ExecMenu.IsEditAllowedDepartSeat = true;
								}
								else
								{
									ExecMenu.ResetDepartSeat();
								}
							}
							else
							{
								ExecMenu.ResetDepartSeat();
							}
						}

						if (currentEditItemCode != TickSalesMenuItemCode.Passenger)
						{
							ExecMenu.ResetPassenger();

							if (session.PassengerSeatDetailList?.Length > 0)
							{
								bool gotCustInfo = false;

								foreach (CustSeatDetail seat in session.PassengerSeatDetailList)
								{
									if (string.IsNullOrWhiteSpace(seat.CustName) == false)
									{
										gotCustInfo = true;
										ExecMenu.AddPassengerData(seat.CustName, seat.Contact);
									}
								}
								if (gotCustInfo)
								{
									ExecMenu.ShowPassengerData();
									ExecMenu.IsEditAllowedPassenger = false;
								}
							}
						}

						if (currentEditItemCode != TickSalesMenuItemCode.Payment)
						{
							if ((string.IsNullOrWhiteSpace(session.PaymentTypeDesc) == false) && (session.TypeOfPayment != PaymentType.Unknown))
							{
								if (session.Language == LanguageCode.Malay)
								{
									if (session.TypeOfPayment == PaymentType.CreditCard)
										ExecMenu.SetPaymentData("KAD KREDIT/DEBIT");

									else if (session.TypeOfPayment == PaymentType.PaymentGateway)
										ExecMenu.SetPaymentData("e-DOMPET");

									else
										ExecMenu.SetPaymentData("TUNAI");
								}
								else
									ExecMenu.SetPaymentData(session.PaymentTypeDesc.ToUpper());

								ExecMenu.ShowPaymentData();
								ExecMenu.IsEditAllowedPayment = false;
							}
							else
							{
								ExecMenu.ResetPayment();
							}
						}

						if ((currentEditItemCode == TickSalesMenuItemCode.Payment) || (currentEditItemCode == TickSalesMenuItemCode.AfterPayment))
						{
							ExecMenu.HideMiniNavigator();
							ExecMenu.IsEditAllowedFromStation = false;
							ExecMenu.IsEditAllowedToStation = false;
							ExecMenu.IsEditAllowedDepartDate = false;
							ExecMenu.IsEditAllowedDepartOperator = false;
							ExecMenu.IsEditAllowedDepartSeat = false;
							ExecMenu.IsEditAllowedPassenger = false;
						}
						else
						{
							ExecMenu.ShowMiniNavigator();
						}
						//--------------------------------------------------
					}
				}));
				
				if (((Page)ExecMenu).ActualWidth != menuWidth)
				{
					((Page)ExecMenu).Width = menuWidth;
				}
			}
			catch (ThreadAbortException) { }
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000314)", ex), classNMethodName: "MainWindow.ShowWelcome");
				Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000314)");
				App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.DisplayInfo;");
			}
		}

		public void UpdateDepartDate(DateTime newDepartDate)
		{
			ExecMenu.SetDepartDateData(newDepartDate.ToString("dd MMM yyyy"));
			ExecMenu.ShowDepartDateData();
			ExecMenu.IsEditAllowedDepartDate = true;
		}

		public bool QueryWindowSize(out double windowWidth, out double windowHeight)
        {
			windowWidth = 0;
			windowHeight = 0;

			try
			{
				double windowWidthX = 0, windowHeightX = 0;

				this.Dispatcher.Invoke(new Action(() => 
				{
					windowWidthX = this.ActualWidth;
					windowHeightX = this.ActualHeight;
				}));

				windowWidth = windowWidthX;
				windowHeight = windowHeightX;

				return true;
			}
			catch { }

			return false;
		}
	}
}
