using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.Log.DB;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NssIT.Kiosk.Common.Tools.ThreadMonitor
{
    public class ThreadSupervisor : IDisposable 
    {
        private const string _logChannel = "ThreadSupervisor";

        private static ThreadState[] _threadEndStateList = new ThreadState[] { ThreadState.Stopped, ThreadState.Aborted, ThreadState.AbortRequested, ThreadState.StopRequested };
        private static object _lock = new object();
        private bool _disposed = false;

        private static ThreadSupervisor _supervisor = null;
        private Thread _supervisorThreadWorker = null;
        private DbLog _log = DbLog.GetDbLog();
        private ConcurrentDictionary<Guid, RunThreadMan> _threadManList = new ConcurrentDictionary<Guid, RunThreadMan>();

        private bool _abortRequested = false;

        private ThreadSupervisor()
        {
            _supervisorThreadWorker = new Thread(SupervisingThreadWorking);
            _supervisorThreadWorker.IsBackground = true;
            _supervisorThreadWorker.Start();
        }

        public static void RequestAbort()
        {
            Thread tWorker = new Thread(delegate () {
                lock (_lock)
                {
                    if (_supervisor != null)
                    {
                        _supervisor._abortRequested = true;
                        Monitor.PulseAll(_lock);
                    }
                }
            });
            tWorker.IsBackground = true;
            tWorker.Start();
        }
        
        public void Dispose()
        {
            _disposed = true;

            lock(_lock)
            {
                Monitor.PulseAll(_lock);
            }
        }

        public static void StartThreadMonitor(RunThreadMan threadMan)
        {
            Thread tWorker = new Thread(delegate() {
                lock (_lock)
                {
                    GetSupervisor().AddThreadManInfo(threadMan);
                    Monitor.PulseAll(_lock);
                }
            });
            tWorker.IsBackground = true;
            tWorker.Start();
        }

        private static ThreadSupervisor GetSupervisor()
        {
            if (_supervisor != null)
                return _supervisor;

            if (_supervisor == null)
            {
                _supervisor = new ThreadSupervisor();
            }

            return _supervisor;
        }

        private void AddThreadManInfo(RunThreadMan newInfo)
        {
            _threadManList.TryAdd(newInfo.ThreadProcessId, newInfo);
        }

        public void SupervisingThreadWorking()
        {
            List<Guid> _endThreadList = new List<Guid>();
            List<Guid> _abordReqThreadList = new List<Guid>();
            List<Guid> _timeoutThreadList = new List<Guid>();
            RunThreadMan thrInfo = null;

            try
            {
                while (_disposed == false)
                {
                    thrInfo = null;
                    _endThreadList.Clear();
                    _abordReqThreadList.Clear();
                    _timeoutThreadList.Clear();

                    if (_abortRequested)
                    {
                        lock (_lock)
                        {
                            _abortRequested = false;
                        }
                    }
                    else if (_threadManList.Count == 0)
                    {
                        lock (_lock)
                        {
                            Monitor.Wait(_lock, 500);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                    //--------------------------------------------------------------------------------------------------------------------------
                    // Check threads execution state
                    foreach (KeyValuePair<Guid, RunThreadMan> workerKeyPair in _threadManList)
                    {
                        thrInfo = workerKeyPair.Value;

                        if (thrInfo != null)
                        {
                            try
                            {
                                string errMsg2 = "";
                                errMsg2 = $@"LogChannel: {thrInfo.LogChannel}; RunningTag: {thrInfo.RunningTag}; LifeTimeSec: {thrInfo.LifeTimeSec}; ExpiredTime: {thrInfo.ExpiredTime:yyyy-MM-dd HH:mm:ss}";

                                if ((thrInfo.IsEnd) || (thrInfo.ThreadWorker == null))
                                {
                                    // Thread ended Normally
                                    _endThreadList.Add(workerKeyPair.Key);
                                }
                                else if (thrInfo.IsAbortRequested)
                                {
                                    // Thread requested to abort
                                    _abordReqThreadList.Add(workerKeyPair.Key);
                                }
                                else if (thrInfo.ExpiredTime.Ticks < DateTime.Now.Ticks)
                                {
                                    // Thread working has expired
                                    _timeoutThreadList.Add(workerKeyPair.Key);
                                }
                            }
                            catch (Exception ex)
                            {
                                string errMsg = "";
                                errMsg = $@"LogChannel: {thrInfo.LogChannel}; RunningTag: {thrInfo.RunningTag}; LifeTimeSec: {thrInfo.LifeTimeSec}; ExpiredTime: {thrInfo.ExpiredTime:yyyy-MM-dd HH:mm:ss}";

                                _log?.LogError(_logChannel, "*", new Exception($@"Error; {ex.Message}; {errMsg}", ex), "EX01", "ThreadSupervisor.SupervisingThreadWorking");
                            }
                        }
                        else
                        {
                            string tt2 = "**";
                        }
                    }
                    //--------------------------------------------------------------------------------------------------------------------------
                    // Abort thread working - For abort requested
                    RunThreadMan abortThreadManInfo = null;
                    foreach (Guid threadProcessId in _abordReqThreadList)
                    {
                        try
                        {
                            abortThreadManInfo = null;

                            lock (_lock)
                            {
                                if (_threadManList.TryRemove(threadProcessId, out abortThreadManInfo))
                                { }
                            }

                            if (abortThreadManInfo != null)
                            {
                                if (abortThreadManInfo.IsEnd == false)
                                {
                                    try
                                    {
                                        abortThreadManInfo.ThreadWorker?.Abort();
                                        Thread.Sleep(500);

                                        _log?.LogText(abortThreadManInfo.LogChannel, abortThreadManInfo.ThreadProcessId.ToString(),
                                            $@"End-Abort; Abort Requested: {abortThreadManInfo.IsAbortRequested}", "A10", "ThreadSupervisor.SupervisingThreadWorking");
                                    }
                                    catch { }
                                }
                                //////////else
                                //////////{
                                //////////    //CYA-TEST
                                //////////    _log?.LogText(abortThreadManInfo.LogChannel, abortThreadManInfo.ThreadProcessId.ToString(),
                                //////////            $@"Thread end operation; Is Expired: {abortThreadManInfo.ExpiredTime.Ticks < DateTime.Now.Ticks}; {abortThreadManInfo.GetLogString()}",
                                //////////            "Z01", "ThreadSupervisor.SupervisingThreadWorking", AppDecorator.Log.MessageType.Debug);
                                //////////    //----------------------------------------------------------------------------------------------------
                                //////////}
                            }
                        }
                        catch (Exception ex)
                        {
                            string errMsg = "";

                            if (abortThreadManInfo != null)
                                errMsg = $@"LogChannel: {abortThreadManInfo.LogChannel}; RunningTag: {abortThreadManInfo.RunningTag}; LifeTimeSec: {abortThreadManInfo.LifeTimeSec}; ExpiredTime: {abortThreadManInfo.ExpiredTime:yyyy-MM-dd HH:mm:ss}";

                            _log?.LogError(_logChannel, "*", new Exception($@"Error; {ex.Message}; {errMsg}", ex), "EX02", "ThreadSupervisor.SupervisingThreadWorking");
                        }
                    }
                    //--------------------------------------------------------------------------------------------------------------------------
                    // Expired thread working - To abort expired working thread
                    RunThreadMan timeoutThreadManInfo = null;
                    foreach (Guid threadProcessId in _timeoutThreadList)
                    {
                        try
                        {
                            timeoutThreadManInfo = null;

                            lock (_lock)
                            {
                                if (_threadManList.TryRemove(threadProcessId, out timeoutThreadManInfo))
                                { }
                            }

                            if (timeoutThreadManInfo != null)
                            {
                                if (timeoutThreadManInfo.IsEnd == false)
                                {
                                    try
                                    {
                                        _log?.LogText(timeoutThreadManInfo.LogChannel, timeoutThreadManInfo.ThreadProcessId.ToString(),
                                                $@"Start - Abort on Expired; Is Expired: {timeoutThreadManInfo.ExpiredTime.Ticks < DateTime.Now.Ticks}; {timeoutThreadManInfo.GetLogString()}",
                                                "A13", "ThreadSupervisor.SupervisingThreadWorking");

                                        timeoutThreadManInfo.SetExpiredFlag();
                                        timeoutThreadManInfo.ThreadWorker?.Abort();
                                        Thread.Sleep(500);

                                        _log?.LogText(timeoutThreadManInfo.LogChannel, timeoutThreadManInfo.ThreadProcessId.ToString(),
                                            $@"End-Abort on Expired", "A15", "ThreadSupervisor.SupervisingThreadWorking");
                                    }
                                    catch { }
                                }
                                //////////else
                                //////////{
                                //////////    //CYA-TEST
                                //////////    _log?.LogText(timeoutThreadManInfo.LogChannel, timeoutThreadManInfo.ThreadProcessId.ToString(),
                                //////////            $@"Thread end operation; Is Expired: {timeoutThreadManInfo.ExpiredTime.Ticks < DateTime.Now.Ticks}; {timeoutThreadManInfo.GetLogString()}",
                                //////////            "Z22", "ThreadSupervisor.SupervisingThreadWorking", AppDecorator.Log.MessageType.Debug);
                                //////////    //----------------------------------------------------------------------------------------------------
                                //////////}
                            }
                        }
                        catch (Exception ex)
                        {
                            string errMsg = "";

                            if (timeoutThreadManInfo != null)
                                errMsg = $@"LogChannel: {timeoutThreadManInfo.LogChannel}; RunningTag: {timeoutThreadManInfo.RunningTag}; LifeTimeSec: {timeoutThreadManInfo.LifeTimeSec}; ExpiredTime: {timeoutThreadManInfo.ExpiredTime:yyyy-MM-dd HH:mm:ss}";

                            _log?.LogError(_logChannel, "*", new Exception($@"Error; {ex.Message}; {errMsg}", ex), "EX02", "ThreadSupervisor.SupervisingThreadWorking");
                        }
                    }
                    //--------------------------------------------------------------------------------------------------------------------------
                    // Remove Normal Ended ThreadMan from Supervisor list
                    if (_endThreadList.Count > 0)
                    {
                        lock (_lock)
                        {
                            foreach (Guid threadProcessId in _endThreadList)
                            {
                                try
                                {
                                    if (_threadManList.TryRemove(threadProcessId, out RunThreadMan removedThreadManInfo))
                                    {
                                        string tt3 = "#";

                                        //CYA-TEST
                                        //////////_log?.LogText(removedThreadManInfo.LogChannel, removedThreadManInfo.ThreadProcessId.ToString(),
                                        //////////        $@"Thread end operation; Is Expired: {removedThreadManInfo.ExpiredTime.Ticks < DateTime.Now.Ticks}; {removedThreadManInfo.GetLogString()}",
                                        //////////        "X01", "ThreadSupervisor.SupervisingThreadWorking", AppDecorator.Log.MessageType.Debug);
                                        //----------------------------------------------------------------------------------------------------
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string ttX8 = ex.Message;
                                }
                            }
                        }
                    }
                    //--------------------------------------------------------------------------------------------------------------------------
                }
            }
            catch (Exception exX)
            {
                _log?.LogError(_logChannel, "*", exX, "EX100", "ThreadSupervisor.SupervisingThreadWorking");
            }
            finally
            {
                _log?.LogText(_logChannel, "*", "Quit ThreadSupervisor", "T01", "ThreadSupervisor.SupervisingThreadWorking");
            }
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        }
    }
}
