using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Xml;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
	public class DayCellSelector : IDisposable
	{
		private const string LogChannel = "ViewPage";

		private const int _maxRangeOfAdvanceMonth = 3; /* 3 mounths advance*/

		public event EventHandler<TextMessageEventArgs> OnTextMessage;
		public event EventHandler<DateSelectedEventArgs> OnDateSelected;
		public event EventHandler OnBeginDayCellMoveAnimate;
		public event EventHandler OnBeginDayCellTouchMove;

		private (bool HasValue, double CvDaysFrameLeftMargin, double CvDaysFrameRightMargin, double FirstLeftOfDayCell) _defaultSetting
			= (HasValue: false, CvDaysFrameLeftMargin: 0.0D, CvDaysFrameRightMargin: 0.0D, FirstLeftOfDayCell: 0.0D);

		private (bool DataIsValid, Point InitPoint, object InitSource, Point LastStartingPoint, double leftEdge, double rightEdge, bool TouchMovingOccurred) _dayCellTouchScroll
			= (DataIsValid: false, InitPoint: new Point(), InitSource: null, LastStartingPoint: new Point(), leftEdge: 0D, rightEdge: 0D, TouchMovingOccurred: false);

		private string _bdDaySelectionCellXaml = @"<Border Canvas.Left=""1200"" Canvas.Top=""0"" 
				Background=""#FFE8E8E8""
                BorderBrush=""#FFD0D1D8"" BorderThickness=""1"" 
                Width=""130"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">

			<Border.Effect>
                <DropShadowEffect ShadowDepth=""0"" Direction=""270"" Color=""#FF898989"" RenderingBias=""Quality"" BlurRadius=""0"" />
            </Border.Effect>

            <Grid Tag=""DAYCELLGRID"">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width = ""*""/>
				</Grid.ColumnDefinitions>
				<Grid.RowDefinitions>
					<RowDefinition Height=""5""/>
                    <RowDefinition Height = ""25""/>
					<RowDefinition Height=""5""/>
                    <RowDefinition Height = ""5""/>
				</Grid.RowDefinitions>

				<TextBlock Tag=""DAY"" Grid.Column=""0"" Grid.Row=""1"" Grid.RowSpan=""2"" Text=""Fri, 14 Jun"" FontSize=""16"" Foreground=""#FF858585"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"" />
                <TextBlock Tag=""PRICE"" Grid.Column=""0"" Grid.Row=""2"" Text=""From 18.00"" FontSize=""12"" Foreground=""#FFADADAD"" VerticalAlignment=""Top"" HorizontalAlignment=""Center"" />
			</Grid>
		</Border>";

		private string _cvDayFrameStoryScript = @"<Storyboard x:Key=""cvDayFrameStory"" 
			xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            
            <ThicknessAnimationUsingKeyFrames x:Name=""MovingMarginCollection"" 
                Storyboard.TargetProperty=""Margin"" BeginTime=""0:0:0""
                DecelerationRatio=""0.5"" Storyboard.TargetName=""cvDaysFrame"">
                <LinearThicknessKeyFrame x:Name=""MovingMargin1"" KeyTime=""0:0:0.5"" Value=""-3000, 0, -3000, 0"" />
            </ThicknessAnimationUsingKeyFrames>

            <DoubleAnimationUsingKeyFrames x:Name=""AniRaiseShadowDepth"" Storyboard.TargetProperty=""ShadowDepth"" Storyboard.TargetName=""DpRaiseShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.5"" Value=""3"" />
            </DoubleAnimationUsingKeyFrames>

            <DoubleAnimationUsingKeyFrames x:Name=""AniRaiseBlurRadius"" Storyboard.TargetProperty=""BlurRadius"" Storyboard.TargetName=""DpRaiseShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.5"" Value=""5"" />
            </DoubleAnimationUsingKeyFrames>

            <DoubleAnimationUsingKeyFrames x:Name=""AniDropShadowDepth"" Storyboard.TargetProperty=""ShadowDepth"" Storyboard.TargetName=""DpDropShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.5"" Value=""0"" />
            </DoubleAnimationUsingKeyFrames>

            <DoubleAnimationUsingKeyFrames x:Name=""AniDropBlurRadius"" Storyboard.TargetProperty=""BlurRadius"" Storyboard.TargetName=""DpDropShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.5"" Value=""0"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

		private string _cvResetDayFrameStoryScript = @"<Storyboard x:Key=""cvResetDayFrameStory"" 
			xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">

            <ThicknessAnimationUsingKeyFrames x:Name=""ResetMovingMarginCollection"" 
                Storyboard.TargetProperty=""Margin"" BeginTime=""0:0:0""
                DecelerationRatio=""0.5"" Storyboard.TargetName=""cvDaysFrame"">
                <LinearThicknessKeyFrame x:Name=""ResetMovingMargin1"" KeyTime=""0:0:0.05"" Value=""-3000, 0, -3000, 0"" />
            </ThicknessAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames x:Name=""AniResetShadowDepth"" Storyboard.TargetProperty=""ShadowDepth"" Storyboard.TargetName=""DpRaiseShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.05"" Value=""0"" />
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames x:Name=""AniResetBlurRadius"" Storyboard.TargetProperty=""BlurRadius"" Storyboard.TargetName=""DpRaiseShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.05"" Value=""0"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

		private string _cvResetDayFrameStoryScript2 = @"<Storyboard x:Key=""cvResetDayFrameStory"" 
			xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">

            <ThicknessAnimationUsingKeyFrames x:Name=""ResetMovingMarginCollection"" 
                Storyboard.TargetProperty=""Margin"" BeginTime=""0:0:0""
                DecelerationRatio=""0.5"" Storyboard.TargetName=""cvDaysFrame"">
                <LinearThicknessKeyFrame x:Name=""ResetMovingMargin1"" KeyTime=""0:0:0.05"" Value=""-3000, 0, -3000, 0"" />
            </ThicknessAnimationUsingKeyFrames>

        </Storyboard>";

		private string _cvRaiseSelectedDayStory = @"<Storyboard x:Key=""cvRaiseSelectedDayStory"" 
			xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">

            <DoubleAnimationUsingKeyFrames x:Name=""AniRaiseSelectedDayShadowDepth"" Storyboard.TargetProperty=""ShadowDepth"" Storyboard.TargetName=""DpRaiseShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.5"" Value=""3"" />
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames x:Name=""AniRaiseSelectedDayBlurRadius"" Storyboard.TargetProperty=""BlurRadius"" Storyboard.TargetName=""DpRaiseShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.5"" Value=""5"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

		private string _effResetRaisedShadowStoryScript = @"<Storyboard x:Key=""effResetRaisedShadowStory"" 
			xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">

            <DoubleAnimationUsingKeyFrames x:Name=""AniResetShadowDepth"" Storyboard.TargetProperty=""ShadowDepth"" Storyboard.TargetName=""DpRaiseShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.05"" Value=""0"" />
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames x:Name=""AniResetBlurRadius"" Storyboard.TargetProperty=""BlurRadius"" Storyboard.TargetName=""DpRaiseShadow"">
                <LinearDoubleKeyFrame KeyTime = ""0:0:0.05"" Value=""0"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

		private double _outFlameMargin = 3000;

		private Border _refDayCalendarCell = null;
		private Border _raiseDayCell = null;
		private Border _dropDayCell = null;
		private ListView _lstTripViewer = null;

		private DayFrameMovingInfo _lastDayFrameMovingInfo = null;
		private Page _pgDaySelection = null;
		private Canvas _cvDaysFrame = null;
		private ScrollViewer _scvDayCalendarContainer = null;

		private Dictionary<string, DayCellInfo> _dayCellList = new Dictionary<string, DayCellInfo>();

		private DateTime _todayDate = DateTime.Now;
		private DateTime _minHistoricalDate = DateTime.Now.AddDays(-20);
		private DateTime _outOfAdvanceDate = DateTime.Now.AddDays(100);
		private DateTime _maxAdvanceDate = DateTime.Now.AddDays(120);
		public DayCellSelector(Page pgDaySelection, Canvas cvDaysFrame, ScrollViewer scvDayCalendarContainer, ListView lstTripViewer = null)
		{
			_pgDaySelection = pgDaySelection;
			_cvDaysFrame = cvDaysFrame;
			_scvDayCalendarContainer = scvDayCalendarContainer;
			_lstTripViewer = lstTripViewer;

			cvDaysFrame.Margin = new Thickness((_outFlameMargin * -1), 0, (_outFlameMargin * -1), 0);

			_cvDaysFrame.MouseDown += _cvDaysFrame_MouseDown;
			_cvDaysFrame.MouseMove += _cvDaysFrame_MouseMove;
			_cvDaysFrame.MouseUp += _cvDaysFrame_MouseUp;
			_cvDaysFrame.MouseLeave += _cvDaysFrame_MouseLeave;
		}

		private SemaphoreSlim _touchMoveLock = new SemaphoreSlim(1);
		private async void _cvDaysFrame_MouseLeave(object sender, MouseEventArgs e)
		{
			try
			{
				await _touchMoveLock.WaitAsync();
				ShowTouchMovingInfo(e.GetPosition(_cvDaysFrame), "_cvDaysFrame_MouseLeave");

				if (_dayCellTouchScroll.DataIsValid && (_dayCellTouchScroll.TouchMovingOccurred == false))
				{
					if (TryMoveDaysFrame(e.Source, out string errMsg, out string msg))
					{
						//if (msg != null)
						//	ShowMsg(msg);

						//ShowMsg("Move DaysFrame");
					}
					else
					{
						//if (errMsg is null)
						//	errMsg = "Unable to move DaysFrame; Unable to allocate error.";

						//ShowMsg(errMsg);
					}
				}

				_dayCellTouchScroll.DataIsValid = false;

			}
			finally
			{
				_touchMoveLock.Release();
			}
		}

		private async void _cvDaysFrame_MouseUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				await _touchMoveLock.WaitAsync();
				ShowTouchMovingInfo(e.GetPosition(_cvDaysFrame), "_cvDaysFrame_MouseUp");

				if (_dayCellTouchScroll.DataIsValid && (_dayCellTouchScroll.TouchMovingOccurred == false))
				{
					if (TryMoveDaysFrame(e.Source, out string errMsg, out string msg))
					{
						//if (msg != null)
						//	ShowMsg(msg);

						//ShowMsg("Move DaysFrame");
					}
					else
					{
						//if (errMsg is null)
						//	errMsg = "Unable to move DaysFrame; Unable to allocate error.";

						//ShowMsg(errMsg);
					}
				}
				_dayCellTouchScroll.DataIsValid = false;
			}
			finally
			{
				_touchMoveLock.Release();
			}
		}

		private double minimumStartMovingRange = 20D;
		private void _cvDaysFrame_MouseMove(object sender, MouseEventArgs e)
		{
			bool isInitiateState = false;
			ShowTouchMovingInfo(e.GetPosition(_cvDaysFrame), "_cvDaysFrame_MouseMove");

			if (_dayCellTouchScroll.DataIsValid)
			{
				App.TimeoutManager.ResetTimeout();

				Point currPoint = e.GetPosition(_cvDaysFrame);

				// When movedRange is positive, mean move cell from left to right
				double movedRange = currPoint.X - _dayCellTouchScroll.LastStartingPoint.X;

				//-----------------------------------------------------------------------
				bool agreeToMove = true;

				DayCellInfo[] cellsList = (from kpDayInfo in _dayCellList
										   orderby kpDayInfo.Value.Day
										   select kpDayInfo.Value).ToArray();

				DayCellInfo leftMostCell = cellsList[0];
				DayCellInfo rightMostCell = cellsList[cellsList.Length - 1];

				//-----------------------------------------------------------------------
				// Validate for Moving
				if (movedRange > 0)
				{
					// Validate for Moving cell from left to right
					if (leftMostCell.Day.Ticks <= _minHistoricalDate.Ticks)
					{
						agreeToMove = false;
					}
				}
				else /* if (movedRange < 0) */
				{
					// Validate for Moving cell from right to left
					if (rightMostCell.Day.Ticks >= _maxAdvanceDate.Ticks)
					{
						agreeToMove = false;
					}
				}

				//-----------------------------------------------------------------------
				if (agreeToMove)
				{
					if (_dayCellTouchScroll.TouchMovingOccurred == false)
					{
						double ActualMovedRange = (movedRange < 0) ? (movedRange * -1) : movedRange;

						if (ActualMovedRange > minimumStartMovingRange)
						{
							_dayCellTouchScroll.TouchMovingOccurred = true;
							isInitiateState = true;
						}
					}

					if ((_dayCellTouchScroll.TouchMovingOccurred) && (movedRange != 0))
					{
						_dayCellTouchScroll.LastStartingPoint = currPoint;
						// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
						// Move All Cell

						if (isInitiateState)
						{
							RaiseOnBeginDayCellTouchMove();
							ResetRaisedShadow();
						}

						foreach (DayCellInfo cellInfo in cellsList)
						{
							Canvas.SetLeft(cellInfo.DayCell, Canvas.GetLeft(cellInfo.DayCell) + movedRange);
						}

						// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
						// Trim End of Cell
						if (movedRange > 0)
						{
							// Move cell from left to right
							DayCellInfo rightMostCellInfo = (from kpDayInfo in _dayCellList
															 orderby kpDayInfo.Value.Day descending
															 select kpDayInfo.Value).Take(1).ToArray()[0];

							double currRightEdge = Canvas.GetLeft(rightMostCellInfo.DayCell) + _refDayCalendarCell.Width;

							if (currRightEdge > _dayCellTouchScroll.rightEdge)
							{
								// Re-allocate the right most cell to left side
								DayCellInfo leftMostCellInfo = (from kpDayInfo in _dayCellList
																orderby kpDayInfo.Value.Day
																select kpDayInfo.Value).Take(1).ToArray()[0];

								_dayCellList.Remove(rightMostCellInfo.Tag);



								Canvas.SetLeft(rightMostCellInfo.DayCell, Canvas.GetLeft(leftMostCellInfo.DayCell) - _refDayCalendarCell.Width);
								rightMostCellInfo.Day = leftMostCellInfo.Day.AddDays(-1);

								_dayCellList.Add(rightMostCellInfo.Tag, rightMostCellInfo);
							}
						}
						else if (movedRange < 0)
						{
							// Move cell from right to left
							DayCellInfo leftMostCellInfo = (from kpDayInfo in _dayCellList
															orderby kpDayInfo.Value.Day
															select kpDayInfo.Value).Take(1).ToArray()[0];

							double currLeftEdge = Canvas.GetLeft(leftMostCellInfo.DayCell);

							if (currLeftEdge < _dayCellTouchScroll.leftEdge)
							{
								// Re-allocate the left most cell to right side
								DayCellInfo rightMostCellInfo = (from kpDayInfo in _dayCellList
																 orderby kpDayInfo.Value.Day descending
																 select kpDayInfo.Value).Take(1).ToArray()[0];

								_dayCellList.Remove(leftMostCellInfo.Tag);

								Canvas.SetLeft(leftMostCellInfo.DayCell, Canvas.GetLeft(rightMostCellInfo.DayCell) + _refDayCalendarCell.Width);
								leftMostCellInfo.Day = rightMostCellInfo.Day.AddDays(1);

								_dayCellList.Add(leftMostCellInfo.Tag, leftMostCellInfo);

							}
						}

						// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
					}
				}
				else
				{
					if (_dayCellTouchScroll.TouchMovingOccurred == false)
					{
						double ActualMovedRange = (movedRange < 0) ? (movedRange * -1) : movedRange;

						if (ActualMovedRange > minimumStartMovingRange)
						{
							_dayCellTouchScroll.TouchMovingOccurred = true;
						}
					}
				}

			}
		}

		private void _cvDaysFrame_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (_dayCellTouchScroll.DataIsValid == false)
			{
				_dayCellTouchScroll.DataIsValid = true;
				_dayCellTouchScroll.InitPoint = e.GetPosition(_cvDaysFrame);
				_dayCellTouchScroll.InitSource = e.Source;

				_dayCellTouchScroll.LastStartingPoint = _dayCellTouchScroll.InitPoint;
				_dayCellTouchScroll.TouchMovingOccurred = false;

				double allCellsWidth = (double)_dayCellList.Count * _refDayCalendarCell.Width;
				double halftof_AllCellsWidth = allCellsWidth / 2D;
				double leftOutViewCellsWidth = halftof_AllCellsWidth - (_scvDayCalendarContainer.ActualHeight / 2D);

				_dayCellTouchScroll.leftEdge = (_cvDaysFrame.Margin.Left * -1) - leftOutViewCellsWidth;
				_dayCellTouchScroll.rightEdge = _dayCellTouchScroll.leftEdge + allCellsWidth;
			}
			ShowTouchMovingInfo(e.GetPosition(_cvDaysFrame), "_cvDaysFrame_MouseDown");

			//MainWindow.ShowMessage($@"_cvDaysFrame_MouseDown; Point X: {e.GetPosition(_cvDaysFrame).X}; leftEdge: {_dayCellTouchScroll.leftEdge}", null);

			//if (TryMoveDaysFrame(e.Source, out string errMsg, out string msg))
			//{
			//	//if (msg != null)
			//	//	ShowMsg(msg);

			//	//ShowMsg("Move DaysFrame");
			//}
			//else
			//{
			//	//if (errMsg is null)
			//	//	errMsg = "Unable to move DaysFrame; Unable to allocate error.";

			//	//ShowMsg(errMsg);
			//}
		}

		private void ShowTouchMovingInfo(Point point, string msg)
		{
			//MainWindow.ShowMessage($@"{msg} Point X: {point.X}", null);
		}

		private DateTime _selectedDate = DateTime.MinValue;
		public DateTime SelectedDate
		{
			get { return _selectedDate; }
			set { _selectedDate = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, 0); }
		}

		#region -- Story -- Day Selected --

		private Storyboard _daySelectedAnimator = null;
		public Storyboard DaySelectedAnimator
		{
			get
			{
				if (_daySelectedAnimator is null)
				{
					MemoryStream storyStream = null;
					StreamReader storyReader = null;
					try
					{
						storyStream = new MemoryStream(Encoding.UTF8.GetBytes(_cvDayFrameStoryScript));
						storyReader = new StreamReader(storyStream);

						storyStream.Seek(0, SeekOrigin.Begin);
						XmlReader xmlReaderRow = XmlReader.Create(storyReader);
						_daySelectedAnimator = (Storyboard)XamlReader.Load(xmlReaderRow);

						_daySelectedAnimator.Completed += _daySelectedAnimator_Completed;
					}
					finally
					{
						if (storyReader != null)
							storyReader.Dispose();
						if (storyStream != null)
							storyStream.Dispose();
					}
				}

				return _daySelectedAnimator;
			}
		}

		private LinearThicknessKeyFrame _marginAnimator;
		private LinearThicknessKeyFrame MovingMargin
		{
			get
			{
				if (_marginAnimator == null)
				{
					Storyboard currStoryboard = DaySelectedAnimator;

					foreach (var obj in currStoryboard.Children)
					{
						if (obj is ThicknessAnimationUsingKeyFrames movMarColl)
							if (movMarColl.Name.Equals("MovingMarginCollection"))
								foreach (var obj2 in movMarColl.KeyFrames)
									if (obj2 is LinearThicknessKeyFrame movMar)
									{
										_marginAnimator = movMar;
										break;
									}

						if (_marginAnimator != null)
							break;
					}
				}

				return _marginAnimator;
			}
		}

		private int _intervalMoveDelaySec = 3;
		private DateTime _lastMovingTime = DateTime.MinValue;
		public bool TryMoveDaysFrame(object relatedControl, out string errMsg, out string msg)
		{
			DateTime validToMoveStartAt = _lastMovingTime.AddSeconds(_intervalMoveDelaySec);
			errMsg = null;
			msg = null;
			bool isValidToMove = false;
			Border dayCellX = null;

			if (relatedControl is Border dayCell)
			{
				if (DayCellInfo.IsValidDayCell(dayCell))
				{
					dayCellX = dayCell;
				}
				else
				{
					errMsg = "cvDaysFrame_MouseDown : Unregconized DayCell";
				}
			}
			else if (relatedControl is Grid dayCellGrid)
			{
				if ((dayCellGrid.Tag != null) && (dayCellGrid.Tag.ToString().Equals("DAYCELLGRID")))
				{
					dayCellX = (Border)dayCellGrid.Parent;
				}
				else
				{
					errMsg = "TryMoveDaysFrame : Unregconized Grid";
				}
			}
			else if (relatedControl is TextBlock dayText)
			{
				if (dayText.Tag != null)
				{
					if ((dayText.Tag.ToString().Equals("DAY")) || (dayText.Tag.ToString().Equals("PRICE")))
					{
						dayCellX = (Border)((Grid)dayText.Parent).Parent;
						msg = $@"TryMoveDaysFrame : {dayText.Tag.ToString()} TextBlock";
					}
					else
					{
						errMsg = "TryMoveDaysFrame : Unregconized TextBlock";
					}
				}
			}

			if ((dayCellX != null) && (DateTime.Now.Subtract(validToMoveStartAt).TotalMilliseconds > 0))
			{
				if (_dayCellList.TryGetValue(dayCellX.Tag.ToString(), out DayCellInfo selCell))
				{
					if (selCell.DayIsValidForSelection)
					{
						isValidToMove = true;
						msg = $@"Selected Day Cell Tag : {dayCellX.Tag.ToString()}";

						_lastMovingTime = DateTime.Now;
						MoveDaysFrame(dayCellX);
					}
					else
						errMsg = "Selected date is out of range";
				}
			}

			return isValidToMove;
		}

		private void MoveDaysFrame(Border selectedDayCell)
		{
			// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
			// Verification 
			if (_lastDayFrameMovingInfo != null)
				return;
			else if (selectedDayCell is null)
				return;
			else if ((_raiseDayCell != null) && (_raiseDayCell.Tag != null) && (selectedDayCell.Tag != null) && (_raiseDayCell.Tag.ToString().Equals(selectedDayCell.Tag.ToString())))
				return;
			// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
			// Calibrate Storyboard properties

			if (_cvDaysFrame.FindName("DpRaiseShadow") != null)
				_pgDaySelection.UnregisterName("DpRaiseShadow");

			if (_cvDaysFrame.FindName("DpDropShadow") != null)
				_pgDaySelection.UnregisterName("DpDropShadow");

			if (_raiseDayCell != null)
				_dropDayCell = _raiseDayCell;
			else
			{
				DayCellInfo cellInfo = (from keyPairDayCell in _dayCellList
										orderby keyPairDayCell.Value.Day
										select keyPairDayCell.Value).Take(1).ToArray()[0];

				_dropDayCell = cellInfo.DayCell;
			}

			_raiseDayCell = selectedDayCell;

			_pgDaySelection.RegisterName("DpDropShadow", (DropShadowEffect)_dropDayCell.Effect);
			_pgDaySelection.RegisterName("DpRaiseShadow", (DropShadowEffect)_raiseDayCell.Effect);

			// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
			// Check direction for moving the _raiseDayCell 
			double totalCellUnits = _dayCellList.Count;
			double totalCellsWidth = _refDayCalendarCell.Width * totalCellUnits;
			double halfTotalCellsWidth = totalCellsWidth / 2;
			double centralPoint = (_cvDaysFrame.Margin.Left * -1) + (_scvDayCalendarContainer.ActualWidth / 2D);

			double leftMostEdge = centralPoint - halfTotalCellsWidth;
			double rightMostEdge = centralPoint + halfTotalCellsWidth;

			double halfCellWidth = (_refDayCalendarCell.Width / 2);
			double centralPointLeftSide = centralPoint - halfCellWidth;
			double centralPointRightSide = centralPoint + halfCellWidth;

			//MoveDirection moveDirection = MoveDirection.ToLeft;
			double differenceCellDistance = 0D;
			double trimCellDistance = 0D;

			_lastDayFrameMovingInfo = new DayFrameMovingInfo();

			if ((Canvas.GetLeft(_raiseDayCell) == centralPointLeftSide)
				||
				(Canvas.GetLeft(_raiseDayCell) < centralPointLeftSide))
			{
				differenceCellDistance = centralPointLeftSide - Canvas.GetLeft(_raiseDayCell);

				_lastDayFrameMovingInfo.MoveDistance = differenceCellDistance;
				_lastDayFrameMovingInfo.MoveDirection = MoveDirection.ToRight;

				DayCellInfo rightMostCell = (from keyPairDayCell in _dayCellList
											 orderby keyPairDayCell.Value.Day descending
											 select keyPairDayCell.Value).Take(1).ToArray()[0];

				trimCellDistance = ((Canvas.GetLeft(rightMostCell.DayCell)) + _refDayCalendarCell.Width - rightMostEdge) + differenceCellDistance;
				if (trimCellDistance > 0)
				{
					_lastDayFrameMovingInfo.NumberOfMovingCell = (double)((int)Math.Ceiling(trimCellDistance / _refDayCalendarCell.Width));
				}
				else
					_lastDayFrameMovingInfo.NumberOfMovingCell = 0;
			}
			else
			{
				differenceCellDistance = Canvas.GetLeft(_raiseDayCell) - centralPointLeftSide;

				_lastDayFrameMovingInfo.MoveDistance = differenceCellDistance;
				_lastDayFrameMovingInfo.MoveDirection = MoveDirection.ToLeft;

				DayCellInfo leftMostCell = (from keyPairDayCell in _dayCellList
											orderby keyPairDayCell.Value.Day
											select keyPairDayCell.Value).Take(1).ToArray()[0];

				trimCellDistance = (leftMostEdge - (Canvas.GetLeft(leftMostCell.DayCell))) + differenceCellDistance;
				if (trimCellDistance > 0)
				{
					_lastDayFrameMovingInfo.NumberOfMovingCell = (double)((int)Math.Ceiling(trimCellDistance / _refDayCalendarCell.Width));
				}
				else
					_lastDayFrameMovingInfo.NumberOfMovingCell = 0;
			}

			//if (Canvas.GetLeft(_raiseDayCell) < centralPoint)
			//{
			//	differenceCellDistance = centralPoint - Canvas.GetLeft(_raiseDayCell);

			//	_lastDayFrameMovingInfo.MoveDirection = MoveDirection.ToRight;
			//	_lastDayFrameMovingInfo.NumberOfMovingCell = (double)((int)Math.Ceiling(differenceCellDistance / _refDayCalendarCell.Width));
			//	_lastDayFrameMovingInfo.NumberOfMovingCell = (_lastDayFrameMovingInfo.NumberOfMovingCell < 1) ? 1D : _lastDayFrameMovingInfo.NumberOfMovingCell;

			//	// .. remove 1 extra Cell width when moving to right.
			//	_lastDayFrameMovingInfo.NumberOfMovingCell = (_lastDayFrameMovingInfo.NumberOfMovingCell >= 2) ? (_lastDayFrameMovingInfo.NumberOfMovingCell - 1) : _lastDayFrameMovingInfo.NumberOfMovingCell;
			//}
			//else
			//{
			//	differenceCellDistance = Canvas.GetLeft(_raiseDayCell) - centralPoint;

			//	_lastDayFrameMovingInfo.MoveDirection = MoveDirection.ToLeft;
			//	_lastDayFrameMovingInfo.NumberOfMovingCell = (double)((int)Math.Ceiling(differenceCellDistance / _refDayCalendarCell.Width));
			//	_lastDayFrameMovingInfo.NumberOfMovingCell = (_lastDayFrameMovingInfo.NumberOfMovingCell < 1) ? 1D : _lastDayFrameMovingInfo.NumberOfMovingCell;
			//}

			// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
			// Moving _raiseDayCell 
			if (_lastDayFrameMovingInfo.MoveDirection == MoveDirection.Unchange)
				return;

			if ((_lastDayFrameMovingInfo.MoveDirection == MoveDirection.ToLeft) || (_lastDayFrameMovingInfo.MoveDirection == MoveDirection.ToRight))
			{
				// Activate & Deactivate Days Cell
				if (_dayCellList.TryGetValue(_raiseDayCell.Tag.ToString(), out DayCellInfo raiseDayInfo))
					raiseDayInfo.ChangeToActived();
				if (_dayCellList.TryGetValue(_dropDayCell.Tag.ToString(), out DayCellInfo dropDayInfo))
					dropDayInfo.ChangeToDeactived();

				// Moving
				if (_lastDayFrameMovingInfo.MoveDirection == MoveDirection.ToLeft)
				{
					MovingMargin.Value = new Thickness((_cvDaysFrame.Margin.Left - _lastDayFrameMovingInfo.MoveDistance), 0, (_scvDayCalendarContainer.ActualWidth + 3000) * -1, 0);
					_lastDayFrameMovingInfo.MovingDayCell = _raiseDayCell;
					SelectedDate = raiseDayInfo.Day;

					RaiseOnBeginDayCellMoveAnimate();
					DaySelectedAnimator.Begin(_pgDaySelection);
				}
				else if (_lastDayFrameMovingInfo.MoveDirection == MoveDirection.ToRight)
				{
					MovingMargin.Value = new Thickness((_cvDaysFrame.Margin.Left + _lastDayFrameMovingInfo.MoveDistance), 0, (_scvDayCalendarContainer.ActualWidth + 3000) * -1, 0);
					_lastDayFrameMovingInfo.MovingDayCell = _raiseDayCell;
					SelectedDate = raiseDayInfo.Day;

					RaiseOnBeginDayCellMoveAnimate();
					DaySelectedAnimator.Begin(_pgDaySelection);
				}
			}

			// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
		}

		public void _daySelectedAnimator_Completed(object sender, EventArgs e)
		{
			try
			{
				DaySelectedAnimator.Stop();

				if (_lastDayFrameMovingInfo != null)
				{
					Border mvDayCell = _lastDayFrameMovingInfo.MovingDayCell;

					// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
					// Re-calibrate cvDaysFrame by trimming day cells.

					if (_lastDayFrameMovingInfo.NumberOfMovingCell > 0)
					{
						if (_lastDayFrameMovingInfo.MoveDirection == MoveDirection.ToLeft)
						{
							DayCellInfo[] removeList = (from kpDayInfo in _dayCellList
														orderby kpDayInfo.Value.Day
														select kpDayInfo.Value).Take((int)_lastDayFrameMovingInfo.NumberOfMovingCell).ToArray();

							DayCellInfo lastCell = (from kpDayInfo in _dayCellList
													orderby kpDayInfo.Value.Day descending
													select kpDayInfo.Value).Take(1).ToArray()[0];

							// Trim Extra Left Cells and move to right side of _cvDaysFrame
							foreach (DayCellInfo cellInfo in removeList)
							{
								_cvDaysFrame.Children.Remove(cellInfo.DayCell);

								if (_dayCellList.TryGetValue(cellInfo.Tag, out DayCellInfo cellInfoX))
								{
									_dayCellList.Remove(cellInfoX.Tag);

									Canvas.SetLeft(cellInfoX.DayCell, Canvas.GetLeft(lastCell.DayCell) + _refDayCalendarCell.Width);

									cellInfoX.Day = lastCell.Day.AddDays(1);

									_cvDaysFrame.Children.Add(cellInfoX.DayCell);
									_dayCellList.Add(cellInfoX.Tag, cellInfoX);

									lastCell = cellInfoX;
								}
							}
						}
						else if (_lastDayFrameMovingInfo.MoveDirection == MoveDirection.ToRight)
						{
							DayCellInfo[] removeList = (from kpDayInfo in _dayCellList
														orderby kpDayInfo.Value.Day descending
														select kpDayInfo.Value).Take((int)_lastDayFrameMovingInfo.NumberOfMovingCell).ToArray();

							DayCellInfo firstCell = (from kpDayInfo in _dayCellList
													 orderby kpDayInfo.Value.Day
													 select kpDayInfo.Value).Take(1).ToArray()[0];

							// Trim Extra Right Cells and move to left side of _cvDaysFrame
							foreach (DayCellInfo cellInfo in removeList)
							{
								_cvDaysFrame.Children.Remove(cellInfo.DayCell);

								if (_dayCellList.TryGetValue(cellInfo.Tag, out DayCellInfo cellInfoX))
								{
									_dayCellList.Remove(cellInfoX.Tag);

									Canvas.SetLeft(cellInfoX.DayCell, Canvas.GetLeft(firstCell.DayCell) - _refDayCalendarCell.Width);

									cellInfoX.Day = firstCell.Day.AddDays(-1);

									_cvDaysFrame.Children.Add(cellInfoX.DayCell);
									_dayCellList.Add(cellInfoX.Tag, cellInfoX);

									firstCell = cellInfoX;
								}
							}
						}
					}

					RaiseOnDateSelected(SelectedDate);
					// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
				}
			}
			finally
			{
				_lastDayFrameMovingInfo = null;
			}
		}
		#endregion

		#region -- Story -- Reset Day Frame --

		private Storyboard _resetDayFrameAnimator = null;
		public Storyboard ResetDayFrameAnimator
		{
			get
			{
				if (_resetDayFrameAnimator != null)
				{
					try
					{
						_resetDayFrameAnimator.Completed -= _resetDayFrameAnimator_Completed;
					}
					catch (Exception ex)
					{
						App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000405)", ex), "EX01", classNMethodName: "DayCellSelector.ResetDayFrameAnimator");
					}
					finally
					{
						_resetDayFrameAnimator = null;
					}
				}

				MemoryStream storyStream = null;
				StreamReader storyReader = null;
				try
				{
					if (_cvDaysFrame.FindName("DpRaiseShadow") != null)
						storyStream = new MemoryStream(Encoding.UTF8.GetBytes(_cvResetDayFrameStoryScript));
					else
						storyStream = new MemoryStream(Encoding.UTF8.GetBytes(_cvResetDayFrameStoryScript2));

					storyReader = new StreamReader(storyStream);

					storyStream.Seek(0, SeekOrigin.Begin);
					XmlReader xmlReaderRow = XmlReader.Create(storyReader);
					_resetDayFrameAnimator = (Storyboard)XamlReader.Load(xmlReaderRow);

					_resetDayFrameAnimator.Completed += _resetDayFrameAnimator_Completed;
				}
				finally
				{
					if (storyReader != null)
						storyReader.Dispose();
					if (storyStream != null)
						storyStream.Dispose();
				}
				
				return _resetDayFrameAnimator;
			}
		}

		public void ResetDayFrame()
		{
			if (_defaultSetting.HasValue)
			{

				//ResetDayFrameAnimator
				//Storyboard currStoryboard = _pgDaySelection.FindResource("cvResetDayFrameStory") as Storyboard;
				Storyboard currStoryboard = ResetDayFrameAnimator;

				foreach (var obj in currStoryboard.Children)
				{
					if (obj is ThicknessAnimationUsingKeyFrames movMarColl)
						if (movMarColl.Name.Equals("ResetMovingMarginCollection"))
							foreach (var obj2 in movMarColl.KeyFrames)
								if (obj2 is LinearThicknessKeyFrame resMar)
								{
									resMar.Value = new Thickness(_defaultSetting.CvDaysFrameLeftMargin, 0, _defaultSetting.CvDaysFrameRightMargin, 0);
									break;
								}
				}
				currStoryboard.Begin(_pgDaySelection);
			}
		}

		public void _resetDayFrameAnimator_Completed(object sender, EventArgs e)
		{
			_cvDaysFrame.Children.Clear();

			if (_defaultSetting.HasValue)
			{
				DayCellInfo[] sortedList = (from keyPairDayCell in _dayCellList
											orderby keyPairDayCell.Value.Day
											select keyPairDayCell.Value).ToArray();

				double dayCellLeft = _defaultSetting.FirstLeftOfDayCell;
				foreach (DayCellInfo cellInfo in sortedList)
				{
					Canvas.SetLeft(cellInfo.DayCell, dayCellLeft);

					cellInfo.ChangeToDeactived();
					_cvDaysFrame.Children.Add(cellInfo.DayCell);
					dayCellLeft += _refDayCalendarCell.Width;
				}
			}

			InitDayCellSelection();
		}
		#endregion

		#region -- Story -- Raise Only Selected Day / Init the page with new Selected A Date -- 

		private Storyboard _raiseSelectedDayAnimator = null;
		public Storyboard SelectedDayAnimator
		{
			get
			{
				if (_raiseSelectedDayAnimator is null)
				{
					MemoryStream storyStream = null;
					StreamReader storyReader = null;
					try
					{
						storyStream = new MemoryStream(Encoding.UTF8.GetBytes(_cvRaiseSelectedDayStory));
						storyReader = new StreamReader(storyStream);

						storyStream.Seek(0, SeekOrigin.Begin);
						XmlReader xmlReaderRow = XmlReader.Create(storyReader);
						_raiseSelectedDayAnimator = (Storyboard)XamlReader.Load(xmlReaderRow);
					}
					finally
					{
						if (storyReader != null)
							storyReader.Dispose();
						if (storyStream != null)
							storyStream.Dispose();
					}
				}

				return _raiseSelectedDayAnimator;
			}
		}

		private void InitDayCellSelection()
		{
			_todayDate = DateTime.Now;
			_todayDate = new DateTime(_todayDate.Year, _todayDate.Month, _todayDate.Day, 0, 0, 0, 0);
			int middleCellCount = (int)Math.Ceiling((double)_dayCellList.Count / 2D);

			// extraLeftHistoryDay below value used to resolve moving left to right problem
			int extraLeftHistoryDay = 3;
			//--------------------------------------------------------------------------------

			_minHistoricalDate = _todayDate.AddDays(((middleCellCount - 1) + extraLeftHistoryDay) * -1);
			//DateTime outOfAdvanceDate = _todayDate.AddMonths(_maxRangeOfAdvanceMonth + 1);
			DateTime outOfAdvanceDate = App.MaxTicketAdvanceDate.AddDays(1);
			_outOfAdvanceDate = new DateTime(outOfAdvanceDate.Year, outOfAdvanceDate.Month, outOfAdvanceDate.Day, 0, 0, 0, 0);
			//_maxAdvanceDate = new DateTime(outOfAdvanceDate.Year, outOfAdvanceDate.Month, (middleCellCount - 1), 0, 0, 0, 0);
			_maxAdvanceDate = _outOfAdvanceDate.AddDays((middleCellCount - 2));

			DayCellInfo.TodayDate = _todayDate;
			DayCellInfo.MaxAdvanceDate = _maxAdvanceDate;
			DayCellInfo.MinHistoricalDate = _minHistoricalDate;
			DayCellInfo.OutOfAdvanceDate = _outOfAdvanceDate;

			int middleCellInx = middleCellCount - 1;
			DateTime startDate = SelectedDate.AddDays(middleCellInx * -1);
			DateTime currRenderDate = startDate;

			if (_pgDaySelection.FindName("DpRaiseShadow") != null)
				_pgDaySelection.UnregisterName("DpRaiseShadow");

			if (_pgDaySelection.FindName("DpDropShadow") != null)
				_pgDaySelection.UnregisterName("DpDropShadow");

			DayCellInfo[] sortedList = (from keyPairDellInfo in _dayCellList
										orderby keyPairDellInfo.Value.Day
										select keyPairDellInfo.Value).ToArray();

			foreach (DayCellInfo dayCellInfo in sortedList)
			{
				if (_dayCellList.TryGetValue(dayCellInfo.Tag, out DayCellInfo dayCellInfoX))
				{
					dayCellInfoX.Day = currRenderDate;

					if (currRenderDate.Equals(startDate))
					{
						_pgDaySelection.RegisterName("DpDropShadow", (DropShadowEffect)dayCellInfoX.DayCell.Effect);
						_dropDayCell = dayCellInfoX.DayCell;
					}

					if (currRenderDate.Equals(SelectedDate))
					{
						dayCellInfoX.ChangeToActived();
						_pgDaySelection.RegisterName("DpRaiseShadow", (DropShadowEffect)dayCellInfoX.DayCell.Effect);
						_raiseDayCell = dayCellInfoX.DayCell;
					}

					currRenderDate = currRenderDate.AddDays(1);
				}
			}

			SelectedDayAnimator.Begin(_pgDaySelection);
		}
		#endregion

		#region -- Story -- Reset Raised Shadow 
		//_effResetRaisedShadowStoryScript

		private Storyboard _resetRaisedShadowAnimator = null;
		public Storyboard ResetRaisedShadowAnimator
		{
			get
			{
				if (_resetRaisedShadowAnimator is null)
				{
					MemoryStream storyStream = null;
					StreamReader storyReader = null;
					try
					{
						storyStream = new MemoryStream(Encoding.UTF8.GetBytes(_effResetRaisedShadowStoryScript));
						storyReader = new StreamReader(storyStream);

						storyStream.Seek(0, SeekOrigin.Begin);
						XmlReader xmlReaderRow = XmlReader.Create(storyReader);
						_resetRaisedShadowAnimator = (Storyboard)XamlReader.Load(xmlReaderRow);

						_resetRaisedShadowAnimator.Completed += _resetRaisedShadowAnimator_Completed;
					}
					finally
					{
						if (storyReader != null)
							storyReader.Dispose();
						if (storyStream != null)
							storyStream.Dispose();
					}
				}
				return _resetRaisedShadowAnimator;
			}
		}

		private void ResetRaisedShadow()
		{
			if (_cvDaysFrame.FindName("DpRaiseShadow") != null)
			{
				if ((_raiseDayCell != null) && (_dayCellList.TryGetValue(_raiseDayCell.Tag.ToString(), out DayCellInfo raiseDayInfo)))
					raiseDayInfo.ChangeToDeactived();

				ResetRaisedShadowAnimator.Begin(_pgDaySelection);
			}
		}

		private void _resetRaisedShadowAnimator_Completed(object sender, EventArgs e)
		{
			_raiseDayCell = null;
			_pgDaySelection.UnregisterName("DpRaiseShadow");
		}

		#endregion

		public void StartSelection(DateTime currentSelectedDate)
		{
			SelectedDate = new DateTime(currentSelectedDate.Year, currentSelectedDate.Month, currentSelectedDate.Day, 0, 0, 0, 0);

			if (_dayCellList.Count == 0)
			{
				CreateCalendarDays();
				InitDayCellSelection();
			}
			else
			{
				ResetDayFrame();
			}
		}

		public void CreateCalendarDays()
		{
			if (_dayCellList.Count > 0)
				return;

			int maxOutFlameDayExtraCell = 2;
			double scvContainerWidth = _scvDayCalendarContainer.ActualWidth;
			double dayCellWidth = 0.0D;

			MemoryStream dayStream = null;
			StreamReader dayReader = null;

			DispatcherProcessingDisabled dispProDisabler;

			try
			{
				dispProDisabler = _pgDaySelection.Dispatcher.DisableProcessing();

				dayStream = new MemoryStream(Encoding.UTF8.GetBytes(_bdDaySelectionCellXaml));
				dayReader = new StreamReader(dayStream);

				Border bdDayCalendarCell = (Border)CreateNewControl(dayStream, dayReader);
				_dayCellList.Clear();

				DateTime dummyDate = DateTime.Now;
				decimal dummyFromPrice = 0.05M;
				double currDayCellLeftMargin = 0.0D;
				int? maxNoOfDayCellIn_cvDaysFrame = null;
				int finishedDayCellCount = 0;
				double startDayCellLeftMargin = 0.0D;
				List<Border> dayCalCellList = new List<Border>();
				int middleCellCount = 0;
				do
				{
					if (!maxNoOfDayCellIn_cvDaysFrame.HasValue)
					{
						dayCellWidth = bdDayCalendarCell.Width;

						int maxDisplayableDayCell = 1;
						double accumulatedDayCellWidth = dayCellWidth;
						while (accumulatedDayCellWidth < scvContainerWidth)
						{
							maxDisplayableDayCell += 2;
							accumulatedDayCellWidth += (dayCellWidth * 2);
						}

						maxDisplayableDayCell = ((maxDisplayableDayCell - 1) / 2) + maxDisplayableDayCell + 2;
						maxDisplayableDayCell = ((maxDisplayableDayCell % 2) == 0) ? (maxDisplayableDayCell + 1) : maxDisplayableDayCell;

						maxNoOfDayCellIn_cvDaysFrame = maxDisplayableDayCell + (maxOutFlameDayExtraCell * 2);
						startDayCellLeftMargin = (_cvDaysFrame.Margin.Left * -1) - ((((double)maxNoOfDayCellIn_cvDaysFrame * dayCellWidth) / 2D) - (scvContainerWidth / 2D));
						middleCellCount = (int)(Math.Ceiling((decimal)((double)(maxNoOfDayCellIn_cvDaysFrame.Value) / 2D)));

						currDayCellLeftMargin = startDayCellLeftMargin;

						Canvas.SetLeft(bdDayCalendarCell, currDayCellLeftMargin);

						finishedDayCellCount++;
					}
					else
					{
						bdDayCalendarCell = (Border)CreateNewControl(dayStream, dayReader);
						currDayCellLeftMargin += dayCellWidth;
						Canvas.SetLeft(bdDayCalendarCell, currDayCellLeftMargin);

						finishedDayCellCount++;
					}

					// ..just to assign _dropDayCell with a dayCell. This does not have any purpose at this line.
					if (_dropDayCell is null)
					{
						_dropDayCell = bdDayCalendarCell;
						_pgDaySelection.RegisterName("DpDropShadow", (DropShadowEffect)_dropDayCell.Effect);
					}

					// ..just to assign _raiseDayCell with a dayCell. This does not have any purpose at this line.
					if ((_raiseDayCell is null) && (finishedDayCellCount == middleCellCount))
					{
						_raiseDayCell = bdDayCalendarCell;
						_pgDaySelection.RegisterName("DpRaiseShadow", (DropShadowEffect)_raiseDayCell.Effect);
					}

					dummyFromPrice++;
					dummyDate = dummyDate.AddDays(1);

					DayCellInfo info = new DayCellInfo(bdDayCalendarCell);
					info.Day = dummyDate;
					info.FromPrice = dummyFromPrice;

					if (_defaultSetting.HasValue == false)
					{
						_defaultSetting.CvDaysFrameLeftMargin = _cvDaysFrame.Margin.Left;
						_defaultSetting.CvDaysFrameRightMargin = _cvDaysFrame.Margin.Right;
						_defaultSetting.FirstLeftOfDayCell = Canvas.GetLeft(bdDayCalendarCell);
						_defaultSetting.HasValue = true;
					}

					_dayCellList.Add(info.Tag, info);
					_cvDaysFrame.Children.Add(bdDayCalendarCell);

					if (_refDayCalendarCell == null)
						_refDayCalendarCell = bdDayCalendarCell;

				} while (finishedDayCellCount < maxNoOfDayCellIn_cvDaysFrame);

				//Storyboard sb = _pgDaySelection.FindResource("cvRaiseSelectedDayStory") as Storyboard;
				//sb.Begin(_pgDaySelection);
			}
			finally
			{
				if (dispProDisabler != null)
					dispProDisabler.Dispose();
			}
		}

		private object CreateNewControl(MemoryStream memoryStream, StreamReader streamReader)
		{
			memoryStream.Seek(0, SeekOrigin.Begin);
			XmlReader xmlReaderRow = XmlReader.Create(streamReader);
			return XamlReader.Load(xmlReaderRow);
		}

		private async void RaiseOnDateSelected(DateTime selectedDate)
		{
			await Task.Run(new Action(() => {
				_pgDaySelection.Dispatcher.Invoke(new Action(() => {
					try
					{
						if (OnDateSelected != null)
						{
							OnDateSelected.Invoke(null, new DateSelectedEventArgs(selectedDate));
						}
					}
					catch (Exception ex)
					{
						App.ShowDebugMsg(($@"Error when DayCellSelector.RaiseOnDateSelected : {ex.Message}; (EXIT10000401)"));
						App.Log.LogError(LogChannel, "-", new Exception("Unhandled exception in DayCellSelector.RaiseOnDateSelected; (EXIT10000401)", ex), "EX01", classNMethodName: "DayCellSelector.RaiseOnDateSelected");
						App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000401)");
					}
				}));
			}));
		}

		private void RaiseOnBeginDayCellMoveAnimate()
		{
			if (OnBeginDayCellMoveAnimate != null)
			{
				_pgDaySelection.Dispatcher.Invoke(new Action(() => {
					try
					{
						OnBeginDayCellMoveAnimate.Invoke(null, new EventArgs());
					}
					catch (Exception ex)
					{
						App.ShowDebugMsg(($@"Error when DayCellSelector.RaiseOnBeginDayCellMoveAnimate : {ex.Message}; (EXIT10000402)"));
						App.Log.LogError(LogChannel, "-", new Exception("Unhandled exception in DayCellSelector.RaiseOnBeginDayCellMoveAnimate; (EXIT10000402)", ex), "EX01", classNMethodName: "DayCellSelector.RaiseOnBeginDayCellMoveAnimate");
						App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000402)");
					}
				}));
			}
		}

		private void RaiseOnBeginDayCellTouchMove()
		{
			if (OnBeginDayCellTouchMove != null)
			{
				_pgDaySelection.Dispatcher.Invoke(new Action(() => 
				{
					try
					{
						OnBeginDayCellTouchMove.Invoke(null, new EventArgs());
					}
					catch (Exception ex)
					{
						App.ShowDebugMsg(($@"Error when DayCellSelector.RaiseOnBeginDayCellTouchMove : {ex.Message}; (EXIT10000403)"));
						App.Log.LogError(LogChannel, "-", new Exception("Unhandled exception in DayCellSelector.RaiseOnBeginDayCellTouchMove; (EXIT10000403)", ex), "EX01", classNMethodName: "DayCellSelector.RaiseOnBeginDayCellTouchMove");
						App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000403)");
					}
				}));
			}
		}

		private void ShowMsg(string msg, DateTime? time)
		{
			time = (time.HasValue) ? time : DateTime.Now;

			if (OnTextMessage != null)
			{
				OnTextMessage.Invoke(null, new TextMessageEventArgs() { Message = msg, Time = time.Value });
			}
		}

		public void Dispose()
		{
			if (OnTextMessage != null)
			{
				Delegate[] delgList = OnTextMessage.GetInvocationList();
				foreach (EventHandler<TextMessageEventArgs> delg in delgList)
				{
					OnTextMessage -= delg;
				}
			}
			if (OnDateSelected != null)
			{
				Delegate[] delgList = OnDateSelected.GetInvocationList();
				foreach (EventHandler<DateSelectedEventArgs> delg in delgList)
				{
					OnDateSelected -= delg;
				}
			}
			if (OnBeginDayCellMoveAnimate != null)
			{
				Delegate[] delgList = OnBeginDayCellMoveAnimate.GetInvocationList();
				foreach (EventHandler delg in delgList)
				{
					OnBeginDayCellMoveAnimate -= delg;
				}
			}
			if (OnBeginDayCellTouchMove != null)
			{
				Delegate[] delgList = OnBeginDayCellTouchMove.GetInvocationList();
				foreach (EventHandler delg in delgList)
				{
					OnBeginDayCellTouchMove -= delg;
				}
			}

			_cvDaysFrame.MouseDown -= _cvDaysFrame_MouseDown;
		}

		enum MoveDirection
		{
			Unchange = 0,
			ToLeft = 1,
			ToRight = 2
		}

		class DayFrameMovingInfo
		{
			public double NumberOfMovingCell { get; set; } = 1;
			public MoveDirection MoveDirection { get; set; } = MoveDirection.Unchange;
			public Border MovingDayCell { get; set; } = null;
			public double MoveDistance { get; set; } = 0D;
		}

	}
}
