using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
	public class TripDetailViewListHelper : IDisposable
	{
		private const string LogChannel = "ViewPage";

		public const string TripIdPrefix = "#TRIPID#";

		public event EventHandler OnBeginQueryNewTripData;
		public event EventHandler OnUpdateTripViewInProgress;

		private ConcurrentQueue<DateSelectionPack> _dateSelectionList = new ConcurrentQueue<DateSelectionPack>();

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 0, 100);

		private Page _pgTripSelection = null;
		private ListView _lstTripViewer = null;
		private ProgressBar _progressBar = null;
		private TripDetailViewList _tripViewDataList = null;

		private bool _pageLoaded = false;

		private string _fromStationCode = null;
		private string _toStationCode = null;

		private TripDataListPack _lastestTripDataListPack = null;

		private CultureInfo _provider = CultureInfo.InvariantCulture;
		private LanguageCode _language = LanguageCode.English;
		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		public TripDetailViewListHelper(Page pgTripSelection, ListView lstTripViewer, TripDetailViewList tripViewDataList, ProgressBar tripViewProgressBar)
		{
			_langMal = CommonFunc.GetXamlResource(@"ViewPage\Trip\rosTripMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\Trip\rosTripEnglish.xaml");

			_pgTripSelection = pgTripSelection;
			_lstTripViewer = lstTripViewer;
			_tripViewDataList = tripViewDataList;
			_progressBar = tripViewProgressBar;

			//ScrollViewer tt1 = GetScrollViewerOfListView(_lstTripViewer);

			Thread listUpdator = new Thread(UpdateTripListThreadWorking);
			listUpdator.IsBackground = true;
			listUpdator.Start();
		}

		private ScrollViewer GetScrollViewerOfListView(ListView list)
		{
			ScrollViewer sViewer = default(ScrollViewer);

			//Start looping the child controls of your listview.
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(list); i++)
			{
				// Retrieve child visual at specified index value.
				Visual childVisual = (Visual)VisualTreeHelper.GetChild(list, i);

				if (childVisual is ScrollViewer)
				{
					sViewer = childVisual as ScrollViewer;
					break;
				}
			}

			return sViewer;
		}

		public void PageLoaded()
		{
			_pageLoaded = true;
		}
		public void PageUnLoaded()
		{
			_pageLoaded = false;
		}

		public TripDetailViewRow GetItemRow(string pickTripIdCode)
		{
			var tpArr = (from row in _tripViewDataList.Items
					   where row.TripIdCode.Equals(pickTripIdCode)
					   select row).ToArray();

			TripDetailViewRow retRow = null;
			if (tpArr.Length > 0)
			{
				retRow = tpArr[0];
			}

			return retRow;
		}

		private DateTime _startFilterTime = DateTime.MinValue;
		private DateTime _endFilterTime = DateTime.MaxValue;

		private TripDetailViewRow[] _lastTripDetailList = null;
		private void UpdateTripListThreadWorking()
		{
			int maxSnapTripCount = 20;
			int currTripRecordInx = -1;

			TripDetailViewRow[] tripDetailRowArr = null;
			DateSelectionPack latestDateSelectionPack = null;
			DateSelectionMode currSelectionMode = DateSelectionMode.NewDate;

			while (_disposed == false)
			{
				try
				{
					latestDateSelectionPack = null;
					latestDateSelectionPack = GetLatestSelectedData();

					if (latestDateSelectionPack != null)
					{
						currSelectionMode = latestDateSelectionPack.SelectionMode;

						if (currSelectionMode == DateSelectionMode.TimeFilter)
						{
							_startFilterTime = latestDateSelectionPack.StartTimeFilter;
							_endFilterTime = latestDateSelectionPack.EndTimeFilter;
							tripDetailRowArr = FilterTime(_lastTripDetailList, _startFilterTime, _endFilterTime);
						}
						else /* if (currSelectionMode == DateSelectionMode.NewDate) */
						{
							DateTime selectedDate = latestDateSelectionPack.selectedDate;
							/////tripDetailRowArr = DebugTestLoadTripData();
							tripDetailRowArr = LoadTripData(selectedDate);

							_lastTripDetailList = tripDetailRowArr;
							tripDetailRowArr = FilterTime(_lastTripDetailList, _startFilterTime, _endFilterTime);
						}
						
						if (_pageLoaded)
						{
							currTripRecordInx = -1;
							_pgTripSelection.Dispatcher.Invoke(new Action(() => {

								DispatcherProcessingDisabled ddp;

								try
								{
									ddp = _pgTripSelection.Dispatcher.DisableProcessing();
									
									_progressBar.Value = 0;
									_progressBar.Visibility = System.Windows.Visibility.Visible;
									_progressBar.Maximum = tripDetailRowArr.Length;
									_lstTripViewer.SelectedIndex = -1;
									_lstTripViewer.SelectedItem = null;
									_tripViewDataList.Clear();
									
									RaiseOnUpdateTripViewInProgress();
									//_lstTripViewer.IsEnabled = true;
									Task.Delay(380).Wait();
								}
								catch (Exception ex)
								{
									App.ShowDebugMsg($@"{ex.Message}; (EXIT10000490); TripDetailViewListHelper.UpdateTripListThreadWorking");
									App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000490)", ex), "EX01", classNMethodName: "TripDetailViewListHelper.UpdateTripListThreadWorking");
								}
								finally
								{
									if (ddp != null)
										ddp.Dispose();

									System.Windows.Forms.Application.DoEvents();
									Task.Delay(200).Wait();
								}
							}));
						}
					}

					if ((tripDetailRowArr != null) && (_pageLoaded))
					{
						// Update Trip Data to ListView
						_pgTripSelection.Dispatcher.Invoke(new Action(() => {
							if ((currTripRecordInx + 1 + maxSnapTripCount) <= tripDetailRowArr.Length)
							{
								for (int inx = 0; inx < maxSnapTripCount; inx++)
								{
									currTripRecordInx++;
									_tripViewDataList.Add(tripDetailRowArr[currTripRecordInx]);

									if (currTripRecordInx == 1)
										_lstTripViewer.ScrollIntoView(_tripViewDataList[0]);
								}
							}
							else
							{
								for (int inx = 0; inx < maxSnapTripCount; inx++)
								{
									currTripRecordInx++;

									if ((currTripRecordInx + 1) <= tripDetailRowArr.Length)
									{
										_tripViewDataList.Add(tripDetailRowArr[currTripRecordInx]);

										if (currTripRecordInx == 1)
											_lstTripViewer.ScrollIntoView(_tripViewDataList[0]);
									}
									else
										break;
								}
							}

							_progressBar.Value = (currTripRecordInx + 1);

							if ((currTripRecordInx + 1) >= tripDetailRowArr.Length)
							{
								tripDetailRowArr = null;
								_progressBar.Visibility = System.Windows.Visibility.Collapsed;
							}

							if (_forceToClearTripLoadingFlag)
							{
								tripDetailRowArr = null;
								_forceToClearTripLoadingFlag = false;
								_progressBar.Visibility = System.Windows.Visibility.Collapsed;
							}

							System.Windows.Forms.Application.DoEvents();
							_lstTripViewer.Items.Refresh();
							System.Windows.Forms.Application.DoEvents();
						}));

						System.Windows.Forms.Application.DoEvents();
						Task.Delay(380).Wait();
					}
					else
					{
						lock (_dateSelectionList)
						{
							Monitor.Wait(_dateSelectionList, 100);
						}
					}
				}
				catch (Exception ex)
				{
					string byPassMsg = ex.Message;
				}
			}
		}

		private DateSelectionPack GetLatestSelectedData()
		{
			DateSelectionPack retTripDataPack = null;

			if (_disposed == false)
			{
				try
				{
					if (_dateSelectionList.Count > 0)
					{
						lock (_dateSelectionList)
						{
							while (_dateSelectionList.TryDequeue(out DateSelectionPack outTripDataPack))
								retTripDataPack = outTripDataPack;
						}
					}
				}
				catch (Exception ex) 
				{ 
					App.ShowDebugMsg($@"{ex.Message}; (EXIT10000485); TripDetailViewListHelper.GetLatestSelectedData");
					App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000485)", ex), "EX01", classNMethodName: "TripDetailViewListHelper.GetLatestSelectedData");
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000485)");
				}
			}
			return retTripDataPack;
		}

		public void UpdateList(DateTime selectedDate)
		{
			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				if (_disposed == false)
				{
					try
					{
						lock (_dateSelectionList)
						{

							RaiseOnBeginQueryNewTripData();

							//_pgTripSelection.Dispatcher.Invoke(new Action(() => {
							//	_lstTripViewer.IsEnabled = false;
							//}));

							_dateSelectionList.Enqueue(new DateSelectionPack() { selectedDate = selectedDate });
							_forceToClearTripLoadingFlag = false;

							Monitor.PulseAll(_dateSelectionList);
						}
					}
					catch (Exception ex2) 
					{ 
						App.ShowDebugMsg($@"{ex2.Message}; (EXIT10000486); TripDetailViewListHelper.UpdateList");
						App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000486)", ex2), "EX01", classNMethodName: "TripDetailViewListHelper.UpdateList");
						App.MainScreenControl.Alert(detailMsg: $@"{ex2.Message}; (EXIT10000486)");
					}
				}
			})));
			execThread.IsBackground = true;
			execThread.Start();
		}

		public void FilterListByTime(DateTime startTime, DateTime endTime)
		{
			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				if (_disposed == false)
				{
					try
					{
						lock (_dateSelectionList)
						{
							RaiseOnBeginQueryNewTripData();

							_dateSelectionList.Enqueue(new DateSelectionPack() { SelectionMode = DateSelectionMode.TimeFilter, 
								StartTimeFilter = startTime, EndTimeFilter = endTime });

							_forceToClearTripLoadingFlag = false;

							Monitor.PulseAll(_dateSelectionList);
						}
					}
					catch (Exception ex2)
					{
						App.ShowDebugMsg($@"{ex2.Message}; (EXIT10000495); TripDetailViewListHelper.FilterListByTime");
						App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000495)", ex2), "EX01", classNMethodName: "TripDetailViewListHelper.FilterListByTime");
						App.MainScreenControl.Alert(detailMsg: $@"{ex2.Message}; (EXIT10000495)");
					}
				}
			})));
			execThread.IsBackground = true;
			execThread.Start();
		}

		private bool _forceToClearTripLoadingFlag = false;
		public void ClearTripLoading()
		{
			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				DateSelectionPack tempTripDataPack = null;
				if (_disposed == false)
				{
					try
					{
						try
						{
							if (_getTripListThreadWorker != null)
							{
								if (_getTripListThreadWorker.ThreadState.IsState(ThreadState.Stopped) == false)
								{
									_getTripListThreadWorker.Abort();
								}
							}
						}
						catch { }

						lock (_dateSelectionList)
						{
							_forceToClearTripLoadingFlag = true;
							while (_dateSelectionList.TryDequeue(out DateSelectionPack outTripDataPack))
								tempTripDataPack = outTripDataPack;
						}
					}
					catch (Exception ex2) { string byPassStr = ex2.Message; }
				}
			})));
			execThread.IsBackground = true;
			execThread.Start();
		}

		private void RaiseOnBeginQueryNewTripData()
		{
			if (OnBeginQueryNewTripData != null)
			{
				_pgTripSelection.Dispatcher.Invoke(new Action(() => {
					try
					{
						OnBeginQueryNewTripData.Invoke(null, new EventArgs());
					}
					catch (Exception ex)
					{
						App.ShowDebugMsg(($@"Error when TripDetailViewListHelper.RaiseOnBeginQueryNewTripData : {ex.Message}; (EXIT10000487)"));
						App.Log.LogError(LogChannel, "-", new Exception("Unhandled exception in TripDetailViewListHelper.RaiseOnBeginQueryNewTripData; (EXIT10000487)", ex), "EX01", classNMethodName: "TripDetailViewListHelper.RaiseOnBeginQueryNewTripData");
						App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000487)");
					}
				}));
			}
		}

		private void RaiseOnUpdateTripViewInProgress()
		{
			if (OnUpdateTripViewInProgress != null)
			{
				_pgTripSelection.Dispatcher.Invoke(new Action(() => {
					try
					{
						OnUpdateTripViewInProgress.Invoke(null, new EventArgs());
					}
					catch (Exception ex)
					{
						App.ShowDebugMsg(($@"Error when TripDetailViewListHelper.RaiseOnUpdateTripViewInProgress : {ex.Message}; (EXIT10000488)"));
						App.Log.LogError(LogChannel, "-", new Exception("Unhandled exception in TripDetailViewListHelper.RaiseOnUpdateTripViewInProgress; (EXIT10000488)", ex), "EX01", classNMethodName: "TripDetailViewListHelper.RaiseOnUpdateTripViewInProgress");
						App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000488)");
					}
				}));
			}
		}

		private bool _disposed = false;
		public void Dispose()
		{
			if (OnBeginQueryNewTripData != null)
			{
				Delegate[] delgList = OnBeginQueryNewTripData.GetInvocationList();
				foreach (EventHandler delg in delgList)
				{
					OnBeginQueryNewTripData -= delg;
				}
			}
			if (OnUpdateTripViewInProgress != null)
			{
				Delegate[] delgList = OnUpdateTripViewInProgress.GetInvocationList();
				foreach (EventHandler delg in delgList)
				{
					OnUpdateTripViewInProgress -= delg;
				}
			}
			lock (_dateSelectionList)
			{
				while (_dateSelectionList.TryDequeue(out DateSelectionPack outTripDataPack))
				{ }
			}

			_disposed = true;
		}

		public void InitTripData(string fromStationCode, string toStationCode, LanguageCode language)
		{
			_fromStationCode = fromStationCode;
			_toStationCode = toStationCode;
			_language = language;

			_startFilterTime = new DateTime(1900, 1, 1, 0, 0, 0, 0);
			_endFilterTime = new DateTime(1900, 1, 1, 23, 59, 59, 999);
		}

		private Thread _getTripListThreadWorker = null;
		private TripDetailViewRow[] LoadTripData(DateTime passengerTravelDate)
		{
			_lastestTripDataListPack = null;

			TripDetailViewRow[] tripRowArr = new TripDetailViewRow[0];
			bool isServerResponded = false;
			bool threadAborted = false;

			try
			{
				_getTripListThreadWorker = new Thread(GetTripList);
				_getTripListThreadWorker.IsBackground = true;
				_getTripListThreadWorker.Start();

				int waitDelaySec = 100;
				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(waitDelaySec);

				while ((endTime.Subtract(DateTime.Now).TotalSeconds > 0) && (_pageLoaded))
				{
					if (_lastestTripDataListPack is null)
						Task.Delay(100).Wait();
					else
					{
						isServerResponded = true;
						break;
					}
				}

				if (_pageLoaded == false)
					return tripRowArr;

				if (_lastestTripDataListPack != null)
				{
					if (_lastestTripDataListPack.PassengerTravelDate.ToString("yyyyMMddd").Equals(passengerTravelDate.ToString("yyyyMMddd")) == false )
					{
						App.ShowDebugMsg(($@"Trip Date MissMatch; (EXIT10000493)"));
						App.Log.LogError(LogChannel, "-", new Exception($@"From Server: {_lastestTripDataListPack.PassengerTravelDate.ToString("yyyyMMddd")}; From Parameter: {passengerTravelDate.ToString("yyyyMMddd")}; (EXIT10000493)"), 
							"EX02", classNMethodName: "TripDetailViewListHelper.RaiseOnUpdateTripViewInProgress");
						App.MainScreenControl.Alert(detailMsg: $@"Trip Date MissMatch; (EXIT10000493)");
					}

					trip_status ts = _lastestTripDataListPack.Trip;

					if (ts.details?.Length > 0)
					{
						trip_detail[] tpArr = (from row in ts.details
											   orderby row.departtime, row.totalseat descending
											   select row).ToArray();

						if (tpArr.Length > 0)
						{
							string soldOutTag = "";

							if (_language == LanguageCode.Malay)
								soldOutTag = _langMal["SOLD_OUT_Label"]?.ToString();
							else
								soldOutTag = _langEng["SOLD_OUT_Label"]?.ToString();

							if (string.IsNullOrWhiteSpace(soldOutTag))
								soldOutTag = "SOLD OUT";

							tripRowArr = new TripDetailViewRow[tpArr.Length];

							string logoUrl = ts.companyLogoURL?.Trim();
							DateTime tmr = DateTime.MinValue;

							int inx = -1;
							foreach (trip_detail td in tpArr)
							{
								//if (td.totalseat <= 0)
								//	continue;
								//_provider
								inx++;
								tmr = DateTime.ParseExact($@"01-01-1900_{td.departtime}", "dd-MM-yyyy_HHmmss", _provider);

								TripDetailViewRow nRow = new TripDetailViewRow()
								{
									// departtime is passenger Depart Time from his/her station
									//TimeStr = (td.departtime?.Length >= 6) ? $@"{td.departtime.Substring(0, 2)}:{td.departtime.Substring(2, 2)}:{td.departtime.Substring(4, 2)}" : "",
									//TimeStr = (td.departtime?.Length >= 6) ? $@"{td.departtime.Substring(0, 2)}:{td.departtime.Substring(2, 2)}" : "",
									TimeStr = tmr.ToString("hh:mm tt"),
									TripIntTime = int.Parse(tmr.ToString("HHmm")),
									OrgTimeStr = td.departtime,
									Currency = td.currency,
									PriceStr = $@"{td.price:#,##0.00}",
									AvailableSeat = (td.totalseat - td.seatoccupied),
									//AvailableSeat = td.seatoccupied,
									TotalSeat = td.totalseat,
									TotalSeatStr = (td.totalseat <= 0) ? soldOutTag : td.totalseat.ToString(),
									//OperatorImgPath = td.. @"http://cts.melakasentral.com.my/eticketing_melaka/Upload/ShowImage.asp?ID=_IMG1901001",
									OperatorImgPath = $@"{logoUrl}?ID={td.complogoid}",
									OperatorDesc = td.compdesc,
									TripClass = "",
									TripDetail = td.routedetail,
									TripIdCode = (td.totalseat > 0) ? ($@"{TripIdPrefix}{td.tripid}-{td.tripno}") : "-",

									NormalSeatAvailableVisible = (td.totalseat > 5) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
									QuickFinishSeatAvailableVisible = ((td.totalseat <= 5) && (td.totalseat > 0)) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
									SoldOutVisible = (td.totalseat <= 0) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,

									IsPickSeatVisible = (td.totalseat > 0) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,

									// Will Used in Session
									//xxxxxxxxxxxxxxxxxxxxxxxx
									Price = td.price,
									TripId = td.tripid,
									// TripDate is Bus Depart Date from first station
									TripDate = td.tripdate,
									TripNo = td.tripno,
									TimePosi = td.timeposi,
									Embed = td.embed,
									PassengerActualFromStationCode = td.from,
									PassengerActualToStationCode = td.to,
									Insurance = td.insurance,
									// Will NOT Used in Session
									SeatOccupied = td.seatoccupied
								};

								tripRowArr[inx] = nRow;
							}
						}


						
					}
				}
			}
			catch (ThreadAbortException ex) 
			{
				threadAborted = true;
				App.ShowDebugMsg(($@"Thread Aborted;  TripDetailViewListHelper.LoadTripData; (EXIT10000494)"));
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000494)", ex), "EX01", classNMethodName: "TripDetailViewListHelper.LoadTripData");
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg(($@"Error when TripDetailViewListHelper.LoadTripData : {ex.Message}; (EXIT10000491)"));
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000491)", ex), "EX01", classNMethodName: "TripDetailViewListHelper.LoadTripData");
			}

			if ((isServerResponded == false) && (threadAborted == false))
			{
				App.ShowDebugMsg(($@"Local Server not responding; (EXIT10000492)"));
				App.Log.LogError(LogChannel, "-", new Exception("Unhandled exception in TripDetailViewListHelper.RaiseOnUpdateTripViewInProgress; (EXIT10000492)"), "EX02", classNMethodName: "TripDetailViewListHelper.RaiseOnUpdateTripViewInProgress");
				App.MainScreenControl.Alert(detailMsg: $@"Local Server not responding; (EXIT10000492)");
			}

			return tripRowArr;

			void GetTripList()
			{
				try
				{
					App.NetClientSvc.SalesService.QueryDepartTripList(passengerTravelDate, _fromStationCode, _toStationCode, out isServerResponded, 80);
				}
				catch (ThreadAbortException) { }
				catch (Exception ex)
				{
					App.ShowDebugMsg(($@"Error when TripDetailViewListHelper.GetTripList : {ex.Message}; (EXIT10000489)"));
					App.Log.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "TripDetailViewListHelper.GetTripList");
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000489)");
				}
			}
		}

		private TripDetailViewRow[] FilterTime (TripDetailViewRow[] fullTripList, DateTime startTime, DateTime endTime)
		{
			TripDetailViewRow[] timeFilteredTripList = new TripDetailViewRow[0];

			if (fullTripList?.Length > 0)
			{
				int startIntTime = int.Parse(startTime.ToString("HHmm"));
				int endIntTime = int.Parse(endTime.ToString("HHmm"));

				if ((startIntTime > 0) && (endIntTime < 2359))
				{
					timeFilteredTripList = (from row in fullTripList
											where (row.TripIntTime >= startIntTime) && (row.TripIntTime <= endIntTime)
											orderby row.TripIntTime, row.TotalSeat descending
											select row).ToArray();
				}
				else if (startIntTime > 0)
				{
					timeFilteredTripList = (from row in fullTripList
											where (row.TripIntTime >= startIntTime)
											orderby row.TripIntTime, row.TotalSeat descending
											select row).ToArray();
				}
				else if (endIntTime < 2359)
				{
					timeFilteredTripList = (from row in fullTripList
											where (row.TripIntTime <= endIntTime)
											orderby row.TripIntTime, row.TotalSeat descending
											select row).ToArray();
				}
				else
					timeFilteredTripList = fullTripList;

			}

			return timeFilteredTripList;
		}

		public void UpdateDepartTripList(UIDepartTripListAck uiTravelDate, UserSession session)
		{
			_lastestTripDataListPack = new TripDataListPack(uiTravelDate, session);
		}

		//private TripDetailViewRow[] DebugTestLoadTripData()
		//{
		//	int numberOfTripCount = 200;
		//	TripDetailViewRow[] tripRowArr = new TripDetailViewRow[numberOfTripCount];

		//	for (int inx = 0; inx < numberOfTripCount; inx++)
		//	{
		//		tripRowArr[inx] = DebugCreateATrip();
		//	}

		//	return tripRowArr;
		//}

		private decimal _lastDebugTestPrice = 0.01M;
		private TripDetailViewRow DebugCreateATrip()
		{
			_lastDebugTestPrice += 0.01M;
			return new TripDetailViewRow()
			{
				TimeStr = "20:30",
				Currency = "RM",
				PriceStr = $@"{_lastDebugTestPrice:#,##0.00}",
				AvailableSeat = 23,
				TotalSeat = 36,
				//OperatorImgPath = @"/NssIT.Kiosk.Client;component/Resources/OperatorLogoTest01.png",
				//OperatorImgPath = @"Resources/OperatorLogoTest01.png",
				//OperatorImgPath = @"C:\dev\RND\Kiosk\ClientUI\KioskClient\NssIT.Kiosk.SalesMachClient\bin\Debug\Resources\OperatorLogoTest01.png",
				//OperatorImgPath = @"C:\Shared\KioskSalesMachClient\Resources\OperatorLogoTest01.png",
				OperatorImgPath = @"http://cts.melakasentral.com.my/eticketing_melaka/Upload/ShowImage.asp?ID=_IMG1901001",
				OperatorDesc = "Melor Interline xxxxx ttttt yyyyy",
				TripClass = "SVIP",
				TripDetail = "TBS - MEL - MUR - XXX - XXX - XXX",
				TripIdCode = TripIdPrefix + Guid.NewGuid().ToString("D")
			};
		}

		class DateSelectionPack
		{
			public DateTime selectedDate { get; set; }
			public DateSelectionMode SelectionMode { get; set; } = DateSelectionMode.NewDate;

			/// <summary>
			/// User for SelectionMode = DateSelectionMode.TimeFilter
			/// </summary>
			public DateTime StartTimeFilter { get; set; } = DateTime.MinValue;
			/// <summary>
			/// User for SelectionMode = DateSelectionMode.TimeFilter
			/// </summary>
			public DateTime EndTimeFilter { get; set; } = DateTime.MinValue;
		}

		class TripDataListPack
		{
			public trip_status Trip { get; private set; }
			public DateTime PassengerTravelDate { get; private set; }

			public TripDataListPack(UIDepartTripListAck uiDepartTripList, UserSession session)
			{
				Trip = (trip_status)uiDepartTripList.MessageData;
				PassengerTravelDate = session.DepartPassengerDate.Value;
			}
		}

		enum DateSelectionMode
		{
			NewDate = 0,
			TimeFilter = 1
		}
	}
}
