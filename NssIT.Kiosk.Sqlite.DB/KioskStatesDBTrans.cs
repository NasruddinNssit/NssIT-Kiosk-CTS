using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.TableEntity;

namespace NssIT.Kiosk.Sqlite.DB
{
    /// <summary>
    /// ClassCode:EXIT20.15
    /// </summary>
    public class KioskStatesDBTrans : IDisposable
    {
        public const string StateNode_LastRebootTimeCode = "LastRebootTime";

        public const string ColName_LastRebootTimeStr = "DateTimeStr";
        public const string ColName_LastRebootTimeInt = "DateTimeInt";

        private DbLog _log = null;

        public KioskStatesDBTrans()
        {
            _log = DbLog.GetDbLog();
        }

        /// <summary>
        /// FuncCode:EXIT20.1559
        /// </summary>
        public void Dispose()
        {
            _log = null;
        }

        /// <summary>
        /// FuncCode:EXIT20.1503
        /// </summary>
        public void UpSertLastRebootTime(DateTime newRebootTime, SQLiteConnection conn, SQLiteTransaction trans)
        {
            SQLiteCommand upSerComm = null;
            try
            {
                DateTime? lastRebootTime = GetLastRebootTime(conn, trans);
                
                if (lastRebootTime.HasValue)
                    upSerComm = CreateCommand_To_UpdateLastRebootTime();

                else
                    upSerComm = CreateCommand_To_InsertLastRebootTime();

                upSerComm.Parameters[ColName_LastRebootTimeStr].Value = newRebootTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                upSerComm.Parameters[ColName_LastRebootTimeInt].Value = newRebootTime.Ticks;

                if (upSerComm.ExecuteNonQuery() <= 0)
                {
                    throw new Exception($@"Fail to insert Last Reboot Time record to Kiosk States table; (XA)");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($@"{ex.Message}; (EXIT20.1503.EX01)", ex);
            }
            finally
            {
                if (upSerComm != null)
                {
                    try
                    {
                        upSerComm.Dispose();
                    }
                    catch { }

                    upSerComm = null;
                }
            }

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            SQLiteCommand CreateCommand_To_InsertLastRebootTime()
            {
                SQLiteCommand commX = conn.CreateCommand();
                commX.Transaction = trans;
                commX.CommandType = System.Data.CommandType.Text;
                commX.CommandText = $@"
INSERT INTO TKioskStates 
(StateNode, {ColName_LastRebootTimeStr}, {ColName_LastRebootTimeInt}) VALUES 
(:StateNode, :{ColName_LastRebootTimeStr}, :{ColName_LastRebootTimeInt})";
                commX.Parameters.Add(new SQLiteParameter() { ParameterName = "StateNode", DbType = System.Data.DbType.String, Value = StateNode_LastRebootTimeCode });
                commX.Parameters.Add(new SQLiteParameter() { ParameterName = ColName_LastRebootTimeStr, DbType = System.Data.DbType.String });
                commX.Parameters.Add(new SQLiteParameter() { ParameterName = ColName_LastRebootTimeInt, DbType = System.Data.DbType.Int64 });

                return commX;
            }

            SQLiteCommand CreateCommand_To_UpdateLastRebootTime()
            {
                SQLiteCommand commX = conn.CreateCommand();
                commX.Transaction = trans;
                commX.CommandType = System.Data.CommandType.Text;
                commX.CommandText = $@"
UPDATE TKioskStates 
SET {ColName_LastRebootTimeStr}=:{ColName_LastRebootTimeStr}, {ColName_LastRebootTimeInt}=:{ColName_LastRebootTimeInt}
WHERE StateNode=:StateNode";
                commX.Parameters.Add(new SQLiteParameter() { ParameterName = "StateNode", DbType = System.Data.DbType.String, Value = StateNode_LastRebootTimeCode });
                commX.Parameters.Add(new SQLiteParameter() { ParameterName = ColName_LastRebootTimeStr, DbType = System.Data.DbType.String });
                commX.Parameters.Add(new SQLiteParameter() { ParameterName = ColName_LastRebootTimeInt, DbType = System.Data.DbType.Int64 });

                return commX;
            }
        }

        /// <summary>
        /// FuncCode:EXIT20.1504
        /// </summary>
        public DateTime? GetLastRebootTime(SQLiteConnection conn, SQLiteTransaction trans)
        {
            SQLiteCommand com = null;
            DateTime? retVal = null;

            try
            {
                com = conn.CreateCommand();
                com.Transaction = trans;
                com.CommandType = System.Data.CommandType.Text;
                com.CommandText = $@"
SELECT {ColName_LastRebootTimeInt} 
FROM TKioskStates 
WHERE StateNode=:StateNode
";
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "StateNode", Value = StateNode_LastRebootTimeCode, DbType = System.Data.DbType.String });

                object result = com.ExecuteScalar();

                if (result is long lastRebootInt)
                {
                    retVal = new DateTime(lastRebootInt);
                }

                return retVal;
            }
            catch (Exception ex)
            {
                throw new Exception($@"{ex.Message}; (EXIT20.1330.EX01)", ex);
            }
            finally
            {
                if (com != null)
                {
                    try
                    {
                        com.Dispose();
                    }
                    catch { }

                    com = null;
                }
            }
        }
    }
}
