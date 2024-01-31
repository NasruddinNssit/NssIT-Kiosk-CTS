using NssIT.Kiosk.AppDecorator.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
	public class DayCellInfo : IDisposable
	{


		private static Brush _activedDayCellBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private static Brush _activedDateTextForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
		private static Brush _activedPriceTextForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x66, 0x66, 0x66));

		private static Brush _deactivedDayCellBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
		private static Brush _deactivedDateTextForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x85, 0x85, 0x85));
		private static Brush _deactivedPriceTextForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0xAD, 0xAD, 0xAD));

		private static int _tagLastInx = 0;
		private static string _preDayCellFix = "#DAYCELL#";

		private TextBlock _txtDay = null;
		private TextBlock _txtFromPrice = null;

		public static DateTime TodayDate = DateTime.Now;
		public static DateTime MinHistoricalDate = DateTime.Now.AddDays(-20);
		public static DateTime OutOfAdvanceDate = DateTime.Now.AddDays(100);
		public static DateTime MaxAdvanceDate = DateTime.Now.AddDays(120);

		public DayCellInfo(Border daycalendarCell)
		{
			_tagLastInx++;
			Tag = $@"{_preDayCellFix}{_tagLastInx:000}";
			daycalendarCell.Tag = Tag;
			DayCell = daycalendarCell;
		}

		
		private static CultureInfo _engProvider = new CultureInfo("en-US");
		private static CultureInfo _malProvider = new CultureInfo("ms-MY");
		private static CultureInfo _currentProvider = new CultureInfo("en-US");
		private static LanguageCode _language = LanguageCode.English;
		public static LanguageCode Language
		{
			get => _language;
			set
			{
				if ((value == LanguageCode.Malay) && (_language != value))
				{
					_currentProvider = _malProvider;
				}
				else
				{
					_currentProvider = _engProvider;
				}
			}
		}

		public Border DayCell { get; private set; } = null;
		public string Tag { get; private set; } = null;
		public bool DayIsValidForSelection { get; private set; } = true;

		private DateTime _date = DateTime.MinValue;
		public DateTime Day
		{
			get
			{
				return _date;
			}
			set
			{
				_date = value;
				WriteDayCellInfo(day: _date);
			}
		}

		public decimal _fromPrice = 0.0M;
		public decimal FromPrice
		{
			get
			{
				return _fromPrice;
			}
			set
			{
				_fromPrice = value;
				WriteDayCellInfo(fromPrice: value);
			}
		}

		public static bool IsValidDayCell(Border dayCell)
		{
			if (dayCell is null)
				return false;

			if ((dayCell.Tag != null) && (dayCell.Tag.ToString().IndexOf(_preDayCellFix) == 0))
				return true;
			else
				return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dayCell"></param>
		/// <param name="date">A null to ignore writing</param>
		/// <param name="fromPrice">A null to ignore writing</param>
		private void WriteDayCellInfo(DateTime? day = null, decimal? fromPrice = null)
		{
			if ((!day.HasValue) && (!fromPrice.HasValue))
				return;

			if (IsValidDayCell(DayCell))
			{
				if ((_txtDay is null) || (_txtFromPrice is null))
				{
					FindAllElements();
				}


				_txtFromPrice.Visibility = System.Windows.Visibility.Collapsed;
				///// Feature Temporary Not Available xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				//if (fromPrice.HasValue && (_txtFromPrice is null))
				//	throw new Exception("Unable to allocate From Price tag when writting Day Cell info.");
				//if (fromPrice.HasValue)
				//	_txtFromPrice.Text = $@"{Currency} {fromPrice.Value:#,###.00}";
				/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

				if (day.HasValue && (_txtDay is null))
					throw new Exception("Unable to allocate Day tag when writting Day Cell info.");

				if (day.HasValue)
				{
					if ((day.Value.Ticks >= TodayDate.Ticks) && (day.Value.Ticks < OutOfAdvanceDate.Ticks))
					{
						DayIsValidForSelection = true;
						//_txtDay.Text = $@"{Enum.GetName(typeof(DayOfWeek), day.Value.DayOfWeek).ToString().Substring(0, 3)}, {day.Value.ToString("MMM dd")}";
						_txtDay.Text = day.Value.ToString("ddd, MMM dd", _currentProvider);
					}
					else
					{
						DayIsValidForSelection = false;
						_txtDay.Text = "-";
					}
				}
			}
		}

		public void ChangeToActived()
		{
			if ((_txtDay is null) || (_txtFromPrice is null))
			{
				FindAllElements();
			}

			DayCell.Background = _activedDayCellBackground;
			_txtDay.Foreground = _activedDateTextForeground;
			_txtFromPrice.Foreground = _activedPriceTextForeground;
		}

		public void ChangeToDeactived()
		{
			if ((_txtDay is null) || (_txtFromPrice is null))
			{
				FindAllElements();
			}

			DayCell.Background = _deactivedDayCellBackground;
			_txtDay.Foreground = _deactivedDateTextForeground;
			_txtFromPrice.Foreground = _deactivedPriceTextForeground;
		}

		private void FindAllElements()
		{
			if (DayCell is null)
				return;

			foreach (var control in ((Grid)DayCell.Child).Children)
			{
				if (control is TextBlock txt)
				{
					if ((txt.Tag != null))
					{
						if ((txt.Tag.ToString().Equals("DAY")))
							_txtDay = txt;
						else if ((txt.Tag.ToString().Equals("PRICE")))
							_txtFromPrice = txt;
					}
				}
			}
		}

		public static string Currency
		{
			get
			{
				return "RM";
			}
		}

		public void Dispose()
		{
			DayCell = null;
			_txtDay = null;
			_txtFromPrice = null;
		}
	}
}
