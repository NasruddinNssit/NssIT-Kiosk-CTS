using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;
using NssIT.Kiosk.Log.DB;

namespace NssIT.Kiosk.Device.B2B.AccessSDK
{
	/// <summary>
	/// B2B Command Reporitory
	/// </summary>
	public class B2BCommandRepository : IDisposable
	{
		private const string LogChannel = "B2BAccess";

		private bool _disposed = false;

		private ConcurrentDictionary<Guid, B2BCommandPack> _commandPackHisList = null;
		public ConcurrentQueue<B2BCommandPack> CommandPackQueue { get; private set; } = new ConcurrentQueue<B2BCommandPack>();

		private Thread _threadWorker = null;

		public bool IsMachineBusy { get; private set; } = false;
		public string BusyMessage { get; private set; } = null;

		private DbLog _log = null;
		private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

		public B2BCommandRepository()
		{
			_commandPackHisList = new ConcurrentDictionary<Guid, B2BCommandPack>();

			_threadWorker = new Thread(new ThreadStart(RepositoryCleanUpThreadWorking));
			_threadWorker.IsBackground = true;
			_threadWorker.Start();
		}

		/// <summary>
		/// Update Or Insert Command Pack
		/// </summary>
		/// <param name="latestCommPack"></param>
		public void UpdateCommandPack(B2BCommandPack latestCommPack)
		{
			lock (_commandPackHisList)
			{
				if (_commandPackHisList.ContainsKey(latestCommPack.ExecutionRefId))
				{
					_commandPackHisList.TryGetValue(latestCommPack.ExecutionRefId, out B2BCommandPack previousCommPack);
					_commandPackHisList.TryUpdate(latestCommPack.ExecutionRefId, latestCommPack, previousCommPack);
				}
			}
		}

		public bool EnQueueNewCommandPack(B2BCommandPack latestCommPack, out string errorMsg)
		{
			errorMsg = null;
			string errMsgAlies = null;

			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				try
				{
					lock (CommandPackQueue)
					{
						if (_disposed)
							errMsgAlies = "Cash Machine has shutdown (EXB0101)";

						if (IsMachineBusy)
						{
							errMsgAlies = BusyMessage;
						}

						if (errMsgAlies == null)
						{
							if (_commandPackHisList.ContainsKey(latestCommPack.ExecutionRefId) == false)
							{
								string processId = string.IsNullOrWhiteSpace(latestCommPack.ProcessId) ? "-" : latestCommPack.ProcessId;
								Guid netProcessId = latestCommPack.NetProcessId.HasValue ? latestCommPack.NetProcessId.Value : Guid.Empty;

								Log.LogText(LogChannel, processId, latestCommPack, "A01", "B2BCommandRepository.EnQueueNewCommandPack", netProcessId: netProcessId,
									extraMsg: "Start - EnQueueNewCommandPack ; MsgObj: B2BCommandPack");

								B2BCommandPack dummyPack = latestCommPack.DuplicatedDummyCommandPack();

								CommandPackQueue.Enqueue(latestCommPack);

								if (_commandPackHisList.TryAdd(latestCommPack.ExecutionRefId, dummyPack))
								{
									if (latestCommPack.CommandCode == B2BCommandCode.StartMakePayment)
									{
										IsMachineBusy = true;
										BusyMessage = "Cash Mashine busy with payment execution.";
									}
								}

								Log.LogText(LogChannel, processId, "End - EnQueueNewCommandPack", "A10", "B2BCommandRepository.EnQueueNewCommandPack", netProcessId: netProcessId,
									extraMsg: "Dispatching .. ; MsgObj: B2BCommandPack");

								Monitor.PulseAll(CommandPackQueue);
							}
						}
					}
				}
				catch (Exception ex)
				{
					errMsgAlies = (ex.Message??"") + "(EXIT8331)";
				}
			})))
			{ IsBackground = true };

			execThread.Start();
			execThread.Join();

			errorMsg = errMsgAlies;

			return (errorMsg == null);
		}

		public void ResetBusyState()
		{
			IsMachineBusy = false;
			BusyMessage = null;
		}

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 10);
		public B2BCommandPack DeQueueCommandPack()
		{
			B2BCommandPack retData = null;

			lock (CommandPackQueue)
			{
				if (CommandPackQueue.Count == 0)
				{
					Monitor.Wait(CommandPackQueue, _MaxWaitPeriod);
				}
				if (CommandPackQueue.TryDequeue(out retData))
				{
					return retData;
				}
			}
			return null;
		}

		public bool GetCommandPack(Guid executionId, out B2BCommandPack commandPack)
		{
			commandPack = null;

			lock (_commandPackHisList)
			{
				if (_commandPackHisList.TryGetValue(executionId, out commandPack) == true)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Return true when result is ready. Else a false as result for result has not ready or Command Pack has not found.
		/// </summary>
		/// <param name="executionId"></param>
		/// <param name="maxWaitPeriod"></param>
		/// <param name="errorMsg"></param>
		/// <param name="resultData"></param>
		/// <param name="isCommandPackNotFound"></param>
		/// <returns></returns>
		public bool GetExecutionResult(Guid executionId, TimeSpan maxWaitPeriod, out string errorMsg, out IKioskMsg resultData, out bool isCommandPackNotFound)
		{
			errorMsg = null;
			resultData = null;
			isCommandPackNotFound = false;

			if (GetCommandPack(executionId, out B2BCommandPack commPack))
			{
				return commPack.PopUpResult(maxWaitPeriod, out errorMsg, out resultData);
			}
			else
			{
				isCommandPackNotFound = true;
				return false;
			}
		}

		private TimeSpan _maxStorePeriod = new TimeSpan(0, 10, 0);
		private TimeSpan _maxCreatedPeriod = new TimeSpan(2, 0, 0);
		private void RepositoryCleanUpThreadWorking()
		{
			TimeSpan noWaitPeriod = new TimeSpan(0);

			while (!_disposed)
			{
				try
				{
					List<Guid> abordKeyList = new List<Guid>();

					lock (_commandPackHisList)
					{
						foreach (KeyValuePair<Guid, B2BCommandPack> valPair in _commandPackHisList)
						{
							if (valPair.Value.IsResultDelivered == true)
							{
								abordKeyList.Add(valPair.Key);
							}
							else
							{
								// Put into the abort list for Result Data that is having expired time.
								bool resultFound = valPair.Value.PreviewResult(out string errorMsg, out IKioskMsg resData);

								if ((resultFound) && (valPair.Value.ResultTimeStamp.Add(_maxStorePeriod).Subtract(DateTime.Now).TotalSeconds < 0))
								{
									abordKeyList.Add(valPair.Key);
								}
								else if (valPair.Value.CreationTimeStamp.Add(_maxCreatedPeriod).Subtract(DateTime.Now).TotalSeconds < 0)
								{
									abordKeyList.Add(valPair.Key);
								}
							}
						}

						if (abordKeyList.Count > 0)
						{
							foreach(Guid abordKey in abordKeyList)
							{
								_commandPackHisList.TryRemove(abordKey, out B2BCommandPack tempRes);
							}
						}
					}

					Thread.Sleep(1000 * 30);
				}
				catch(Exception ex)
				{
					string tmpMsg = ex.Message;
				}
				
			}
		}

		public void Dispose()
		{
			_disposed = true;

			try
			{
				if (_threadWorker.ThreadState.IsState(ThreadState.Stopped) == false)
					_threadWorker.Interrupt();
			}
			catch { }

			Task.Delay(10).Wait();

			try
			{
				if (_threadWorker.ThreadState.IsState(ThreadState.Stopped) == false)
					_threadWorker.Abort();
			}
			catch { }

			Task.Delay(300).Wait();

			if (_commandPackHisList != null)
				_commandPackHisList.Clear();

			lock (CommandPackQueue)
			{
				try
				{
					while (CommandPackQueue.TryDequeue(out B2BCommandPack commPack)) { commPack.Dispose(); }
				}
				catch { }
				Monitor.PulseAll(CommandPackQueue);
			}

			_log = null;
		}
	}
}