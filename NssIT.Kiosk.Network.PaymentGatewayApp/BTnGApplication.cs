using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UIx;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Network.PaymentGatewayApp.CustomApp;
using NssIT.Kiosk.Network.PaymentGatewayApp.CustomApp.KTMBApp;
using NssIT.Kiosk.Network.PaymentGatewayApp.JobApp.BTnGJob;
using NssIT.Kiosk.Server.AccessDB.AxCommand;
using NssIT.Kiosk.Server.AccessDB.AxCommand.BTnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.PaymentGatewayApp
{
    /// <summary>
    /// ClassCode:EXIT60.01
    /// </summary>
    public class BTnGApplication : IUIApplicationJob, IDisposable
    {
        private const string LogChannel = "BTnG_ServerApplication";
        private const AppModule appModule = AppModule.UIBTnG;

        public event EventHandler<UIMessageEventArgs> OnShowResultMessage;
        private SemaphoreSlim _asyncSendLock = new SemaphoreSlim(1);
        private AppCallBackDelg _appCallBackHandle = null;
        private IAppAccessCallBackPlan _accCallBackPlan = null;
        private IServerAppPlan _IServerAppPlan = null;
        private BTnGJobSale _bTnGJobSale = null;

        private LibShowMessageWindow.MessageWindow _msgDebug = null;
        private DbLog _log = null;
        private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

        /// <summary>
        /// FuncCode:EXIT60.0102
        /// </summary>
        public BTnGApplication(LibShowMessageWindow.MessageWindow msg = null)
        {
            _msgDebug = msg;
            _appCallBackHandle = new AppCallBackDelg(AccessCallBackEvent);
            _accCallBackPlan = new KTMBApp_AccessCallBackPlan(this);
            _IServerAppPlan = new KTMBAppPlan(_appCallBackHandle);
            _bTnGJobSale = new BTnGJobSale(msg);

            _bTnGJobSale.OnInProgress += _bTnGJobSale_OnInProgress;
            _bTnGJobSale.OnCompleted += _bTnGJobSale_OnCompleted;
        }

        private void _bTnGJobSale_OnCompleted(object sender, Base.BTnGEventArg<UIxBTnGPaymentEndAck> e)
        {
            RaiseOnShowResultMessage(e.EventData.BaseRefNetProcessId, new UIAck<UIxBTnGPaymentEndAck>(e.EventData.BaseRefNetProcessId, e.EventData.BaseProcessId, appModule, DateTime.Now, e.EventData));
        }

        private void _bTnGJobSale_OnInProgress(object sender, Base.BTnGEventArg<IUIxBTnGPaymentGroupAck> e)
        {
            if (e.EventData is UIxKioskDataAckBase data)
            {
                RaiseOnShowResultMessage(data.BaseRefNetProcessId, new UIAck<UIxKioskDataAckBase>(data.BaseRefNetProcessId, data.BaseProcessId, appModule, DateTime.Now, data));
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
        /// FuncCode:EXIT60.0105
        /// </summary>
        public async void AccessCallBackEvent(UIxKioskDataAckBase accessResult)
        {
            string processId = string.IsNullOrWhiteSpace(accessResult?.BaseProcessId) ? "*" : accessResult?.BaseProcessId;

            try
            {
                if (accessResult.BaseRefNetProcessId.HasValue == false)
                    throw new Exception($@"Unable to read NetProcessId from '{accessResult.GetType().Name}'");

                await _accCallBackPlan.DeliverAccessResult(accessResult);
            }
            catch (Exception ex)
            {
                Log?.LogError(LogChannel, processId, new WithDataException(ex.Message, ex, accessResult), "EX01", "BTnGApplication.AccessCallBackEvent");
            }
        }

        /// <summary>
        /// FuncCode:EXIT60.0106
        /// </summary>
        public async Task<bool> SendInternalCommand(string processId, Guid? netProcessId, IKioskMsg svcMsg)
        {
            bool lockSuccess = false;
            try
            {
                Guid pId = Guid.NewGuid();

                Log.LogText(LogChannel, "-", svcMsg, "A01", "BTnGApplication.SendInternalCommand", netProcessId: netProcessId,
                    extraMsg: $@"Start - SendInternalCommand; pId: {pId};MsgObj: {svcMsg.GetType().ToString()}");

                lockSuccess = await _asyncSendLock.WaitAsync(5 * 60 * 1000);

                Log.LogText(LogChannel, "-", $@"pId: {pId}; lockSuccess : {lockSuccess}", "A02", "BTnGApplication.SendInternalCommand", netProcessId: netProcessId);

                if (lockSuccess == false)
                    return false;

                _IServerAppPlan.PreProcess(svcMsg);

                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // Getting Available Payment Gateway
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                if (svcMsg is UIReq<UIxBTnGGetAvailablePaymentGatewayRequest> uiReq1)
                {
                    IAx ax = new AxGetAvailablePaymentGateway("*", uiReq1.BaseNetProcessId, _appCallBackHandle);
                    RunThreadMan tWorker = new RunThreadMan(new ThreadStart(ax.Execute), $@"{ax.GetType().Name}.Execute::BTnGApplication.SendInternalCommand; (EXIT60.0106.A02)", (2 * 60), LogChannel);
                }
                else if (svcMsg is UIAck<UIxGnBTnGAck<BTnGGetPaymentGatewayResult>>)
                {
                    RaiseOnShowResultMessage(netProcessId, svcMsg);
                }
                else if (svcMsg is UIReq<UIxBTnGPaymentGatewayLogSuccessRequest> uiReq3)
                {
                    IAx ax = new AxUpdateSuccessTransLog(uiReq3.ProcessId, uiReq3.BaseNetProcessId, uiReq3.MsgData.BTnGSaleTransactionNo);
                    RunThreadMan tWorker = new RunThreadMan(new ThreadStart(ax.Execute), $@"{ax.GetType().Name}.Execute::BTnGApplication.SendInternalCommand; (EXIT60.0106.A03)", (2 * 60), LogChannel);
                }

                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // Start New Payment
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                else if(svcMsg is UIReq<UIxBTnGPaymentMakeNewPaymentRequest> uiNewPay)
                {
                    if (uiNewPay.MsgData != null )
                    {
                        var req = uiNewPay.MsgData;
                        _bTnGJobSale.StartNewSale(netProcessId, req.DocNo, req.Price, req.PaymentGateway, req.Currency,
                            req.CustomerFirstName, req.CustomerLastName, req.ContactNo, req.FinancePaymentMethod);
                    }
                }
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // Cancel Refund Payment Gateway
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                else if (svcMsg is UIReq<UIxBTnGCancelRefundPaymentRequest> uiCancRefn)
                {
                    var req = uiCancRefn.MsgData;
                    _bTnGJobSale.CancelSaleRequest(req.DocNo, req.BTnGSalesTransactionNo, req.Currency,
                        req.PaymentGateway, req.Amount);
                }
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // End Payment Gateway
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                else if (svcMsg is UIReq<UIxBTnGEndPaymentRequest> uiEndTrans)
                {
                    var req = uiEndTrans.MsgData;
                    _bTnGJobSale.EndSale();
                }
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // Web API SignalR Test New Payment // 
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                else if (svcMsg is UIReq<UIxBTnGTestReadServerTimeRequest> uiTest1)
                {
                    var req = uiTest1.MsgData;
                    _bTnGJobSale.ReadServerTime(netProcessId, req.BaseProcessId);
                }
                else if (svcMsg is UIReq<UIxBTnGTestEchoMessageSendRequest> uiTest2)
                {
                    var req = uiTest2.MsgData;
                    _bTnGJobSale.SendEchoMessage(netProcessId, req.EchoMessage);
                }
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                else if (svcMsg is UIReq<UIxSampleDateRequest>)
                {
                    // Do process/job here
                }
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError(LogChannel, processId, new WithDataException("MsgObj : IKioskMsg", ex, svcMsg), "E02", "BTnGApplication.SendInternalCommand", netProcessId: netProcessId);
            }
            finally
            {
                if ((lockSuccess == true) && (_asyncSendLock.CurrentCount == 0))
                    _asyncSendLock.Release();
            }

            return false;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        }

        private object _showResultMessageLock = new object();

        /// <summary>
        /// FuncCode:EXIT60.0107
        /// </summary>
        private void RaiseOnShowResultMessage(Guid? netProcessId, IKioskMsg kioskMsg)
        {
            if (OnShowResultMessage is null)
                return;

            RunThreadMan tMan = new RunThreadMan(new ThreadStart(DoRaiseOnShowResultMessage), "BTnGApplication.RaiseOnShowResultMessage; (EXIT60.0107.A02)", 20, LogChannel, ThreadPriority.AboveNormal);

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            /// <summary>
            /// FuncCode:EXIT60.018A
            /// </summary>
            void DoRaiseOnShowResultMessage()
            {
                try
                {
                    if (kioskMsg is null)
                        throw new Exception("Invalid IKioskMsg message");

                    lock(_showResultMessageLock)
                    {
                        if (kioskMsg.GetMsgData() is IUIxBTnGPaymentGroupAck uixBTnG)
                        {
                            OnShowResultMessage.Invoke(null, new UIMessageEventArgs(netProcessId) { KioskMsg = kioskMsg, MsgType = MessageType.NormalType, Message = "*" });
                        }
                        else if (kioskMsg.GetMsgData() is IUIxGenericData uixData)
                        {
                            if (uixData.Error is Exception exX)
                            {
                                if (string.IsNullOrWhiteSpace(exX.Message))
                                    OnShowResultMessage.Invoke(null, new UIMessageEventArgs(netProcessId) { KioskMsg = kioskMsg, MsgType = MessageType.ErrorType, Message = "Unknown error; (EXIT60.018A.X01)" });
                                else
                                    OnShowResultMessage.Invoke(null, new UIMessageEventArgs(netProcessId) { KioskMsg = kioskMsg, MsgType = MessageType.ErrorType, Message = exX.Message });
                            }
                            else
                                OnShowResultMessage.Invoke(null, new UIMessageEventArgs(netProcessId) { KioskMsg = kioskMsg, MsgType = MessageType.NormalType, Message = "*" });
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, kioskMsg.ProcessId, new WithDataException("MsgObj : IKioskMsg", ex, kioskMsg), "EX01", "BTnGApplication.DoRaiseOnShowResultMessage", netProcessId: netProcessId);
                }
            }
        }

        public bool ShutDown()
        {
            Dispose();
            return true;
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed == false)
            {
                try
                {
                    _bTnGJobSale?.Dispose();
                }
                catch { }
                _disposed = true;
            }
        }
    }
}
