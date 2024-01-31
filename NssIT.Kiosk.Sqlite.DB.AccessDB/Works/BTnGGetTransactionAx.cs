using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.TableEntity;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Sqlite.DB.AccessDB.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access.Echo;
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
	/// ClassCode:EXIT25.17
	/// </summary>
    public class BTnGGetTransactionAx<Echo> : IDBAxExecution<Echo>, IDisposable
        where Echo : BTnGGetTransactionEcho
    {
        public Guid CommandId { get; } = Guid.NewGuid();
        public Echo SuccessEcho { get; private set; } = null;
        public DBTransStatus ResultStatus { get; private set; } = new DBTransStatus();

        private DbLog Log = DbLog.GetDbLog();
        ///// xxxxxx xxxxxx xxxxxx xxxxxx xxxxxx 
        // Parameters (_prm)
        private string _prmBTnGSalesTransactionNo = null;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        /// <summary>
        /// FuncCode:EXIT25.1702
        /// </summary>
        public BTnGGetTransactionAx(string bTnGSalesTransactionNo)
        {
            _prmBTnGSalesTransactionNo = bTnGSalesTransactionNo;
        }

        /// <summary>
        /// FuncCode:EXIT25.1703
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

                using (BTnGTransactionDBTrans dbTrans = new BTnGTransactionDBTrans())
                {
                    // Database Validation
                    // ..
                    //-------------------------------------------------------

                    // Get Header Status
                    if (dbTrans.QueryTnGTransaction(_prmBTnGSalesTransactionNo, conn, trn, out BTnGTransactionEnt saleTransaction) == true)
                    {
                        SuccessEcho = (Echo)new BTnGGetTransactionEcho(saleTransaction, isRecordFound: true);
                        ResultStatus.SetTransSuccess();
                    }
                    else
                    {
                        SuccessEcho = (Echo)new BTnGGetTransactionEcho(saleTransaction, isRecordFound: false);
                        ResultStatus.SetTransSuccess();
                    }
                }

                trn.Commit();
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
                ResultStatus.SetTransFail(new Exception($@"{ex?.Message}; (EXIT25.1703.EX01)", ex));
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