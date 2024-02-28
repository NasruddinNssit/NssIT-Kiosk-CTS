using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
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

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    /// <summary>
    /// Interaction logic for pgTrip.xaml
    /// </summary>
    public partial class pgTrip : Page, ITrip, IKioskViewPage
	{
		private string _logChannel = "ViewPage";

		private LanguageCode _language = LanguageCode.English;

		private string _departureStationDesc = null;
		private string _destinationeStationDesc = null;
		private DateTime _selectedDay = DateTime.MinValue;

		private double _designHeight = 710D;
		private double _designWidth = 750D;
		private bool _pageLoaded = false;

		private DayCellSelector _daySelection = null;
		private TripMode _tripMode = TripMode.Depart;
		private string _fromStationCode = null;
		private string _toStationCode = null;

		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private (double OrgTimeColWidth, double OrgFareColWidth, double OrgSeatColWidth, double OrgTripDetailColWidth, double OrgPickingColWidth) _lstTripViewDefault;

		private TripDetailViewList _tripDetailList = new TripDetailViewList();
		private TripDetailViewListHelper _tripListHelper = null;
		//private TripDetailEnt[] _tripDataList = null;

		private ITimeFilter _timeFilterPage = null;

		public pgTrip()
		{
			InitializeComponent();

			try
			{
				_langMal = CommonFunc.GetXamlResource(@"ViewPage\Trip\rosTripMalay.xaml");
				_langEng = CommonFunc.GetXamlResource(@"ViewPage\Trip\rosTripEnglish.xaml");

				DateTime today = DateTime.Now;
				_selectedDay = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, 0);

				LstTrip.DataContext = _tripDetailList;
				_tripListHelper = new TripDetailViewListHelper(this, LstTrip, _tripDetailList, PgbTripLoading);
				_tripListHelper.OnBeginQueryNewTripData += _tripListingHelper_OnBeginQueryNewTripData;
				_tripListHelper.OnUpdateTripViewInProgress += _tripListingHelper_OnUpdateTripViewInProgress;

				GridViewColumn gvc;
				gvc = (GridViewColumn)this.FindName("GvcTimeCol");
				if (gvc != null)
					_lstTripViewDefault.OrgTimeColWidth = gvc.Width;

				gvc = (GridViewColumn)this.FindName("GvcFareCol");
				if (gvc != null)
					_lstTripViewDefault.OrgFareColWidth = gvc.Width;

				gvc = (GridViewColumn)this.FindName("GvcSeatCol");
				if (gvc != null)
					_lstTripViewDefault.OrgSeatColWidth = gvc.Width;

				gvc = (GridViewColumn)this.FindName("GvcTripDetailCol");
				if (gvc != null)
					_lstTripViewDefault.OrgTripDetailColWidth = gvc.Width;

				gvc = (GridViewColumn)this.FindName("GvcPickingCol");
				if (gvc != null)
					_lstTripViewDefault.OrgPickingColWidth = gvc.Width;
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000471); pgTrip.pgTrip");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000471)", ex), "EX01", classNMethodName: "pgTrip.Page_Unloaded");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000471)");
			}
		}

		private bool _isInitTimeFilterPage = false;
		public void InitTimeFilterPage(ITimeFilter timeFilterPage)
		{
			if (_isInitTimeFilterPage == false)
			{
				_isInitTimeFilterPage = true;
				_timeFilterPage = timeFilterPage;
				_timeFilterPage.OnTimeFilterChangedTrigger += _timeFilterPage_OnTimeFilterChangedTrigger;
			}
			
		}

		private void _timeFilterPage_OnTimeFilterChangedTrigger(object sender, TimeFilterEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();

				_tripListHelper.FilterListByTime(e.StartTime, e.EndTime);
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000461); pgTrip._timeFilterPage_OnTimeFilterChangedTrigger");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000461)", ex), "EX01", classNMethodName: "pgTrip._timeFilterPage_OnTimeFilterChangedTrigger");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000461)");
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				_timeFilterPage.IsFilterActived = true;
				//App.MainScreenControl.ExecMenu.IsTimeFilterVisibled = true;

				System.Windows.Forms.Application.DoEvents();

				_timeFilterPage.ResetFilter();

				_tripSelected = false;

				TxtScreenShield1.Visibility = Visibility.Visible;
				TxtScreenShield2.Visibility = Visibility.Collapsed;
				BdError.Visibility = Visibility.Collapsed;
				TxtScreenShield2.Text = "";
				TxtErrorCode.Visibility = TxtScreenShield2.Visibility;
				TxtErrorCode.Text = "";

				GrdScreenShield.Visibility = Visibility.Collapsed;

				this.Resources.MergedDictionaries.Clear();

				if (_language == LanguageCode.Malay)
					this.Resources.MergedDictionaries.Add(_langMal);
				else
					this.Resources.MergedDictionaries.Add(_langEng);

				LstTrip.SelectedIndex = -1;
				LstTrip.SelectedItem = null;

				_tripListHelper.PageLoaded();
				InitShowTrips();
				_pageLoaded = true;
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000472); pgTrip.Page_Loaded");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000472)", ex), "EX01", classNMethodName: "pgTrip.Page_Loaded");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000472)");
			}
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			try
			{
				_pageLoaded = false;
				_timeFilterPage.IsFilterActived = false;
				App.MainScreenControl.ExecMenu.IsTimeFilterVisibled = false;

				_tripListHelper.PageUnLoaded();
				_tripListHelper.ClearTripLoading();
				App.ShowDebugMsg("pgTrip Page_Unloaded");
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000473); pgTrip.Page_Unloaded");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000473)", ex), "EX01", classNMethodName: "pgTrip.Page_Unloaded");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000473)");
			}
		}

		private void _tripListingHelper_OnBeginQueryNewTripData(object sender, EventArgs e)
		{
			try
			{
				App.MainScreenControl.ExecMenu.ShieldMenu();

				BdTripViewerAllShield.Visibility = Visibility.Visible;

				if (_language == LanguageCode.Malay)
					TxtShieldAllCoverMsg.Text = _langMal["LOADING_IN_PROGRESS_Label"]?.ToString();
				else
					TxtShieldAllCoverMsg.Text = _langEng["LOADING_IN_PROGRESS_Label"]?.ToString();

				TxtShieldCoverMsg.Text = "---- ..";
				BdTripViewerShield.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000474); pgTrip._tripListingHelper_OnBeginQueryNewTripData");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000474)", ex), "EX01", classNMethodName: "pgTrip._tripListingHelper_OnBeginQueryNewTripData");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000474)");
			}
		}

		private void _tripListingHelper_OnUpdateTripViewInProgress(object sender, EventArgs e)
		{
			try
			{
				App.MainScreenControl.ExecMenu.UnShieldMenu();

				if (App.MainScreenControl.ExecMenu.IsTimeFilterVisibled == false)
					App.MainScreenControl.ExecMenu.IsTimeFilterVisibled = true;

				App.TimeoutManager.ResetTimeout();

				BdTripViewerAllShield.Visibility = Visibility.Collapsed;

				TxtShieldCoverMsg.Text = "----- ..";
				BdTripViewerShield.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000475); pgTrip._tripListingHelper_OnUpdateTripViewInProgress");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000475)", ex), "EX01", classNMethodName: "pgTrip._tripListingHelper_OnUpdateTripViewInProgress");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000475)");
			}
		}

		public DateTime SelectedDay => _selectedDay;
		public string SelectedTripId { get; private set; } = null;
		public TripMode TripMode => _tripMode;

		private DayCellSelector DaySelectionHandle
		{
			get
			{
				if (_daySelection is null)
				{
					try
					{
						_daySelection = new DayCellSelector(this, cvDaysFrame, ScvDayCalendarContainer, LstTrip);
						_daySelection.OnDateSelected += _daySelection_OnDateSelected;
						_daySelection.OnBeginDayCellMoveAnimate += _daySelection_OnBeginDayCellMoveAnimate;
						_daySelection.OnBeginDayCellTouchMove += _daySelection_OnBeginDayCellTouchMove;
					}
					catch (Exception ex)
					{
						App.ShowDebugMsg($@"{ex.Message}; (EXIT10000476); pgTrip.DaySelectionHandle");
						App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000476)", ex), "EX01", classNMethodName: "pgTrip.DaySelectionHandle");
						App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000476)");
					}
				}
				return _daySelection;
			}
		}

		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			try
			{
				ResizeTripList();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000477); pgTrip.Page_SizeChanged");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000477)", ex), "EX01", classNMethodName: "pgTrip.Page_SizeChanged");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000477)");
			}
		}

		private void _daySelection_OnDateSelected(object sender, DateSelectedEventArgs e)
		{
			try
			{
				_selectedDay = e.SelectedDate;

				App.MainScreenControl.UpdateDepartDate(_selectedDay);

				//ScvDayCalendarContainer.IsEnabled = false;
				LoadTripData();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000478); pgTrip._daySelection_OnDateSelected");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000478)", ex), "EX01", classNMethodName: "pgTrip._daySelection_OnDateSelected");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000478)");
			}
		}

		private void _daySelection_OnBeginDayCellTouchMove(object sender, EventArgs e)
		{
			try
			{
				App.MainScreenControl.ExecMenu.ShieldMenu();

				BdTripViewerAllShield.Visibility = Visibility.Collapsed;

				_tripListHelper.ClearTripLoading();

				if (_language == LanguageCode.Malay)
				{
					if (TripMode == TripMode.Depart)
						TxtShieldCoverMsg.Text = _langMal["SELECT_DEPART_DATE_Label"]?.ToString();
					else if (TripMode == TripMode.Return)
						TxtShieldCoverMsg.Text = _langMal["SELECT_RETURN_DATE_Label"]?.ToString();
					else
						TxtShieldCoverMsg.Text = _langMal["SELECT_DATE_Label"]?.ToString();
				}
				else
				{
					if (TripMode == TripMode.Depart)
						TxtShieldCoverMsg.Text = _langEng["SELECT_DEPART_DATE_Label"]?.ToString();
					else if (TripMode == TripMode.Return)
						TxtShieldCoverMsg.Text = _langEng["SELECT_RETURN_DATE_Label"]?.ToString();
					else
						TxtShieldCoverMsg.Text = _langEng["SELECT_DATE_Label"]?.ToString();
				}
				BdTripViewerShield.Visibility = Visibility.Visible;

			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000479); pgTrip._daySelection_OnBeginDayCellTouchMove");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000479)", ex), "EX01", classNMethodName: "pgTrip._daySelection_OnBeginDayCellTouchMove");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000479)");
			}
		}

		private void _daySelection_OnBeginDayCellMoveAnimate(object sender, EventArgs e)
		{
			App.MainScreenControl.ExecMenu.ShieldMenu();

			BdTripViewerAllShield.Visibility = Visibility.Visible;

			TxtShieldAllCoverMsg.Text = "Loading in progress ..";
			if (_language == LanguageCode.Malay)
				TxtShieldAllCoverMsg.Text = _langMal["LOADING_IN_PROGRESS_Label"]?.ToString();
			else
				TxtShieldAllCoverMsg.Text = _langEng["LOADING_IN_PROGRESS_Label"]?.ToString();

			TxtShieldCoverMsg.Text = "----- ..";
			BdTripViewerShield.Visibility = Visibility.Collapsed;
		}

		public void InitData(UserSession session)
		{
			try
			{
				// At thie moment (2020-03-15) Only implement Depart Travel
				_tripMode = (session.TravelMode == TravelMode.DepartOnly) ? TripMode.Depart : TripMode.Depart;

				_language = session.Language;
				_fromStationCode = session.OriginStationCode;
				_toStationCode = session.DestinationStationCode;
				_departureStationDesc = session.OriginStationName;
				_destinationeStationDesc = session.DestinationStationName;

				TxtDepartureStationDesc.Text = (string.IsNullOrWhiteSpace(_departureStationDesc) == false) ? _departureStationDesc : "--";
				TxtDestinationStationDesc.Text = (string.IsNullOrWhiteSpace(_destinationeStationDesc) == false) ? _destinationeStationDesc : "--";

				DayCellInfo.Language = _language;

				if (_language == LanguageCode.Malay)
				{
					_timeFilterPage?.InitFilter(_langMal);

					if (_tripMode == TripMode.Return)
						TxtTripMode.Text = _langMal["RETURN_TRIP_MODE_Label"]?.ToString();
					else
						TxtTripMode.Text = _langMal["DEPART_TRIP_MODE_Label"]?.ToString();
				}
				else
				{
					_timeFilterPage?.InitFilter(_langEng);

					if (_tripMode == TripMode.Return)
						TxtTripMode.Text = _langEng["RETURN_TRIP_MODE_Label"]?.ToString();
					else
						TxtTripMode.Text = _langEng["DEPART_TRIP_MODE_Label"]?.ToString();
				}

				_tripListHelper.InitTripData(_fromStationCode, _toStationCode, _language);

				if (session.DepartPassengerDate.HasValue)
					_selectedDay = new DateTime(session.DepartPassengerDate.Value.Year, session.DepartPassengerDate.Value.Month, session.DepartPassengerDate.Value.Day, 0, 0, 0, 0);
				else
				{
					DateTime currDate = DateTime.Now;
					_selectedDay = new DateTime(currDate.Year, currDate.Month, currDate.Day, 0, 0, 0, 0);
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000481); pgTrip.InitData");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000481)", ex), "EX01", classNMethodName: "pgTrip.InitData");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000481)");
			}
		}

		public void UpdateDepartTripList(UIDepartTripListAck uiDTrip, UserSession session)
		{
			_tripListHelper.UpdateDepartTripList(uiDTrip, session);
		}

		private void InitShowTrips()
		{
			//TxtShieldCoverMsg.Text = "Loading in progress ..";
			SelectedTripId = null;

			DaySelectionHandle.StartSelection(_selectedDay);
			ResizeTripList();

			if (_language == LanguageCode.Malay)
			{
				TxtShieldCoverMsg.Text = _langMal["LOADING_IN_PROGRESS_Label"]?.ToString();

				if (_tripMode == TripMode.Return)
					TxtTripMode.Text = _langMal["RETURN_TRIP_MODE_Label"]?.ToString();
				else
					TxtTripMode.Text = _langMal["DEPART_TRIP_MODE_Label"]?.ToString();
			}
			else
			{
				TxtShieldCoverMsg.Text = _langEng["LOADING_IN_PROGRESS_Label"]?.ToString();

				if (_tripMode == TripMode.Return)
					TxtTripMode.Text = _langEng["RETURN_TRIP_MODE_Label"]?.ToString();
				else
					TxtTripMode.Text = _langEng["DEPART_TRIP_MODE_Label"]?.ToString();
			}
			

			LoadTripData();
		}

		private void ResizeTripList()
		{
			if (_lstTripViewDefault.OrgTimeColWidth > 0)
			{
				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcTimeCol");
				if (gvc != null)
					gvc.Width = _lstTripViewDefault.OrgTimeColWidth;
			}
			if (_lstTripViewDefault.OrgFareColWidth > 0)
			{
				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcFareCol");
				if (gvc != null)
					gvc.Width = _lstTripViewDefault.OrgFareColWidth;
			}
			if (_lstTripViewDefault.OrgSeatColWidth > 0)
			{
				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcSeatCol");
				if (gvc != null)
					gvc.Width = _lstTripViewDefault.OrgSeatColWidth;
			}
			if ((_lstTripViewDefault.OrgTripDetailColWidth > 0) && (this.ActualWidth > 0))
			{
				double newWidth = (_lstTripViewDefault.OrgTripDetailColWidth / _designWidth) * this.ActualWidth;

				if (this.ActualWidth == _designWidth)
					newWidth = _lstTripViewDefault.OrgTripDetailColWidth;

				else if ((this.ActualWidth - _designWidth) > 0)
					newWidth = _lstTripViewDefault.OrgTripDetailColWidth + (this.ActualWidth - _designWidth);

				else
					newWidth = _lstTripViewDefault.OrgTripDetailColWidth + (_designWidth - this.ActualWidth);

				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcTripDetailCol");
				if (gvc != null)
					gvc.Width = newWidth;
			}
			if (_lstTripViewDefault.OrgPickingColWidth > 0)
			{
				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcPickingCol");
				if (gvc != null)
					gvc.Width = _lstTripViewDefault.OrgPickingColWidth;
			}
		}

		private int _debugInx = 0;
		private bool _tripSelected = false;
		private void BdPick_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (_tripSelected)
					return;

				if (sender is Border bd)
				{
					if (bd.Tag is string tagStr)
					{
						SelectTrip(tagStr);
					}
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000480); pgTrip.BdPick_MouseLeftButtonDown");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000480)", ex), "EX01", classNMethodName: "pgTrip.BdPick_MouseLeftButtonDown");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000480)");
			}
		}

		private void SelectTrip(string tripIdCode)
		{
			string id = GetTripId(tripIdCode);
			if (id != null)
			{
				App.MainScreenControl.ExecMenu.ShieldMenu();

				SelectedTripId = id;

				TripDetailViewRow row = _tripListHelper.GetItemRow(tripIdCode);

				App.ShowDebugMsg($@"Selected Trip Tag : {tripIdCode}; Trip ID : {SelectedTripId}");
				_tripListHelper.ClearTripLoading();

				string embed = row.Embed;
				if (App.SysParam.PrmIsDebugMode)
				{
					//	//if ((_debugInx % 2) == 0)
					//	//{

					/////// CYA-DEBUG - Test With Embed
					embed = "0";

					//	//}
					////	_debugInx++;
					////	if (_debugInx > 10)
					////		_debugInx = 0;
				}

				_tripSelected = true;
				Submit(_selectedDay, row.OrgTimeStr, row.Currency, row.OperatorDesc, row.OperatorImgPath, row.Price, row.TripId, row.TripDate, row.TripNo,
					row.TimePosi, row.TripDetail, embed, row.PassengerActualFromStationCode, row.PassengerActualToStationCode, row.Insurance, row.Skywayamount);
			}
		}

		private void Submit(DateTime departPassengerDate,
			string departPassengerDepartTime, string departCurrency,
			string departOperatorDesc, string departOperatorLogoUrl, decimal departPrice,
			string departTripId, string departVehicleTripDate,
			string departTripNo, short departTimePosi,
			string departRouteDetail, string departEmbed,
			string departPassengerActualFromStationCode, string departPassengerActualToStationCode,
			decimal departInsurance, decimal departSkyWayAmount)
		{

			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitDepartTrip(departPassengerDate, departPassengerDepartTime, departCurrency, departOperatorDesc, departOperatorLogoUrl, departPrice, departTripId, departVehicleTripDate, departTripNo,
								departTimePosi, departRouteDetail, departEmbed, departPassengerActualFromStationCode, departPassengerActualToStationCode, departInsurance,departSkyWayAmount, out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000482)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000483)");
					App.Log.LogError(_logChannel, "", new Exception("(EXIT10000483)", ex), "EX01", "pgTrip.Submit");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		private void LstTrip_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (_tripSelected)
					return;

				App.ShowDebugMsg($@"SelectedItem : {LstTrip.SelectedItem?.GetType().ToString()}; pgTrip.LstTrip_SelectionChanged");

				string tagStr = null;
				if (LstTrip.SelectedItem is TripDetailViewRow tripRow)
				{
					tagStr = tripRow.TripIdCode;
					SelectTrip(tagStr);
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000484); pgTrip.LstTrip_SelectionChanged");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000484)", ex), "EX01", classNMethodName: "pgTrip.LstTrip_SelectionChanged");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000484)");
			}
		}

		private void CloseError_Click(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (_pageLoaded == false)
					return;

				ResetPageAfterError();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000651); pgTrip.CloseError_Click");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000651)", ex), "EX01", classNMethodName: "pgTrip.CloseError_Click");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000484)");
			}
		}

		public void ShieldPage()
		{
			TxtScreenShield1.Visibility = Visibility.Visible;
			TxtScreenShield2.Visibility = Visibility.Collapsed;
			BdError.Visibility = Visibility.Collapsed;

			TxtErrorCode.Visibility = TxtScreenShield2.Visibility;

			GrdScreenShield.Visibility = Visibility.Visible;

		}

		public void UpdateShieldErrorMessage(string message)
		{
			TxtScreenShield1.Visibility = Visibility.Collapsed;
			TxtScreenShield2.Visibility = Visibility.Visible;
			TxtErrorCode.Visibility = TxtScreenShield2.Visibility;

			BdError.Visibility = Visibility.Visible;

			message = message ?? "";

			string errorCode = "";

			if (message.Contains("(EXIT21638)"))
				errorCode = "(EXIT21638)";

			else if (message.Contains("(EXIT21639)"))
				errorCode = "(EXIT21639)";

			if (_language == LanguageCode.Malay)
				TxtScreenShield2.Text = _langMal["SEAT_QUERY_ERROR_Label"]?.ToString();
			else
				TxtScreenShield2.Text = _langEng["SEAT_QUERY_ERROR_Label"]?.ToString();

			TxtErrorCode.Text = errorCode;

			_tripSelected = false;
			SelectedTripId = null;

			LstTrip.SelectedIndex = -1;
			LstTrip.SelectedItem = null;

		}

		public void ResetPageAfterError()
		{
			TxtScreenShield1.Visibility = Visibility.Visible;
			TxtScreenShield2.Visibility = Visibility.Collapsed;
			BdError.Visibility = Visibility.Collapsed;

			TxtScreenShield2.Text = "";
			TxtErrorCode.Visibility = TxtScreenShield2.Visibility;
			TxtErrorCode.Text = "";

			GrdScreenShield.Visibility = Visibility.Collapsed;

			App.MainScreenControl.ExecMenu.UnShieldMenu();
		}

		private string GetTripId(string tripIdCode)
		{
			if ((tripIdCode != null) && (tripIdCode.IndexOf(TripDetailViewListHelper.TripIdPrefix) == 0))
			{
				string retId = tripIdCode.Substring(TripDetailViewListHelper.TripIdPrefix.Length).Trim();
				if (retId.Length > 0)
					return retId;
				else
					return null;
			}
			else
				return null;
		}

		private void LoadTripData()
		{
			_tripListHelper.UpdateList(_selectedDay);
		}

		private void LstTrip_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstTrip_ScrollChanged");
			}
		}

        
    }
}
