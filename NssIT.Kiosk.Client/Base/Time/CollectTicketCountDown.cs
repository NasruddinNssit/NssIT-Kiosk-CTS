using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.Common.Tools.Timer;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base.Time
{
    /// <summary>
    /// ClassCode:EXIT80.15
    /// </summary>
    public class CollectTicketCountDown : IDisposable
    {
        private const string _logChannel = "CollectTicketCountDown";
        private StrongCountDownTimer CountDownTimer { get; set; } = new StrongCountDownTimer("CollectTicketCountDown");
        private bool _isServerRequestPhase = false;

        public string LastCountDownCode { get; private set; }
        public string LastTag { get; private set; }
        public int LastExpiredPeriodSec { get; private set; } = 5;

        public DbLog _log = DbLog.GetDbLog();
        public bool _disposed = false;
        public bool _isReNewRequested = false;
        public int _reNewAllowedIntervalSec = 3;

        /// <summary>
        /// FuncCode:EXIT80.1590
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            CountDownTimer.ForceResetCounter();
            CountDownTimer.Dispose();

            lock (_reNewLock)
            {
                Monitor.PulseAll(_reNewLock);
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.1501
        /// </summary>
        public CollectTicketCountDown()
        {
            CountDownTimer.OnExpired += CountDownTimer_OnExpired;
            //CYA-TEST (Test with IsDebugMode=Yes) .. CountDownTimer.OnCountDown += CountDownTimer_OnCountDown;
            //--------------------------------
            Thread _reNewCountDownThreadWorker = new Thread(new ThreadStart(ReNewCountDownThreadWorking));
            _reNewCountDownThreadWorker.IsBackground = true;
            _reNewCountDownThreadWorker.Start();
        }

        private void CountDownTimer_OnCountDown(object sender, CountDownEventArgs e)
        {
            App.ShowDebugMsg($@"CollectTicketCountDown => Code: {e.CountDownCode} ; Countdown: {e.TimeRemainderSec}");
        }

        /// <summary>
        /// FuncCode:EXIT80.1505
        /// </summary>
        private void CountDownTimer_OnExpired(object sender, ExpiredEventArgs e)
        {
            bool isServerResponse = false;
            Exception err = null;

            try
            {
                App.NetClientSvc?.CollectTicketService?.ResetUserSession(out isServerResponse, 20);
            }
            catch (Exception ex)
            {
                err = ex;
            }

            if (App.NetClientSvc?.CollectTicketService != null)
            {
                if (err != null)
                {
                    _log?.LogError(_logChannel, "*", new Exception($@"{err.Message}; (EXIT80.1505.EX01)", err), "EX01", "CollectTicketCountDown.CountDownTimer_OnExpired");
                    App.MainScreenControl?.Alert(detailMsg: $@"{err.Message}(EXIT80.1505.EX01)");
                }
                else if (isServerResponse == false)
                {
                    _log?.LogError(_logChannel, "*", new Exception("Local Server not responding (EXIT80.1505.EX02)"), "EX02", "CollectTicketCountDown.CountDownTimer_OnExpired");
                    App.MainScreenControl?.Alert(detailMsg: $@"Unable to communicate with Kiosk Service (EXIT80.1505.EX02)");
                }
                else
                {
                    _log?.LogText(_logChannel, "*", $@"Session Expired; LastCountDownCode: {CountDownTimer.LastCountDownCode}",
                                $@"B01", "CollectTicketCountDown.CountDownTimer_OnExpired");
                    App.MainScreenControl?.ShowWelcome();
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.1506
        /// </summary>
        public bool ChangeCountDown(string tag, string countDownCode, int expiredPeriodSec)
        {
            expiredPeriodSec = (expiredPeriodSec <= 0) ? 5 : expiredPeriodSec;

            bool retVal = CountDownTimer.ChangeCountDown(tag, countDownCode, expiredPeriodSec, 900, out _);

            if (retVal)
            {
                if (countDownCode?.ToString().Equals(ColTickCountDownCode.SERVER_REQUEST) == true)
                    _isServerRequestPhase = true;

                else
                {
                    _isServerRequestPhase = false;
                    LastCountDownCode = countDownCode;
                    LastExpiredPeriodSec = expiredPeriodSec;
                    LastTag = tag;
                }
            }
            return retVal;
        }

        /// <summary>
        /// FuncCode:EXIT80.1507
        /// </summary>
        public DateTime? ExpireTime
        {
            get
            {
                if (CountDownTimer.ExpireTime.Equals(DateTime.MaxValue))
                    return null;

                return CountDownTimer.ExpireTime;
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.1508
        /// </summary>
        public void ForceResetCounter()
        {
            _isReNewRequested = false;
            CountDownTimer.ForceResetCounter();
            _isServerRequestPhase = false;
            LastCountDownCode = null;
        }

        private DateTime _nextReNewTime = DateTime.Now;
        private object _reNewLock = new object();


        public void ReNewCountdown()
        {
            if ((_disposed == false) && (_nextReNewTime.Ticks < DateTime.Now.Ticks))
            {
                lock (_reNewLock)
                {
                    if ((_disposed == false) && (_nextReNewTime.Ticks < DateTime.Now.Ticks))
                    {
                        _isReNewRequested = true;
                        _nextReNewTime = DateTime.Now.AddSeconds(_reNewAllowedIntervalSec);
                    }
                    Monitor.PulseAll(_reNewLock);
                }
            }
        }

        public bool ReNewCountdownAndWaitForResult()
        {
            return ChangeCountDown(LastTag, LastCountDownCode, LastExpiredPeriodSec);
        }

        private void ReNewCountDownThreadWorking()
        {
            bool isProceedReNew = false;
            while (_disposed == false)
            {
                try
                {
                    lock (_reNewLock)
                    {
                        if (_isReNewRequested)
                        {
                            isProceedReNew = true;
                            _isReNewRequested = false;
                        }
                        else
                        {
                            Monitor.Wait(_reNewLock, _reNewAllowedIntervalSec * 1000);
                        }
                    }

                    if (isProceedReNew)
                    {
                        bool retVal = CountDownTimer.ReNewCountDown(LastCountDownCode);
                    }
                    isProceedReNew = false;
                }
                catch (Exception ex)
                {
                    _log.LogError(_logChannel, "*", ex, "EX01", "CollectTicketCountDown.ReOrderCountDownThreadWorking");
                }
            }
        }

        public bool IsExpired
        {
            get
            {
                if (ExpireTime.HasValue)
                {
                    try
                    {
                        DateTime tM = ExpireTime.Value;

                        if (tM.Subtract(DateTime.Now).TotalMilliseconds > 0)
                            return false;

                        else
                            return true;
                    }
                    catch (Exception ex)
                    {
                        //Note : Possible error only when ExpireTime is NULL
                        return true;
                    }
                }
                else
                    return true;
            }
        }



        public class ColTickCountDownCode
        {
            public static string Language = "LanguageStage";
            public static string BusCompany = "BusCompanyStage";
            public static string DepartureDate = "DepartureDateStage";
            public static string TicketNumberEntry = "TicketNumberEntryStage";
            public static string TicketInfo = "TicketInfoStage";
            public static string Payment = "PaymentStage";
            public static string Printing = "PrintingStage";
            public static string Printing_Pause = "PrintingStage_Pause";

            /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            public static string SERVER_REQUEST = "SERVER_REQUEST";
        }
    }
}
