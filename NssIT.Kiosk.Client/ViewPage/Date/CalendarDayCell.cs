using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Date
{
	public class CalendarDayCell
	{
		private Brush _circleNormalFillColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
		private Brush _circleSelectedFillColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x2B, 0x9C, 0xDB));

		private Brush _dayTextNormalColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x77, 0x77, 0x77));
		private Brush _dayTextSelectedColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
		private Brush _dayTextCrossOverColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x08, 0x08));

		private Brush _dayTextDisabledColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC));
		private Brush _dayTextNotRelatedColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));

		private double BlockCornerRadius { get; } = 7;

		private System.Windows.Threading.Dispatcher PageDispatcher { get; set; }

		private Border _dayCellX = null;
		private Border _leftBlockX = null;
		private Border _rightBlockX = null;

		private Ellipse _circleX = null;
		private TextBlock _dayInxTextX = null;
		private Line _dateLineX = null;

		public DateTime CellDate { get; private set; }
		public int SelectedYear { get; private set; }
		public int SelectedMonth { get; private set; }
		public CellState TheCellState { get; private set; }

		public CalendarDayCell(System.Windows.Threading.Dispatcher dispatcher, Border dayCellX, Border leftBlockX, Border rightBlockX, Ellipse circleX, TextBlock dayInxTextX, Line dateLineX)
		{
			PageDispatcher = dispatcher ?? throw new Exception("Dispatcher cannot be null; Message Calendar Day Cell");
			_dayCellX = dayCellX ?? throw new Exception("Day Cell cannot be null; Message Calendar Day Cell");
			_leftBlockX = leftBlockX ?? throw new Exception("Left Block cannot be null; Message Calendar Day Cell");
			_rightBlockX = rightBlockX ?? throw new Exception("Right Block cannot be null; Message Calendar Day Cell");
			_circleX = circleX ?? throw new Exception("Circle cannot be null; Message Calendar Day Cell");
			_dayInxTextX = dayInxTextX ?? throw new Exception("Day Inx. Text cannot be null; Message Calendar Day Cell");
			_dateLineX = dateLineX ?? throw new Exception("Date Line cannot be null; Message Calendar Day Cell");
		}

		public void InitData(DateTime nowTime, int selectedYear, int selectedMonth, DateTime cellDate, DateTime outOfAdvanceDate)
		{
			PageDispatcher.Invoke(new Action(() => {
				DateTime nowTime2 = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, 0, 0, 0, 0);
				DateTime cellDate2 = new DateTime(cellDate.Year, cellDate.Month, cellDate.Day, 0, 0, 0, 0);

				SelectedYear = selectedYear;
				SelectedMonth = selectedMonth;
				CellDate = cellDate;

				if ((cellDate.DayOfWeek == DayOfWeek.Saturday) || (cellDate.DayOfWeek == DayOfWeek.Sunday))
					_dayInxTextX.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
				else
					_dayInxTextX.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Text = cellDate.Day.ToString();
				_dateLineX.Visibility = Visibility.Collapsed;

				if (CellDate.Year == SelectedYear && CellDate.Month == SelectedMonth)
				{
					if (cellDate2.Ticks >= nowTime2.Ticks)
					{
						//TheCellState = CellState.Normal;
						//CellNormal();

						if (cellDate.Ticks < outOfAdvanceDate.Ticks)
						{
							TheCellState = CellState.Normal;
							CellNormal();
							SetLine(nowTime, cellDate);
						}
						else
						{
							TheCellState = CellState.Disabled;
							CellDisabled();
						}

					}
					else
					{
						TheCellState = CellState.Disabled;
						CellDisabled();
					}
				}
				else
				{
					//TheCellState = CellState.NotInCurrentMonth;
					//CellNotInCurrentMonth();
					
					if (cellDate2.Ticks < nowTime2.Ticks)
					{
						TheCellState = CellState.Disabled;
						CellDisabled();
					}
					else if (cellDate.Ticks < outOfAdvanceDate.Ticks)
					{
						TheCellState = CellState.Normal;
						CellNormal();
					}
					else
					{
						TheCellState = CellState.Disabled;
						CellDisabled();
					}
					
				}
			}));
		}

		private void SetLine(DateTime todayDate, DateTime cellDate)
		{
			if ((todayDate.Day == cellDate.Day) && (todayDate.Month == cellDate.Month) && (todayDate.Year == cellDate.Year))
				_dateLineX.Visibility = Visibility.Visible;
			else
				_dateLineX.Visibility = Visibility.Collapsed;
		}

		public void CellNormal()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Hidden;
				_rightBlockX.Visibility = Visibility.Hidden;

				_circleX.Fill = _circleNormalFillColor;
				//_circleX.Visibility = Visibility.Hidden;
				_circleX.Visibility = Visibility.Visible;

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextNormalColor;
			}));
		}

		public void CellDisabled()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Hidden;
				_rightBlockX.Visibility = Visibility.Hidden;

				_circleX.Fill = _circleNormalFillColor;
				//_circleX.Visibility = Visibility.Hidden;
				_circleX.Visibility = Visibility.Visible;

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextDisabledColor;
			}));
		}

		public void CellNotInCurrentMonth()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Hidden;
				_rightBlockX.Visibility = Visibility.Hidden;

				_circleX.Fill = _circleNormalFillColor;
				//_circleX.Visibility = Visibility.Hidden;
				_circleX.Visibility = Visibility.Visible;

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextNotRelatedColor;
			}));
		}

		public bool CellIsHidden
		{
			get
			{
				if ((_dayInxTextX.Visibility == Visibility.Hidden)
					|| (_dayInxTextX.Visibility == Visibility.Collapsed))
					return true;
				else
					return false;
			}
		}

		public void CellHide()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Hidden;
				_rightBlockX.Visibility = Visibility.Hidden;

				_circleX.Fill = _circleNormalFillColor;
				//_circleX.Visibility = Visibility.Hidden;
				_circleX.Visibility = Visibility.Visible;

				_dayInxTextX.Visibility = Visibility.Hidden;
				_dayInxTextX.Foreground = _dayTextDisabledColor;
			}));
		}

		public void CellSelectedSingleDay()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Hidden;
				_rightBlockX.Visibility = Visibility.Hidden;

				_circleX.Fill = _circleSelectedFillColor;
				_circleX.Visibility = Visibility.Visible;

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextSelectedColor;
			}));
		}

		/// <summary>
		/// Selection start at a last-day of the month OR last-day of the week
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CellSelectedStartDay()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Hidden;
				_rightBlockX.Visibility = Visibility.Visible;

				_circleX.Fill = _circleSelectedFillColor;
				_circleX.Visibility = Visibility.Visible;

				_rightBlockX.CornerRadius = new CornerRadius() { TopRight = BlockCornerRadius, BottomRight = BlockCornerRadius };

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextSelectedColor;
			}));
		}

		/// <summary>
		/// Selection end at a first-day of the month OR first-day of the week
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CellSelectedEndDay()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Visible;
				_rightBlockX.Visibility = Visibility.Hidden;

				_circleX.Fill = _circleSelectedFillColor;
				_circleX.Visibility = Visibility.Visible;

				_leftBlockX.CornerRadius = new CornerRadius() { TopLeft = BlockCornerRadius, BottomLeft = BlockCornerRadius };

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextSelectedColor;
			}));
		}

		/// <summary>
		/// Day not selected but related in between the selected range.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CellRelateSelectedRangeAcrossDay()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Visible;
				_rightBlockX.Visibility = Visibility.Visible;

				_circleX.Fill = _circleNormalFillColor;
				_circleX.Visibility = Visibility.Hidden;
				//_circleX.Visibility = Visibility.Visible;

				_leftBlockX.CornerRadius = new CornerRadius() { TopLeft = 0, BottomLeft = 0 };
				_rightBlockX.CornerRadius = new CornerRadius() { TopRight = 0, BottomRight = 0 };

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextSelectedColor;
			}));
		}

		/// <summary>
		/// Day not selected but related in between the selected range; Day is first day in month OR first day in the week. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CellRelateSelectedRangeAcrossFirstDay()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Visible;
				_rightBlockX.Visibility = Visibility.Visible;

				_circleX.Fill = _circleNormalFillColor;
				//_circleX.Visibility = Visibility.Hidden;
				_circleX.Visibility = Visibility.Visible;

				_leftBlockX.CornerRadius = new CornerRadius() { TopLeft = BlockCornerRadius, BottomLeft = BlockCornerRadius };
				_rightBlockX.CornerRadius = new CornerRadius() { TopRight = 0, BottomRight = 0 };

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextSelectedColor;
			}));
		}

		/// <summary>
		/// Day not selected but related in between the selected range; Day is last day in month OR last day in the week.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CellRelateSelectedRangeAcrossLastDay()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Visible;
				_rightBlockX.Visibility = Visibility.Visible;

				_circleX.Fill = _circleNormalFillColor;
				//_circleX.Visibility = Visibility.Hidden;
				_circleX.Visibility = Visibility.Visible;

				_leftBlockX.CornerRadius = new CornerRadius() { TopLeft = 0, BottomLeft = 0 };
				_rightBlockX.CornerRadius = new CornerRadius() { TopRight = BlockCornerRadius, BottomRight = BlockCornerRadius };

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextSelectedColor;
			}));
		}

		/// <summary>
		/// Day not selected but related in between the selected range; Day is first day in month appear at last Column OR Day is last day in month appear at first Column.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CellRelateSelectedRangeAcrossSingleColumn()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Visible;
				_rightBlockX.Visibility = Visibility.Visible;

				_circleX.Fill = _circleNormalFillColor;
				//_circleX.Visibility = Visibility.Hidden;
				_circleX.Visibility = Visibility.Visible;

				_leftBlockX.CornerRadius = new CornerRadius() { TopLeft = BlockCornerRadius, BottomLeft = BlockCornerRadius };
				_rightBlockX.CornerRadius = new CornerRadius() { TopRight = BlockCornerRadius, BottomRight = BlockCornerRadius };

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextSelectedColor;
			}));
		}

		/// <summary>
		/// Selection start at a non-last-day of the month AND non-last-day in the week.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CellRelateSelectedRangeAcrossStartDay()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Hidden;
				_rightBlockX.Visibility = Visibility.Visible;

				_circleX.Fill = _circleSelectedFillColor;
				_circleX.Visibility = Visibility.Visible;

				_rightBlockX.CornerRadius = new CornerRadius() { TopRight = 0, BottomRight = 0 };

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextSelectedColor;
			}));
		}

		/// <summary>
		/// Selection end at a non-first-day of the month OR non-first-day in the week. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CellRelateSelectedRangeAcrossEndDay()
		{
			PageDispatcher.Invoke(new Action(() => {
				_leftBlockX.Visibility = Visibility.Visible;
				_rightBlockX.Visibility = Visibility.Hidden;

				_circleX.Fill = _circleSelectedFillColor;
				_circleX.Visibility = Visibility.Visible;

				_leftBlockX.CornerRadius = new CornerRadius() { TopLeft = 0, BottomLeft = 0 };

				_dayInxTextX.Visibility = Visibility.Visible;
				_dayInxTextX.Foreground = _dayTextSelectedColor;
			}));
		}
	}
}
