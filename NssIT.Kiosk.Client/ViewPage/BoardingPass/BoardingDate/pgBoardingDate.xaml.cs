using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.ViewPage.Date;
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

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.BoardingDate
{
	/// <summary>
	/// ClassCode:EXIT80.18; Interaction logic for pgBoardingDate.xaml
	/// </summary>
	public partial class pgBoardingDate : Page, ICTBoardingDate, IKioskViewPage
	{
		private string _logChannel = "ViewPage";

		private bool _selectedFlag = false;
		private bool _pageLoaded = false;

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

		private ResourceDictionary _currentRosLang = null;
		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private string _busCompanyName = null;
		private string _busCompanyCode = null;
		private string _busCompanyLogoURL = null;

		private DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

		private NetServiceAnswerMan _netClientSvcAnswerMan = null;

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

		/// <summary>
		/// FuncCode:EXIT80.1801
		/// </summary>
		public pgBoardingDate()
        {
            InitializeComponent();

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\BoardingDate\rosBoardingDateMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\BoardingDate\rosBoardingDateEnglish.xaml");
		}

		/// <summary>
		/// FuncCode:EXIT80.1805
		/// </summary>
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

			UscNav.LoadControl(_language);

			this.Resources.MergedDictionaries.Clear();
			this.Resources.MergedDictionaries.Add(_currentRosLang);

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

			LoadCompanyLogo();
			Init(new DateTime(todayDate.Year, todayDate.Month, 1));
			RefreshCalendar();
			RefreshSearchButton();

			TxtBusCompanyName.Text = _busCompanyName ?? "";

			_pageLoaded = true;
			return;
			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			async Task LoadCompanyLogo()
            {
				if (_busCompanyLogoURL != null)
					ImgBusComapnyLogo.Source = await App.CTCompanyLogoCache.GetImage(_busCompanyLogoURL);
				else
    				ImgBusComapnyLogo.Source = null;

				ImgBusComapnyLogoX.Source = ImgBusComapnyLogo.Source;

			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1810
		/// </summary>
		private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
			_pageLoaded = false;
			_netClientSvcAnswerMan?.Dispose();
		}

		/// <summary>
		/// FuncCode:EXIT80.1815
		/// </summary>
		public void InitData(IKioskMsg kioskMsg)
		{
			if ((kioskMsg is IUserSession usrSession) && (usrSession.Session != null))
			{
				_language = usrSession.Session.Language;

				if (_language == LanguageCode.Malay)
					_currentRosLang = _langMal;
				else
					_currentRosLang = _langEng;

				_travelMode = TravelMode.DepartOnly;
				_departDate = null;
				_returnDate = null;

				_busCompanyName = (usrSession.Session.TicketCollection?.BusCompanyName) ?? "?UnKnown Bus Company?";
				_busCompanyCode = (usrSession.Session.TicketCollection?.BusCompanyCode) ?? "?UnKnown Bus Company Code?";
				_busCompanyLogoURL = (usrSession.Session.TicketCollection?.BusCompanyLogoURL) ?? null;
			}
			else
            {
				_language = LanguageCode.English;
				_currentRosLang = _langEng;

				_travelMode = TravelMode.DepartOnly;
				_departDate = null;
				_returnDate = null;

				_busCompanyName = "?UnKnown Bus Company?";
				_busCompanyCode = "?UnKnown Bus Company Code?";
				_busCompanyLogoURL = null;
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1820
		/// </summary>
		public void Init(DateTime? referMonth)
		{
			DateTime cTime = referMonth.HasValue ? referMonth.Value : DateTime.Now;
			_selectedYear = cTime.Year;
			_selectedMonth = cTime.Month;
		}

		/// <summary>
		/// FuncCode:EXIT80.1825
		/// </summary>
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
					App.Log.LogError(_logChannel, "-", new Exception($@"Tag: {tag}", ex), "EX01", "pgBoardingDate.ReNewCountdown");
				}
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1830
		/// </summary>
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

		/// <summary>
		/// FuncCode:EXIT80.1835
		/// </summary>
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
				ReNewCountdown("PreviousMonth_MouseDown");
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgBoardingDate.Previous_MouseDown");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1840
		/// </summary>
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
				ReNewCountdown("NextMonth_MouseDown");
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgBoardingDate.NextMonth_MouseDown");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1845
		/// </summary>
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
						ReNewCountdown("DayCell_MouseDown");
					}
					catch (Exception ex)
					{
						App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgBoardingDate.DayCell_MouseDown");
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

		/// <summary>
		/// FuncCode:EXIT80.1850
		/// </summary>
		private void BdSearch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if ((_departDate.HasValue == false) || (_pageLoaded = false))
					return;

				if (_selectedFlag == false)
				{
					_selectedFlag = true;
					Submit();
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1850.EX01); pgDate.BdSearch_MouseLeftButtonDown");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.1850.EX01)", ex), "EX01", classNMethodName: "pgBoardingDate.BdSearch_MouseLeftButtonDown");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1850.EX01)");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1855
		/// </summary>
		private void Submit()
		{
			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			try
			{
				_netClientSvcAnswerMan?.Dispose();

				_netClientSvcAnswerMan = App.NetClientSvc.CollectTicketService.SubmitDepartureDate(_departDate.Value,
					"Local Server not responding (EXIT80.1855.X1)",
						new NetServiceAnswerMan.FailLocalServerResponseCallBackDelg(delegate (string failMessage)
						{
							App.MainScreenControl.Alert(detailMsg: failMessage);
						}));

				ShieldPage();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT80.1855.EX01); pgBoardingDate.Submit");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT80.1855.EX01)", ex), "EX01", classNMethodName: "pgBoardingDate.Submit");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT80.1615.EX01)");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1860
		/// </summary>
		public void ShieldPage()
		{
			GrdScreenShield.Visibility = Visibility.Visible;
		}

		/// <summary>
		/// FuncCode:EXIT80.1865
		/// </summary>
		private void RefreshSearchButton()
		{
			if (_departDate.HasValue)
				BdSearch.Background = _enableButtonColor;
			else
				BdSearch.Background = _disableButtonColor;
		}

		/// <summary>
		/// FuncCode:EXIT80.1870
		/// </summary>
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

		/// <summary>
		/// FuncCode:EXIT80.1875
		/// </summary>
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

	}
}
