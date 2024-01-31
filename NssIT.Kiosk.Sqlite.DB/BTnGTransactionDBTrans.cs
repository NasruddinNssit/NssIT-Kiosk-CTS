using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
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
    /// ClassCode:EXIT20.13
    /// </summary>
    public class BTnGTransactionDBTrans : IDisposable
    {
        private const string _logChannel = "Database-DBTrans";

        private DbLog _log = null;

        public BTnGTransactionDBTrans()
        {
            _log = DbLog.GetDbLog();
        }

        /// <summary>
        /// FuncCode:EXIT20.1359
        /// </summary>
        public void Dispose()
        {
            _log = null;
        }

        /// <summary>
        /// FuncCode:EXIT20.1302
        /// </summary>
        /// <param name="salesTransactionNo"></param>
        /// <param name="paymentGateway"></param>
        /// <param name="bookingNo"></param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        public void NewPayment(string salesTransactionNo, string paymentGateway, string bookingNo, string currency, decimal amount,
            SQLiteConnection conn, SQLiteTransaction trans)
        {
            
            SQLiteCommand insCom = null;
            DateTime currentTime = DateTime.Now;

            try
            {
                insCom = CreateCommand_To_InsertCreateNewTransaction();
                AddPaymentTransaction(insCom, salesTransactionNo, paymentGateway, bookingNo, currency, amount);
            }
            catch (Exception ex)
            {
                throw new Exception($@"{ex.Message}; (EXIT20.1302.EX01)", ex);
            }
            finally
            {
                if (insCom != null)
                {
                    try
                    {
                        insCom.Dispose();
                    }
                    catch { }

                    insCom = null;
                }
            }

            /// <summary>
            /// FuncCode:EXIT20.1321
            /// </summary>
            void AddPaymentTransaction(SQLiteCommand comX, string salesTransactionNoX, string paymentGatewayX, string bookingNoX, string currencyX, decimal amountX)
            {
                try
                {
                    comX.Parameters["SalesTransactionNo"].Value = salesTransactionNoX.Trim();
                    comX.Parameters["PaymentGateway"].Value = paymentGatewayX;
                    comX.Parameters["MerchantTransactionNo"].Value = bookingNoX;
                    comX.Parameters["Currency"].Value = currencyX;
                    comX.Parameters["Amount"].Value = amountX;
                    comX.Parameters["LastHeaderStatus"].Value = BTnGHeaderStatus.NEW.ToString();
                    comX.Parameters["LastDetailStatus"].Value = BTnGDetailStatus.@new.ToString();

                    if (comX.ExecuteNonQuery() <= 0)
                    {
                        throw new Exception($@"Fail to insert BTnG Transaction record; SalesTransactionNo : {salesTransactionNoX}; (EXIT20.1321.EX01)");
                    }
                }
                catch (Exception ex)
                {
                    _log?.LogError(_logChannel, "*", new Exception($@"{ex.Message}; (EXIT20.1321.EX02)", ex), "EX02", "BTnGTransactionDBTrans.AddPaymentTransaction",
                        adminMsg: $@"Fail to add TBTnGTransaction; SalesTransactionNo : {salesTransactionNoX}");
                }
            }

            /// <summary>
            /// FuncCode:EXIT20.1322
            /// </summary>
            SQLiteCommand CreateCommand_To_InsertCreateNewTransaction()
            {
                SQLiteCommand comm = conn.CreateCommand();
                comm.Transaction = trans;
                comm.CommandType = System.Data.CommandType.Text;
                comm.CommandText = $@"
INSERT INTO TBTnGTransaction 
(SalesTransactionNo, PaymentGateway, MerchantTransactionNo, 
Currency, Amount, 
LastHeaderStatus, LastDetailStatus, 
CreatedDate, LastModifiedDate, CreatedDateTicks, LastModifiedDateTicks) VALUES 
(:SalesTransactionNo, :PaymentGateway, :MerchantTransactionNo, 
:Currency, :Amount, 
:LastHeaderStatus, :LastDetailStatus, 
:CreatedDate, :LastModifiedDate, :CreatedDateTicks, :LastModifiedDateTicks)";

                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "SalesTransactionNo", DbType = System.Data.DbType.String });
                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "PaymentGateway", DbType = System.Data.DbType.String });
                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "MerchantTransactionNo", DbType = System.Data.DbType.String });
                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "Currency", DbType = System.Data.DbType.String });
                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "Amount", DbType = System.Data.DbType.Decimal });
                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "LastHeaderStatus", DbType = System.Data.DbType.String });
                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "LastDetailStatus", DbType = System.Data.DbType.String });

                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "CreatedDate", Value = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), DbType = System.Data.DbType.String });
                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "LastModifiedDate", Value = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), DbType = System.Data.DbType.String });
                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "CreatedDateTicks", Value = currentTime.Ticks, DbType = System.Data.DbType.Int64 });
                comm.Parameters.Add(new SQLiteParameter() { ParameterName = "LastModifiedDateTicks", Value = currentTime.Ticks, DbType = System.Data.DbType.Int64 });
                return comm;
            }
        }

        /// <summary>
        /// Return true if found.; FuncCode:EXIT20.1323
        /// </summary>
        /// <param name="salesTransactionNo"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="bTnGTransaction"></param>
        /// <returns></returns>
        public bool QueryTnGTransaction(string salesTransactionNo, SQLiteConnection conn, SQLiteTransaction trans, out BTnGTransactionEnt bTnGTransaction)
        {
            bTnGTransaction = null;

            SQLiteCommand com = null;
            try
            {
                com = conn.CreateCommand();
                com.Transaction = trans;
                com.CommandType = System.Data.CommandType.Text;
                com.CommandText = $@"SELECT SalesTransactionNo, PaymentGateway, MerchantTransactionNo, Currency, Amount, 
LastHeaderStatus, LastDetailStatus, CreatedDate, LastModifiedDate, CreatedDateTicks, 
LastModifiedDateTicks FROM TBTnGTransaction WHERE SalesTransactionNo=:SalesTransactionNo";
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "SalesTransactionNo", Value = salesTransactionNo.Trim(), DbType = System.Data.DbType.String });

                SQLiteDataReader rowRead = com.ExecuteReader();
                if (rowRead.Read())
                {
                    bTnGTransaction = new BTnGTransactionEnt()
                    {
                        SalesTransactionNo = salesTransactionNo,
                        PaymentGateway = rowRead["PaymentGateway"]?.ToString(),
                        MerchantTransactionNo = rowRead["MerchantTransactionNo"]?.ToString(),
                        Currency = rowRead["Currency"]?.ToString(),
                        Amount = (rowRead["Amount"] != null) ? (decimal)rowRead["Amount"] : 0,
                        LastHeaderStatus = rowRead["LastHeaderStatus"].ToString().ToBTnGHeaderStatus(),
                        LastDetailStatus = rowRead["LastDetailStatus"].ToString().ToBTnGDetailStatus(),
                        CreatedDate = rowRead["CreatedDate"].ToString(), 
                        LastModifiedDate = rowRead["LastModifiedDate"].ToString(), 
                        CreatedDateTicks = (long)rowRead["CreatedDateTicks"], 
                        LastModifiedDateTicks = (long)rowRead["LastModifiedDateTicks"], 
                    };
                }
                rowRead.Close();

                return (bTnGTransaction != null);
            }
            catch (Exception ex)
            {
                bTnGTransaction = null;
                throw new Exception($@"{ex.Message}; (EXIT20.1323.EX01)", ex);
            }
            finally
            {
                if (com != null)
                {
                    //com.Transaction = null;
                    try
                    {
                        com.Dispose();
                    }
                    catch { }

                    com = null;
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT20.1315
        /// </summary>
        public void UpdatePaymentStatus(string salesTransactionNo, BTnGHeaderStatus headerStatus, BTnGDetailStatus detailStatus, SQLiteConnection conn, SQLiteTransaction trans)
        {
            DbLog log = DbLog.GetDbLog();
            SQLiteCommand com = null;
            try
            {
                DateTime now = DateTime.Now;
                com = conn.CreateCommand();
                com.Transaction = trans;

                //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Updating 
                com.CommandType = System.Data.CommandType.Text;
                com.CommandText = $@"UPDATE TBTnGTransaction SET LastHeaderStatus=:LastHeaderStatus, LastDetailStatus=:LastDetailStatus, LastModifiedDate =:LastModifiedDate, LastModifiedDateTicks=:LastModifiedDateTicks WHERE SalesTransactionNo=:SalesTransactionNo";

                com.Parameters.Add(new SQLiteParameter() { ParameterName = "LastHeaderStatus", Value = headerStatus.ToString(), DbType = System.Data.DbType.String });
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "LastDetailStatus", Value = detailStatus.ToString(), DbType = System.Data.DbType.String });
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "LastModifiedDate", Value = now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), DbType = System.Data.DbType.String });
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "LastModifiedDateTicks", Value = now.Ticks, DbType = System.Data.DbType.Int64 });
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "SalesTransactionNo", Value = salesTransactionNo, DbType = System.Data.DbType.String });

                if (com.ExecuteNonQuery() <= 0)
                {
                    log.LogError(_logChannel, "*", new Exception($@"No record updated when update status to BTnGTransaction Table; SalesTransactionNo.: {salesTransactionNo}; (EXIT20.1315.X02)"));
                    //throw new Exception($@"Unable to update check-in field to Booking Table; Payment No.: {paymentNo}; (EXIT20.1315.X02)");
                }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                throw new Exception($@"{ex.Message}; (EXIT20.1315.EX01)", ex);
            }
            finally
            {
                if (com != null)
                {
                    //com.Transaction = null;
                    try
                    {
                        com.Dispose();
                    }
                    catch { }

                    com = null;
                }

                log = null;
            }
            return;

            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        }

        /// <summary>
        /// FuncCode:EXIT20.1330
        /// </summary>
        /// <param name="salesTransactionNo"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="headerStatus"></param>
        /// <param name="isFound"></param>
        public void QueryHeaderStatus(string salesTransactionNo, SQLiteConnection conn, SQLiteTransaction trans, 
            out BTnGHeaderStatus? headerStatus, out bool isFound)
        {
            SQLiteCommand com = null;
            headerStatus = null;
            isFound = false;

            try
            {
                com = conn.CreateCommand();
                com.Transaction = trans;
                com.CommandType = System.Data.CommandType.Text;
                com.CommandText = $@"
SELECT LastHeaderStatus 
FROM TBTnGTransaction 
WHERE SalesTransactionNo = :SalesTransactionNo
";
                com.Parameters.Add(new SQLiteParameter() { ParameterName = "SalesTransactionNo", Value = salesTransactionNo, DbType = System.Data.DbType.String });

                object result = com.ExecuteScalar();

                if (result is string statusStr)
                {
                    isFound = true;
                    headerStatus = statusStr.ToBTnGHeaderStatus();
                }
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
