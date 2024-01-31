using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
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
    /// ClassCode:EXIT25.12
    /// </summary>
    public class BTnGCancelRefundRequestAx<Echo> : IDBAxExecution<Echo>, IDisposable
        where Echo : SuccessXEcho
    {
        private const string LogChannel = "SQLite_DB";

        public Guid CommandId { get; } = Guid.NewGuid();
        public Echo SuccessEcho { get; private set; } = null;
        public DBTransStatus ResultStatus { get; private set; } = new DBTransStatus();

        private DbLog Log = DbLog.GetDbLog();
        ///// xxxxxx xxxxxx xxxxxx xxxxxx xxxxxx 
        // Parameters (_prm)
        private string _prmBTnGSalesTransactionNo = null;
        private BTnGHeaderStatus _prmHeaderStatus = BTnGHeaderStatus.FAIL;
        private BTnGDetailStatus _prmDetailStatus = BTnGDetailStatus.w_cancel_refund_request;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        /// <summary>
        /// FuncCode:EXIT25.1202
        /// </summary>
        public BTnGCancelRefundRequestAx(string bTngSalesTransactionNo, BTnGHeaderStatus headerStatus, BTnGDetailStatus detailStatus)
        {
            _prmBTnGSalesTransactionNo = bTngSalesTransactionNo;
            _prmHeaderStatus = headerStatus;
            _prmDetailStatus = detailStatus;
        }

        /// <summary>
        /// FuncCode:EXIT25.1203
        /// </summary>
        public void DBExecute(DatabaseAx dbAx)
        {
            SQLiteConnection conn = null;
            SQLiteTransaction trn = null;

            string paramStr = $@"SalesTransactionNo: {_prmBTnGSalesTransactionNo}; HeaderStatus:{_prmHeaderStatus}; DetailStatus: {_prmDetailStatus}";

            try
            {
                Log?.LogText(LogChannel, _prmBTnGSalesTransactionNo, $@"Parametes => {paramStr}", "A01", "BTnGCancelRefundRequestAx.DBExecute");

                conn = new SQLiteConnection(DBManager.DBMan.ConnectionString);
                conn.Open();
                trn = conn.BeginTransaction();

                using (BTnGTransactionDBTrans dbTrans = new BTnGTransactionDBTrans())
                {
                    // Database Validation
                    // ..
                    //-------------------------------------------------------

                    // Get Header Status
                    dbTrans.QueryHeaderStatus(_prmBTnGSalesTransactionNo, conn, trn, 
                        out AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG.BTnGHeaderStatus? resHeaderStatus, out bool isFound);

                    if (isFound == false)
                        throw new Exception("BTnG sale transaction record not found; (EXIT25.1203.X01)");

                    else if (resHeaderStatus.HasValue == false)
                        throw new Exception("BTnG sale transaction header's status not found; (EXIT25.1203.X02)");

                    else if ((resHeaderStatus.Value == BTnGHeaderStatus.FAIL) ||
                        (resHeaderStatus.Value == BTnGHeaderStatus.SUCCESS) ||
                        (resHeaderStatus.Value == BTnGHeaderStatus.CANCEL)

                        )
                    {
                        /*By-pass*/
                    }

                    else if ((resHeaderStatus.Value == BTnGHeaderStatus.NEW) ||
                            (resHeaderStatus.Value == BTnGHeaderStatus.CANCEL_REQUEST) ||
                            (resHeaderStatus.Value == BTnGHeaderStatus.PENDING)
                            )
                    {
                        dbTrans.UpdatePaymentStatus(_prmBTnGSalesTransactionNo, _prmHeaderStatus, _prmDetailStatus, conn, trn);
                    }
                    else
                        throw new Exception($@"Invalid BTnG request. BTnG sale transaction header's status is {resHeaderStatus.Value.ToString()}; (EXIT25.1203.X03)");
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
                ResultStatus.SetTransFail(new Exception($@"{ex?.Message}; (EXIT25.1203.EX01)", ex));
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
