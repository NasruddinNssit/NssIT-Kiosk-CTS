using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.Tools.Timer
{
    public class CountDownTimer : IDisposable
    {
        public event EventHandler<CountDownEventArgs> OnCountDown;
        public event EventHandler<ExpiredEventArgs> OnExpired;

        private const string _logChannel = "SysBase";
        private bool _disposed = false;

        private string _countDownCode = null;
        private DateTime _expiredTime = DateTime.MinValue;
        private int _notificationIntervalMilliSec = 1000;
        private bool _expiredFlagRaised = true;

        private Thread _countDownThreadWorker = null;

        public bool Activated { get; set; } = false;
        public object _settingLock = new object();
        private DbLog _log = null;

        public CountDownTimer()
        {
            _log = DbLog.GetDbLog();

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

        public void ChangeCountDown(string countDownCode, int expiredPeriodSec, int notificationIntervalMilliSec, bool activateCountDown = true)
        {
            if (_disposed == true)
                return;

            lock (_settingLock)
            {
                if (notificationIntervalMilliSec < 100)
                    _notificationIntervalMilliSec = 100;
                else
                    _notificationIntervalMilliSec = notificationIntervalMilliSec;

                _countDownCode = string.IsNullOrWhiteSpace(countDownCode) ? null : countDownCode.Trim();
                _expiredTime = DateTime.Now.AddSeconds(expiredPeriodSec);
                _expiredFlagRaised = false;
                Activated = activateCountDown;

                Monitor.PulseAll(_settingLock);
            }
        }

        public void ResetCounter()
        {
            Activated = false;
            _expiredFlagRaised = true;
            _expiredTime = DateTime.MinValue;
        }

        private void CountDownThreadWorking()
        {
            while (_disposed == false)
            {
                if ((_expiredFlagRaised == false) && (Activated))
                {
                    DateTime curr = DateTime.Now;

                    if (_expiredTime.Ticks < curr.Ticks)
                    {
                        //Raise Expired Event
                        //-----------------------------
                        _expiredFlagRaised = true;
                        //-----------------------------
                        RaiseOnExpired(_countDownCode, _expiredTime);
                    }
                    else
                    {
                        //Raise CountDown Event
                        //---------------------------------
                        int remainderSec = Convert.ToInt32(_expiredTime.Subtract(curr).TotalSeconds);
                        remainderSec = (remainderSec < 0) ? 0 : remainderSec;

                        RaiseOnCountDown(_countDownCode, remainderSec);

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
                    _log?.LogError(_logChannel, "*", ex, "EX01", "CountDownTimer.RaiseOnCountDown");
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
                    _log?.LogError(_logChannel, "*", ex, "EX01", "CountDownTimer.RaiseOnExpired");
                }
            }
        }
    }
}
