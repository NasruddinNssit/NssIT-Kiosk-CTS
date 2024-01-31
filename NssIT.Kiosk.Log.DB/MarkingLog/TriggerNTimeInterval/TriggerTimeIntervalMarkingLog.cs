using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk;
using NssIT.Kiosk.AppDecorator.Log.Marking;

namespace NssIT.Kiosk.Log.DB.MarkingLog.TriggerNTimeInterval
{
    /// <summary>
    /// Allow manual trigger with Time Interval round-robin.
    /// </summary>
    public class TriggerTimeIntervalMarkingLog : IDisposable 
    {
        /// <summary>
        /// Interval in seconds that will request to log into db. This will consider _minDataCountOnLog value.
        /// If _minDataCountOnLog value is not reach when requesting, system will not able to log.
        /// </summary>
        private int _intervalLogReqSec = 30 * 60; 
        private int _minDataCountOnLog = 1;
        /// <summary>
        /// Interval in minutes that will request to log into db with the condition have at least 1 log data count.
        /// When this value is 0, mean disable this checking
        /// </summary>
        private int _maxIntervalLogReqMinutes = 0;

        private int _validLogIntervalSec = 0;

        private DateTime _nextLogSchdTime = DateTime.MaxValue;
        private DateTime? _nextMaxLogSchdTime = null;
        private DateTime _nextValidLogTime = DateTime.MinValue;

        private MarkLogSection _markLogSect = null;
        private BaseMarkingLog _baseMarkingLog = null;
        private Thread _schdThreadWorker = null;
        private string _outstandingMarkTail = "";
        private object _lock = new object();
        /// <summary>
        /// Note : Beware of dead lock when coding !
        /// </summary>
        private object _outstandingLock = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intervalLogReqSec"></param>
        /// <param name="minDataCountOnLog"></param>
        /// <param name="maxIntervalLogReqMinutes">Time interval to trigger log, even log count is less then minDataCountOnLog;  0 to disable this parameter</param>
        /// <param name="validLogIntervalSec">Valid time interval to accept next marking msg; 0 to disable this parameter</param>
        public TriggerTimeIntervalMarkingLog(string clsMetName,
            int intervalLogReqSec, int minDataCountOnLog, int maxIntervalLogReqMinutes = 0, int validLogIntervalSec = 0)
        {
            _intervalLogReqSec = (intervalLogReqSec <= 0) ? 10 : intervalLogReqSec;
            _minDataCountOnLog = (minDataCountOnLog <= 0) ? 1 : minDataCountOnLog;
            _validLogIntervalSec = (validLogIntervalSec <= 0) ? 0 : validLogIntervalSec;

            if (maxIntervalLogReqMinutes <= 0)
                _maxIntervalLogReqMinutes = 0;

            else
            {
                TimeSpan tS = new TimeSpan(0, 0, _intervalLogReqSec);

                if (_intervalLogReqSec > (maxIntervalLogReqMinutes * 60))
                    _maxIntervalLogReqMinutes = Convert.ToInt32(tS.TotalMinutes) + 2;
                else
                    _maxIntervalLogReqMinutes = maxIntervalLogReqMinutes;
            }

            _markLogSect = new MarkLogSection(Guid.NewGuid(), clsMetName, MarkingLogType.TriggerLog_With_MinTimeIntervalSec_N_MinCapacity);
            _baseMarkingLog = BaseMarkingLog.GetDbLog();

            _nextLogSchdTime = DateTime.Now.AddSeconds(_intervalLogReqSec);

            if (_maxIntervalLogReqMinutes > 0)
                _nextMaxLogSchdTime = DateTime.Now.AddMinutes(_maxIntervalLogReqMinutes);

            _schdThreadWorker = new Thread(SchdLogThreadWorking);
            _schdThreadWorker.IsBackground = true;
            _schdThreadWorker.Start();
        }

        private bool _disposed = false;
        public void Dispose()
        {
            _disposed = true;
        }

        private void SchdLogThreadWorking()
        {
            while (_disposed == false)
            {
                Thread.Sleep(1000);

                if (_nextMaxLogSchdTime.HasValue && _nextMaxLogSchdTime.Value.Ticks < DateTime.Now.Ticks)
                {
                    try
                    {
                        _baseMarkingLog.LogRequest(_markLogSect, DateTime.Now);
                    }
                    catch { }
                    finally
                    {
                        _nextMaxLogSchdTime = DateTime.Now.AddMinutes(_maxIntervalLogReqMinutes);
                    }
                }
                else if (_nextLogSchdTime.Ticks < DateTime.Now.Ticks)
                {
                    /////Note : Below code same as SendOutstanding()
                    
                    lock (_outstandingLock)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(_outstandingMarkTail) == false)
                            {
                                bool proceedToLog = false;
                                MarkLog mLog = null;
                                lock (_lock)
                                {
                                    if (string.IsNullOrWhiteSpace(_outstandingMarkTail) == false)
                                    {
                                        mLog = new MarkLog()
                                        {
                                            ATime = DateTime.Now,
                                            Mark = $@"?Outstanding?<=={_outstandingMarkTail}"
                                        };
                                        _outstandingMarkTail = "";
                                        proceedToLog = true;
                                    }
                                }

                                if ((proceedToLog) && (mLog != null))
                                {
                                    _baseMarkingLog.LogMark(_markLogSect, mLog, isLogRequest: true);
                                }
                            }
                        }
                        catch { }

                        try
                        {
                            _nextLogSchdTime = DateTime.Now.AddSeconds(_intervalLogReqSec);

                            _baseMarkingLog.LogRequestByLogCount(_markLogSect, DateTime.Now, _minDataCountOnLog);
                        }
                        catch { }
                    }
                }
            }
        }

        public void LogMark(string markMsg, bool isLogTrigger = false)
        {
            DateTime logTime = DateTime.Now;
            Thread tWorker = new Thread(delegate() 
            {
                try
                {
                    bool proceedToLog = false;
                    MarkLog mLog = null;

                    lock (_lock)
                    {
                        if ((_nextValidLogTime.Ticks < DateTime.Now.Ticks) || (isLogTrigger))
                        {
                            string markMsgX = null;

                            if (string.IsNullOrWhiteSpace(_outstandingMarkTail) == false)
                            {
                                markMsgX = $@"{markMsg}<=={_outstandingMarkTail}";
                            }
                            else
                                markMsgX = markMsg;

                            mLog = new MarkLog()
                            {
                                ATime = logTime,
                                Mark = markMsgX
                            };

                            if (_validLogIntervalSec > 0)
                                _nextValidLogTime = DateTime.Now.AddSeconds(_validLogIntervalSec);

                            _outstandingMarkTail = "";
                            proceedToLog = true;
                        }
                        else
                            _outstandingMarkTail = $@"^{markMsg}{_outstandingMarkTail}";
                    }

                    if ((proceedToLog) && (mLog != null))
                    {
                        _baseMarkingLog.LogMark(_markLogSect, mLog, isLogTrigger);
                    }
                }
                catch (Exception ex)
                {
                    string m1 = ex.Message;
                }
            });
            tWorker.IsBackground = true;
            tWorker.Start();
        }

        public void SendOutstanding()
        {
            DateTime logTime = DateTime.Now;
            Thread tWorker = new Thread(delegate() 
            {
                lock (_outstandingLock)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(_outstandingMarkTail) == false)
                        {
                            bool proceedToLog = false;
                            MarkLog mLog = null;
                            lock (_lock)
                            {
                                if (string.IsNullOrWhiteSpace(_outstandingMarkTail) == false)
                                {
                                    mLog = new MarkLog()
                                    {
                                        ATime = logTime,
                                        Mark = $@"?Outstanding?<=={_outstandingMarkTail}"
                                    };
                                    _outstandingMarkTail = "";
                                    proceedToLog = true;
                                }
                            }

                            if ((proceedToLog) && (mLog != null))
                            {
                                _baseMarkingLog.LogMark(_markLogSect, mLog, isLogRequest: true);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        _nextLogSchdTime = DateTime.Now.AddSeconds(_intervalLogReqSec);

                        _baseMarkingLog.LogRequestByLogCount(_markLogSect, DateTime.Now, _minDataCountOnLog);
                    }
                    catch { }
                }
            });
            tWorker.IsBackground = true;
            tWorker.Priority = ThreadPriority.Highest;
            tWorker.Start();
        }
    }
}
