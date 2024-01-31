using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Concurrent;
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

namespace NssIT.Kiosk.Client.ViewPage.Date
{
    /// <summary>
    /// Interaction logic for pgDate.xaml
    /// </summary>
    public partial class pgDate : Page, ITravelDate, IKioskViewPage
	{
		private string _logChannel = "ViewPage";

		private bool _selectedFlag = false;

		private static Brush _enableButtonColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x43, 0xD8, 0x2C));
		private static Brush _disableButtonColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));

		private LanguageCode _language = LanguageCode.English;
		private TravelMode _travelMode = TravelMode.DepartOnly;

		//private const int _maxRangeOfAdvanceMonth = 3; /* 3 mounths advance*/
		private DateTime _outOfAdvanceDate = DateTime.Now.AddDays(91);

		// _departDate _returnDate
		private DateTime? _departDate = null;
		private DateTime? _returnDate = null;

		private DateTime? _firstDateOfCalendar = null;
		private DateTime? _lastDateOfCalendar = null;

		private int _selectedYear = DateTime.Now.Year;
		private int _selectedMonth = DateTime.Now.Month;

		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

		private ConcurrentDictionary<string, CalendarDayCell> _dayCellList = null;
		private ConcurrentDictionary<string, CalendarDayCell> DayCellList
		{
			get
			{
				if (_dayCellList == null)
				{
					string namePostFix = "";
					Border dayCellX = null;
					Border leftBlockX = null;
					Border rightBlockX = null;
					Ellipse circleX = null;
					TextBlock dayInxTextX = null;
					Line dayInxTextLineX = null;

					CalendarDayCell tmpCalendarDayCell = null;

					_dayCellList = new ConcurrentDictionary<string, CalendarDayCell>();

					for (int colInx = 0; colInx <= 6; colInx++)
					{
						for (int rowInx = 0; rowInx <= 5; rowInx++)
						{
							namePostFix = $@"{colInx.ToString("0#")}{rowInx.ToString("0#")}";

							dayCellX = (Border)this.FindName($@"CellX{namePostFix}");
							leftBlockX = (Border)dayCellX.FindName($@"LeftBlockX{namePostFix}");
							rightBlockX = (Border)dayCellX.FindName($@"RightBlockX{namePostFix}");
							circleX = (Ellipse)dayCellX.FindName($@"CircleX{namePostFix}");
							dayInxTextX = (TextBlock)dayCellX.FindName($@"DayInxTextX{namePostFix}");
							dayInxTextLineX = (Line)dayCellX.FindName($@"DayInxTextLineX{namePostFix}");

							tmpCalendarDayCell = new CalendarDayCell(this.Dispatcher, dayCellX, leftBlockX, rightBlockX, circleX, dayInxTextX, dayInxTextLineX);

							_dayCellList.TryAdd(namePostFix, tmpCalendarDayCell);
						}
					}
				}

				return _dayCellList;
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			string txtName;
			int columnInx = 0;

			_selectedFlag = false;

			GrdScreenShield.Visibility = Visibility.Collapsed;

			TextBlock TxtWeekDayTagX = null;
			int currentDayOfWeek = (int)FirstDayOfWeek;

			//DateTime outOfAdvanceDate = DateTime.Now.AddMonths(_maxRangeOfAdvanceMonth + 1);

			DateTime outOfAdvanceDate = App.MaxTicketAdvanceDate.AddDays(1);
			_outOfAdvanceDate = new DateTime(outOfAdvanceDate.Year, outOfAdvanceDate.Month, outOfAdvanceDate.Day, 0, 0, 0, 0);

			this.Resources.MergedDictionaries.Clear();

			if (_language == LanguageCode.Malay)
				this.Resources.MergedDictionaries.Add(_langMal);
			else
				this.Resources.MergedDictionaries.Add(_langEng);

			//CYA-DEBUG
			//var aPREVLabel = _langEng["PREVLabel"];
			//var s1 = _langEng["S1"];
			//var s2 = _langEng["S2"];
			//var s3 = _langEng["S3"];
			//var s4 = string.Format((string)_langEng["S4"], "Poul", 23);

			do
			{
				txtName = $@"TxtWeekDayTagX{columnInx.ToString("0#")}";

				TxtWeekDayTagX = (TextBlock)this.FindName(txtName);
				TxtWeekDayTagX.Text = WeekDayString(Enum.GetName(typeof(DayOfWeek), (DayOfWeek)currentDayOfWeek).ToUpper().Substring(0, 3));

				// .. next
				currentDayOfWeek++;

				if (currentDayOfWeek > 6)
					currentDayOfWeek = 0;

				columnInx++;
			} while (columnInx < 7);

			// Testing Only -- FirstDayOfWeek = DayOfWeek.Monday
			//_departDate = new DateTime(2020, 2, 2);
			//_returnDate = new DateTime(2020, 4, 10);

			DateTime todayDate = DateTime.Now;

			if (_departDate.HasValue)
				todayDate = _departDate.Value;

			Init(new DateTime(todayDate.Year, todayDate.Month, 1));
			RefreshCalendar();
			RefreshSearchButton();
			ChangeScreenSize();


			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		}

		public pgDate()
		{
			InitializeComponent();

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\Date\rosDateMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\Date\rosDateEnglish.xaml");
		}

		public void InitData(UserSession session)
		{
			if (session != null)
			{
				_language = session.Language;
				_travelMode = session.TravelMode;
				_departDate = session.DepartPassengerDate;
				_returnDate = session.ReturnPassengerDate;
			}
			else
			{
				_language = LanguageCode.English;
				_departDate = null;
				_returnDate = null;
			}
		}

		public void QuerySelectedDate(out DateTime? departDate, out DateTime? returnDate)
		{
			returnDate = null;
			departDate = _departDate;

			if (_travelMode == TravelMode.DepartOrReturn)
				returnDate = _returnDate;
		}

		public void Init(DateTime? referMonth)
		{
			DateTime cTime = referMonth.HasValue ? referMonth.Value : DateTime.Now;
			_selectedYear = cTime.Year;
			_selectedMonth = cTime.Month;
		}

		private void Previous_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DateTime previousMonth = new DateTime(_selectedYear, _selectedMonth, 1);
			DateTime newMonth = previousMonth.AddMonths(-1);
			_selectedYear = newMonth.Year;
			_selectedMonth = newMonth.Month;

			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgDate.Previous_MouseDown");
			}
		}

		private void Next_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DateTime previousMonth = new DateTime(_selectedYear, _selectedMonth, 1);
			DateTime newMonth = previousMonth.AddMonths(1);
			_selectedYear = newMonth.Year;
			_selectedMonth = newMonth.Month;

			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgDate.Next_MouseDown");
			}
		}

		private void RefreshCalendar()
		{
			DateTime currentTime = DateTime.Now;
			DateTime firstDayDate = new DateTime(_selectedYear, _selectedMonth, 1, 0, 0, 0, 0);
			DateTime nextMonthFirstDayDate = firstDayDate.AddMonths(1);
			DateTime lastDayDate = nextMonthFirstDayDate.AddDays(-1);
			DateTime currentDate = firstDayDate;
			DateTime previousMonthLastDay = firstDayDate.AddDays(-1);

			_firstDateOfCalendar = null;
			_lastDateOfCalendar = null;

			// MonthYearString
			//TxtMonthYearTag.Text = firstDayDate.ToString("MMMM yyyy");
			TxtMonthYearTag.Text = MonthYearString(firstDayDate);

			//Fix Previous Month Calendar
			if (firstDayDate.DayOfWeek != FirstDayOfWeek)
			{
				string namePostFix = "";
				int maxConstVariantDay = (int)DayOfWeek.Saturday - (int)FirstDayOfWeek;
				int variantDayOfWeek = -10;
				int columnInx = -1;
				DateTime previousMonthRefDate = firstDayDate.AddDays(-1);
				do
				{
					// Find column index in calendar.
					variantDayOfWeek = (int)previousMonthRefDate.DayOfWeek - (int)FirstDayOfWeek;
					if (variantDayOfWeek >= 0)
					{
						_firstDateOfCalendar = previousMonthRefDate;
						columnInx = variantDayOfWeek;
					}
					else /* if variantDayOfWeek < 0 */
						columnInx = maxConstVariantDay + ((int)FirstDayOfWeek + variantDayOfWeek) + 1;

					namePostFix = $@"{columnInx.ToString("0#")}00";
					//---------------------------------------------------------

					if (DayCellList.TryGetValue(namePostFix, out CalendarDayCell dayCell) == false)
						throw new Exception($@"Unable to translate calendar code ({namePostFix}). Msg. from pg-Date-Selection");
					else
					{
						dayCell.InitData(currentTime, _selectedYear, _selectedMonth, previousMonthRefDate, _outOfAdvanceDate);
					}

					// prepare for Next Day
					previousMonthRefDate = previousMonthRefDate.AddDays(-1);

				} while (columnInx > 0);
			}


			//Fix Current Month Calendar
			int lastRowInx = 0;
			{
				string namePostFix = "";
				int maxConstVariantDay = (int)DayOfWeek.Saturday - (int)FirstDayOfWeek;
				int variantDayOfWeek = -10;
				int columnInx = -1;
				int rowInx = 0;
				currentDate = firstDayDate;

				if (_firstDateOfCalendar.HasValue == false)
					_firstDateOfCalendar = firstDayDate;

				do
				{
					// Find column index in calendar.
					variantDayOfWeek = (int)currentDate.DayOfWeek - (int)FirstDayOfWeek;
					if (variantDayOfWeek >= 0)
						columnInx = variantDayOfWeek;

					else /* if variantDayOfWeek < 0 */
						columnInx = maxConstVariantDay + ((int)FirstDayOfWeek + variantDayOfWeek) + 1;

					namePostFix = $@"{columnInx.ToString("0#")}{rowInx.ToString("0#")}";
					//---------------------------------------------------------

					// Refresh day cell in calendar.
					if (DayCellList.TryGetValue(namePostFix, out CalendarDayCell dayCell) == false)
						throw new Exception($@"Unable to translate calendar code ({namePostFix}). Msg. from pg-Date-Selection");
					else
					{
						dayCell.InitData(currentTime, _selectedYear, _selectedMonth, currentDate, _outOfAdvanceDate);

						_lastDateOfCalendar = currentDate;
					}

					lastRowInx = rowInx;
					// prepare for Next Day
					currentDate = currentDate.AddDays(1);
					if (columnInx == 6)
						rowInx++;

				} while (currentDate < nextMonthFirstDayDate);
			}

			//Fix Next Month Calendar
			{
				string namePostFix = "";
				int maxConstVariantDay = (int)DayOfWeek.Saturday - (int)FirstDayOfWeek;
				int variantDayOfWeek = -10;
				int columnInx = -1;
				//int rowInx = lastRowInx;
				int? rowInx = null;
				bool startToHide = false;

				currentDate = nextMonthFirstDayDate;
				do
				{
					// Find column index in calendar.
					variantDayOfWeek = (int)currentDate.DayOfWeek - (int)FirstDayOfWeek;
					if (variantDayOfWeek >= 0)
						columnInx = variantDayOfWeek;

					else /* if variantDayOfWeek < 0 */
						columnInx = maxConstVariantDay + ((int)FirstDayOfWeek + variantDayOfWeek) + 1;

					if (columnInx == 0)
					{
						startToHide = true;
						rowInx = (rowInx.HasValue == false) ? lastRowInx + 1 : rowInx;
					}
					else
						rowInx = (rowInx.HasValue == false) ? lastRowInx : rowInx;


					if (rowInx > 5)
						break;
					//---------------------------------------------------------

					namePostFix = $@"{columnInx.ToString("0#")}{rowInx.Value.ToString("0#")}";
					//---------------------------------------------------------

					if (DayCellList.TryGetValue(namePostFix, out CalendarDayCell dayCell) == false)
						throw new Exception($@"Unable to translate calendar code ({namePostFix}). Msg. from pg-Date-Selection");
					else
					{
						dayCell.InitData(currentTime, _selectedYear, _selectedMonth, currentDate, _outOfAdvanceDate);

						if (startToHide)
						{
							dayCell.CellHide();
						}
						else
							_lastDateOfCalendar = currentDate;
					}

					// prepare for Next Day
					currentDate = currentDate.AddDays(1);
					if (columnInx == 6)
						rowInx++;

				} while (rowInx.Value <= 5);
			}

			DateTime nextCalendarFirstDayDate = _lastDateOfCalendar.Value.AddDays(1);
			//To further Decorate/Make-up calendar
			{
				string namePostFix = "";
				int maxConstVariantDay = (int)DayOfWeek.Saturday - (int)FirstDayOfWeek;
				int variantDayOfWeek = -10;
				int columnInx = -1;
				int rowInx = 0;
				//currentDate = firstDayDate;
				currentDate = _firstDateOfCalendar.Value;
				do
				{
					// Find column index in calendar.
					variantDayOfWeek = (int)currentDate.DayOfWeek - (int)FirstDayOfWeek;
					if (variantDayOfWeek >= 0)
						columnInx = variantDayOfWeek;

					else /* if variantDayOfWeek < 0 */
						columnInx = maxConstVariantDay + ((int)FirstDayOfWeek + variantDayOfWeek) + 1;

					namePostFix = $@"{columnInx.ToString("0#")}{rowInx.ToString("0#")}";
					//---------------------------------------------------------

					// Refresh day cell in calendar.
					if (DayCellList.TryGetValue(namePostFix, out CalendarDayCell dayCell) == false)
						throw new Exception($@"Unable to translate calendar code ({namePostFix}). Msg. from pg-Date-Selection");
					else
					{
						//dayCell.InitData(currentTime, _selectedYear, _selectedMonth, currentDate);

						// No selected Dates OR dayCell's Day is not in selection Range
						if (
							((!_departDate.HasValue) && (!_returnDate.HasValue))
							||
							((_departDate.HasValue) && (_returnDate.HasValue) && ((currentDate < _departDate) || (currentDate > _returnDate)))
							||
							((_departDate.HasValue) && (!_returnDate.HasValue) && ((currentDate < _departDate) || (currentDate > _departDate)))
							||
							((!_departDate.HasValue) && (_returnDate.HasValue) && ((currentDate < _returnDate) || (currentDate > _returnDate)))
							)
						{
							//By Pass
						}

						// dayCell's Day is matched with a selected date, but Only One date ( Either Depart or Return date ) has selected.
						else if (
							((_departDate.HasValue) && (!_returnDate.HasValue) && (currentDate == _departDate))
							||
							((!_departDate.HasValue) && (_returnDate.HasValue) && (currentDate == _returnDate))
							)
						{
							dayCell.CellSelectedSingleDay();
						}

						// Depart and Return date have selected. dayCell's Day is not a selected date but is in selection range.
						else if (
							((_departDate.HasValue) && (_returnDate.HasValue) && ((currentDate > _departDate) && (currentDate < _returnDate)))
							)
						{
							//if (
							//	((columnInx == 6) && (currentDate.Day == 1))
							//	||
							//	((columnInx == 0) && (currentDate == lastDayDate))
							//	)
							//{
							//	dayCell.CellRelateSelectedRangeAcrossSingleColumn();
							//}
							//else 
							//if ((columnInx == 0) || (currentDate.Day == 1))
							if (columnInx == 0)
							{
								dayCell.CellRelateSelectedRangeAcrossFirstDay();
							}
							//else if ((columnInx == 6) || (currentDate == lastDayDate))
							else if (columnInx == 6)
							{
								dayCell.CellRelateSelectedRangeAcrossLastDay();
							}
							else
								dayCell.CellRelateSelectedRangeAcrossDay();
						}

						// Depart and Return date have selected. dayCell's Day is one of the selected date.
						else if (
							((_departDate.HasValue) && (_returnDate.HasValue) && ((currentDate == _departDate) || (currentDate == _returnDate)))
							)
						{
							if (currentDate == _departDate)
							{
								//if ((columnInx < 6) && (currentDate.Ticks != lastDayDate.Ticks))
								if (columnInx < 6)
								{
									dayCell.CellRelateSelectedRangeAcrossStartDay();
								}
								else /* (columnInx == 6) OR (currentDate.Ticks == lastDayDate.Ticks) */
								{
									dayCell.CellSelectedStartDay();
								}
							}
							else /* (currentDate == _returnDate) */
							{
								//if ((columnInx > 0) && (currentDate.Day != 1))
								if (columnInx > 0)
								{
									dayCell.CellRelateSelectedRangeAcrossEndDay();
								}
								else /* (columnInx == 1) OR (currentDate.Day == 1) */
								{
									dayCell.CellSelectedEndDay();
								}
							}
						}
					}

					//lastRowInx = rowInx;
					// prepare for Next Day
					currentDate = currentDate.AddDays(1);
					if (columnInx == 6)
						rowInx++;

				} while (currentDate < nextCalendarFirstDayDate);

				//Show OR Hide Previous & Next Mouth Button
				if (previousMonthLastDay.Ticks < currentTime.Ticks)
					SkpPrevious.Visibility = Visibility.Hidden;
				else
					SkpPrevious.Visibility = Visibility.Visible;

				if (nextMonthFirstDayDate.Ticks < _outOfAdvanceDate.Ticks)
					SkpNext.Visibility = Visibility.Visible;
				else
					SkpNext.Visibility = Visibility.Hidden;

			}
		}

		private void PreviousMonth_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DateTime existingRefMonth = new DateTime(_selectedYear, _selectedMonth, 1, 0, 0, 0, 0);
			DateTime previousRefMonth = existingRefMonth.AddMonths(-1);

			if (int.Parse(previousRefMonth.ToString("yyyyMM")) < int.Parse(DateTime.Now.ToString("yyyyMM")))
				return;

			Init(previousRefMonth);
			RefreshCalendar();
			RefreshSearchButton();

			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgDate.Previous_MouseDown");
			}
		}

		private void NextMonth_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DateTime existingRefMonth = new DateTime(_selectedYear, _selectedMonth, 1, 0, 0, 0, 0);
			DateTime nextRefMonth = existingRefMonth.AddMonths(1);

			if (nextRefMonth.Ticks >= _outOfAdvanceDate.Ticks)
				return;

			Init(nextRefMonth);
			RefreshCalendar();
			RefreshSearchButton();

			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgDate.NextMonth_MouseDown");
			}
		}

		private void DayCell_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Border dayCell = (Border)sender;
			string dayCellName = dayCell.Name;
			string dayCellPostFix = dayCellName.Replace("CellX", "");
			DateTime tmpTodayDate = DateTime.Now;
			DateTime todayDate = new DateTime(tmpTodayDate.Year, tmpTodayDate.Month, tmpTodayDate.Day, 0, 0, 0);

			if (DayCellList.TryGetValue(dayCellPostFix, out CalendarDayCell caldDayCell) == true)
			{
				if (caldDayCell.CellIsHidden)
				{
					//ShowMsg("DayCell is hidden.");
				}
				else if (caldDayCell.CellDate.Ticks < todayDate.Ticks)
				{
					//ShowMsg("Please selected a day start from Today's date.");
				}
				else if (caldDayCell.CellDate.Ticks >= _outOfAdvanceDate.Ticks)
				{
					//MainWindow.ShowMessage($@"Please selected a day within 6 months in advance. Date : {caldDayCell.CellDate.ToString("dd MMM yyyy")}");
				}
				//else if ((caldDayCell.CellDate.Year != _selectedYear) || (caldDayCell.CellDate.Month != _selectedMonth))
				//{
				//	//ShowMsg("Please selected day of current month.");
				//}
				else
				{
					if (
						(_departDate.HasValue) && 
						((_returnDate.HasValue) || (_travelMode == TravelMode.DepartOnly))
						)
					{
						_returnDate = null;
						_departDate = caldDayCell.CellDate;

						RefreshCalendar();
						RefreshSearchButton();
					}
					else if ((_departDate.HasValue) &&
						(_departDate.Value.ToString("yyyyMMdd").Equals(caldDayCell.CellDate.ToString("yyyyMMdd"), StringComparison.InvariantCultureIgnoreCase)))
					{
						//_departDate = null;
						//RefreshCalendar();
					}
					else if ((_departDate.HasValue == false) || (_returnDate.HasValue == false))
					{
						if (_departDate.HasValue == false)
							_departDate = caldDayCell.CellDate;
						else
							_returnDate = caldDayCell.CellDate;

						if (_departDate.HasValue && _returnDate.HasValue)
						{
							if (_departDate.Value.Ticks > _returnDate.Value.Ticks)
							{
								DateTime tmpdepartDate = _departDate.Value;

								_departDate = _returnDate;
								_returnDate = tmpdepartDate;
							}
						}

						RefreshCalendar();
						RefreshSearchButton();
					}
					else
					{
						//ShowMsg("You need to unpick a selected date to reselect new date");
					}


					try
					{
						App.TimeoutManager.ResetTimeout();
					}
					catch (Exception ex)
					{
						App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgDate.DayCell_MouseDown");
					}
				}
			}
			else
			{
				//ShowMsg("CalendarDayCell NOT found !!");
			}
			//DayCellList
			//CalendarDayCell
		}

		private void BdSearch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (_departDate.HasValue == false)
					return;

				if (_selectedFlag == false)
				{
					App.MainScreenControl.ExecMenu.ShieldMenu();

					_selectedFlag = true;
					//App.NetClientSvc.SalesService.SubmitTravelDates( _departDate, _returnDate, out bool isServerResponded);
					//if (isServerResponded == false)
					//	App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000601)");
					Submit();
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000602); pgDate.BdSearch_MouseLeftButtonDown");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000602)", ex), "EX01", classNMethodName: "pgDate.BdSearch_MouseLeftButtonDown");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000602)");
			}
		}

		private void Submit()
		{
			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitTravelDates(_departDate, _returnDate, out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000601)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000603)");
					App.Log.LogError(_logChannel, "", new Exception("(EXIT10000603)", ex), "EX01", "pgStation.Submit");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		public void ShieldPage()
		{
			GrdScreenShield.Visibility = Visibility.Visible;
		}

		private void RefreshSearchButton()
		{
			if (_departDate.HasValue)
				BdSearch.Background = _enableButtonColor;
			else
				BdSearch.Background = _disableButtonColor;
		}

		//private void ShowMsg(string message)
		//{
		//	this.Dispatcher.Invoke(new Action(() => {

		//		if (message == null)
		//			TxtMsg.AppendText("\r\n");
		//		else
		//		{
		//			TxtMsg.AppendText($@"{DateTime.Now.ToString("HH:mm:ss")} - {message}{"\r\n"}");
		//		}
		//		TxtMsg.ScrollToEnd();
		//	}));
		//}

		private string WeekDayString(string engWord)
		{
			string wkDyStr = engWord;
			if (_language == LanguageCode.Malay)
			{
				if (engWord.ToUpper().Equals("MON"))
					wkDyStr = "ISN";
				else if (engWord.ToUpper().Equals("TUE"))
					wkDyStr = "SEL";
				else if (engWord.ToUpper().Equals("WED"))
					wkDyStr = "RAB";
				else if (engWord.ToUpper().Equals("THU"))
					wkDyStr = "KHA";
				else if (engWord.ToUpper().Equals("FRI"))
					wkDyStr = "JUM";
				else if (engWord.ToUpper().Equals("SAT"))
					wkDyStr = "SAB";
				else if (engWord.ToUpper().Equals("SUN"))
					wkDyStr = "AHA";
			}
			return wkDyStr;
		}

		private string MonthYearString(DateTime month)
		{
			string monthStr = month.ToString("MMMM");
			string yearStr = month.ToString("yyyy");
			string retStr = $@"{monthStr} {yearStr}";

			if (_language == LanguageCode.Malay)
			{
				switch (month.Month)
				{
					case 1:
						monthStr = "JANUARI";
						break;
					case 2:
						monthStr = "FEBRUARI";
						break;
					case 3:
						monthStr = "MAC";
						break;
					case 4:
						monthStr = "APRIL";
						break;
					case 5:
						monthStr = "MEI";
						break;
					case 6:
						monthStr = "JUN";
						break;
					case 7:
						monthStr = "JULY";
						break;
					case 8:
						monthStr = "OGOS";
						break;
					case 9:
						monthStr = "SEPTEMBER";
						break;
					case 10:
						monthStr = "OKTOBER";
						break;
					case 11:
						monthStr = "NOVEMBER";
						break;
					case 12:
						monthStr = "DISEMBER";
						break;
				}

				retStr = $@"{monthStr} {yearStr}";
			}

			return retStr;
		}

		//private void Button_Click(object sender, RoutedEventArgs e)
		//{
		//	throw new Exception("Test Error");
		//}

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
						BdSearch.Height = 60;
						BdStacker1.Height = 50;
						BdStacker2.Height = 750;
					}
					_changeScreenSizeCount++;
				}
			}
		}
	}
}
