using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB
{
	public class AccessCommandRepository
	{
		private const string LogChannel = "ServerAccess";

		private bool _disposed = false;

		private ConcurrentDictionary<Guid, AccessCommandPack> _commandPackHisList = null;
		public ConcurrentQueue<AccessCommandPack> CommandPackQueue { get; private set; } = new ConcurrentQueue<AccessCommandPack>();

		private Thread _threadWorker = null;

		private DbLog _log = null;
		private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

		public AccessCommandRepository()
		{
			_commandPackHisList = new ConcurrentDictionary<Guid, AccessCommandPack>();

			_threadWorker = new Thread(new ThreadStart(RepositoryCleanUpThreadWorking));
			_threadWorker.IsBackground = true;
			_threadWorker.Start();
		}

		/// <summary>
		/// Update Or Insert Command Pack
		/// </summary>
		/// <param name="latestCommPack"></param>
		public void UpdateCommandPack(AccessCommandPack latestCommPack)
		{
			lock (_commandPackHisList)
			{
				if (_commandPackHisList.ContainsKey(latestCommPack.ExecutionRefId))
				{
					_commandPackHisList.TryGetValue(latestCommPack.ExecutionRefId, out AccessCommandPack previousCommPack);
					_commandPackHisList.TryUpdate(latestCommPack.ExecutionRefId, latestCommPack, previousCommPack);
				}
			}
		}

		public bool EnQueueNewCommandPack(AccessCommandPack latestCommPack, out string errorMsg)
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
							errMsgAlies = "Service has shutdown (EXB0201)";

						if (errMsgAlies == null)
						{
							if (_commandPackHisList.ContainsKey(latestCommPack.ExecutionRefId) == false)
							{
								string processId = string.IsNullOrWhiteSpace(latestCommPack.ProcessId) ? "-" : latestCommPack.ProcessId;
								Guid netProcessId = latestCommPack.NetProcessId.HasValue ? latestCommPack.NetProcessId.Value : Guid.Empty;

								Log.LogText(LogChannel, processId, latestCommPack, "A01", "AccessCommandRepository.EnQueueNewCommandPack", netProcessId: netProcessId,
									extraMsg: "Start - EnQueueNewCommandPack ; MsgObj: AccessCommandPack");

								AccessCommandPack dummyPack = latestCommPack.DuplicatedDummyCommandPack();

								CommandPackQueue.Enqueue(latestCommPack);

								Log.LogText(LogChannel, processId, "End - EnQueueNewCommandPack", "A10", "AccessCommandRepository.EnQueueNewCommandPack", netProcessId: netProcessId);

								Monitor.PulseAll(CommandPackQueue);
							}
						}
					}
				}
				catch (Exception ex)
				{
					errMsgAlies = (ex.Message ?? "") + "(EXIT21331)";
				}
			})))
			{ IsBackground = true };

			execThread.Start();
			execThread.Join();

			errorMsg = errMsgAlies;

			return (errorMsg == null);
		}

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 10);
		public AccessCommandPack DeQueueCommandPack()
		{
			AccessCommandPack retData = null;

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

		public bool GetCommandPack(Guid executionId, out AccessCommandPack commandPack)
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

			if (GetCommandPack(executionId, out AccessCommandPack commPack))
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
						foreach (KeyValuePair<Guid, AccessCommandPack> valPair in _commandPackHisList)
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
							foreach (Guid abordKey in abordKeyList)
							{
								_commandPackHisList.TryRemove(abordKey, out AccessCommandPack tempRes);
							}
						}
					}

					Thread.Sleep(1000 * 30);
				}
				catch (Exception ex)
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
					while (CommandPackQueue.TryDequeue(out AccessCommandPack commPack)) { commPack.Dispose(); }
				}
				catch { }
				Monitor.PulseAll(CommandPackQueue);
			}

			_log = null;
		}
	}
}
