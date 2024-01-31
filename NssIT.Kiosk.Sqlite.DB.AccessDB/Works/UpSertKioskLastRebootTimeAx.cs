using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Sqlite.DB.AccessDB.Base;
using NssIT.Kiosk.AppDecorator.Common.Access;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Sqlite.DB.AccessDB.Works
{
    /// <summary>
	/// ClassCode:EXIT25.19
	/// </summary>
    public class UpSertKioskLastRebootTimeAx<Echo> : IDBAxExecution<Echo>, IDisposable
        where Echo : SuccessXEcho
    {
        public Guid CommandId { get; } = Guid.NewGuid();
        public Echo SuccessEcho { get; private set; } = null;
        public DBTransStatus ResultStatus { get; private set; } = new DBTransStatus();

        private DbLog Log = DbLog.GetDbLog();
        ///// xxxxxx xxxxxx xxxxxx xxxxxx xxxxxx 
        ///// Parameters (_prm)
        private DateTime _prmLastRebootTime = DateTime.Now;
        ///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        
        public UpSertKioskLastRebootTimeAx(DateTime lastRebootTime)
        {
            _prmLastRebootTime = lastRebootTime;
        }

        /// <summary>
        /// FuncCode:EXIT25.1902
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
                    dbTrans.UpSertLastRebootTime(_prmLastRebootTime, conn, trn);
                }

                trn.Commit();

                SuccessEcho = (Echo)new SuccessXEcho();
                ResultStatus.SetTransSuccess();
            }
            catch (Exception ex)
            {
                if (trn != null)
                {
                    try
                    {
                        trn.Rollback();
                    }
                    catch { }
                }
                ResultStatus.SetTransFail(new Exception($@"{ex?.Message}; (EXIT25.1902.EX01)", ex));
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
