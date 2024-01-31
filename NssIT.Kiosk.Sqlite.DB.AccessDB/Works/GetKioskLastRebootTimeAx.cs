using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Sqlite.DB.AccessDB.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access.Echo;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Sqlite.DB.AccessDB.Works
{
    /// <summary>
	/// ClassCode:EXIT25.20
	/// </summary>
    public class GetKioskLastRebootTimeAx<Echo> : IDBAxExecution<Echo>, IDisposable
        where Echo : KioskLastRebootTimeEcho
    {
        public Guid CommandId { get; } = Guid.NewGuid();
        public Echo SuccessEcho { get; private set; } = null;
        public DBTransStatus ResultStatus { get; private set; } = new DBTransStatus();

        private DbLog Log = DbLog.GetDbLog();
        ///// xxxxxx xxxxxx xxxxxx xxxxxx xxxxxx 
        ///// Parameters (_prm)
        ///// private string _prmBTnGSalesTransactionNo = null;
        ///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        /// <summary>
        /// FuncCode:EXIT25.2002
        /// </summary>
        public GetKioskLastRebootTimeAx()
        { }

        /// <summary>
        /// FuncCode:EXIT25.2003
        /// </summary>
        public void DBExecute(DatabaseAx dbAx)
        {
            SQLiteConnection conn = null;
            SQLiteTransaction trn = null;

            try
            {
                conn = new SQLiteConnection(DBManager.DBMan.ConnectionString);
                conn.Open();
                trn = conn.BeginTransaction();

                using (KioskStatesDBTrans dbTrans = new KioskStatesDBTrans())
                {
                    // Database Validation
                    // ..
                    //-------------------------------------------------------

                    // Get Header Status
                    DateTime? lastRebootTime = dbTrans.GetLastRebootTime(conn, trn);
                    SuccessEcho = (Echo)new KioskLastRebootTimeEcho().Init(lastRebootTime);
                    ResultStatus.SetTransSuccess();
                }

                trn.Commit();
            }
            catch (Exception ex)
            {
                SuccessEcho = null;

                if (trn != null)
                {
                    try
                    {
                        trn.Rollback();
                    }
                    catch { }
                }
                ResultStatus.SetTransFail(new Exception($@"{ex?.Message}; (EXIT25.2003.EX01)", ex));
            }
            finally
            {
                trn = null;
                if (conn != null)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch { }
                    try
                    {
                        conn.Dispose();
                    }
                    catch { }
                }
            }
        }

        public void Dispose()
        {
            SuccessEcho = null;
            ResultStatus = null;
            Log = null;
        }
    }
}
