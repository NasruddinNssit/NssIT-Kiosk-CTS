using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
	public class CustInfoVerticalMover
	{
		//public event EventHandler OnControlSelected;

		private (bool DataIsValid, Point InitPoint, object InitSource, Point LastStartingPoint, bool TouchMovingOccurred) _touchScroll
			= (DataIsValid: false, InitPoint: new Point(), InitSource: null, LastStartingPoint: new Point(), TouchMovingOccurred: false);

		private Page _pgCustInfo;
		private ScrollViewer _svPassengerInfoList;

		public CustInfoVerticalMover(Page pgCustInfo, ScrollViewer svPassengerInfoList)
		{
			_pgCustInfo = pgCustInfo;
			_svPassengerInfoList = svPassengerInfoList;

			_svPassengerInfoList.TouchDown += _svPassengerInfoList_TouchDown;
			_svPassengerInfoList.TouchMove += _svPassengerInfoList_TouchMove;
			_svPassengerInfoList.TouchUp += _svPassengerInfoList_TouchUp;
			_svPassengerInfoList.TouchLeave += _svPassengerInfoList_TouchLeave;
		}

		private SemaphoreSlim _touchMoveLock = new SemaphoreSlim(1);
		private async void _svPassengerInfoList_TouchLeave(object sender, TouchEventArgs e)
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
						//RaiseOnBeginDayCellTouchMove(_touchScroll.InitSource);
					}
					//MainWindow.ShowMessage("_svSeatSelection_TouchLeave -- Fire a selected seat", null);
				}

				_touchScroll.DataIsValid = false;

			}
			finally
			{
				_touchMoveLock.Release();
			}
		}

		private async void _svPassengerInfoList_TouchUp(object sender, TouchEventArgs e)
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
						//RaiseOnBeginDayCellTouchMove(_touchScroll.InitSource);
					}
					//MainWindow.ShowMessage("_svSeatSelection_TouchUp -- Fire a selected seat", null);
				}

				_touchScroll.DataIsValid = false;

			}
			finally
			{
				_touchMoveLock.Release();
			}
		}

		private double minimumStartMovingRange = 20D;
		private void _svPassengerInfoList_TouchMove(object sender, TouchEventArgs e)
		{
			//MainWindow.ShowMessage("_svSeatSelection_TouchMove", null);

			bool isInitiateState = false;

			if (_touchScroll.DataIsValid)
			{
				Point currPoint = e.GetTouchPoint(_svPassengerInfoList).Position;

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

					double fullOffset = _svPassengerInfoList.ExtentHeight - _svPassengerInfoList.ActualHeight;
					_svPassengerInfoList.ScrollToVerticalOffset(_svPassengerInfoList.VerticalOffset + movedRange);

					// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
				}
			}
		}

		private void _svPassengerInfoList_TouchDown(object sender, TouchEventArgs e)
		{
			if (_touchScroll.DataIsValid == false)
			{
				_touchScroll.DataIsValid = true;
				_touchScroll.InitPoint = e.GetTouchPoint(_svPassengerInfoList).Position;
				_touchScroll.InitSource = e.Source;

				_touchScroll.LastStartingPoint = _touchScroll.InitPoint;
				_touchScroll.TouchMovingOccurred = false;

				//MainWindow.ShowMessage($@"_svSeatSelection_TouchDown; Source: {_touchScroll.InitSource.GetType().FullName}", null);
			}
			//else
			//	MainWindow.ShowMessage($@"_svSeatSelection_TouchDown; Source : --> {e.Source.GetType().FullName}", null);
		}

		public void PageInit()
		{
			_svPassengerInfoList.ScrollToVerticalOffset(0);
		}

	}
}
