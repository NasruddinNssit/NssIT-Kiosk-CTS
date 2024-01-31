using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using Newtonsoft.Json;
using NssIT.Kiosk;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;

namespace NssIT.Kiosk.Client.Base.Time
{
    public class ResetTimeoutManager
    {
		private const string LogChannel = "ResetTimeoutManager";

		private Thread _tmChangeThread = null;
		private SemaphoreSlim _asyncLock = new SemaphoreSlim(1);
		private ConcurrentQueue<TimeChangeRequest> _timeChangeList = new ConcurrentQueue<TimeChangeRequest>();

		private static ResetTimeoutManager _man = null;
		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);

		public static ResetTimeoutManager GetLocalTimeoutManager()
		{
			if (_man == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_man == null)
					{
						_man = new ResetTimeoutManager();
					}
					return _man;
				}
				finally
				{
					_manLock.Release();
				}
			}
			else
				return _man;
		}

		private ResetTimeoutManager()
		{
			Init();
		}

		private void Init()
		{
			try
			{
				_tmChangeThread = new Thread(ChangeTimeoutThreadWorking);
				_tmChangeThread.IsBackground = true;
				_tmChangeThread.Start();
			}
			catch (Exception ex)
			{
				string byPassMsg = ex.Message;
			}
		}

		private async void ChangeTimeoutThreadWorking()
		{
			TimeChangeRequest changeReq = null;
			while (_disposed == false)
			{
				try
				{
					changeReq = await GetNextTimeChangeReq();

					if (changeReq != null)
					{
						ChangeTimeout(changeReq);
					}
				}
				catch (Exception ex)
				{
					string byPassMsg = ex.Message;
				}
			}
		}

		private int _custInfoTimeoutExtensionCounter = 0;
		private DateTime _nextValidCustInfoTimeoutExtensionTime = DateTime.Now;
		public void ResetCustomerInfoTimeoutCounter()
		{
			_nextValidCustInfoTimeoutExtensionTime = DateTime.Now.AddMinutes(2);
			_custInfoTimeoutExtensionCounter = 0;
		}

		public void ExtendCustomerInfoTimeout(int additionalSec)
		{
			if (_custInfoTimeoutExtensionCounter > 0)
				return;
			else if (_nextValidCustInfoTimeoutExtensionTime.Ticks > DateTime.Now.Ticks)
				return;

			_custInfoTimeoutExtensionCounter++;

			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				// Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
				//        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
				if (_disposed == false)
				{
					try
					{
						lock (_timeChangeList)
						{
							_timeChangeList.Enqueue(new TimeChangeRequest(TimeoutChangeMode.MandatoryExtension, additionalSec));
							Monitor.PulseAll(_timeChangeList);
						}
					}
					// Used to handle "_logList" is null after disposed
					catch (Exception ex2) { string byPassStr = ex2.Message; }
				}
			})));
			execThread.IsBackground = true;
			//execThread.Priority = ThreadPriority.AboveNormal;
			execThread.Start();
			execThread.Join();
		}

		public void RemoveCustomerInfoTimeoutExtension()
		{
			_custInfoTimeoutExtensionCounter = 0;

			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				// Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
				//        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
				if (_disposed == false)
				{
					try
					{
						lock (_timeChangeList)
						{
							_timeChangeList.Enqueue(new TimeChangeRequest(TimeoutChangeMode.RemoveMandatoryTimeout, 0));
							_custInfoTimeoutExtensionCounter++;
							Monitor.PulseAll(_timeChangeList);
						}
					}
					// Used to handle "_logList" is null after disposed
					catch (Exception ex2) { string byPassStr = ex2.Message; }
				}
			})));
			execThread.IsBackground = true;
			execThread.Priority = ThreadPriority.AboveNormal;
			execThread.Start();
		}

		public void ResetTimeout()
		{
			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				// Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
				//        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
				if (_disposed == false)
				{
					try
					{
						lock (_timeChangeList)
						{
							_timeChangeList.Enqueue(new TimeChangeRequest());
							Monitor.PulseAll(_timeChangeList);
						}
					}
					// Used to handle "_logList" is null after disposed
					catch (Exception ex2) { string byPassStr = ex2.Message; }
				}
			})));
			execThread.IsBackground = true;
			execThread.Priority = ThreadPriority.AboveNormal;
			execThread.Start();
		}

		private void ChangeTimeout(TimeChangeRequest msg)
		{
			try
			{
				if (msg.ChangeMode == TimeoutChangeMode.ResetNormalTimeout)
					App.NetClientSvc.SalesService.ResetTimeout();

				else if (msg.ChangeMode == TimeoutChangeMode.MandatoryExtension)
					App.NetClientSvc.SalesService.ExtendCustomerInfoEntryTimeout(msg.TimeChangeSec);

				else if (msg.ChangeMode == TimeoutChangeMode.RemoveMandatoryTimeout)
					App.NetClientSvc.SalesService.RemoveCustomerInfoEntryTimeoutExtension();

				//RemoveMandatoryTimeout

			}
			catch (Exception ex)
			{
				string byPass = ex.Message;
				App.Log.LogError(LogChannel, "-", ex, "EX01", "ResetTimeoutManager.ChangeTimeout");
			}
		}


		private int _validResetTimeoutIntervalSec = 5;
		private DateTime _lastResetTimeoutTime = DateTime.Now;
		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 1);
		private async Task<TimeChangeRequest> GetNextTimeChangeReq()
		{
			TimeChangeRequest retTmChgReq = null;
			bool reqFound = false;

			if (_disposed == false)
			{
				try
				{
					await _asyncLock.WaitAsync();

					lock (_timeChangeList)
					{
						if (_timeChangeList.Count == 0)
						{
							Monitor.Wait(_timeChangeList, _MaxWaitPeriod);
						}
						reqFound = _timeChangeList.TryDequeue(out retTmChgReq);

						if (reqFound)
						{
							if (retTmChgReq.ChangeMode == TimeoutChangeMode.ResetNormalTimeout)
							{
								while (_timeChangeList.TryPeek(out TimeChangeRequest peekTmChgReq) && (peekTmChgReq?.ChangeMode == TimeoutChangeMode.ResetNormalTimeout))
								{
									bool x = _timeChangeList.TryDequeue(out TimeChangeRequest retTmChgReqX);
								}

								if (DateTime.Now.Subtract(_lastResetTimeoutTime).TotalSeconds < _validResetTimeoutIntervalSec)
								{
									reqFound = false;
									retTmChgReq = null;
								}
								else
									_lastResetTimeoutTime = DateTime.Now;
							}
						}
					}
				}
				// Used to handle "_logList" is null after disposed
				catch (Exception ex) { string byPassStr = ex.Message; }
				finally
				{
					_asyncLock.Release();
				}
			}

			if (reqFound)
				return retTmChgReq;
			else
				return null;
		}

		private bool _disposed = false;
		public async void Dispose()
		{
			if (_disposed == false)
			{
				_disposed = true;

				try
				{
					await _asyncLock.WaitAsync();

					lock (_timeChangeList)
					{
						TimeChangeRequest tmp = null;
						do
						{
							tmp = null;
							_timeChangeList.TryDequeue(out tmp);
						} while (tmp != null);

						Monitor.PulseAll(_timeChangeList);
					}
				}
				catch { }
				finally
				{
					_timeChangeList = null;
					_asyncLock.Release();

					_asyncLock.Dispose();
					_asyncLock = null;
				}
			}
		}

		class TimeChangeRequest
		{
			public TimeoutChangeMode ChangeMode { get; private set; } = TimeoutChangeMode.ResetNormalTimeout;
			public DateTime RequestTime { get; private set; } = DateTime.Now;


			/// <summary>
			/// For TimeoutChangeMode.MandatoryExtension;
			/// </summary>
			public int TimeChangeSec { get; private set; } = 30;

			public TimeChangeRequest() { }

			public TimeChangeRequest(TimeoutChangeMode changeMode, int timeChangeSec) 
			{
				ChangeMode = changeMode;
				TimeChangeSec = timeChangeSec;
			}

		}
	}
}
