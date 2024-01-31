using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp
{
    public class AppCountDown : IDisposable 
    {
        private const string LogChannel = "ServerApplication";

        private int _tolerantPeriodSec = 5;

        private (Guid SessionId, bool Expired, DateTime ExpireTime, bool Pause) _session = (SessionId: Guid.Empty, Expired: true, ExpireTime: DateTime.MinValue, Pause: false);

        private ServerSalesApplication _jobApp = null;
        private object _threadLock = new object();

        private DbLog _log = DbLog.GetDbLog();

        private bool _expiredActionOnHold = false;
        private DateTime? _lastMandatoryTimeout = null;

        public AppCountDown(ServerSalesApplication jobApp)
        {
            _jobApp = jobApp;

            Thread threadWorker = new Thread(new ThreadStart(CountingThreadWorking));
            threadWorker.IsBackground = true;
            threadWorker.Start();
        }

        public void Abort()
        {
            lock (_threadLock)
            {
                _session.SessionId = Guid.Empty;
                _session.Expired = true;
                _lastMandatoryTimeout = null;
                _session.ExpireTime = DateTime.MinValue;
            }
        }

        //public bool Pause()
        //{
        //    bool isSuccess = false;

        //    lock (_threadLock)
        //    {
        //        if (_session.Expired == false)
        //        {
        //            _session.Pause = true;
        //            isSuccess = true;
        //        }
        //    }

        //    return isSuccess;
        //}

        //public void UnPause()
        //{
        //    lock (_threadLock)
        //    {
        //        _session.Pause = false;
        //    }
        //}

        public bool IsSessionExpired(Guid sessionId, out bool isSessionFound, out bool isExpiredActionOnHold)
        {
            isExpiredActionOnHold = false;
            isSessionFound = false;
            bool isExpired = true;

            lock (_threadLock)
            {
                if (sessionId.Equals(Guid.Empty))
                    isExpired = true;

                else if (_session.SessionId.Equals(sessionId) == false)
                    isExpired = false;

                else if (_session.SessionId.Equals(sessionId))
                {
                    if (_expiredActionOnHold)
                    {
                        isExpiredActionOnHold = _expiredActionOnHold;
                        isExpired = false;
                    }
                    else
                    {
                        isExpired = _session.Expired;
                    }
                    
                    isSessionFound = true;
                }
            }
            return isExpired;
        }

        /// <summary>
        /// Return true if success
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public bool Pause(Guid sessionId)
        {
            bool isSuccess = true;

            lock (_threadLock)
            {
                if (sessionId.Equals(Guid.Empty))
                    isSuccess = false;

                else if (_session.SessionId.Equals(sessionId) == false)
                    isSuccess = false;

                else if (_session.SessionId.Equals(sessionId))
                {
                    isSuccess = true;
                    _expiredActionOnHold = true;
                }
            }
            return isSuccess;
        }

        public void UnPause()
        {
            lock (_threadLock)
            {
                _expiredActionOnHold = false; ;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countDownPeriodSec"></param>
        /// <param name="sessionNetProcessId">Refer to NetProcess Id of UIStartCountDownRequest</param>
        /// <returns></returns>
        public void SetNewCountDown(int countDownPeriodSec, Guid sessionNetProcessId)
        {
            lock (_threadLock)
            {
                _expiredActionOnHold = false;
                _session.SessionId = sessionNetProcessId;
                _session.Expired = false;
                _session.ExpireTime = DateTime.Now.AddSeconds(countDownPeriodSec);
                _lastMandatoryTimeout = null;
            }
        }

        //public void ChangeTimeoutX(TimeoutChangeMode changeMode, int countDownPeriodSec, Guid sessionNetProcessId)
        //{
        //    bool ret = false;
        //    if (changeMode == TimeoutChangeMode.ResetNormalTimeout)
        //    {
        //        ret = UpdateCountDown(countDownPeriodSec, sessionNetProcessId, out bool isMatchedSession);
        //    }
        //}

        public void ClientChangeTimeout(TimeoutChangeMode changeMode, int countDownPeriodSec, Guid sessionNetProcessId)
        {
            bool ret = false;
            if (changeMode == TimeoutChangeMode.ResetNormalTimeout)
            {
                lock (_threadLock)
                {
                    DateTime expectedExpireTime = DateTime.Now.AddSeconds(countDownPeriodSec);

                    if (_session.ExpireTime.Ticks < expectedExpireTime.Ticks)
                    {
                        ret = UpdateCountDown(countDownPeriodSec, sessionNetProcessId, isMandatoryExtensionChange: false, out bool isMatchedSession);
                    }
                }
            }
            else if (changeMode == TimeoutChangeMode.MandatoryExtension)
            {
                lock (_threadLock)
                {
                    int totalExtensionSec = Convert.ToInt32( _session.ExpireTime.Subtract(DateTime.Now).TotalSeconds);

                    totalExtensionSec = (totalExtensionSec > 0) ? totalExtensionSec : 0;
                    totalExtensionSec = totalExtensionSec + countDownPeriodSec;

                    ret = UpdateCountDown(totalExtensionSec, sessionNetProcessId, isMandatoryExtensionChange: true, out bool isMatchedSession);
                }
            }

            else if (changeMode == TimeoutChangeMode.RemoveMandatoryTimeout)
            {
                _lastMandatoryTimeout = null;
            }
        }

        /// <summary>
        /// Return false when session expired or session not matched
        /// </summary>
        /// <param name="countDownPeriodSec"></param>
        /// <param name="sessionNetProcessId">Refer to NetProcess Id of UIStartCountDownRequest</param>
        /// <returns></returns>
        public bool UpdateCountDown(int countDownPeriodSec, Guid sessionNetProcessId, bool isMandatoryExtensionChange, out bool isMatchedSession)
        {
            isMatchedSession = false;
            bool isSuccess = false;

            lock (_threadLock)
            {
                if (_session.SessionId.Equals(sessionNetProcessId) == false)
                {
                    isMatchedSession = false;
                    isSuccess = false;
                }
                else if (_session.SessionId.Equals(sessionNetProcessId))
                {
                    isMatchedSession = true;
                    if (_session.Expired == false)
                    {
                        DateTime newExpireTime = DateTime.Now.AddSeconds(countDownPeriodSec);

                        if (isMandatoryExtensionChange)
                        {
                            _lastMandatoryTimeout = newExpireTime;
                            _session.ExpireTime = newExpireTime;
                        }
                        else
                        {
                            if (_lastMandatoryTimeout.HasValue)
                            {
                                if (newExpireTime.Ticks > _lastMandatoryTimeout.Value.Ticks)
                                {
                                    _session.ExpireTime = newExpireTime;
                                }
                            }
                            else
                            {
                                _session.ExpireTime = newExpireTime;
                            }
                        }
                        isSuccess = true;
                    }
                    else
                        isSuccess = false;
                }
            }

            return isSuccess;
        }

        private void CountingThreadWorking()
        {
            _log.LogText(LogChannel, "", "Start of Count Down Thread", "A01", "AppCountDown.CountingThreadWorking");

            while (_disposed == false)
            {
                Thread.Sleep(1000);
                if ((_session.SessionId.Equals(Guid.Empty) == false) && (_session.Expired != true))
                {
                    try
                    {
                        lock (_threadLock)
                        {
                            if ((_session.SessionId.Equals(Guid.Empty) == false) && (_session.Expired != true))
                            {

                                if (_expiredActionOnHold == true)
                                {
                                    // By Pass
                                    string tt1 = "_expiredActionOnHold";
                                }
                                else if ((_session.ExpireTime.Subtract(DateTime.Now).TotalMilliseconds <= 0) && (_session.Pause == false ))
                                {
                                    _session.Expired = true;
                                    _lastMandatoryTimeout = null;
                                    _jobApp.RaiseOnShowResultMessage(_session.SessionId, new UICountDownExpiredAck(_session.SessionId, "-", DateTime.Now));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(LogChannel, "-", ex, "EX01", "AppCountDown.CountingThreadWorking");
                    }
                }
            }

            _log.LogText(LogChannel, "-", "End of Count Down Thread", "A10", "AppCountDown.CountingThreadWorking");
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed == false)
            {

                _disposed = true;
            }
        }

        //public enum TimeSettingMode
        //{
        //    // Below Time Setting mode design according to Priority.
        //    // Note : The bigger the integer number will cause the mode more priority. Like InternalProcess having higher priority than ClientOutGoingInstruction.
        //    //      : If a mode (like ForceStandartResetRequest, ManualSetRequest, or StandartResetRequest) priority having move advance expired time compare with InternalProcess OR ClientOutGoingInstruction,  
        //    //        then this will be used as the expire time.
        //    //      : Although ClientOutGoingInstruction is lower priority then InternalProcess, but still ClientOutGoingInstruction mode able to override the expired time. Because
        //    //        InternalProcess mode is server internal process timeout, but ClientOutGoingInstruction is client out-going timeout (after finished a internal process).

        //    /// <summary>
        //    /// Used for internal (server) process. This will extend the timeout for a internal process to finish.
        //    /// Normally this delay time is langest compare to other Time Setting Mode 
        //    /// </summary>
        //    InternalProcess = 100,

        //    /// <summary>
        //    /// Timeout set for an instruction sent to client.
        //    /// </summary>
        //    ClientOutGoingInstruction = 90,

        //    /// <summary>
        //    /// Used to overide "ManualSetRequest"; And set the timeout back to Standard delay time.
        //    /// </summary>
        //    ForceStandartResetRequest = 80,

        //    /// <summary>
        //    /// Request by client for timeout in any delay time.
        //    /// </summary>
        //    ManualSetRequest = 70,

        //    /// <summary>
        //    /// Request by client for timeout with a standard delay time.
        //    /// </summary>
        //    StandartResetRequest = 60
        //}
    }
}
