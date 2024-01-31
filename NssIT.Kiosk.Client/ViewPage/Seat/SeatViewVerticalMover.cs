using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
	public class SeatViewVerticalMover
	{
		private const string LogChannel = "ViewPage";

		public event EventHandler OnControlSelected;

		private (bool DataIsValid, Point InitPoint, object InitSource, Point LastStartingPoint, bool TouchMovingOccurred) _touchScroll
			= (DataIsValid: false, InitPoint: new Point(), InitSource: null, LastStartingPoint: new Point(), TouchMovingOccurred: false);

		private Page _pgSeatSelection;
		private ScrollViewer _svSeatSelection;

		public SeatViewVerticalMover(Page pgSeatSelection, ScrollViewer svSeatSelection)
		{
			_pgSeatSelection = pgSeatSelection;
			_svSeatSelection = svSeatSelection;

			_svSeatSelection.TouchDown += _svSeatSelection_TouchDown;
			_svSeatSelection.TouchMove += _svSeatSelection_TouchMove;
			_svSeatSelection.TouchUp += _svSeatSelection_TouchUp;
			_svSeatSelection.TouchLeave += _svSeatSelection_TouchLeave;
		}

		private void RaiseOnBeginDayCellTouchMove(object sender)
		{
			if (OnControlSelected != null)
			{
				_pgSeatSelection.Dispatcher.Invoke(new Action(() => {
					try
					{
						OnControlSelected.Invoke(sender, new EventArgs());
					}
					catch (Exception ex)
					{
						App.ShowDebugMsg($@"{ex.Message}; (EXIT10000501); SeatViewVerticalMover.RaiseOnBeginDayCellTouchMove");
						App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000501)", ex), "EX01", classNMethodName: "SeatViewVerticalMover.RaiseOnBeginDayCellTouchMove");
						//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000501)");
					}
				}));
			}
		}

		public void ResetScrollIndex()
		{
			_svSeatSelection.ScrollToVerticalOffset(0.0D);
		}

		private SemaphoreSlim _touchMoveLock = new SemaphoreSlim(1);
		private async void _svSeatSelection_TouchLeave(object sender, TouchEventArgs e)
		{
			//MainWindow.ShowMessage("_svSeatSelection_TouchLeave", null);
			try
			{
				await _touchMoveLock.WaitAsync();

				if (_touchScroll.DataIsValid && (_touchScroll.TouchMovingOccurred == false))
				{
					// Fire a selected seat if a related Border has selected
					if (_touchScroll.InitSource != null)
					{
						RaiseOnBeginDayCellTouchMove(_touchScroll.InitSource);
					}
					//MainWindow.ShowMessage("_svSeatSelection_TouchLeave -- Fire a selected seat", null);
				}

				_touchScroll.DataIsValid = false;

			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000502); SeatViewVerticalMover._svSeatSelection_TouchLeave");
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000502)", ex), "EX01", classNMethodName: "SeatViewVerticalMover._svSeatSelection_TouchLeave");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000502)");
			}
			finally
			{
				_touchMoveLock.Release();
			}
		}

		private async void _svSeatSelection_TouchUp(object sender, TouchEventArgs e)
		{
			//MainWindow.ShowMessage("_svSeatSelection_TouchUp", null);
			try
			{
				await _touchMoveLock.WaitAsync();

				if (_touchScroll.DataIsValid && (_touchScroll.TouchMovingOccurred == false))
				{
					// Fire a selected seat if a related Border has selected
					if (_touchScroll.InitSource != null)
					{
						RaiseOnBeginDayCellTouchMove(_touchScroll.InitSource);
					}
					//MainWindow.ShowMessage("_svSeatSelection_TouchUp -- Fire a selected seat", null);
				}

				_touchScroll.DataIsValid = false;

			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000503); SeatViewVerticalMover._svSeatSelection_TouchUp");
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000503)", ex), "EX01", classNMethodName: "SeatViewVerticalMover._svSeatSelection_TouchUp");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000503)");
			}
			finally
			{
				_touchMoveLock.Release();
			}
		}

		private double minimumStartMovingRange = 20D;
		private void _svSeatSelection_TouchMove(object sender, TouchEventArgs e)
		{
			//MainWindow.ShowMessage("_svSeatSelection_TouchMove", null);

			bool isInitiateState = false;

			try
			{
				if (_touchScroll.DataIsValid)
				{
					Point currPoint = e.GetTouchPoint(_svSeatSelection).Position;

					// When movedRange is positive, mean move container from bottom to top
					double movedRange = _touchScroll.LastStartingPoint.Y - currPoint.Y;
					if (_touchScroll.TouchMovingOccurred == false)
					{
						double ActualMovedRange = (movedRange < 0) ? (movedRange * -1) : movedRange;

						if (ActualMovedRange > minimumStartMovingRange)
						{
							_touchScroll.TouchMovingOccurred = true;
							isInitiateState = true;
						}
					}

					if ((_touchScroll.TouchMovingOccurred) && (movedRange != 0))
					{
						_touchScroll.LastStartingPoint = currPoint;
						// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
						// Move Container

						double fullOffset = _svSeatSelection.ExtentHeight - _svSeatSelection.ActualHeight;
						_svSeatSelection.ScrollToVerticalOffset(_svSeatSelection.VerticalOffset + movedRange);

						// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
					}
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000504); SeatViewVerticalMover._svSeatSelection_TouchMove");
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000504)", ex), "EX01", classNMethodName: "SeatViewVerticalMover._svSeatSelection_TouchMove");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000504)");
			}


		}

		private void _svSeatSelection_TouchDown(object sender, TouchEventArgs e)
		{
			try
			{
				if (_touchScroll.DataIsValid == false)
				{
					_touchScroll.DataIsValid = true;
					_touchScroll.InitPoint = e.GetTouchPoint(_svSeatSelection).Position;
					_touchScroll.InitSource = e.Source;

					_touchScroll.LastStartingPoint = _touchScroll.InitPoint;
					_touchScroll.TouchMovingOccurred = false;

					//MainWindow.ShowMessage($@"_svSeatSelection_TouchDown; Source: {_touchScroll.InitSource.GetType().FullName}", null);
				}
				//else
				//	MainWindow.ShowMessage($@"_svSeatSelection_TouchDown; Source : --> {e.Source.GetType().FullName}", null);
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000505); SeatViewVerticalMover._svSeatSelection_TouchDown");
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000505)", ex), "EX01", classNMethodName: "SeatViewVerticalMover._svSeatSelection_TouchDown");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000505)");
			}
		}
	}
}
