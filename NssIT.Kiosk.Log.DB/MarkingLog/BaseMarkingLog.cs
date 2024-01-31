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

namespace NssIT.Kiosk.Log.DB.MarkingLog
{
    public class BaseMarkingLog : IDisposable
	{
		private const string _logChannel = "MarkingLog";
		private Thread _logThread = null;
		
		private ConcurrentDictionary<Guid, MarkLogSection> _logList = new ConcurrentDictionary<Guid, MarkLogSection>();
		private DbLog _dbLog = null;

		/// <summary>
		/// The lock for accessing _logList & MarkLogSection.MsgList
		/// </summary>
		private object _allDataLock = new object();

		private static BaseMarkingLog _mkLog = null;
		private static object _manLock = new object();
		
		public static BaseMarkingLog GetDbLog()
		{
			if (_mkLog == null)
			{
				Thread t1 = new Thread(delegate() 
				{
					lock (_manLock)
					{
						if (_mkLog == null)
						{
							_mkLog = new BaseMarkingLog();
						}
					}
				});
				t1.IsBackground = true;
				t1.Priority = ThreadPriority.AboveNormal;
				t1.Start();
				t1.Join();

				return _mkLog;
			}
			else
				return _mkLog;
		}

		private BaseMarkingLog()
		{
			_dbLog = DbLog.GetDbLog();
			Init();
		}

		private void Init()
		{
			try
			{
				_logThread = new Thread(LogInfoToDb);
				_logThread.IsBackground = true;
				_logThread.Start();
			}
			catch (Exception ex)
			{
				string byPassMsg = ex.Message;
			}
		}

		private void LogInfoToDb()
		{
			int cleanUpIntervalMinutes = 3;
			LogFileInfo inforMsg = null;
			Guid[] existingMKList = new Guid[0];
			StringBuilder msg = new StringBuilder();
			MarkLogSection currMkSect = null;
			DateTime currLogReqTime = DateTime.MinValue;
			DateTime firstLogTime = DateTime.MinValue;
			DateTime nextCleanUpTime = DateTime.Now.AddMinutes(cleanUpIntervalMinutes);

			while (_disposed == false)
			{
				try
				{
					existingMKList = new Guid[0];

					lock (_allDataLock)
					{
						existingMKList = _logList.Keys.ToArray();
					}

					if (existingMKList.Length > 0)
					{
						foreach (Guid headId in existingMKList)
						{
							currMkSect = null;
							currLogReqTime = DateTime.MinValue;

							// Get Log-Requested Log Mark Section
							lock (_allDataLock)
							{
								if (_logList.TryGetValue(headId, out currMkSect) && (currMkSect?.LogRequestedTime.HasValue == true))
								{
									currLogReqTime = currMkSect.LogRequestedTime.Value;
									// Temporary remove currMkSect from list
									_logList.TryRemove(headId, out _);
									currMkSect.LogRequestedTime = null;
								}
								else
									currMkSect = null;
							}

							// Log to DB
							if (currMkSect != null)
							{
								try
								{
									MarkLog[] mkLgArr = null;

									// Get related Msg refer to currLogReqTime
									lock (_allDataLock)
									{
										mkLgArr = (from lg in currMkSect.MsgList
													where lg.ATime.Ticks <= currLogReqTime.Ticks
													select lg).OrderBy(i => i.ATime).ToArray();
									}

									if (mkLgArr?.Length > 0)
									{
										firstLogTime = mkLgArr[0].ATime;
										msg.Clear();

										// Log Msg to db
										foreach (MarkLog lgX in mkLgArr)
										{
											msg.Append($@"{lgX.ATime:HH:mm:ss.fff} {lgX.Mark}{"\r\n"}");
										}

										if (msg.Length > 0)
										{
											// Log into db log
											_dbLog.LogData
											(
												new LogFileInfo()
												{
													Channel = _logChannel, 
													MsgXStr = msg.ToString(),
													LoctXTag = "A01",
													ClsMetXName = currMkSect.MarkTitle,
													MsgXType = MessageType.Marking,
													Time = firstLogTime,
													ProXId = currMkSect.HeaderId.ToString()
												}
											);
										}

										// Remove "already log Msg" from MarkLogSection
										lock (_allDataLock)
										{
											foreach (MarkLog lgX in mkLgArr)
											{
												currMkSect.MsgList.Remove(lgX);
											}
										}
									}
								}
								catch (Exception ex)
								{
									string errMsg = ex.Message;
								}
								finally
								{
									///// Update currMkSect back to _logList if still have outstanding log-msg
									try
									{
										lock (_allDataLock)
										{
											if (currMkSect?.MsgList?.Count > 0)
											{
												_logList.TryAdd(currMkSect.HeaderId, currMkSect);
											}
										}
									}
									catch { }
                                }
							}
						}
					}
					CleanUp();

					lock (_allDataLock)
					{
						Monitor.Wait(_allDataLock, 1000);
					}
				}
				catch (Exception ex)
				{
					string byPassMsg = ex.Message;
				}
			}
			LastLogToDb(out bool isLogWritten);

			if (isLogWritten)
				Thread.Sleep(500);

			return;
			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			void LastLogToDb(out bool isLogWrittenX)
			{
				isLogWrittenX = false;
				Guid[] existingMKListX = null;

				lock (_allDataLock)
				{
					existingMKListX = _logList.Keys.ToArray();
				}
			
				if (existingMKListX.Length > 0)
				{
					foreach (Guid headIdX in existingMKListX)
					{
						currMkSect = null;
						currLogReqTime = DateTime.MinValue;

						try
						{
							// Get related Mark Log Section
							lock (_allDataLock)
							{
								_logList.TryGetValue(headIdX, out currMkSect);
							}
						}
						catch { }

						// Log to DB
						if (currMkSect != null) 
						{
							try
							{
								MarkLog[] lgArrX = null;

								// Get related Msg refer to currLogReqTime
								lock (_allDataLock)
								{
									lgArrX = currMkSect.MsgList.OrderBy(i => i.ATime).ToArray();
								}

								if (lgArrX?.Length > 0)
								{
									firstLogTime = lgArrX[0].ATime;
									msg.Clear();

									// Log Msg to db
									foreach (MarkLog lgX in lgArrX)
									{
										msg.Append($@"{lgX.ATime:HH:mm:ss.fff} {lgX.Mark}{"\r\n"}");
									}

									if (msg.Length > 0)
									{
										// Log into db log
										isLogWrittenX = true;
										_dbLog.LogData
											(
												new LogFileInfo()
												{
													Channel = _logChannel,
													MsgXStr = msg.ToString(),
													LoctXTag = "END01",
													ClsMetXName = currMkSect.MarkTitle,
													MsgXType = MessageType.Marking,
													Time = firstLogTime,
													ProXId = currMkSect.HeaderId.ToString()
												}
											);
									}
								}
							}
							catch (Exception ex)
							{
								string byPassMsg = ex.Message;
							}
						}
					}
				}
			}

			void CleanUp()
			{
				if (_disposed)
					return;

				if (nextCleanUpTime.Ticks <= DateTime.Now.Ticks)
				{
					Guid[] existingMKListX = null;

					try
					{
						lock (_allDataLock)
						{
							existingMKListX = _logList.Keys.ToArray();

							foreach (Guid hdId in existingMKListX)
							{
								// Remove from _logList if MarkLogSection is blank
								try
								{
									if ((_logList.TryGetValue(hdId, out MarkLogSection mkSectX)) && (mkSectX.MsgList.Count == 0))
									{
										_logList.TryRemove(hdId, out _);
									}
								}
								catch { }
							}
						}
					}
					catch (Exception ex)
					{
						string t1 = ex.Message;
					}
					finally
					{
						nextCleanUpTime = DateTime.Now.AddMinutes(cleanUpIntervalMinutes);
					}
				}
			}
		}

		public void LogMark(MarkLogSection section, MarkLog log, bool isLogRequest)
		{
			if (_disposed)
				return;

			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				// Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
				//        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
				if (_disposed == false)
				{
					try
					{
						lock (_allDataLock)
						{
							MarkLogSection mkSect = null;

							_logList.TryRemove(section.HeaderId, out mkSect);

							if (mkSect is null)
							{
								mkSect = new MarkLogSection(section.HeaderId, section.MarkTitle, section.MarkLogType);
							}

							if (mkSect != null)
							{
								mkSect.MsgList.Add(log);

								if (isLogRequest)
								{
									mkSect.LogRequestedTime = log.ATime;
								}

								if (_disposed == false)
								{
									_logList.TryAdd(mkSect.HeaderId, mkSect);
									Monitor.PulseAll(_allDataLock);
								}
							}
						}
					}
					catch (Exception ex2)
					{
						string byPassStr = ex2.Message;
					}
				}
			})));
			execThread.IsBackground = true;
			execThread.Start();
		}

		public void LogRequest(MarkLogSection section, DateTime requestTime)
		{
			if (_disposed)
				return;

			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				// Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
				//        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
				if (_disposed == false)
				{
					try
					{
						lock (_allDataLock)
						{
							MarkLogSection mkSect = null;

							_logList.TryRemove(section.HeaderId, out mkSect);

							if (mkSect != null)
							{
								mkSect.LogRequestedTime = requestTime;

								if (_disposed == false)
								{
									_logList.TryAdd(mkSect.HeaderId, mkSect);
									Monitor.PulseAll(_allDataLock);
								}
							}
						}
					}
					catch (Exception ex2)
					{
						string byPassStr = ex2.Message;
					}
				}
			})));
			execThread.IsBackground = true;
			execThread.Start();
		}

		public void LogRequestByLogCount(MarkLogSection section, DateTime requestTime, int minLogCount)
		{
			if (_disposed)
				return;

			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				// Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
				//        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
				if (_disposed == false)
				{
					try
					{
						lock (_allDataLock)
						{
							MarkLogSection mkSect = null;
				
							_logList.TryRemove(section.HeaderId, out mkSect);

							if (mkSect != null)
							{
								if (mkSect.MsgList.Where(r => (r.ATime.Ticks <= requestTime.Ticks)).ToArray().Length >= minLogCount)
								{
									mkSect.LogRequestedTime = requestTime;
								}

								if (_disposed == false)
								{
									_logList.TryAdd(mkSect.HeaderId, mkSect);
									Monitor.PulseAll(_allDataLock);
								}
							}
						}
					}
					catch (Exception ex2)
					{
						string byPassStr = ex2.Message;
					}
				}
			})));
			execThread.IsBackground = true;
			execThread.Start();
		}

		private bool _disposed = false;
		public void Dispose()
		{
			if (_disposed == false)
			{
				_disposed = true;
				lock (_allDataLock)
				{
					Monitor.PulseAll(_allDataLock);
				}
			}
		}
	}
}
