using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Sqlite.DB.AccessDB.Base;
using NssIT.Kiosk.AppDecorator.Common.Access;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Sqlite.DB.AccessDB
{
	/// <summary>
	/// ClassCode:EXIT25.11
	/// </summary>
	public class DatabaseAx
	{
		private const string _logChannel = "Database-AX";

		private DbLog _log = null;

		private Thread _accessThreadWorker = null;
		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 1);

		private ConcurrentQueue<IDBAxExecution<ITransSuccessEcho>> _commandList
			= new ConcurrentQueue<IDBAxExecution<ITransSuccessEcho>>();

		private ConcurrentDictionary<Guid, IDBAxExecution<ITransSuccessEcho>> _answerList
			= new ConcurrentDictionary<Guid, IDBAxExecution<ITransSuccessEcho>>();

		private bool _disposed = false;

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		//private static SemaphoreSlim _answerLock = new SemaphoreSlim(1);

		private static DatabaseAx _dbAccess = null;

		/// <summary>
		/// Return null is api creation in progress;
		/// </summary>
		public bool? IsApiCreatedSuccessfully { get; private set; } = null;
		public Exception ApiError { get; private set; } = null;

		public DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		public static DatabaseAx GetAccess()
		{
			if (_dbAccess == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_dbAccess == null)
					{
						_dbAccess = new DatabaseAx();
					}
					return _dbAccess;
				}
				finally
				{
					if (_manLock.CurrentCount == 0)
						_manLock.Release();
				}
			}
			else
				return _dbAccess;
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				lock (_commandList)
				{
					Log.LogText(_logChannel, "*", "Start1 - Dispose", "A01", "DatabaseAx.Dispose");

					try
					{
						while (_commandList.Count > 0)
							_commandList.TryDequeue(out _);

						Monitor.PulseAll(_commandList);
						Thread.Sleep(100);
					}
					catch { }

					Log.LogText(_logChannel, "*", "End1 - Dispose", "A05", "DatabaseAx.Dispose");
				}

				Log.LogText(_logChannel, "*", "Start2 - Dispose", "A06", "DatabaseAx.Dispose");

				try
				{
					_answerList.Clear();
					Thread.Sleep(100);
				}
				catch { }

				Log.LogText(_logChannel, "*", "End2 - Dispose", "A10", "DatabaseAx.Dispose");
			}
		}

		private DatabaseAx()
		{
			_accessThreadWorker = new Thread(AccessThreadWorking);
			_accessThreadWorker.IsBackground = true;
			_accessThreadWorker.Start();
		}

		/// <summary>
		/// FuncCode:005
		/// </summary>
		private void AccessThreadWorking()
		{
			///// .. LastTimePeriod to Keep the answer; keep last 10 minutes Answers
			int keepLastAnswerListPeriodSec = 60 * 10;
			DateTime? nextAnswerListCleanUpTime = null;

			try
			{
				nextAnswerListCleanUpTime = GetNextAnswerListCleanUpTime();
				while (!_disposed)
				{
					//--------------------------------------------------------------------------------------------------------------------
					if (GetExecCommand() is IDBAxExecution<ITransSuccessEcho> axCommand)
					{
						if (_disposed)
							break;

						try
						{

							axCommand.DBExecute(this);
							UpdateAnswer(axCommand);
						}
						catch (Exception ex)
						{
							Log.LogText(_logChannel, "*", axCommand, "EX01", "DatabaseAx.AccessThreadWorking", AppDecorator.Log.MessageType.Error,
								extraMsg: $@"{ex.Message}; MsgObj: {axCommand?.GetType()}");
							Log.LogError(_logChannel, "*", ex, "EX02", "DatabaseAx.AccessThreadWorking");
						}
					}
					else
					{
						if (nextAnswerListCleanUpTime.Value.Ticks < DateTime.Now.Ticks)
						{
							CleanUpAnswerList(DateTime.Now.AddSeconds(keepLastAnswerListPeriodSec * -1));
							nextAnswerListCleanUpTime = GetNextAnswerListCleanUpTime();
						}
					}
					//--------------------------------------------------------------------------------------------------------------------
				}

				Log.LogText(_logChannel, "*", "Quit AccessExecutionThreadWorking", "A20", "DatabaseAx.AccessThreadWorking");
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "*", ex, "EX10", "DatabaseAx.AccessThreadWorking");
			}

			return;
			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			/// <summary>
			/// FuncCode:EXIT25.1106
			/// </summary>
			void UpdateAnswer(IDBAxExecution<ITransSuccessEcho> answer)
			{
				try
				{
					lock(_answerList)
                    {
						_answerList.TryAdd(answer.CommandId, answer);
					}
				}
				catch { }
			}

			/// <summary>
			/// FuncCode:EXIT25.1107
			/// </summary>
			IDBAxExecution<ITransSuccessEcho> GetExecCommand()
			{
				IDBAxExecution<ITransSuccessEcho> retcommand = null;
				bool commandFound = false;

				if (_disposed == false)
				{
					try
					{
						lock (_commandList)
						{
							if (_commandList.Count == 0)
							{
								Monitor.Wait(_commandList, _MaxWaitPeriod);
							}

							commandFound = _commandList.TryDequeue(out retcommand);
						}
					}
					// Used to handle "_lightParamList" is null after disposed
					catch (Exception ex) { string byPassStr = ex.Message; }
				}

				if (commandFound)
					return retcommand;
				else
					return null;
			}

			/// <summary>
			/// FuncCode:008
			/// </summary>
			DateTime GetNextAnswerListCleanUpTime()
			{
				return DateTime.Now.AddSeconds(keepLastAnswerListPeriodSec);
			}

			/// <summary>
			/// FuncCode:EXIT25.1109
			/// </summary>
			void CleanUpAnswerList(DateTime keepLastTime)
			{
				try
				{
					lock (_answerList)
                    {
						KeyValuePair<Guid, IDBAxExecution<ITransSuccessEcho>>[] ansList = _answerList.ToArray();

						foreach(KeyValuePair<Guid, IDBAxExecution<ITransSuccessEcho>> keyPair in ansList)
                        {
                            try
                            {
								if ((keyPair.Value?.ResultStatus) is null)
                                {
									bool tt1 = _answerList.TryRemove(keyPair.Key, out _);
									string tt2 = "debug";
								}
								else if ((keyPair.Value.ResultStatus.TransCreatedTime.Ticks < keepLastTime.Ticks))
								{
									bool tt1 = _answerList.TryRemove(keyPair.Key, out _);
									string tt2 = "debug";
								}
							}
							catch (Exception ex)
                            {
								try
								{
									bool tt1 = _answerList.TryRemove(keyPair.Key, out _);
								}
								catch { }
								string tt3 = ex.Message;
							}
					    }
					}
				}
				catch { }
			}
		}

		/// <summary>
		/// Return a executed success result; else throw error; FuncCode:EXIT25.1110
		/// </summary>
		/// <param name="command"></param>
		/// <param name="waitDelaySec"></param>
		/// <returns></returns>
		public IDBAxExecution<ITransSuccessEcho> ExecCommand(
			IDBAxExecution<ITransSuccessEcho> command, 
			int waitDelaySec = 60)
		{
			if (command == null)
				throw new Exception("Command cannot be NULL at DatabaseAx.ExecCommand; (EXIT25.1110.X01)");
			//-----------------------------

			Guid commandId = command.CommandId;

			//-----------------------------
			// Send Command to Execute
			if ((_disposed == false))
			{
				Thread threadWorker = new Thread(new ThreadStart(new Action(() => {
					lock (_commandList)
					{
						_commandList.Enqueue(command);
						Monitor.PulseAll(_commandList);
					}
				})));
				threadWorker.IsBackground = true;
				threadWorker.Start();
			}
			else
				return null;
			//-----------------------------
			// Wait for answer
			DateTime endWaitTime = DateTime.Now.AddSeconds(waitDelaySec);

			IDBAxExecution<ITransSuccessEcho> answer = null;
			Thread restThreadWorker = new Thread(new ThreadStart(new Action(() => {
				while ((_disposed == false) && (endWaitTime.Ticks > DateTime.Now.Ticks) && (answer is null))
				{
					Thread.Sleep(100);
					try
					{
						lock(_answerList)
                        {
							if (_answerList.TryRemove(commandId, out answer) == true)
								break;
							else
								answer = null;
						}
					}
					catch { }
				}
			})));
			NssITThreadTools.WaitThreadToFinish(restThreadWorker, waitDelaySec + 5);

			//-----------------------------
			// Validate answer
			if (answer != null)
			{
				if (answer.ResultStatus.IsSuccess)
				{
					return answer;
				}
				if (answer.ResultStatus.IsErrorFound)
				{
					if (answer.ResultStatus.Error != null)
						throw answer.ResultStatus.Error;
					else
						throw new Exception("Unknown error when executing data access command; (EXIT25.1110.X02)");
				}
				else if (answer.ResultStatus.IsTransDone)
				{
					throw new Exception("Data access command is not running; (EXIT25.1110.X03)");
				}
				else
				{
					throw new Exception("Unknown data access status, (EXIT25.1101.X05)");
				}
			}

			//-----------------------------
			if (_disposed == false)
				throw new Exception("Timeout; Fail to execute data access command; (EXIT25.1101.X10)");
			else
				return null;

			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		}
	}
}
