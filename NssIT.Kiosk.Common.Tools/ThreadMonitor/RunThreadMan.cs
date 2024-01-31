using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.Tools.ThreadMonitor
{
    public class RunThreadMan : IDisposable
    {
        private static ThreadState[] _threadEndStateList = new ThreadState[] { ThreadState.Stopped, ThreadState.Aborted, ThreadState.AbortRequested, ThreadState.StopRequested };

        public int LifeTimeSec { get; private set; } = 3;
        public bool IsAbortRequested { get; private set; } = false;
        public bool IsExpired { get; private set; } = false;
        public DateTime ExpiredTime { get; private set; }
        public Thread ThreadWorker { get; private set; } = null;
        public Guid ThreadProcessId { get; private set; } = Guid.NewGuid();
        public string RunningTag { get; private set; } = "RunThreadMan";
        public string LogChannel { get; private set; } = "::RunThreadMan";

        private bool _isLogReq = false;
        private bool IsThreadWorkingEnd { get; set; } = false;
        private DbLog _log = DbLog.GetDbLog();

        public RunThreadMan(Action action, string runningTag, int lifeTimeSec, string logChannel, ThreadPriority threadPriority = ThreadPriority.Normal, bool isLogReq = false)
            : this(new ThreadStart(action), runningTag, lifeTimeSec, logChannel, threadPriority, isLogReq)
        { }

        public RunThreadMan(ThreadStart threadStart, string runningTag, int lifeTimeSec, string logChannel, ThreadPriority threadPriority = ThreadPriority.Normal, bool isLogReq = false)
        {
            RunningTag = runningTag ?? "RunThreadMan**";
            LogChannel = (string.IsNullOrWhiteSpace(logChannel) ? "*" : logChannel.Trim()) + LogChannel;
            IsAbortRequested = false;
            LifeTimeSec = (lifeTimeSec <= 0) ? 3 : lifeTimeSec;
            ExpiredTime = DateTime.Now.AddSeconds(LifeTimeSec + 1);
            IsThreadWorkingEnd = false;
            _isLogReq = isLogReq;
            ThreadWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    if (_isLogReq)
                        _log?.LogText(LogChannel, ThreadProcessId.ToString(), $@"Start new ThreadMan; Running Tag : {RunningTag}", "T52", "ThreadMan.(ctor)");
                    /////CYA-PENDING-CODE : RnD for BeginInvode/EndInvode for async execution
                    threadStart.Invoke();
                }
                catch (ThreadAbortException)
                {
                    IsThreadWorkingEnd = true;
                }
                catch (Exception ex)
                {
                    IsThreadWorkingEnd = true;
                    _log?.LogError(LogChannel, ThreadProcessId.ToString(), new Exception($@"RunningTag: {RunningTag}", ex), "EX52", "ThreadMan.(ctor)");
                    string msg = ex.Message;
                }
                finally
                {
                    if (_isLogReq)
                        _log?.LogText(LogChannel, ThreadProcessId.ToString(), $@"End ThreadMan; Running Tag : {RunningTag}", "T72", "ThreadMan.(ctor)");
                    IsThreadWorkingEnd = true;
                }
            })));
            ThreadWorker.IsBackground = true;
            ThreadWorker.Priority = threadPriority;
            ThreadWorker.Start();

            ThreadSupervisor.StartThreadMonitor(this);
        }

        public RunThreadMan(ParameterizedThreadStart threadStart, object parametersObj, string runningTag, int lifeTimeSec, string logChannel, ThreadPriority threadPriority = ThreadPriority.Normal, bool isLogReq = false)
        {
            RunningTag = runningTag ?? "RunThreadMan**";
            LogChannel = (string.IsNullOrWhiteSpace(logChannel) ? "*" : logChannel.Trim()) + LogChannel;
            IsAbortRequested = false;
            LifeTimeSec = (lifeTimeSec <= 0) ? 3 : lifeTimeSec;
            IsThreadWorkingEnd = false;
            _isLogReq = isLogReq;
            ThreadWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    if (_isLogReq)
                        _log?.LogText(LogChannel, ThreadProcessId.ToString(), $@"Start new ThreadMan; Running Tag : {RunningTag}", "T51", "ThreadMan.(ctor)");

                    /////CYA-PENDING-CODE : RnD for BeginInvode/EndInvode for async execution
                    threadStart.Invoke(parametersObj);
                }
                catch (ThreadAbortException)
                {
                    IsThreadWorkingEnd = true;
                }
                catch (Exception ex)
                {
                    IsThreadWorkingEnd = true;
                    _log?.LogError(LogChannel, ThreadProcessId.ToString(), new Exception($@"RunningTag: {RunningTag}", ex), "EX51", "ThreadMan.(ctor)");
                    string msg = ex.Message;
                }
                finally
                {
                    if (_isLogReq)
                        _log?.LogText(LogChannel, ThreadProcessId.ToString(), $@"End ThreadMan; Running Tag : {RunningTag}", "T71", "ThreadMan.(ctor)");
                    IsThreadWorkingEnd = true;
                }
            })));
            ThreadWorker.IsBackground = true;
            ThreadWorker.Priority = threadPriority;
            ThreadWorker.Start();

            ThreadSupervisor.StartThreadMonitor(this);
        }

        public bool IsAborted
        {
            get
            {
                if (ThreadWorker?.ThreadState == ThreadState.Aborted)
                    return true;

                else if (ThreadWorker?.ThreadState == ThreadState.AbortRequested)
                    return true;

                else
                    return false;
            }
        }

        public string GetLogString()
        {
            return $@"LogChannel: {LogChannel}; RunningTag: {RunningTag}; Is Aborted: {IsAborted}; Is Abort Requested:{IsAbortRequested}; Is Expired:{IsExpired}; ThreadWorker Status: {ThreadWorker?.ThreadState.ToString()}; LifeTimeSec; {LifeTimeSec}; ExpiredTime: {ExpiredTime: yyyy-MM-dd HH:mm:ss}; Is ThreadWorking Ended: {IsThreadWorkingEnd}; ThreadProcessId: {ThreadProcessId}; ";
        }

        public void SetExpiredFlag()
        {
            IsExpired = true;
        }

        public bool IsEnd
        {
            get => ((IsThreadWorkingEnd) || (ThreadWorker?.ThreadState.IsStateInList(_threadEndStateList) == true));
        }

        /// <summary>
        /// Abort thread if working still on going. If does not care the result of isThreadEnded, just let waitToEndInMillicec = 0;
        /// </summary>
        /// <param name="isThreadEnded">Result of isThreadEnded is depend on waitToEndInMillicec parameter.</param>
        /// <param name="waitToEndInMillicec">Wait thread to finish for limited period in milliseconds</param>
        public void AbortRequest(out bool isThreadEnded, int waitToEndInMillicec = 0)
        {
            isThreadEnded = IsEnd;

            if (IsEnd)
            {
                isThreadEnded = true;
                return;
            }

            IsAbortRequested = true;
            _log?.LogText(LogChannel, ThreadProcessId.ToString(), $@"RunThreadMan abort operation; {RunningTag}", "A01", "RunThreadMan.AbortRequest");
            ThreadSupervisor.RequestAbort();

            waitToEndInMillicec = (waitToEndInMillicec < 0) ? 0 : waitToEndInMillicec;

            if (waitToEndInMillicec <= 0)
            {
                isThreadEnded = IsEnd;
                return;
            }

            DateTime expireTime = DateTime.Now.AddMilliseconds(waitToEndInMillicec);

            while ((expireTime.Ticks > DateTime.Now.Ticks) && (IsEnd == false))
                Thread.Sleep(90);

            isThreadEnded = IsEnd;
        }

        public void WaitUntilCompleted()
        {
            while (IsEnd == false)
            {
                Thread.Sleep(100);
            }
        }



        public void Dispose()
        {
            AbortRequest(out _);
            /////ThreadWorker = null;
            _log = null;
        }
    }
}