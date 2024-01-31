using NssIT.Kiosk.Log.DB;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static NssIT.Kiosk.Client.ViewPage.KeyboardEventArgs;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.AppDecorator.Common;
using System.Threading;
using System.Windows.Media;
using System.Threading.Tasks;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command;

namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
    /// <summary>
    /// Interaction logic for pgStation.xaml
    /// </summary>
    public partial class pgStation : Page, IStation, IKioskViewPage
	{
		private string _logChannel = "ViewPage";

		private bool _selectedFlag = false;
		private DbLog Log { get; set; }
		private StateViewListHelper _stateViewListHelper = null;
		private StationViewListHelper _stationViewListHelper = null;

		private string _selectedState = null;

		private LanguageCode _language = LanguageCode.English;
		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private StationSelectionMode _stationMode = StationSelectionMode.OriginStation;

		public pgStation()
		{
			InitializeComponent();

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\StationTerminal\rosStationMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\StationTerminal\rosStationEng.xaml");

			Log = DbLog.GetDbLog();

			_stateViewListHelper = new StateViewListHelper(this, LstStateView);

			KbKeys.OnKeyPressed += KbKeys_OnKeyPressed;
			_stateViewListHelper.OnStateChanged += _stateViewListHelper_OnStateChanged;

			_stationViewListHelper = new StationViewListHelper(this, LstStationView);
		}

		
		private destination_detail[] _destDetailList = null;
		private destination_statedetail[] _destStateList = null;
		public void InitDestStationData(UIDestinationListAck uiDest)
		{
			_stationMode = StationSelectionMode.DestinationStation;
			destination_status dest = (destination_status)uiDest.MessageData;

			try
			{
				if (dest is null)
					throw new Exception("Fail to show Dest. Station List; (EXIT10000201)");

				else if ((dest.details is null) || (dest.details.Length == 0))
					throw new Exception("Fail to show Dest. Station List; (EXIT10000202)");

				else
				{
					_language = (uiDest.Session != null ? uiDest.Session.Language : LanguageCode.English);
					_stateViewListHelper.InitView(_language);

					App.ShowDebugMsg($@"pgStation ==> destination_status => statedetails : {dest.statedetails.Length}; details: {dest.details.Length}");
					_destDetailList = dest.details;
					_destStateList = dest.statedetails;					
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg(ex.Message);
				Log.LogText(_logChannel, "-", uiDest, "EX01", "pgStation.InitDestStationData", AppDecorator.Log.MessageType.Fatal, extraMsg: "MsgObj: UIDestinationList");
				Log.LogFatal(_logChannel, "-", ex, "EX02", "pgStation.InitDestStationData");
				App.MainScreenControl.Alert(detailMsg: ex.Message);
			}
		}

		public void InitOriginStationData(UIOriginListAck uiOrig)
		{
			_stationMode = StationSelectionMode.OriginStation;
			destination_status dest = (destination_status)uiOrig.MessageData;

			try
			{
				if (dest is null)
					throw new Exception("Fail to show Orign Station List; (EXIT10000203)");

				else if ((dest.details is null) || (dest.details.Length == 0))
					throw new Exception("Fail to show Orign Station List; (EXIT10000204)");

				else
				{
					_language = (uiOrig.Session != null ? uiOrig.Session.Language : LanguageCode.English);
					_stateViewListHelper.InitView(_language);

					App.ShowDebugMsg($@"pgStation ==> destination_status => statedetails : {dest.statedetails.Length}; details: {dest.details.Length}");
					_destDetailList = dest.details;
					_destStateList = dest.statedetails;
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg(ex.Message);
				Log.LogText(_logChannel, "-", uiOrig, "EX01", "pgStation.InitOriginStationData", AppDecorator.Log.MessageType.Fatal, extraMsg: "MsgObj: UIOriginListAck");
				Log.LogFatal(_logChannel, "-", ex, "EX02", "pgStation.InitOriginStationData");
				App.MainScreenControl.Alert(detailMsg: ex.Message);
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				GrdScreenShield.Visibility = Visibility.Collapsed;

				this.Resources.MergedDictionaries.Clear();

				if (_language == LanguageCode.Malay)
					this.Resources.MergedDictionaries.Add(_langMal);
				else
					this.Resources.MergedDictionaries.Add(_langEng);

				TxtStationFilter.Text = "";
				TxtStationFilterWatermark.Visibility = Visibility.Visible;
				GrdStationFilter.Visibility = Visibility.Collapsed;

				_selectedState = null;
				_selectedFlag = false;

				_stateViewListHelper.CreateStateList(_destStateList);
				//DEBUG-Testing _stateViewListHelper.CreateStateList(_stateViewListHelper.Debug_SampleStates());

				_stationViewListHelper.CreateStationList(_destDetailList, _destStateList);
				//DEBUG-Testing _stationViewListHelper.CreateStationList(_stationViewListHelper.Debug_SampleStations(), _stateViewListHelper.Debug_SampleStates());
				System.Windows.Forms.Application.DoEvents();
				LstStationView.Focus();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.Page_Loaded");
				App.MainScreenControl.Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000205)");
			}
		}

		private void _stateViewListHelper_OnStateChanged(object sender, StateChangedEventArgs e)
		{
			try
			{
				
				App.ShowDebugMsg($@"Selected State : {e.State}");

				if (string.IsNullOrWhiteSpace(e.State) == false)
				{
					if ((e.State.Equals(_stateViewListHelper.AllStateCode)))
						_selectedState = null;
					else
						_selectedState = e.State;
				}
				else
					_selectedState = null;

				_stationViewListHelper.Filter(TxtStationFilter.Text, _selectedState);

				//App.NetClientSvc.SalesService.ResetTimeout();
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation._stateViewListHelper_OnStateChanged");
			}
		}

		private void BdState_MouseDown(object sender, MouseButtonEventArgs e) => _stateViewListHelper.StateMouseDownHandle(sender, e);

		private void Station_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (_selectedFlag == false)
				{
					_selectedFlag = true;

					App.MainScreenControl.ExecMenu.ShieldMenu();
					StationViewRow row = (StationViewRow)((Border)sender).DataContext;

					if (_stationMode == StationSelectionMode.OriginStation )
						SubmitOrigin(row.Station, row.StationDesc);
					else
						SubmitDest(row.Station, row.StationDesc);

				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000208)", ex), "EX01", classNMethodName: "pgLanguage.Station_MouseDown");
				App.MainScreenControl.Alert(detailMsg: $@"Error on station; StationMode: {_stationMode}; selection; (EXIT10000208)");
			}
		}

		private void SubmitDest(string destinationCode, string destinationName)
		{
			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {

				try
				{
					App.NetClientSvc.SalesService.SubmitDestination(destinationCode, destinationName, out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000209)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000210)");
					App.Log.LogError(_logChannel, "", new Exception("(EXIT10000210)", ex), "EX01", "pgStation.SubmitDest");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		private void SubmitOrigin(string originCode, string originName)
		{
			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {

				try
				{
					App.NetClientSvc.SalesService.SubmitOrigin(originCode, originName, out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000206)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000207)");
					App.Log.LogError(_logChannel, "", new Exception("(EXIT10000207)", ex), "EX01", "pgStation.SubmitOrigin");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		public void ShieldPage()
		{
			GrdScreenShield.Visibility = Visibility.Visible;
		}

		private void KbKeys_OnKeyPressed(object sender, KeyboardEventArgs e)
		{
			//bool needToResetTimeout = false;
			try
			{
				if (e.KyCat == KeyCat.NormalChar)
				{
					TxtStationFilter.Text += e.KeyString;
				}
				else
				{
					if (e.KyCat == KeyCat.Backspace)
					{
						if (TxtStationFilter.Text.Length > 0)
						{
							TxtStationFilter.Text = TxtStationFilter.Text.Substring(0, TxtStationFilter.Text.Length - 1);
						}
					}
					else if (e.KyCat == KeyCat.Enter)
					{

					}
					else if (e.KyCat == KeyCat.Space)
					{
						TxtStationFilter.Text += " ";
					}
				}

				if (string.IsNullOrWhiteSpace(TxtStationFilter.Text))
				{
					Visibility watermarkHistoryVisibility = TxtStationFilterWatermark.Visibility;

					TxtStationFilterWatermark.Visibility = Visibility.Visible;
					GrdStationFilter.Visibility = Visibility.Collapsed;

					//if (watermarkHistoryVisibility == Visibility.Collapsed)
					//	needToResetTimeout = true;
				}
				else
				{
					TxtStationFilterWatermark.Visibility = Visibility.Collapsed;
					GrdStationFilter.Visibility = Visibility.Visible;
					TxtStationFilter.Focus();
					TxtStationFilter.CaretIndex = TxtStationFilter.Text.Length;

					//needToResetTimeout = true;
				}

				System.Windows.Forms.Application.DoEvents();
				_stationViewListHelper.Filter(TxtStationFilter.Text, _selectedState);

				//if (needToResetTimeout)
				//{
					//App.NetClientSvc.SalesService.ResetTimeout();
					App.TimeoutManager.ResetTimeout();
				//}
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.KbKeys_OnKeyPressed");
			}
			finally
            {
				System.Windows.Forms.Application.DoEvents();
			}
		}

		private void TxtStationFilter_LostFocus(object sender, RoutedEventArgs e)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(TxtStationFilter.Text))
				{
					TxtStationFilterWatermark.Visibility = Visibility.Visible;
					GrdStationFilter.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.TxtStationFilter_LostFocus");
			}
		}

		private void TxtStationFilterWatermark_GotFocus(object sender, RoutedEventArgs e)
		{
			try
			{
				TxtStationFilterWatermark.Visibility = Visibility.Collapsed;
				GrdStationFilter.Visibility = Visibility.Visible;
				TxtStationFilter.Focus();
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "", ex, "EX01", "pgStation.TxtStationFilterWatermark_GotFocus");
			}
		}

		private void BtnRefreshState_Click(object sender, RoutedEventArgs e)
		{
			_stateViewListHelper.DebugTest_CreateStateList();
		}

		private void Button_ClearStationFilter(object sender, MouseButtonEventArgs e)
		{
			try
			{
				TxtStationFilter.Text = "";

				TxtStationFilterWatermark.Visibility = Visibility.Visible;
				GrdStationFilter.Visibility = Visibility.Collapsed;

				_stationViewListHelper.Filter(TxtStationFilter.Text, _selectedState);

				//App.NetClientSvc.SalesService.ResetTimeout();
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.Button_ClearStationFilter");
			}
		}

		private void LstStationView_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstStationView_ScrollChanged");
			}
		}

		private void LstStateView_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstStateView_ScrollChanged");
			}
		}
	}
}
