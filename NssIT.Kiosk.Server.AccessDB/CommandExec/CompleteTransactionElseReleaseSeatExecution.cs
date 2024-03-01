using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Sqlite.DB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    public class CompleteTransactionElseReleaseSeatExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;
        private CompleteTransactionElseReleaseSeatCommand _command = null;

        private PaymentType _paymentType = PaymentType.Cash;
        private string _paymentMethodCode = "C";
        private string _paymentRefNo = "";
        private CreditCardResponse _creditCardResponse = null;
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            _serverAccess = serverAccess;
            _commandPack = commPack;
           
            bool eventSent = false;
            UICompleteTransactionResult uiCompleteResult;

            try
            {
                _command = (CompleteTransactionElseReleaseSeatCommand)commPack.Command;

                bool isNetworkTimeout = false;
                bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

                _paymentType = _command.TypeOfPayment;
                _paymentMethodCode = _command.PaymentMethodCode;
                _paymentRefNo = _command.PaymentRefNo ?? "";
                _creditCardResponse = _command.CardCreditAnswer ?? null;
               transcomplete_status transCompltStatus = null;

                if (isWebServiceDetected)


                    if(_paymentMethodCode == "D" && _creditCardResponse != null)
                    {
                        transCompltStatus = CompleteTransaction(_command.TransactionNo, _command.TotalAmount,
                       _command.Cassette1NoteCount, _command.Cassette2NoteCount, _command.Cassette3NoteCount, _command.RefundCoinAmount,
                       _paymentType, _paymentMethodCode, _paymentRefNo,
                       out isNetworkTimeout,_creditCardResponse.trdt, 
                       _creditCardResponse.hsno, 
                       _creditCardResponse.mid, 
                       _creditCardResponse.rmsg, 
                       _creditCardResponse.cdno, 
                       
                       _creditCardResponse.cdnm, 
                       _creditCardResponse.cdty, 
                       _creditCardResponse.stcd, 
                       _creditCardResponse.adat, 
                       _creditCardResponse.bcno, 
                       _creditCardResponse.ttce, 
                       _creditCardResponse.rrn, 
                       _creditCardResponse.apvc, 
                       _creditCardResponse.aid, 
                       _creditCardResponse.trcy, 
                       _creditCardResponse.camt, 
                       _creditCardResponse.tid, 
                       _creditCardResponse.btct, 
                       _creditCardResponse.bcam, 
                       _creditCardResponse.mcid, 
                       _creditCardResponse.erms, 
                       _creditCardResponse.stmTrid, 
                       _creditCardResponse.stmStcd, 60);
                    }
                    else
                    {
                        transCompltStatus = CompleteTransaction(_command.TransactionNo, _command.TotalAmount,
                      _command.Cassette1NoteCount, _command.Cassette2NoteCount, _command.Cassette3NoteCount, _command.RefundCoinAmount,
                      _paymentType, _paymentMethodCode, _paymentRefNo,
                      out isNetworkTimeout, DateTime.Now,"","", "", "","","", "", "", "", "", "", "", "", "", 0,"","", 0, "", "", 0, "",  60);
                    }
                   
                else
                    transCompltStatus = new transcomplete_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (II)" };

                //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                if ((transCompltStatus != null) && ((transCompltStatus.code == 20 || transCompltStatus.code == 21)))
                {
                    if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                    {
                        isNetworkTimeout = false;

                        if(_paymentMethodCode == "D" && _creditCardResponse != null)
                        {
                            transCompltStatus = CompleteTransaction(_command.TransactionNo, _command.TotalAmount,
                      _command.Cassette1NoteCount, _command.Cassette2NoteCount, _command.Cassette3NoteCount, _command.RefundCoinAmount,
                      _paymentType, _paymentMethodCode, _paymentRefNo,
                      out isNetworkTimeout, _creditCardResponse.trdt,
                      _creditCardResponse.hsno,
                      _creditCardResponse.mid,
                      _creditCardResponse.rmsg,
                      _creditCardResponse.cdno,

                      _creditCardResponse.cdnm,
                      _creditCardResponse.cdty,
                      _creditCardResponse.stcd,
                      _creditCardResponse.adat,
                      _creditCardResponse.bcno,
                      _creditCardResponse.ttce,
                      _creditCardResponse.rrn,
                      _creditCardResponse.apvc,
                      _creditCardResponse.aid,
                      _creditCardResponse.trcy,
                      _creditCardResponse.camt,
                      _creditCardResponse.tid,
                      _creditCardResponse.btct,
                      _creditCardResponse.bcam,
                      _creditCardResponse.mcid,
                      _creditCardResponse.erms,
                      _creditCardResponse.stmTrid,
                      _creditCardResponse.stmStcd, 60);
                        }
                        else
                        {
                            transCompltStatus = CompleteTransaction(_command.TransactionNo, _command.TotalAmount,
                                _command.Cassette1NoteCount, _command.Cassette2NoteCount, _command.Cassette3NoteCount, _command.RefundCoinAmount,
                                _paymentType, _paymentMethodCode, _paymentRefNo,
                                out isNetworkTimeout, DateTime.Now, "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, "", "", 0, "", "", 0, "", 60);
                        }

                       
                    }
                    else
                    {
                        if (networkTimeout == true)
                            transCompltStatus = new transcomplete_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (III)" };
                        else
                            transCompltStatus = new transcomplete_status() { code = ServerAccess.InvalidAuthentication, msg = "Invalid authentication" };
                    }
                }

                if (transCompltStatus?.code == 0)
                {
                    uiCompleteResult = new UICompleteTransactionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, transCompltStatus, AppDecorator.Common.AppService.Sales.ProcessResult.Success);

                    commPack.UpSertResult(false, uiCompleteResult);

                    if (_command.TypeOfPayment == PaymentType.PaymentGateway)
                        UpdateLocalTransactionDB(_command.PaymentRefNo, BTnGHeaderStatus.SUCCESS, BTnGDetailStatus.w_paid_ack);

                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, "CompleteTransactionElseReleaseSeatExecution.Execute:A02", uiCompleteResult);
                }
                else
                {
                    if (_command.TypeOfPayment == PaymentType.PaymentGateway)
                        UpdateLocalTransactionDB(_command.PaymentRefNo, BTnGHeaderStatus.PENDING, BTnGDetailStatus.w_web_api_error);

                    if (transCompltStatus == null)
                        transCompltStatus = new transcomplete_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (IV)" };

                    uiCompleteResult = new UICompleteTransactionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, transCompltStatus, AppDecorator.Common.AppService.Sales.ProcessResult.Fail)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(transCompltStatus.msg) == true ? "Unknown error when update customer list (EXIT21647)" : transCompltStatus.msg) };

                    commPack.UpSertResult(true, uiCompleteResult, uiCompleteResult.ErrorMessage);

                    whenCompletedSendEvent((isNetworkTimeout == true ? ResultStatus.NetworkTimeout : ResultStatus.Fail),
                        _commandPack.NetProcessId, _commandPack.ProcessId, "CompleteTransactionElseReleaseSeatExecution.Execute:A05", uiCompleteResult, new Exception(uiCompleteResult.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiCompleteResult = new UICompleteTransactionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null, AppDecorator.Common.AppService.Sales.ProcessResult.Fail) 
                { ErrorMessage = $@"{ex.Message}; (EXIT21648)" };
                commPack.UpSertResult(true, uiCompleteResult, errorMessage: uiCompleteResult.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, "CompleteTransactionElseReleaseSeatExecution.Execute:A11", uiCompleteResult, new Exception(uiCompleteResult.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, UICompleteTransactionResult uiTransCompResult,
                Exception error = null)
            {
                if (eventSent)
                    return;

                SendMessageEventArgs compEv;

                if ((error == null) && (resultState == ResultStatus.Success))
                {
                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiTransCompResult);

                    Log.LogText(LogChannel, processId, compEv,
                        "A01", "CompleteTransactionElseReleaseSeatExecution.whenCompletedSendEvent", netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }
                else
                {
                    if (resultState == ResultStatus.Success)
                        resultState = ResultStatus.ErrorFound;

                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiTransCompResult)
                    {
                        Message = ((error?.Message) is null) ? "Unknown error found;" : error.Message
                    };

                    Log.LogText(LogChannel, processId, compEv,
                        "A02", "CompleteTransactionElseReleaseSeatExecution.whenCompletedSendEvent", AppDecorator.Log.MessageType.Error, netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }

                _serverAccess.RaiseOnSendMessage(compEv, lineTag);
                eventSent = true;

                Log.LogText(LogChannel, processId, "End - whenCompletedSendEvent", "A10", "CompleteTransactionElseReleaseSeatExecution.whenCompletedSendEvent", netProcessId: netProcessId);
            }
        }

        private transcomplete_status CompleteTransaction(string transactionNo, decimal totalAmount,
            int cassette1NoteCount, int cassette2NoteCount, int cassette3NoteCount, int refundCoinAmount,
            PaymentType paymentType, string paymentMethodCode, string paymentRefNo,
            out bool isNetworkTimeout, DateTime trdt, string hsno, string mid, string rmsg, string cdno, string cdnm, string cdty, string stcd, string adat, string bcno, string ttce, string rrn, string apvc, string aid, string trcy, decimal camt, string tid, string btct, decimal bcam, string mcid, string erms, long stmTrid, string stmStcd, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;

            transcomplete_status resultState = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                resultState = null;
                Thread threadWorker = new Thread(new ThreadStart(CompleteTransThreadWorking));
                threadWorker.IsBackground = true;

                DateTime timeOut = DateTime.Now.AddSeconds(waitSec);
                threadWorker.Start();

                while ((timeOut.Subtract(DateTime.Now).TotalMilliseconds > 0) && (resultState is null) && (threadWorker.ThreadState.IsState(ThreadState.Stopped) == false))
                {
                    Task.Delay(100).Wait();
                }

                if (resultState is null)
                {
                    try
                    {
                        threadWorker.Abort();
                        Task.Delay(500).Wait();
                    }
                    catch { }

                    if ((retryInx + 1) == maxRetryTimes)
                    {
                        isNetworkTimeout = true;
                        resultState = new transcomplete_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout; (EXIT21649)" };
                    }
                }

                if (resultState != null)
                    break;
                else
                    Task.Delay(ServerAccess.RetryIntervalSec * 1000).Wait();
            }

            return resultState;

            void CompleteTransThreadWorking()
            {
                try
                {
                    string token = Security.AccessToken;
                    string kioskId = Security.KioskID;

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        resultState = null;
                    }
                    else
                    {
                        transcomplete_status compltStatus = _serverAccess.Soap.TransComplete(token, transactionNo ?? "", totalAmount, paymentMethodCode, paymentRefNo, 
                            trdt, 
                            hsno, 
                            mid, 
                            rmsg, 
                            cdno, 
                            cdnm, 
                            cdty, 
                            stcd, 
                            adat,
                            bcno, 
                            ttce, 
                            rrn, 
                            apvc, 
                            aid,
                            trcy, 
                            camt, 
                            tid, 
                            btct, 
                            bcam, 
                            mcid, 
                            erms, 
                            stmTrid,stmStcd);

                        if ((compltStatus.code == 0) && (paymentType == PaymentType.Cash))
                        {
                            try
                            {
                                _serverAccess.Soap.UpdateKioskCash(kioskId, token, cassette1NoteCount, cassette2NoteCount, cassette3NoteCount);
                            }
                            catch (Exception ex)
                            {
                                Log.LogError(LogChannel, transactionNo ?? "-", ex, "EX01", classNMethodName: "CompleteTransactionElseReleaseSeatExecution.CompleteTransThreadWorking");
                            }

                            try
                            {
                                _serverAccess.Soap.UpdateKioskCoin(kioskId, token, refundCoinAmount);
                            }
                            catch (Exception ex)
                            {
                                Log.LogError(LogChannel, transactionNo ?? "-", ex, "EX02", classNMethodName: "CompleteTransactionElseReleaseSeatExecution.CompleteTransThreadWorking");
                            }
                        }

                        //DEBUG-Testing .. compltStatus = new transcomplete_status() { code = 99, msg = "Test Fail" };

                        //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                        if ((compltStatus.code != 0) && (compltStatus.code != 20) && (compltStatus.code != 21))
                        {
                            try
                            {
                                // Release Seat on Fail Update Customer Info
                                seatrelease_status retRState = _serverAccess.Soap.SeatRelease(kioskId, token, (transactionNo ?? ""));
                                string debugBlockStr = "";
                            }
                            catch (Exception ex)
                            {
                                Log.LogError(LogChannel, transactionNo ?? "-", ex, "EX02", classNMethodName: "CompleteTransactionElseReleaseSeatExecution.CompleteTransThreadWorking");
                            }
                        }

                        resultState = compltStatus;
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, transactionNo ?? "-", ex, "EX03", classNMethodName: "CompleteTransactionElseReleaseSeatExecution.CompleteTransThreadWorking");
                }
            }
        }

        private void UpdateLocalTransactionDB(string bTnGSaleTransNo, BTnGHeaderStatus headerStatus, BTnGDetailStatus detailStatus)
        {
            string paymentRefNo = _command.PaymentRefNo;

            RunThreadMan upLocalDbWorker = new RunThreadMan(new Action(() =>
            {
                try
                {
                    DoUpdateLocalTransactionDB(bTnGSaleTransNo, headerStatus, detailStatus);
                }
                catch (Exception ex)
                {
                    DbLog.GetDbLog().LogError(LogChannel, paymentRefNo, ex, "EX01", "CompleteTransactionElseReleaseSeatExecution.UpdateLocalTransactionDB");
                }
            }), "CompleteTransactionElseReleaseSeatExecution.UpdateLocalTransactionDB::DoUpdateLocalTransactionDB", 30, LogChannel);

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            void DoUpdateLocalTransactionDB(string bTnGSaleTransNoX, BTnGHeaderStatus headerStatusX, BTnGDetailStatus detailStatusX)
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
                        // Get Header Status
                        dbTrans.QueryHeaderStatus(bTnGSaleTransNoX, conn, trn,
                            out AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG.BTnGHeaderStatus? resHeaderStatus, out bool isFound);

                        if (isFound == false)
                            throw new Exception("BTnG sale transaction record not found; (EXIT21655)");

                        else if (resHeaderStatus.HasValue == false)
                            throw new Exception("BTnG sale transaction header's status not found; (EXIT21656)");

                        //else if ((resHeaderStatus.Value == BTnGHeaderStatus.FAIL) ||
                        //    (resHeaderStatus.Value == BTnGHeaderStatus.SUCCESS) ||
                        //    (resHeaderStatus.Value == BTnGHeaderStatus.CANCEL)
                        //    )
                        //{
                        //    /*By-pass*/
                        //}

                        else
                        {
                            DbLog.GetDbLog().LogText(LogChannel, bTnGSaleTransNoX, $@"Last HeaderStatus: {resHeaderStatus.Value}; Curr.Header Status : {headerStatusX}; Curr.Detail Status : {detailStatusX}; BTnG-{bTnGSaleTransNoX}", "BTNG_B02", "CompleteTransactionElseReleaseSeatExecution.DoUpdateLocalTransactionDB");
                            dbTrans.UpdatePaymentStatus(bTnGSaleTransNoX, headerStatusX, detailStatusX, conn, trn);
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

                    DbLog.GetDbLog().LogError(LogChannel, $@"BTnG-{bTnGSaleTransNoX}", ex, "BTNG_EX01", "CompleteTransactionElseReleaseSeatExecution.DoUpdateLocalTransactionDB");
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
        }

        public void Dispose()
        {
            _commandPack = null;
            _log = null;
            _serverAccess = null;
            _command = null;
        }

    }
}
