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
    public class BTnGNewPaymentAx<Echo> : IDBAxExecution<Echo>, IDisposable
        where Echo : SuccessXEcho
    {
        public Guid CommandId { get; } = Guid.NewGuid();
        public Echo SuccessEcho { get; private set; } = null;
        public DBTransStatus ResultStatus { get; private set; } = new DBTransStatus();

        private DbLog Log = DbLog.GetDbLog();
        ///// xxxxxx xxxxxx xxxxxx xxxxxx xxxxxx 
        // Parameters (_prm)
        private string _prmKtmbSalesTransactionNo = null;
        private string _prmPaymentGateway = null;
        private string _prmBooking = null;
        private string _prmCurrency = "MYR";
        private decimal _prmAmount = 0M;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        public BTnGNewPaymentAx(string ktmbSalesTransactionNo, string paymentGateway, string booking, string currency, decimal amount)
        {
            _prmKtmbSalesTransactionNo = ktmbSalesTransactionNo;
            _prmPaymentGateway = paymentGateway;
            _prmBooking = booking;
            _prmCurrency = currency;
            _prmAmount = amount;
        }

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
                    dbTrans.NewPayment(_prmKtmbSalesTransactionNo, _prmPaymentGateway, _prmBooking, _prmCurrency, _prmAmount, conn, trn);
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
                ResultStatus.SetTransFail(new Exception($@"{ex?.Message}; (EXIT25.123002.EX01)", ex));
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
