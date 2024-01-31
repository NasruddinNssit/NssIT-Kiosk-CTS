using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    public class TimeFilterThumbMoveHelper
    {
        private const string LogChannel = "ViewPage";

        public event EventHandler<TimeFilterEventArgs> OnTimeFilterChanged;
        public event EventHandler<TimeFilterEventArgs> OnTimeFilterReset;

        private Image _imgThumb = null;
        private Canvas _cvContainer = null;

        private (double TopEdge, double Height) _imgThunbData
            = (TopEdge: 0D, Height: 0D);

        private double _cvContainerHeight = 0D;

        private (bool DataIsValid, Point InitPoint, Point LastStartingPoint) _thumbMover
            = (DataIsValid: false, InitPoint: new Point(), LastStartingPoint: new Point());

        private TimeFilterThumbMoveHelper _anotherThumbHelper = null;

        public TimeFilterThumbMoveHelper(Image dbThumb, Canvas container)
        {
            _imgThumb = dbThumb;
            _cvContainer = container;

            _imgThumb.MouseLeftButtonDown += _dbThumb_MouseLeftButtonDown;
            _imgThumb.MouseLeftButtonUp += _dbThumb_MouseLeftButtonUp;
            _imgThumb.MouseLeave += _dbThumb_MouseLeave;
            _imgThumb.MouseMove += _dbThumb_MouseMove;
        }

        public double GetThumbHeight()
        {
            return _imgThumb.ActualHeight;
        }

        public double GetThumbTop()
        {
            return Canvas.GetTop(_imgThumb);
        }

        public void InitThumb(TimeFilterThumbMoveHelper anotherThumbHelper, double cvContainerHeight)
        {
            _anotherThumbHelper = anotherThumbHelper;
            _cvContainerHeight = cvContainerHeight;

            double timeVarianHeight = _cvContainerHeight - _imgThumb.ActualHeight - anotherThumbHelper.GetThumbHeight();

            GapHeight = timeVarianHeight / 24D;

            if (GetThumbTop() < anotherThumbHelper.GetThumbTop())
                Canvas.SetTop(_imgThumb, 0D);
            else
                Canvas.SetTop(_imgThumb, (_cvContainerHeight - _imgThumb.ActualHeight));
        }

        private void _dbThumb_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pt2 = e.GetPosition(_cvContainer);
            //MainWindow.ShowMessage($@"MouseLeftButtonDown : Canvas.x: {pt2.X}; Canvas.y: {pt2.Y}");

            _imgThunbData.TopEdge = Canvas.GetTop(_imgThumb);
            _imgThunbData.Height = _imgThumb.ActualHeight;

            _thumbMover.DataIsValid = true;
            _thumbMover.InitPoint = pt2;
            _thumbMover.LastStartingPoint = pt2;
        }

        private void _dbThumb_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_thumbMover.DataIsValid)
            {
                Point currPnt = e.GetPosition(_cvContainer);
                Point earlyPnt = _thumbMover.LastStartingPoint;

                double yDiff = currPnt.Y - earlyPnt.Y;

                if (yDiff < 0)
                {
                    // Note : When yDiff is -ve mean moving up

                    //----------------------------------------------------------------------------
                    // Verify & Check yDiff againt limit

                    double chkNewTop = GetThumbTop() + yDiff;
                    //double chkNewBottom = chkNewTop + topThumb.GetThumbHeight();

                    if (chkNewTop < 0)
                    {
                        chkNewTop = 0;
                        yDiff = chkNewTop - GetThumbTop();

                        if (yDiff >= 0)
                            return;
                    }
                    //----------------------------------------------------------------------------
                    // Note : When yDiff is -ve mean moving up
                    double newTop = (GetThumbTop() + yDiff);
                    if (newTop >= 0)
                    {
                        if (_anotherThumbHelper.TryPasiveMoveUp(yDiff, newTop, _imgThumb, out double outDiff))
                        {
                            newTop = (GetThumbTop() + outDiff);
                            Canvas.SetTop(_imgThumb, newTop);
                            //MainWindow.ShowMessage($@"MouseMove : Canvas.x: {currPnt.X}; Canvas.y: {currPnt.Y}; yDiff: {yDiff}");
                            ReadFilterTimes(isReset: false, out DateTime startTime, out DateTime endTime);
                        }
                    }
                }
                else if (yDiff > 0)
                {
                    // Note : When yDiff is +ve mean moving down

                    //----------------------------------------------------------------------------
                    // Verify & Check yDiff againt limit
                    double chkNewTop = GetThumbTop() + yDiff;
                    double chkNewBottom = chkNewTop + GetThumbHeight();

                    if (chkNewBottom > _cvContainerHeight)
                    {
                        chkNewTop = _cvContainerHeight - GetThumbHeight();
                        yDiff = chkNewTop - GetThumbTop();

                        if (yDiff <= 0)
                            return;
                    }
                    //----------------------------------------------------------------------------
                    // Note : When yDiff is +ve mean moving down
                    double newTop = (GetThumbTop() + yDiff);
                    double newBottom = newTop + _imgThunbData.Height;

                    if (newBottom <= _cvContainerHeight)
                    {
                        if (_anotherThumbHelper.TryPassiveMoveDown(yDiff, newTop, _imgThumb, out double outDiff))
                        {
                            newTop = (GetThumbTop() + outDiff);
                            Canvas.SetTop(_imgThumb, newTop);
                            //MainWindow.ShowMessage($@"MouseMove : Canvas.x: {currPnt.X}; Canvas.y: {currPnt.Y}; yDiff: {yDiff}");
                            ReadFilterTimes(isReset: false, out DateTime startTime, out DateTime endTime);
                        }
                    }
                }
                _thumbMover.LastStartingPoint = currPnt;
            }
        }

        public void ResetFilter()
        {
            double newTop = 0D;
            if (GetThumbTop() < _anotherThumbHelper.GetThumbTop())
            {
                newTop = 0D;
                Canvas.SetTop(_imgThumb, newTop);
                ReadFilterTimes(isReset: true, out DateTime startTime, out DateTime endTime);
            }
            else
            {
                newTop = _cvContainerHeight - GetThumbHeight();
                Canvas.SetTop(_imgThumb, newTop);
                ReadFilterTimes(isReset: true, out DateTime startTime, out DateTime endTime);
            }
        }

        private void ReadFilterTimes(bool isReset, out DateTime startTime, out DateTime endTime)
        {
            startTime = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            endTime = new DateTime(1900, 1, 1, 23, 59, 59, 999);

            TimeFilterThumbMoveHelper topThumb = null;
            TimeFilterThumbMoveHelper bottomThumb = null;

            if (this.GetThumbTop() < _anotherThumbHelper.GetThumbTop())
            {
                topThumb = this;
                bottomThumb = _anotherThumbHelper;
            }
            else
            {
                topThumb = _anotherThumbHelper;
                bottomThumb = this;
            }

            double topMostLevel = topThumb.GetThumbHeight();
            double bottomMostLevel = _cvContainerHeight - _anotherThumbHelper.GetThumbHeight();

            double topThumbLevel = topThumb.GetThumbTop() + topThumb.GetThumbHeight();
            double bottomThumbLevel = bottomThumb.GetThumbTop();

            double timeVarianHeight = bottomMostLevel - topMostLevel;
            double minutesPerHeight = (60 * 24) / timeVarianHeight;

            double fromTimeHeight = topThumbLevel - topMostLevel;
            fromTimeHeight = (fromTimeHeight < 0) ? 0D : fromTimeHeight;

            double toTimeHeight = bottomMostLevel - bottomThumbLevel;
            toTimeHeight = (toTimeHeight < 0) ? 0D : toTimeHeight;

            double from_TotalMinutes = fromTimeHeight * minutesPerHeight;
            double to_TotalMinutes = toTimeHeight * minutesPerHeight;

            if ((fromTimeHeight > 0) && (from_TotalMinutes > 9))
            {
                startTime = startTime.AddMinutes(from_TotalMinutes);

                double minutes = 0D;

                if (startTime.Minute > 0)
                    minutes = Math.Floor(double.Parse(startTime.Minute.ToString()) / 10D) * 10;

                //startTime = new DateTime(1900, 1, 1, startTime.Hour, Convert.ToInt32(minutes), 0, 0);
                startTime = new DateTime(1900, 1, 1, startTime.Hour, 0, 0, 0);
            }

            if ((toTimeHeight > 0) && (to_TotalMinutes > 9))
            {
                endTime = endTime.AddMinutes(to_TotalMinutes * -1);

                double hour = endTime.Hour;
                double minutes = 0D;

                if (endTime.Minute > 0)
                    minutes = Math.Ceiling(double.Parse(endTime.Minute.ToString()) / 10D) * 10;

                if (hour < 23)
                {
                    if (minutes >= 60)
                    {
                        hour = hour + 1;
                        minutes = 0;
                    }

                    endTime = new DateTime(1900, 1, 1, Convert.ToInt32(hour), 0, 59, 999);
                }
                else if (hour == 23)
                {
                    if (minutes >= 60)
                    {
                        minutes = 59;

                        endTime = new DateTime(1900, 1, 1, Convert.ToInt32(hour), Convert.ToInt32(minutes), 59, 999);
                    }
                    else
                    {
                        endTime = new DateTime(1900, 1, 1, Convert.ToInt32(hour), 0, 0, 999);
                    }
                }
            }

            //MainWindow.ShowMessage($@"Start Time : {startTime.ToString("hh:mm tt")}; End Time : {endTime.ToString("hh:mm tt")}");

            if ((isReset == false) && (OnTimeFilterChanged != null))
            {
                try
                {
                    OnTimeFilterChanged.Invoke(null, new TimeFilterEventArgs(startTime, endTime));
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Error OnTimeFilterChanged; {ex.Message}");
                    App.Log.LogError(LogChannel, "-", ex, "", "ThumbMoveHelper.OnTimeFilterChanged");
                }
            }
            else if ((isReset == true) && (OnTimeFilterReset != null))
            {
                try
                {
                    OnTimeFilterReset.Invoke(null, new TimeFilterEventArgs(startTime, endTime));
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Error OnTimeFilterReset; {ex.Message}");
                    App.Log.LogError(LogChannel, "-", ex, "", "ThumbMoveHelper.OnTimeFilterReset");
                }
            }
        }

        private bool TryPasiveMoveUp(double yDiff, double activeThumbNewTop, Image activeThumb, out double outDiff)
        {
            outDiff = yDiff;

            // When Canvas.GetTop(_dbThumb) < Canvas.GetTop(activeThumb), mean _dbThumb on the top of activeThumb
            if (Canvas.GetTop(_imgThumb) < Canvas.GetTop(activeThumb))
            {
                if (activeThumbNewTop < 0)
                    return false;
                else
                {
                    double thisTop = GetThumbTop();

                    double thisCurrBottom = thisTop + GetThumbHeight();
                    double thisCurrBottomWithGap = thisCurrBottom + GapHeight;
                    double thisNewTop = (thisTop + yDiff);

                    if (activeThumbNewTop >= thisCurrBottomWithGap)
                        return true;

                    else if (thisTop <= 0)
                        return false;

                    else if (thisNewTop < 0)
                    {
                        thisNewTop = 0;
                        outDiff = thisNewTop - thisTop;

                        if (outDiff >= 0)
                            return false;
                    }
                    Canvas.SetTop(_imgThumb, thisNewTop);
                }
            }
            return true;
        }

        private bool TryPassiveMoveDown(double yDiff, double activeThumbNewTop, Image activeThumb, out double outDiff)
        {
            outDiff = yDiff;

            // When Canvas.GetTop(_dbThumb) > Canvas.GetTop(activeThumb), mean _dbThumb on the bottom of activeThumb
            if (Canvas.GetTop(_imgThumb) > Canvas.GetTop(activeThumb))
            {
                double thisTop = GetThumbTop();
                double thisBottom = thisTop + GetThumbHeight();

                double thisNewTop = (thisTop + yDiff);
                double thisNewBottom = thisTop + _imgThumb.ActualHeight + yDiff;

                double activeNewBottom = Canvas.GetTop(activeThumb) + activeThumb.ActualHeight + yDiff;
                double thisTopUpGap = thisTop - GapHeight;

                if (thisNewTop < 0)
                    return false;

                else if (activeNewBottom <= thisTopUpGap)
                    return true;

                else if (thisBottom >= _cvContainerHeight)
                    return false;

                else if (thisNewBottom > _cvContainerHeight)
                {
                    thisNewTop = _cvContainerHeight - GetThumbHeight();
                    //thisNewBottom = _cvContainerHeight;
                    outDiff = thisNewTop - thisTop;

                    if (outDiff <= 0)
                        return false;
                }

                Canvas.SetTop(_imgThumb, thisNewTop);
            }
            return true;
        }

        private void _dbThumb_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _thumbMover.DataIsValid = false;
        }

        private void _dbThumb_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _thumbMover.DataIsValid = false;
        }

        public double GapHeight { get; private set; } = 3D;

    }
}
