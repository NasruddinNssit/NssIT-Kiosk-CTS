using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.Tools.Timer
{
    /// <summary>
    /// When a countdown has expire, system not allowed to change new count down time. Unless Force Reset countdown occur.
    /// </summary>
    public class StrongCountDownTimer : IDisposable
    {
        public delegate bool CountdownChangingValidationDelg();

        public event EventHandler<CountDownEventArgs> OnCountDown;
        public event EventHandler<ExpiredEventArgs> OnExpired;

        private const string _logChannel = "Tools";
        private bool _disposed = false;

        private string _countDownTag = "#";
        private DateTime _expiredTime = DateTime.MaxValue;
        private int _notificationIntervalMilliSec = 1000;
        private bool _expiredFlagRaised = false;
        private bool _countDownHasStarted = false;

        private Thread _countDownThreadWorker = null;
        private object _settingLock = new object();
        private DbLog _log = null;
        private (string Tag, string CountDownCode, int ExpiredPeriodSec, int NotificationIntervalMilliSec) _lastCountDown
            = (Tag: null, CountDownCode: null, ExpiredPeriodSec: 0, NotificationIntervalMilliSec: 0);

        public string LastCountDownCode => _lastCountDown.CountDownCode;

        public bool _activated = false;

        public bool Activated
        {
            get => _activated;
            set
            {
                _activated = value;

                if (_activated)
                    _countDownHasStarted = true;
            }
        }

        public bool IsTimeout => _expiredFlagRaised;

        public StrongCountDownTimer(string countDownTag)
        {
            _log = DbLog.GetDbLog();
            _countDownTag = string.IsNullOrWhiteSpace(countDownTag) ? "#" : countDownTag.Trim();
            _countDownThreadWorker = new Thread(CountDownThreadWorking);
            _countDownThreadWorker.IsBackground = true;
            _countDownThreadWorker.Priority = ThreadPriority.AboveNormal;
            _countDownThreadWorker.Start();
        }

        public void Dispose()
        {
            _disposed = true;
            _log = null;

            lock (_settingLock)
            {
                if (OnCountDown != null)
                {
                    try
                    {
                        Delegate[] delgList = OnCountDown.GetInvocationList();
                        foreach (EventHandler<CountDownEventArgs> delg in delgList)
                            OnCountDown -= delg;
                    }
                    catch { }
                }
                if (OnExpired != null)
                {
                    try
                    {
                        Delegate[] delgList = OnExpired.GetInvocationList();
                        foreach (EventHandler<ExpiredEventArgs> delg in delgList)
                            OnExpired -= delg;
                    }
                    catch { }
                }

                Monitor.PulseAll(_settingLock);
            }
        }

        public bool ChangeCountDown(string tag, string countDownCode, int expiredPeriodSec, int notificationIntervalMilliSec,
            out bool isAlreadyExpired, bool activateCountDown = true, bool isReNewChange = false)
        {
            isAlreadyExpired = false;

            if (_disposed == true)
                return false;

            bool isAlreadyExpiredX = false;
            bool retVal = false;
            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                lock (_settingLock)
                {
                    if (_expiredFlagRaised || (_disposed == true))
                    {
                        retVal = false;
                        isAlreadyExpiredX = _expiredFlagRaised;
                    }
                    else
                    {
                        if (notificationIntervalMilliSec < 100)
                            _notificationIntervalMilliSec = 100;
                        else
                            _notificationIntervalMilliSec = notificationIntervalMilliSec;

                        _lastCountDown.CountDownCode = (string.IsNullOrWhiteSpace(countDownCode)) ? null : countDownCode.Trim();
                        _lastCountDown.Tag = tag;
                        _lastCountDown.ExpiredPeriodSec = expiredPeriodSec;
                        _lastCountDown.NotificationIntervalMilliSec = notificationIntervalMilliSec;

                        _expiredTime = DateTime.Now.AddSeconds(expiredPeriodSec);
                        Activated = activateCountDown;

                        if (isReNewChange == false)
                            _log.LogText(_logChannel, $@"{_countDownTag}-NewCountDown", $@"New Expired Time: {_expiredTime: yyyy-MM-dd HH:mm:ss}; Tag: {tag}; countDownCode: {countDownCode}; expiredPeriodSec: {expiredPeriodSec}; notificationIntervalMilliSec: {notificationIntervalMilliSec}; activateCountDown: {activateCountDown}",
                                $@"D01", "StrongCountDownTimer.ChangeCountDown");

                        Monitor.PulseAll(_settingLock);
                        retVal = true;
                    }
                }
            })));
            tWorker.IsBackground = true;
            tWorker.Start();
            tWorker.Join();
            isAlreadyExpired = isAlreadyExpiredX;

            return retVal;
        }

        public bool ReNewCountDown(string countDownCode)
        {
            if ((_disposed == true) || string.IsNullOrWhiteSpace(countDownCode))
                return false;

            bool retVal = false;
            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                lock (_settingLock)
                {
                    if (_expiredFlagRaised || (_disposed == true))
                    {
                        retVal = false;
                    }
                    else if ((_lastCountDown.CountDownCode?.ToUpper().Equals(countDownCode.ToUpper()) == true) 
                        && (_lastCountDown.ExpiredPeriodSec > 0))
                    {
                        _expiredTime = DateTime.Now.AddSeconds(_lastCountDown.ExpiredPeriodSec);

                        Monitor.PulseAll(_settingLock);
                        retVal = true;
                    }
                }
            })));
            tWorker.IsBackground = true;
            tWorker.Start();
            tWorker.Join();

            return retVal;
        }

        public DateTime ExpireTime
        {
            get
            {
                return _expiredTime;
            }
        }

        /// <summary>
        /// Advise to use ResetCounter() first before call this method.
        /// </summary>
        public void ForceResetCounter()
        {
            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                lock (_settingLock)
                {
                    Activated = false;
                    _expiredFlagRaised = false;
                    _expiredTime = DateTime.MaxValue;
                }
            })));
            tWorker.IsBackground = true;
            tWorker.Start();
            tWorker.Join();
        }

        private void CountDownThreadWorking()
        {
            while (_disposed == false)
            {
                if ((_expiredFlagRaised == false) && Activated)
                {
                    DateTime curr = DateTime.Now;
                    
                    if (_expiredTime.Ticks < curr.Ticks)
                    {
                        bool needToRaiseOnExpire = false;
                        lock (_settingLock)
                        {
                            if ((_expiredFlagRaised == false) && Activated && (_expiredTime.Ticks < curr.Ticks))
                            {
                                //Raise Expired Event
                                //-----------------------------
                                _expiredFlagRaised = true;
                                //-----------------------------
                                needToRaiseOnExpire = true;
                            }
                        }

                        if ((needToRaiseOnExpire) && (_disposed == false))
                            RaiseOnExpired(LastCountDownCode, _expiredTime);
                    }
                    else
                    {
                        //Raise CountDown Event
                        //---------------------------------
                        int remainderSec = Convert.ToInt32(_expiredTime.Subtract(curr).TotalSeconds);
                        remainderSec = (remainderSec < 0) ? 0 : remainderSec;

                        RaiseOnCountDown(LastCountDownCode, remainderSec);

                        lock (_settingLock)
                        {
                            Monitor.Wait(_settingLock, _notificationIntervalMilliSec);
                        }
                    }
                }
                else
                {
                    lock (_settingLock)
                    {
                        Monitor.Wait(_settingLock, 10 * 1000);
                    }
                }
            }

            void RaiseOnCountDown(string countDownCode, int timeRemainderSec)
            {
                if (_disposed == true)
                    return;

                try
                {
                    OnCountDown?.Invoke(null, new CountDownEventArgs(countDownCode, timeRemainderSec));
                }
                catch (Exception ex)
                {
                    _log?.LogError(_logChannel, "*", ex, $@"{_countDownTag}::EX01", "CountDownTimer.RaiseOnCountDown");
                }
            }

            void RaiseOnExpired(string countDownCode, DateTime expiredTime)
            {
                if (_disposed == true)
                    return;

                try
                {
                    OnExpired?.Invoke(null, new ExpiredEventArgs(countDownCode, expiredTime));
                }
                catch (Exception ex)
                {
                    _log?.LogError(_logChannel, "*", ex, $@"{_countDownTag}::EX01", "CountDownTimer.RaiseOnExpired");
                }
            }
        }
    }
}
