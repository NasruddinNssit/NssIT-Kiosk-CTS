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

namespace NssIT.Kiosk.Log.DB
{
    public class DbLog : IDisposable
    {
        private Thread _logThread = null;
        private SemaphoreSlim _asyncLock = new SemaphoreSlim(1);
        private ConcurrentQueue<LogFileInfo> _logList = new ConcurrentQueue<LogFileInfo>();
        private object _listLock = new object();
        private LogDBManager _logMan = null;

        private string _dbConnStr = $@"Data Source=C:\dev\RND\Sqlite\LogDb\DB\NssITKioskLogMaster.db;Version=3";

        private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
        private static DbLog _dbLog = null;

        //public static void Init()
        //      {
        //	SemaphoreSlim manLock = _manLock;
        //	DbLog dbLog = GetDbLog();
        //}

        public static DbLog GetDbLog()
        {
            if (_dbLog == null)
            {
                try
                {
                    _manLock.WaitAsync().Wait();
                    if (_dbLog == null)
                    {
                        //_dbLog = new DbLog(AppDecorator.Config.Setting.GetSetting().LogDbConnectionStr);
                        _dbLog = new DbLog("-");
                    }
                    return _dbLog;
                }
                finally
                {
                    if (_manLock.CurrentCount == 0)
                        _manLock.Release();
                }
            }
            else
                return _dbLog;
        }

        private DbLog(string connectionString)
        {
            //_dbConnStr = connectionString;
            _logMan = new LogDBManager();
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

        private string ConnectionString
        {
            get
            {
                string dbConnStr = $@"Data Source={_logMan.CurrentDBFilePath};Version=3";
                return dbConnStr;
            }
        }

        public DataTable GetLatestLog(int numberOfRow)
        {
            SQLiteConnection conn = null;
            SQLiteDataAdapter adp = null;
            DataTable retDt = new DataTable();

            try
            {
                numberOfRow = numberOfRow < 0 ? 0 : (numberOfRow > 500 ? 500 : numberOfRow);

                conn = new SQLiteConnection(ConnectionString);
                conn.Open();
                adp = new SQLiteDataAdapter($@"SELECT * FROM KioskLog ORDER BY Time DESC, LogId DESC LIMIT {numberOfRow.ToString()}", conn);
                adp.Fill(retDt);
            }
            finally
            {
                if (adp != null)
                    try { adp.Dispose(); } catch { }

                if (conn != null)
                {
                    if (conn.State != System.Data.ConnectionState.Closed)
                        try { conn.Close(); } catch { }
                    try { conn.Dispose(); } catch { }
                }
            }

            return retDt;
        }

        private async void LogInfoToDb()
        {
            LogFileInfo inforMsg = null;
            while (_disposed == false)
            {
                try
                {
                    inforMsg = await GetNextLogInfo();

                    if (inforMsg != null)
                    {
                        WriteToTable(inforMsg);
                    }
                }
                catch (Exception ex)
                {
                    string byPassMsg = ex.Message;
                }
            }
        }

        /// <summary>
        ///		Log message refer to process Id.
        /// </summary>
        /// <param name="channel">Channel Name, Major Group Name OR File Name</param>
        /// <param name="processId">A spefific process Id for a working. This allow to trace a bunch of working at a specifix time</param>
        /// <param name="msg">Any message string</param>
        /// <param name="subBlockTag">A tag that used to indicate a line location code in a method/function</param>
        /// <param name="classNMethodName">Class together with method name (MyClass#FirstMethod)</param>
        /// <param name="messageType">Type of message refer to MesageType</param>
        /// <param name="currentTime"></param>
        public void LogText(string channel, string processId, string msg, string subBlockTag = null, string classNMethodName = null, MessageType messageType = MessageType.Info,
            DateTime? currentTime = null, Guid? netProcessId = null, string adminMsg = null)
        {
            currentTime = currentTime ?? DateTime.Now;
            Thread execThread = new Thread(new ThreadStart(new Action(() =>
            {
                // Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
                //        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
                if (_disposed == false)
                {
                    try
                    {
                        lock (_listLock)
                        {
                            msg = (msg ?? "").Replace("\"", "'");

                            netProcessId = (netProcessId.HasValue && (netProcessId.Value.Equals(Guid.Empty) == false)) ? netProcessId : null;

                            _logList.Enqueue(new LogFileInfo()
                            {
                                Channel = channel,
                                LoctXTag = subBlockTag,
                                ClsMetXName = classNMethodName,
                                MsgXStr = msg,
                                MsgXType = messageType,
                                Time = currentTime.Value,
                                ProXId = processId,
                                NetProcessId = netProcessId,
                                AdminMsg = adminMsg
                            });
                            Monitor.PulseAll(_listLock);
                        }
                    }
                    // Used to handle "_logList" is null after disposed
                    catch (Exception ex2)
                    {
                        string byPassStr = ex2.Message;
                    }
                }
            })));
            execThread.IsBackground = true;
            execThread.Start();
        }

        /// <summary>
        ///		Log message refer to process Id.
        /// </summary>
        /// <param name="channel">Channel Name, Major Group Name OR File Name</param>
        /// <param name="processId">A spefific process Id for a working. This allow to trace a bunch of working at a specifix time</param>
        /// <param name="msg">Any object</param>
        /// <param name="subBlockTag">A tag that used to indicate a line location code in a method/function</param>
        /// <param name="classNMethodName">Class together with method name (MyClass#FirstMethod)</param>
        /// <param name="messageType">Type of message refer to MesageType</param>
        /// <param name="currentTime"></param>
        public void LogText(string channel, string processId, object msg, string subBlockTag = null, string classNMethodName = null, MessageType messageType = MessageType.Info,
            DateTime? currentTime = null, string extraMsg = null, Guid? netProcessId = null, string adminMsg = null)
        {
            currentTime = currentTime ?? DateTime.Now;
            Thread execThread = new Thread(new ThreadStart(new Action(() =>
            {
                // Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
                //        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
                if (_disposed == false)
                {
                    try
                    {
                        lock (_listLock)
                        {
                            netProcessId = (netProcessId.HasValue && (netProcessId.Value.Equals(Guid.Empty) == false)) ? netProcessId : null;
                            _logList.Enqueue(new LogFileInfo()
                            {
                                Channel = channel,
                                LoctXTag = subBlockTag,
                                ClsMetXName = classNMethodName,
                                MsgXObj = msg,
                                MsgXType = messageType,
                                Time = currentTime.Value,
                                ProXId = processId,
                                MsgXStr = extraMsg,
                                NetProcessId = netProcessId,
                                AdminMsg = adminMsg
                            });
                            Monitor.PulseAll(_listLock);
                        }
                    }
                    // Used to handle "_logList" is null after disposed
                    catch (Exception ex2)
                    {
                        string byPassStr = ex2.Message;
                    }
                }
            })));
            execThread.IsBackground = true;
            execThread.Start();
        }

        /// <summary>
        ///		Log Error refer to process Id.
        /// </summary>
        /// <param name="channel">Channel Name, Major Group Name OR File Name</param>
        /// <param name="processId">A spefific process Id for a working. This allow to trace a bunch of working at a specifix time</param>
        /// <param name="ex">Error Exception object</param>
        /// <param name="subBlockTag">A tag that used to indicate a line location code in a method/function</param>
        /// <param name="classNMethodName">Class together with method name (MyClass#FirstMethod)</param>
        /// <param name="currentTime"></param>
        public void LogError(string channel, string processId, Exception ex, string subBlockTag = null, string classNMethodName = null,
            DateTime? currentTime = null, Guid? netProcessId = null, string adminMsg = null)
        {
            currentTime = currentTime ?? DateTime.Now;
            Thread execThread = new Thread(new ThreadStart(new Action(() =>
            {
                // Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
                //        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
                if (_disposed == false)
                {
                    try
                    {
                        currentTime = currentTime ?? DateTime.Now;
                        lock (_listLock)
                        {
                            netProcessId = (netProcessId.HasValue && (netProcessId.Value.Equals(Guid.Empty) == false)) ? netProcessId : null;
                            _logList.Enqueue(new LogFileInfo()
                            {
                                Channel = channel,
                                LoctXTag = subBlockTag,
                                ClsMetXName = classNMethodName,
                                MsgXObj = ex,
                                MsgXType = MessageType.Error,
                                Time = currentTime.Value,
                                ProXId = processId,
                                NetProcessId = netProcessId,
                                AdminMsg = adminMsg
                            });
                            Monitor.PulseAll(_listLock);
                        }
                    }
                    // Used to handle "_logList" is null after disposed
                    catch (Exception ex2)
                    {
                        string byPassStr = ex2.Message;
                    }
                }
            })));
            execThread.IsBackground = true;
            execThread.Start();
        }

        /// <summary>
        ///		Log Error refer to process Id.
        /// </summary>
        /// <param name="channel">Channel Name, Major Group Name OR File Name</param>
        /// <param name="processId">A spefific process Id for a working. This allow to trace a bunch of working at a specifix time</param>
        /// <param name="ex">Error Exception object</param>
        /// <param name="subBlockTag">A tag that used to indicate a line location code in a method/function</param>
        /// <param name="classNMethodName">Class together with method name (MyClass#FirstMethod)</param>
        /// <param name="currentTime"></param>
        public void LogFatal(string channel, string processId, Exception ex, string subBlockTag = null, string classNMethodName = null,
            DateTime? currentTime = null, Guid? netProcessId = null, string adminMsg = null)
        {
            currentTime = currentTime ?? DateTime.Now;
            Thread execThread = new Thread(new ThreadStart(new Action(() =>
            {
                // Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
                //        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
                if (_disposed == false)
                {
                    try
                    {
                        currentTime = currentTime ?? DateTime.Now;

                        lock (_listLock)
                        {
                            netProcessId = (netProcessId.HasValue && (netProcessId.Value.Equals(Guid.Empty) == false)) ? netProcessId : null;
                            _logList.Enqueue(new LogFileInfo()
                            {
                                Channel = channel,
                                LoctXTag = subBlockTag,
                                ClsMetXName = classNMethodName,
                                MsgXObj = ex,
                                MsgXType = MessageType.Fatal,
                                Time = currentTime.Value,
                                ProXId = processId,
                                NetProcessId = netProcessId,
                                AdminMsg = adminMsg
                            });
                            Monitor.PulseAll(_listLock);
                        }
                    }
                    // Used to handle "_logList" is null after disposed
                    catch (Exception ex2)
                    {
                        string byPassStr = ex2.Message;
                    }
                }
            })));
            execThread.IsBackground = true;
            execThread.Start();
        }

        public void LogData(LogFileInfo logData)
        {
            Thread execThread = new Thread(new ThreadStart(new Action(() =>
            {
                if (_disposed == false)
                {
                    try
                    {
                        lock (_listLock)
                        {
                            _logList.Enqueue(logData);
                            Monitor.PulseAll(_listLock);
                        }
                    }
                    // Used to handle "_logList" is null after disposed
                    catch (Exception ex2)
                    {
                        string byPassStr = ex2.Message;
                    }
                }
            })));
            execThread.IsBackground = true;
            execThread.Start();
        }

        private void WriteToTable(LogFileInfo msg)
        {
            SQLiteConnection conn = null;
            SQLiteCommand com = null;

            try
            {
                conn = new SQLiteConnection(ConnectionString);

                conn.Open();
                com = conn.CreateCommand();

                msg.SetDefaultIfEmpty();

                com.CommandText = $@"INSERT INTO KioskLog (TimeStr, Msg, MsgObj, AdminMsg, Channel, LoctTag, MsgType, ProcId, NetProcessId, ClsMetName, Time, GotAdminMsg) VALUES 
										(:TimeStr, :Msg, :MsgObj, :AdminMsg, :Channel, :LoctTag, :MsgType, :ProcId, :NetProcessId, :ClsMetName, :Time, :GotAdminMsg)";

                com.Parameters.Add(new SQLiteParameter() { ParameterName = "TimeStr", Value = msg.Time.ToString("yyyy-MM-dd HH:mm:ss.fff"), DbType = System.Data.DbType.String });
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "Msg", Value = msg.MsgXStr, DbType = System.Data.DbType.String });

                com.Parameters.Add(new SQLiteParameter() { ParameterName = "MsgObj", Value = msg.GetMsgXObjJSonStr(), DbType = System.Data.DbType.String });
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "AdminMsg", Value = msg.AdminMsg, DbType = System.Data.DbType.String });

                com.Parameters.Add(new SQLiteParameter() { ParameterName = "Channel", Value = msg.Channel, DbType = System.Data.DbType.String });

                com.Parameters.Add(new SQLiteParameter() { ParameterName = "LoctTag", Value = msg.LoctXTag, DbType = System.Data.DbType.String });
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "MsgType", Value = (long)msg.MsgXType, DbType = System.Data.DbType.Int64 });
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "ProcId", Value = msg.ProXId, DbType = System.Data.DbType.String });

                com.Parameters.Add(new SQLiteParameter()
                {
                    ParameterName = "NetProcessId",
                    Value = ((msg.NetProcessId.HasValue) ? msg.NetProcessId.Value.ToString("D") : null),
                    DbType = System.Data.DbType.String
                });

                com.Parameters.Add(new SQLiteParameter() { ParameterName = "ClsMetName", Value = msg.ClsMetXName, DbType = System.Data.DbType.String });

                com.Parameters.Add(new SQLiteParameter() { ParameterName = "Time", Value = msg.Time.Ticks, DbType = System.Data.DbType.Int64 });
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "GotAdminMsg", Value = ((string.IsNullOrWhiteSpace(msg.AdminMsg)) ? 0 : 1), DbType = System.Data.DbType.Int64 });

                int resultCount = com.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                string byPass = ex.Message;
            }
            finally
            {
                if (com != null)
                    try { com.Dispose(); } catch { }

                if (conn != null)
                {
                    if (conn.State != System.Data.ConnectionState.Closed)
                        try { conn.Close(); } catch { }
                    try { conn.Dispose(); } catch { }
                }
            }
        }

        private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 1);
        private async Task<LogFileInfo> GetNextLogInfo()
        {
            LogFileInfo retLogInfo = null;
            bool logFound = false;

            if (_disposed == false)
            {
                try
                {
                    await _asyncLock.WaitAsync();

                    lock (_listLock)
                    {
                        if (_logList.Count == 0)
                        {
                            Monitor.Wait(_listLock, _MaxWaitPeriod);
                        }
                        logFound = _logList.TryDequeue(out retLogInfo);
                    }
                }
                // Used to handle "_logList" is null after disposed
                catch (Exception ex) { string byPassStr = ex.Message; }
                finally
                {
                    if (_asyncLock.CurrentCount == 0)
                        _asyncLock.Release();
                }
            }

            if (logFound)
                return retLogInfo;
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

                    lock (_listLock)
                    {
                        LogFileInfo tmp = null;
                        do
                        {
                            tmp = null;
                            _logList.TryDequeue(out tmp);

                            if (tmp != null)
                                tmp.MsgXObj = null;

                        } while (tmp != null);

                        Monitor.PulseAll(_listLock);
                    }
                }
                catch { }
                finally
                {
                    //_logList = null;
                    if (_asyncLock.CurrentCount == 0)
                        _asyncLock.Release();

                    //_asyncLock.Dispose();
                    //_asyncLock = null;
                }
            }
        }
    }

}
