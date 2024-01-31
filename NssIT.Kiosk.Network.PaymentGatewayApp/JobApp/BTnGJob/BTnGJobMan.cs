using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
using NssIT.Kiosk.Common.WebAPI;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.PaymentGatewayApp.JobApp.BTnGJob
{
    /// <summary>
    /// ClassCode:EXIT60.03
    /// </summary>
    public class BTnGJobMan : IDisposable
    {
        private const string _logChannel = "BTnGPaymentGateway_Schd";

        private bool _disposed = false;
        private Thread _workThread = null;

        private ConcurrentQueue<IBTnGJob> _newCancelList = new ConcurrentQueue<IBTnGJob>();

        private ConcurrentQueue<IBTnGJob> _pendingJobList = new ConcurrentQueue<IBTnGJob>();
        private ConcurrentQueue<IBTnGJob> _failJobList = new ConcurrentQueue<IBTnGJob>();

        private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
        private static BTnGJobMan _jobCtrl = null;

        private BTnGJobMan_CancelRefundExec _cancelRefundExec = null;
        
        private DbLog Log => DbLog.GetDbLog();

        /// <summary>
        /// FuncCode:EXIT60.0301
        /// </summary>
        private BTnGJobMan()
        {
            _cancelRefundExec = new BTnGJobMan_CancelRefundExec(this);
            Init();
        }

        private WebAPIAgent _webAPI = null;
        public WebAPIAgent WebAPI
        {
            get
            {
                if (_webAPI == null)
                {
                    _webAPI = new WebAPIAgent(AppDecorator.Config.Setting.GetSetting().WebApiURL);
                }
                return _webAPI;
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0303
        /// </summary>
        public static BTnGJobMan GetJobMan()
        {
            if (_jobCtrl == null)
            {
                try
                {
                    _manLock.WaitAsync().Wait();
                    if (_jobCtrl == null)
                    {
                        _jobCtrl = _jobCtrl ?? new BTnGJobMan();
                    }
                    return _jobCtrl;
                }
                finally
                {
                    if (_manLock.CurrentCount == 0)
                        _manLock.Release();
                }
            }
            else
                return _jobCtrl;
        }

        /// <summary>
        /// FuncCode:EXIT60.0304
        /// </summary>
        public void CancelRefundSale(string btngSalesTransactionNo, string bookingNo, string currency, string paymentGateway, decimal amount,
                BTnGKioskVoidTransactionState voidState)
        {
            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                lock (_newCancelList)
                {
                    _newCancelList.Enqueue(new CancelRefundAcquire(btngSalesTransactionNo, bookingNo, currency, paymentGateway, amount,
                        voidState));
                    Monitor.PulseAll(_newCancelList);
                }
            })));
            tWorker.IsBackground = true;
            tWorker.Priority = ThreadPriority.Highest;
            tWorker.Start();
        }

        private void Init()
        {
            try
            {
                _workThread = new Thread(JobThreadWorking);
                _workThread.IsBackground = true;
                _workThread.Start();
            }
            catch (Exception ex)
            {
                string byPassMsg = ex.Message;
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0306
        /// </summary>
        private void JobThreadWorking()
        {
            while (_disposed == false)
            {
                string processId = "*";
                try
                {
                    IBTnGJob job = GetNextJob();

                    if (job is CancelRefundAcquire cancelRefund)
                    {
                        processId = $@"CancelRefund-{cancelRefund.BTnGSalesTransactionNo}";
                        CancelingSale(processId, cancelRefund);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(_logChannel, processId, new Exception($@"{ex.Message}; (EXIT60.0306.EX01)", ex), "BTnGJob.JobThreadWorking");
                }
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            /// <summary>
            /// FuncCode:EXIT60.038A
            /// </summary>
            void CancelingSale(string processIdX, CancelRefundAcquire cancelRefundX)
            {
                //CYA-PENDING-Work .. Remainder : Test fail handling
                int maxTry = 3;
                for (int tryCount = 0; tryCount < maxTry; tryCount++)
                {
                    try
                    {
                        _cancelRefundExec.DoJob(WebAPI, 
                            cancelRefundX.BTnGSalesTransactionNo, 
                            cancelRefundX.BookingNo, 
                            cancelRefundX.Currency, 
                            cancelRefundX.PaymentGateway, 
                            cancelRefundX.Amount, 
                            cancelRefundX.KioskVoidTransactionState);
                        break;
                    }
                    catch (Exception ex2)
                    {
                        Log.LogError(_logChannel, cancelRefundX?.BookingNo, ex2, "EX01", "BTnGJobMan.CancelingSale");

                        if (tryCount >= (maxTry - 1))
                        {
                            // Put Fail Job to Pending cache
                            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
                            {
                                lock (_newCancelList)
                                {
                                    _failJobList.Enqueue(cancelRefundX);
                                }
                            })));
                            tWorker.IsBackground = true;
                            tWorker.Priority = ThreadPriority.Highest;
                            tWorker.Start();
                            throw ex2;
                        }
                        else
                            Thread.Sleep(1000);
                    }
                }
            }
        }

        private DateTime? _nextPendingJobActivatedTime = null;
        private int _maxTransExecutionPeriodMinutes = 10;
        private TimeSpan _maxWaitPeriod = new TimeSpan(0, 1, 0);
        /// <summary>
        /// FuncCode:EXIT60.0308
        /// </summary>
        private IBTnGJob GetNextJob()
        {
            IBTnGJob retJob = null;
            bool isJobFound = false;

            if (_disposed == false)
            {
                try
                {
                    lock (_newCancelList)
                    {
                        if ((_newCancelList.Count == 0) && (_pendingJobList.Count == 0))
                        {
                            _nextPendingJobActivatedTime = DateTime.Now.AddSeconds(_maxWaitPeriod.TotalSeconds - 1);
                            Monitor.Wait(_newCancelList, _maxWaitPeriod);
                        }

                        if (_disposed == false)
                        {
                            if (_newCancelList.Count > 0)
                                isJobFound = _newCancelList.TryDequeue(out retJob);

                            else if (_pendingJobList.Count > 0)
                                isJobFound = _pendingJobList.TryDequeue(out retJob);

                            else if ((_failJobList.Count > 0) &&
                                (_nextPendingJobActivatedTime.HasValue) &&
                                (_nextPendingJobActivatedTime.Value.Ticks <= DateTime.Now.Ticks)
                                )
                            {
                                _nextPendingJobActivatedTime = null;

                                while (_failJobList.TryDequeue(out IBTnGJob reTryJob) == true)
                                {
                                    if (reTryJob.CreationTime.AddMinutes(_maxTransExecutionPeriodMinutes).Ticks >= DateTime.Now.Ticks)
                                    {
                                        if (isJobFound == false)
                                        {
                                            retJob = reTryJob;
                                            isJobFound = true;
                                        }
                                        else
                                        {
                                            _pendingJobList.Enqueue(reTryJob);
                                        }
                                    }
                                    else
                                    {
                                        string tt1 = "debug-stop";
                                    }
                                }
                            }
                        }
                    }
                }
                // Used to handle "_logList" is null after disposed
                catch (Exception ex) { string byPassStr = ex.Message; }
            }

            if (isJobFound)
                return retJob;
            else
                return null;
        }

        public void Dispose()
        {
            _disposed = true;

            lock (_newCancelList)
            {
                Monitor.PulseAll(_newCancelList);
            }
        }

        class CancelRefundAcquire : IBTnGJob, IDisposable
        {
            public bool IsDone { get; private set; } = false;
            public bool? IsSuccess { get; private set; } = null;
            public Exception Error { get; private set; } = null;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            public string BTnGSalesTransactionNo { get; private set; }
            public string BookingNo { get; private set; }
            public string Currency { get; private set; }
            public string PaymentGateway { get; private set; }
            public decimal Amount { get; private set; }
            public BTnGKioskVoidTransactionState KioskVoidTransactionState { get; private set; }

            public DateTime CreationTime { get; private set; }

            public CancelRefundAcquire(string salesTransactionNo, string bookingNo, string currency, string paymentGateway, decimal amount,
                BTnGKioskVoidTransactionState voidState)
            {
                BTnGSalesTransactionNo = salesTransactionNo;
                BookingNo = bookingNo;
                Currency = currency; 
                PaymentGateway = paymentGateway;
                Amount = amount;
                KioskVoidTransactionState = voidState;
                CreationTime = DateTime.Now;
            }

            public void Dispose()
            {
                Error = null;
            }

            public void SetFail(Exception ex)
            {
                IsDone = true;
                IsSuccess = false;
                Error = ex;
            }

            public void SetSuccess()
            {
                IsDone = true;
                IsSuccess = true;
            }
        }
    }
}
