using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Network.PaymentGatewayApp.Base;
using NssIT.Kiosk.Network.SignalRClient.API.Base.Extension;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data;
using NssIT.Kiosk.Sqlite.DB.AccessDB;
using NssIT.Kiosk.Sqlite.DB.AccessDB.Works;
using NssIT.Kiosk.AppDecorator.Common.Access;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk.Common.Tools.Timer;
//using WpfBoostTouchNGoAPITest.Base;
//using WpfBoostTouchNGoAPITest.Base.Extension;
//using WpfBoostTouchNGoAPITest.Data;
//using WpfBoostTouchNGoAPITest.Data.Response;

namespace NssIT.Kiosk.Network.PaymentGatewayApp.JobApp.BTnGJob
{
    /// <summary>
    /// ClassCode:EXIT60.05
    /// </summary>
    public partial class BTnGJobSale : IDisposable
    {
        public event EventHandler<BTnGEventArg<IUIxBTnGPaymentGroupAck>> OnInProgress;
        public event EventHandler<BTnGEventArg<UIxBTnGPaymentEndAck>> OnCompleted;

        private const string LogChannel = "BTnG_ServerApplication";
        private LibShowMessageWindow.MessageWindow _msgDebug = null;

        private int _maxCountDown_StartInitSec = 60;
        //private int _maxCountDown_ScanningSec = 75;
        private int _maxCountDown_ScanningSec = (10 * 60);

        private object _stateDecisionLock = new object();
        //private string _eWalletWebApiBaseURL = "";
        private string _webApiURL = "";
        private string _deviceId = "*";

        
        private BTnGJobMan _bTnGJobMan = null;
        private LocalSaleInfo _localSaleInfo = null;
        private TransactionState _transactionState = null;
        private CountDownTimer _countDown = null;
        private BTnGPaymentSnRClient _payClient = null;

        // Count Down Codes
        private string _countDownCode_InitToStart = "BTnG_CountDown_InitToStart";
        //Not Available//private string _countDownCode_FailToStart = "BTnG_CountDown_FailToStart";
        private string _countDownCode_WaitForScanning = "BTnG_CountDown_WaitForScanning";
        //Not Available//private string _countDownCode_WaitForSuccessEnd = "BTnG_CountDown_WaitForSuccessEnd";
        //Not Available//private string _countDownCode_WaitForCancelEnd = "BTnG_CountDown_WaitForCancelEnd";
        //Not Available//private string _countDownCode_WaitForFailTransactionEnd = "BTnG_CountDown_WaitForFailTransactionEnd"; /*Payment Transaction finished with error found (UI or Internal error)*/

        private DbLog _log = null;
        private DbLog Log => _log;

        /// <summary>
        /// FuncCode:EXIT60.0501
        /// </summary>
        public BTnGJobSale(LibShowMessageWindow.MessageWindow messageWindows)
        {
            _log = DbLog.GetDbLog();

            _deviceId = RegistrySetup.GetRegistrySetting().DeviceId;

            _msgDebug = messageWindows;
            //_eWalletWebApiBaseURL = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().EWalletWebApiBaseURL;
            _webApiURL = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().WebApiURL;
            _maxCountDown_ScanningSec = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().BTnGMinimumWaitingPeriod;
            _bTnGJobMan = BTnGJobMan.GetJobMan();
            _countDown = new CountDownTimer();
            _countDown.OnCountDown += _countDown_OnCountDown;
            _countDown.OnExpired += _countDown_OnExpired;
        }

        /// <summary>
        /// FuncCode:EXIT60.0502
        /// </summary>
        private void RaiseOnInProgress(IUIxBTnGPaymentGroupAck dataObj)
        {
            try
            {
                OnInProgress.Invoke(null, new BTnGEventArg<IUIxBTnGPaymentGroupAck>(dataObj));
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                _log?.LogError(LogChannel, (_localSaleInfo?.DocNo)??"*", new WithDataException(ex.Message, ex, dataObj), "EX01", "BTnGJobSale.RaiseOnInProgress");
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0503
        /// </summary>
        private void RaiseOnCompleted(UIxBTnGPaymentEndAck completedDataObj)
        {
            try
            {
                _countDown.ResetCounter();
                OnCompleted.Invoke(null, new BTnGEventArg<UIxBTnGPaymentEndAck>(completedDataObj));
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                _log?.LogError(LogChannel, (_localSaleInfo?.DocNo) ?? "*", new WithDataException(ex.Message, ex, completedDataObj), "EX01", "BTnGJobSale.RaiseOnCompleted");
            }
        }

        private void ShowDebugMessage(string message)
        {
            if (_msgDebug != null)
            {
                _msgDebug.ShowMessage(message);
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0504
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _countDown_OnExpired(object sender, ExpiredEventArgs e)
        {
            if ((_localSaleInfo is null) || (string.IsNullOrWhiteSpace(_localSaleInfo.BTnGSalesTransactionNo)))
                return;

            RunThreadMan treadMan = new RunThreadMan(new Action(() =>
            {
                // InitToStart has expired
                if (e.CountDownCode.Equals(_countDownCode_InitToStart) && (_transactionState.SetInitPaymentState(isInitPaymentSuccess: false) == true))
                {
                    Exception exx = new Exception("Fail initiating 'Boost/Tooch n Go' transaction; (EXIT60.0504.X02)");
                    RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, 
                        PaymentResult.Fail, errorMsg: exx.Message));
                    CancelSale(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, 
                        _localSaleInfo.BTnGSalesTransactionNo, _localSaleInfo.Currency, _localSaleInfo.PaymentGateway, _localSaleInfo.Amount, PaymentResult.Fail);

                    EndSale();
                }

                // .. timeout waiting for BTnG payment response
                else if (e.CountDownCode.Equals(_countDownCode_WaitForScanning))
                {
                    Exception exx = new Exception("Payment Timeout for barcode scanning; (EXIT60.0504.X05)");
                    if (_transactionState.SetFail(exx))
                    {
                        RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, 
                            PaymentResult.Timeout, errorMsg: exx.Message));
                        CancelSale(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo,
                        _localSaleInfo.BTnGSalesTransactionNo, _localSaleInfo.Currency, _localSaleInfo.PaymentGateway, _localSaleInfo.Amount, PaymentResult.Timeout);
                    }

                    EndSale();
                }
            }),
            "BTnGPaymentGatewayJob._countDown_OnExpired",
            65, LogChannel, ThreadPriority.AboveNormal, isLogReq: true);
        }

        private RunThreadMan coundDownThreadMan = null;
        private void _countDown_OnCountDown(object sender, CountDownEventArgs e)
        {
            if ((coundDownThreadMan is null) || (coundDownThreadMan.IsEnd))
            {
                coundDownThreadMan = new RunThreadMan(new Action(() =>
                {
                    RaiseOnInProgress(new UIxBTnGPaymentCountDownAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, e.TimeRemainderSec));
                }),
                "BTnGPaymentGatewayJob._countDown_OnCountDown",
                20, LogChannel, ThreadPriority.AboveNormal);
            }
        }


        private RunThreadMan _startNewSaleThreadMan = null;
        /// <summary>
        /// FuncCode:EXIT60.0506
        /// </summary>
        public void StartNewSale(Guid? netProcessId, string docNo, decimal amount, string paymentGateway, string currency,
            string customerFirstName, string lastFirstName, string contactNo, string financePaymentMethod)
        {
            _startNewSaleThreadMan?.AbortRequest(out _, 500);

            try
            {
                _payClient?.Dispose();
            }
            catch { }

            _startNewSaleThreadMan = new RunThreadMan(new ThreadStart(DoStartNewSale), "BTnGJobSale.StartNewSale", 40, LogChannel, isLogReq: true);
            return;
            //-----------------------------------------------------------------------------------------------------------------------------------
            /// <summary>
            /// FuncCode:EXIT60.058B
            /// </summary>
            void DoStartNewSale()
            {
                ShowDebugMessage("");
                try
                {
                    _deviceId = RegistrySetup.GetRegistrySetting().DeviceId;

                    _localSaleInfo = new LocalSaleInfo()
                    {
                        NetProcessId = netProcessId,
                        Amount = amount,
                        Currency = currency,
                        DocNo = docNo,
                        PaymentGateway = paymentGateway,
                        FirstName = customerFirstName,
                        LastName = lastFirstName,
                        ContactNo = contactNo
                    };

                    _transactionState = new TransactionState();
                    
                    _payClient = new BTnGPaymentSnRClient(_webApiURL, _deviceId, Setting.GetSetting().KioskId,
                        _localSaleInfo.DocNo, _localSaleInfo.Amount, _localSaleInfo.Currency, _localSaleInfo.PaymentGateway,
                        _localSaleInfo.FirstName, _localSaleInfo.LastName, _localSaleInfo.ContactNo, financePaymentMethod);
                    _payClient.OnPaymentRequestResult += PayClient_OnPaymentRequestResult;
                    _payClient.OnPaymentCompleted += _payClient_OnPaymentCompleted;
                    _payClient.OnPaymentEchoMessageReceived += _payClient_OnPaymentEchoMessageReceived;

                    ShowDebugMessage("Payment Start ..");
                    RaiseOnInProgress(new UIxBTnGPaymentCustomerMsgAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, "InitNewTrans; Initializing new transaction .."));

                    _payClient.StartPayment();
                    _countDown.ChangeCountDown(_countDownCode_InitToStart, _maxCountDown_StartInitSec, 500);
                }
                catch (Exception ex)
                {
                    _log?.LogError(LogChannel, (_localSaleInfo?.DocNo) ?? "*", ex, "EX01", "BTnGJobSale.DoStartNewSale");

                    ShowDebugMessage(ex.ToString());                    
                    string errMsg = $@"Error when initiate new payment gateway transaction; (EXIT60.058B.EX01); {ex.Message} ";
                    if (_transactionState.SetFail(new Exception(errMsg)))
                    {
                        RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo,
                            PaymentResult.Fail, errorMsg: $@"{errMsg}"));
                    }
                    ShowDebugMessage("");
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0507
        /// </summary>
        /// <param name="netProcessId"></param>
        /// <param name="processId"></param>
        public void ReadServerTime(Guid? netProcessId, string processId)
        {
            RunThreadMan tMan = new RunThreadMan(new ThreadStart(DoReadServerTime), "BTnGJobSale.ReadServerTime", 40, LogChannel);
            return;
            //-----------------------------------------------------------------------------------------------------------------------------------
            void DoReadServerTime()
            {
                ShowDebugMessage("");
                try
                {
                    ShowDebugMessage("Start Read Server Time from local server ..");
                    
                    string timeStr = _payClient.GetServerTime().GetAwaiter().GetResult();

                    RaiseOnInProgress(new UIxBTnGPaymentInProgressMsgAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, $@"ReadServerTime; Reserver Time : {timeStr}"));
                }
                catch (Exception ex)
                {
                    _log?.LogError(LogChannel, (_localSaleInfo?.DocNo) ?? "*", ex, "EX01", "BTnGJobSale.DoReadServerTime");

                    ShowDebugMessage(ex.ToString());
                    string errMsg = $@"Error when Read Server Time; (EXIT60.0507.EX01); {ex.Message} ";
                    if (_transactionState.SetFail(new Exception(errMsg)))
                    {
                        RaiseOnInProgress(new UIxBTnGPaymentInProgressMsgAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, $@"{errMsg}"));
                    }
                    ShowDebugMessage("");
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0508
        /// </summary>
        /// <param name="netProcessId"></param>
        /// <param name="echoMessage"></param>
        public void SendEchoMessage(Guid? netProcessId, string echoMessage)
        {
            RunThreadMan tMan = new RunThreadMan(new ThreadStart(DoSendEchoMessage), "BTnGJobSale.SendEchoMessage", 40, LogChannel);
            return;
            //-----------------------------------------------------------------------------------------------------------------------------------
            /// <summary>
            /// FuncCode:EXIT60.058A
            /// </summary>
            void DoSendEchoMessage()
            {
                ShowDebugMessage("");
                try
                {
                    ShowDebugMessage("Start Send Echo Message from local server ..");

                    string msg = string.IsNullOrWhiteSpace(echoMessage) ? $@"***** Echo .. {DateTime.Now.Ticks} *****" : echoMessage;

                    _payClient.SendEcho(echoMessage).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _log?.LogError(LogChannel, (_localSaleInfo?.DocNo) ?? "*", ex, "EX01", "BTnGJobSale.DoSendEchoMessage");

                    ShowDebugMessage(ex.ToString());
                    string errMsg = $@"Error when Send Echo Message; (EXIT60.058A.X01); {ex.Message} ";
                    if (_transactionState.SetFail(new Exception(errMsg)))
                    {
                        RaiseOnInProgress(new UIxBTnGPaymentInProgressMsgAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, $@"{errMsg}"));
                    }
                    ShowDebugMessage("");
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0509
        /// </summary>
        private void _payClient_OnPaymentEchoMessageReceived(object sender, NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Event.PaymentEchoMessageEventArgs e)
        {
            ShowDebugMessage("");
            try
            {
                ShowDebugMessage(e.EchoMessage);

                RaiseOnInProgress(new UIxBTnGPaymentInProgressMsgAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, $@"SignalREchoMessage; {e.EchoMessage}"));
            }
            catch (Exception ex)
            {
                ShowDebugMessage($@"Unknown error when showing echo message; {ex.Message}");

                RaiseOnInProgress(new UIxBTnGPaymentInProgressMsgAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, $@"SignalREchoMessage;Error: {ex.Message}"));
            }
            finally
            {
                ShowDebugMessage("");
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0510
        /// </summary>
        private void _payClient_OnPaymentCompleted(object sender, NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Event.PaymentCompletedResultEventArgs e)
        {
            ShowDebugMessage("");
            try
            {
                _log?.LogText(LogChannel, (_localSaleInfo?.DocNo) ?? "*", e, "A01", "BTnGJobSale._payClient_OnPaymentCompleted");

                ShowDebugMessage(JsonConvert.SerializeObject(e, Formatting.Indented));

                if (e.Result == PaymentResult.Success)
                {
                    if (_transactionState.SetPaid())
                    {
                        ShowDebugMessage("Payment Success");
                        RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, 
                            PaymentResult.Success, "PaymentSuccess;"));
                    }
                }

                else if (e.Result == PaymentResult.Timeout)
                {
                    // May not applicable

                    if (_transactionState.SetFail(new Exception("Transaction has timeout")))
                    {
                        ShowDebugMessage("Transaction has timeout");
                        RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, 
                            PaymentResult.Timeout, errorMsg: "PaymentTimeout; No response found for 2D barcode scanning"));
                    }
                }

                else if (e.Result == PaymentResult.Cancel)
                {
                    // May not applicable

                    if (_transactionState.SetCancel())
                    {
                        ShowDebugMessage("Transaction has been canceled");
                        //RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, 
                        //    PaymentResult.Cancel, errorMsg: "Transaction has been canceled"));

                        // Only customer allow to raise Cancel
                        RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo,
                            PaymentResult.Fail, errorMsg: "Transaction has been canceled (XX)"));
                    }
                }
                else if (e.Result == PaymentResult.Fail)
                {
                    if (string.IsNullOrWhiteSpace(e.Error?.Message))
                        throw new Exception("Fail payment; Unknown error; (EXIT60.0510.X01)");

                    else
                        throw new Exception(e.Error.Message);
                }
                else
                    throw new Exception($@"Unknown payment result. Code: {e.Result}; (EXIT60.0510.X02)");
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message ?? "";

                if (errMsg.Contains("(EXIT") == false)
                {
                    errMsg += "; (EXIT60.0510.EX01)";
                }

                _log?.LogError(LogChannel, (_localSaleInfo?.DocNo) ?? "*", new WithDataException(ex.Message, ex, e), "EX01", "BTnGJobSale._payClient_OnPaymentCompleted");

                if (_transactionState.SetFail(new Exception(errMsg)))
                {
                    RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, 
                        PaymentResult.Fail, errorMsg: errMsg));
                }
            }
            finally
            {
                EndSale();
                ShowDebugMessage("");
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0511
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PayClient_OnPaymentRequestResult(object sender, NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Event.PaymentRequestResultEventArgs e)
        {
            ShowDebugMessage("");
            try
            {
                if (_localSaleInfo is null)
                    throw new Exception("Previous sale not detected");

                ShowDebugMessage("PayClient_OnPaymentRequestResult with _lastSaleInfo object ..");
                ShowDebugMessage(JsonConvert.SerializeObject(_localSaleInfo, Formatting.Indented));

                MerchantSaleInfo saleInfo = e.PaymentRequestResult.Data;

                _log?.LogText(LogChannel, (_localSaleInfo?.DocNo) ?? "*", e, "A05", "BTnGJobSale.PayClient_OnPaymentRequestResult");

                ShowDebugMessage(JsonConvert.SerializeObject(e.PaymentRequestResult, Formatting.Indented));

                //Verify Signature
                bool isValidSignature = saleInfo.CheckSignature();

                if (isValidSignature == false)
                {
                    throw new Exception($@"Invalid Signature found in result of payment request");
                }
                //-------------------------------------------------------------------------

                _localSaleInfo.BTnGSalesTransactionNo = saleInfo.SalesTransactionNo;

                //ShowBarCode(saleInfo.Base64ImageQrCode);

                DatabaseAx dbAx = DatabaseAx.GetAccess();
                using (var newPayAx = new BTnGNewPaymentAx<SuccessXEcho>(saleInfo.SalesTransactionNo, _localSaleInfo.PaymentGateway, _localSaleInfo.DocNo, _localSaleInfo.Currency, saleInfo.Amount))
                {
                    using (var transResult = (BTnGNewPaymentAx<SuccessXEcho>)dbAx.ExecCommand(newPayAx, waitDelaySec: 20))
                    {
                        if (transResult.ResultStatus.IsSuccess)
                        {
                            // return transResult.SuccessEcho;
                        }
                        else if (transResult.ResultStatus.Error?.Message?.Length > 0)
                        {
                            throw transResult.ResultStatus.Error;
                        }
                        else
                        {
                            throw new Exception("Unknown error when adding new BTnG Payment record; (EXIT60.0511.X01)");
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(_payClient?.CurrentSnRConnectionId) == false)
                {
                    _localSaleInfo.SnRClientId = _payClient.CurrentSnRConnectionId;
                }

                if (_transactionState.SetInitPaymentState(isInitPaymentSuccess: true) == true)
                {
                    _countDown.ChangeCountDown(_countDownCode_WaitForScanning, _maxCountDown_ScanningSec, 500);

                    RaiseOnInProgress(new UIxBTnGPaymentNewTransStartedAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, 
                        saleInfo.SalesTransactionNo, saleInfo.Base64ImageQrCode, saleInfo.MerchantTransactionNo, saleInfo.Amount, _payClient?.CurrentSnRConnectionId));

                    RaiseOnInProgress(new UIxBTnGPaymentCustomerMsgAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, @"PleaseScan2D;Please scan 2D barcode with your smartphone then proceed to complete payment working with your smartphone application"));
                }
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, (_localSaleInfo?.DocNo) ?? "*", new WithDataException(ex.Message, ex, e), "EX01", "BTnGJobSale.PayClient_OnPaymentRequestResult");

                string errMsg = $@"Error when receiving BTnG Payment Request Result. {ex.Message}; (EXIT60.0511.EX01)";

                if (_transactionState.SetFail(new Exception(errMsg)))
                {
                    RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, _localSaleInfo.DocNo, 
                        PaymentResult.Fail, errorMsg: $@"{errMsg}"));
                }

            }
            return;
        }

        /// <summary>
        /// FuncCode:EXIT60.0512
        /// </summary>
        public void CancelSaleRequest(string docNo, string salesTransactionNo, string currency, string paymentGateway, decimal amount)
        {
            RunThreadMan tMan = new RunThreadMan(new ThreadStart(DoCancelSaleRequest), "BTnGJobSale.StartNewSale", 20, LogChannel, isLogReq: true);
            return;
            //-----------------------------------------------------------------------------------------------------------------------------------
            void DoCancelSaleRequest()
            {
                ShowDebugMessage("");
                try
                {
                    if ((_localSaleInfo is null) || (_localSaleInfo.NetProcessId.HasValue == false))
                        throw new Exception($@"Last sale not found; NetProcessId: {_localSaleInfo?.NetProcessId}; (EXIT60.0512.X01)");

                    if (_transactionState.SetCancel())
                    {
                        RaiseOnCompleted(new UIxBTnGPaymentEndAck(_localSaleInfo.NetProcessId, docNo, 
                            PaymentResult.Cancel, "<UserCanceledTransaction>Transaction Canceled"));

                        CancelSale(_localSaleInfo.NetProcessId, docNo, salesTransactionNo, currency, paymentGateway, amount, PaymentResult.Cancel);
                    }
                }
                catch (Exception ex)
                {
                    _log?.LogError(LogChannel, (_localSaleInfo?.DocNo) ?? "*", 
                        new Exception($@"Error; Parameters => DocNo: {docNo}; BTnG SalesTransactionNo: {salesTransactionNo}; Currency: {currency}; PaymentGateway: {paymentGateway}; Amount: {amount}", ex), 
                        "EX01", "BTnGJobSale.CancelSaleRequest");

                    ShowDebugMessage($@"Error when receiving Payment Request Result. {ex.ToString()}");
                }
                finally
                {
                    EndSale();
                }
            }
        }

        private string _lastCanceledSaleTransactionNo = null;
        /// <summary>
        /// FuncCode:EXIT60.0513
        /// </summary>
        private void CancelSale(Guid? NetProcessId, string docNo, string salesTransactionNo, string currency, string paymentGateway, decimal amount,
            PaymentResult paymentResult)
        {
            string paramStr = $@"NetProcessId:{NetProcessId}; DocNo: {docNo}; BTnG SalesTransactionNo: {salesTransactionNo}; Currency: {currency}; PaymentGateway: {paymentGateway}; Amount:{amount}";
            try
            {
                _log?.LogText(LogChannel, (_localSaleInfo?.DocNo) ?? "*", 
                    $@"Parameters => {paramStr}", 
                    "A01", "BTnGJobSale.CancelSale");

                if (_localSaleInfo is null)
                    throw new Exception("Previous sale not detected");

                if (_lastCanceledSaleTransactionNo?.Equals(salesTransactionNo) == true)
                    return;

                ShowDebugMessage("CancelSale_Click with _lastSaleInfo object ..");
                ShowDebugMessage(JsonConvert.SerializeObject(_localSaleInfo, Formatting.Indented));

                BTnGKioskVoidTransactionState voidState = BTnGKioskVoidTransactionState.Error;

                if (paymentResult == PaymentResult.Cancel)
                    voidState = BTnGKioskVoidTransactionState.CancelRefundRequest;

                else if (paymentResult == PaymentResult.Timeout)
                    voidState = BTnGKioskVoidTransactionState.Timeout;

                // .. if (paymentResult == PaymentResult.Unknown) || (paymentResult == PaymentResult.Fail)
                else
                    voidState = BTnGKioskVoidTransactionState.Error;

                _bTnGJobMan.CancelRefundSale(salesTransactionNo, docNo, currency, paymentGateway, amount, voidState);

                _lastCanceledSaleTransactionNo = salesTransactionNo;

                ShowDebugMessage($@"CancelRefundSale '{_localSaleInfo.BTnGSalesTransactionNo}' Sent");
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, (_localSaleInfo?.DocNo) ?? "*", new Exception($@"Error; Paramters => {paramStr}", ex), "EX01", "BTnGJobSale.CancelSale");
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0514
        /// </summary>
        public void EndSale()
        {
            try
            {
                if (_payClient is null)
                    ShowDebugMessage("No valid Sale Instant found");

                else
                    _payClient?.Dispose();

                ShowDebugMessage(".. End Payment");
            }
            catch (Exception ex)
            {
                ShowDebugMessage($@"{ex.ToString()} ");
            }
        }

        public void Dispose()
        {
            try
            {
                _bTnGJobMan?.Dispose();
            }
            catch { };
        }

        class LocalSaleInfo
        {
            public Guid? NetProcessId { get; set; }
            public string SnRClientId { get; set; }
            public string BTnGSalesTransactionNo { get; set; }
            public string DocNo { get; set; }
            public string Currency { get; set; }
            public string PaymentGateway { get; set; }
            public decimal Amount { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ContactNo { get; set; }

            public void Reset()
            {
                NetProcessId = null;
                SnRClientId = null;
                BTnGSalesTransactionNo = null;
                DocNo = null;
                Currency = null;
                PaymentGateway = null;
                Amount = 0.0M;
                FirstName = null;
                LastName = null;
                ContactNo = null;
            }
        }

        /// <summary>
        /// ClassCode:EXIT60.06
        /// </summary>
        class TransactionState : IDisposable
        {
            private object _decisionLock = null;

            public bool? IsInitPaymentSuccess { get; private set; }
            public bool IsPaid { get; private set; }
            public bool IsFail { get; private set; }
            public bool IsCancel { get; private set; }
            public Exception Error { get; private set; }
            public bool IsEnd
            {
                get
                {
                    if (IsInitPaymentSuccess.HasValue && (IsInitPaymentSuccess.Value == false))
                        return true;

                    return (IsPaid || IsFail || IsCancel);
                }
            }

            public TransactionState()
            {
                _decisionLock = new object();
                Error = null;
                IsPaid = false;
                IsFail = false;
                IsCancel = false;
                IsInitPaymentSuccess = null;
            }

            /// <summary>
            /// FuncCode:EXIT60.0602
            /// </summary>
            public bool SetInitPaymentState(bool isInitPaymentSuccess)
            {
                bool success = false;
                Thread tWorker = new Thread(new ThreadStart(new Action(() =>
                {
                    lock (_decisionLock)
                    {
                        if (IsEnd == false)
                        {
                            if (IsInitPaymentSuccess.HasValue == false)
                            {
                                IsInitPaymentSuccess = isInitPaymentSuccess;

                                if (isInitPaymentSuccess == false)
                                {
                                    IsFail = true;
                                    Error = new Exception("Fail initiating Boost/TnG transaction; (EXIT60.0602.X01)");
                                }

                                success = true;
                            }
                        }
                    }
                })));
                tWorker.IsBackground = true;
                tWorker.Priority = ThreadPriority.Highest;
                tWorker.Start();
                tWorker.Join();

                return success;
            }

            public bool SetPaid()
            {
                bool success = false;
                Thread tWorker = new Thread(new ThreadStart(new Action(() =>
                {
                    lock (_decisionLock)
                    {
                        if (IsEnd == false)
                        {
                            IsPaid = true;
                            success = true;
                        }
                    }
                })));
                tWorker.IsBackground = true;
                tWorker.Priority = ThreadPriority.Highest;
                tWorker.Start();
                tWorker.Join();

                return success;
            }

            public bool SetCancel()
            {
                bool success = false;
                Thread tWorker = new Thread(new ThreadStart(new Action(() =>
                {
                    lock (_decisionLock)
                    {
                        // Cancelling is mandatory        
                        IsCancel = true;
                        success = true;
                    }
                })));
                tWorker.IsBackground = true;
                tWorker.Priority = ThreadPriority.Highest;
                tWorker.Start();
                tWorker.Join();

                return success;
            }

            /// <summary>
            /// FuncCode:EXIT60.0605
            /// </summary>
            public bool SetFail(Exception ex)
            {
                if (ex is null)
                    ex = new Exception("Unknown error exception; (EXIT60.0605.X01)");

                bool success = false;
                Thread tWorker = new Thread(new ThreadStart(new Action(() =>
                {
                    lock (_decisionLock)
                    {
                        if (IsEnd == false)
                        {
                            IsFail = true;
                            Error = ex;
                            success = true;
                        }
                    }
                })));
                tWorker.IsBackground = true;
                tWorker.Priority = ThreadPriority.Highest;
                tWorker.Start();
                tWorker.Join();

                return success;
            }

            public void Dispose()
            {
                _decisionLock = null;
            }
        }
    }
}