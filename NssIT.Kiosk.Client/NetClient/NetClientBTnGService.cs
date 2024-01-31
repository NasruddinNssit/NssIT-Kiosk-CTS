using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NssIT.Kiosk.Client.NetClient.NetClientSalesService;

namespace NssIT.Kiosk.Client.NetClient
{
    /// <summary>
    /// ClassCode:EXIT80.03
    /// </summary>
    public class NetClientBTnGService
    {
        public const int WebServerTimeout = 9999999;
        public const int InvalidAuthentication = 9999998;

        private AppModule _appModule = AppModule.UIBTnG;
        private string _logChannel = "NetClientService";

        private NssIT.Kiosk.Log.DB.DbLog _log = null;
        private INetMediaInterface _netInterface;
        private ReceivedNetProcessIdTracker _recvedNetProcIdTracker = new ReceivedNetProcessIdTracker();

        // OnDataReceived : New Implemention for UI Page change only and special event like Ongoing Payment process.
        public event EventHandler<DataReceivedEventArgs> OnDataReceived;

        public NetClientBTnGService(INetMediaInterface netInterface)
        {
            _netInterface = netInterface;

            _log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

            if (_netInterface != null)
                _netInterface.OnDataReceived += _netInterface_OnDataReceived;
        }

        public void Dispose()
        {
            try
            {
                if (_netInterface != null)
                    _netInterface.OnDataReceived -= _netInterface_OnDataReceived;
            }
            catch { }

            try
            {
                if (OnDataReceived?.GetInvocationList() is Delegate[] delgList)
                {
                    foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
                        OnDataReceived -= delg;
                }
            }
            catch { }

            _netInterface = null;
        }

        private int GetServerPort() => App.SysParam.PrmLocalServerPort;

        public bool QueryAllPaymentGateway(out BTnGGetPaymentGatewayResult payGateResult,
            out bool isServerResponded, int waitDelaySec = 60)
        {
            isServerResponded = false;
            payGateResult = null;

            var uiReq = new UIReq<UIxBTnGGetAvailablePaymentGatewayRequest>("*", _appModule, DateTime.Now, new UIxBTnGGetAvailablePaymentGatewayRequest("*"));

            UIxGnBTnGAck<BTnGGetPaymentGatewayResult> uIAck = Request<UIxGnBTnGAck<BTnGGetPaymentGatewayResult>>(
                uiReq, "NetClientBTnGService.QueryAllPaymentGateWay", out bool isResponded, waitDelaySec);

            isServerResponded = isResponded;

            if ((isResponded) && (uIAck.Data?.Messages?.Count > 0))
            {
                payGateResult = uIAck.Data;
                return true;
            }
            else
                return false;
        }

        public void SendSuccessPaymentGatewayLog(string bTnGSaleTransLog)
        {
            var uiReq = new UIReq<UIxBTnGPaymentGatewayLogSuccessRequest>(bTnGSaleTransLog, _appModule, DateTime.Now, 
                new UIxBTnGPaymentGatewayLogSuccessRequest(bTnGSaleTransLog, bTnGSaleTransLog));

            SendToServerOnly(uiReq, $@"SendSuccessPaymentGatewayLog('{bTnGSaleTransLog}')");
        }

        public bool MakeNewPaymentRequest(
            string docNo,
            decimal price, string paymentGateway,
            string currency, string customerFirstName,
            string customerLastName, string contactNo, string financePaymentMethod,
            out bool isServerResponded, int waitDelaySec = 60)
        {
            isServerResponded = false;

            var uiReq = new UIReq<UIxBTnGPaymentMakeNewPaymentRequest>(docNo, _appModule, DateTime.Now,
                new UIxBTnGPaymentMakeNewPaymentRequest(docNo, docNo, price, paymentGateway, currency, customerFirstName, customerLastName, contactNo, financePaymentMethod));

            UIAck<UIxBTnGPaymentNewTransStartedAck> uIAck = Request<UIAck<UIxBTnGPaymentNewTransStartedAck>>(
                uiReq, "NetClientBTnGService.PaymentMakeNewPaymentRequest", out bool isResponded, waitDelaySec);

            isServerResponded = isResponded;

            return isServerResponded;
        }

        public void CancelRefundPaymentRequest(
            string processId, string btngSalesTransactionNo,
            string docNo, string currency,
            string paymentGateway, decimal amount)
        {
            var uiReq = new UIReq<UIxBTnGCancelRefundPaymentRequest>(docNo, _appModule, DateTime.Now,
                new UIxBTnGCancelRefundPaymentRequest(docNo, btngSalesTransactionNo, docNo, currency, paymentGateway, amount));

            SendToServerOnly(uiReq, "NetClientBTnGService.CancelRefundPaymentRequest");
        }

        public void EndPaymentRequest()
        {
            var uiReq = new UIReq<UIxBTnGEndPaymentRequest>("*", _appModule, DateTime.Now, new UIxBTnGEndPaymentRequest("*"));
            SendToServerOnly(uiReq, "NetClientBTnGService.EndPaymentRequest");
        }

        public void TestGetServerTimeRequest(
            string docNo)
        {
            var uiReq = new UIReq<UIxBTnGTestReadServerTimeRequest>(docNo, _appModule, DateTime.Now,
                new UIxBTnGTestReadServerTimeRequest(docNo));

            SendToServerOnly(uiReq, "NetClientBTnGService.TestGetServerTimeRequest");
        }

        public void TestSendEchoRequest(
            string docNo, string echoMessage)
        {
            var uiReq = new UIReq<UIxBTnGTestEchoMessageSendRequest>(docNo, _appModule, DateTime.Now,
                new UIxBTnGTestEchoMessageSendRequest(docNo, echoMessage));

            SendToServerOnly(uiReq, "NetClientBTnGService.TestGetServerTimeRequest");
        }

        /// <summary>
        /// Send to local server then expect one/many response/s.
        /// </summary>
        /// <param name="sendKioskMsg"></param>
        /// <param name="processTag"></param>
        /// <param name="waitDelaySec"></param>
        /// <returns></returns>
        private UIxTResult Request<UIxTResult>(IKioskMsg sendKioskMsg, string processTag, out bool isServerResponded, int waitDelaySec = 60)
        {
            string pId = Guid.NewGuid().ToString();

            isServerResponded = false;
            IKioskMsg kioskMsg = null;

            Guid lastNetProcessId;

            waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

            _log?.LogText(_logChannel, pId, $@"Start - {processTag}", "A01", "NetClientBTnGService.Request");

            NetMessagePack msgPack = new NetMessagePack(sendKioskMsg) { DestinationPort = GetServerPort() };
            lastNetProcessId = msgPack.NetProcessId;

            _log?.LogText(_logChannel, pId,
                msgPack, "A05", $@"{processTag} => NetClientBTnGService.Request", extraMsg: "MsgObj: NetMessagePack");

            if (_netInterface == null)
                return default;

            _netInterface?.SendMsgPack(msgPack);

            UIxKioskDataRequestBase reqBase = sendKioskMsg?.GetMsgData();
            DateTime endTime = DateTime.Now.AddSeconds(waitDelaySec);

            while (endTime.Ticks >= DateTime.Now.Ticks)
            {
                if (Thread.CurrentThread.ThreadState.IsStateInList(
                    ThreadState.AbortRequested, ThreadState.StopRequested,
                    ThreadState.Aborted, ThreadState.Stopped))
                {
                    break;
                }

                else if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out IKioskMsg data) == false)
                    Task.Delay(100).Wait();

                else
                {
                    kioskMsg = data;
                    isServerResponded = true;
                    break;
                }
            }

            _log?.LogText(_logChannel, pId, kioskMsg, "A10", "NetClientBTnGService.Request", extraMsg: $@"MsgObj: {kioskMsg?.GetType().Name}");

            if ((isServerResponded) && (kioskMsg?.GetMsgData() is UIxTResult result))
            {
                return result;
            }
            else
            {
                _log?.LogText(_logChannel, pId, $@"Problem; isServerResponded : {isServerResponded}", "B01", "NetClientBTnGService.Request");
                return default;
            }
        }


        /// <summary>
        /// Send to local server without expecting response 
        /// </summary>
        /// <param name="sendKioskMsg"></param>
        /// <param name="processTag"></param>
        private void SendToServerOnly(IKioskMsg sendKioskMsg, string processTag)
        {
            Guid lastNetProcessId;

            _log.LogText(_logChannel, "-", $@"Start - {processTag}", "A01", "NetClientBTnGService.SendToServerOnly");

            NetMessagePack msgPack = new NetMessagePack(sendKioskMsg) { DestinationPort = GetServerPort() };
            lastNetProcessId = msgPack.NetProcessId;

            _log.LogText(_logChannel, "-",
                msgPack, "A05", $@"{processTag} => NetClientBTnGService.SendToServerOnly", extraMsg: "MsgObject: NetMessagePack");

            _netInterface?.SendMsgPack(msgPack);
        }

        /// <summary>
        /// FuncCode:EXIT80.0304
        /// </summary>
        private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.ReceivedData?.Module == AppModule.UIBTnG)
            {
                _recvedNetProcIdTracker.AddNetProcessId(e.ReceivedData.NetProcessId, e.ReceivedData?.MsgObject);

                if (e.ReceivedData.MsgObject?.GetMsgData() is IUIxBTnGPaymentOngoingGroupAck uIAck)
                {
                    RaiseOnDataReceived(sender, e);
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.0301
        /// </summary>
        private void RaiseOnDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (OnDataReceived != null)
                {
                    OnDataReceived.Invoke(sender, e);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, "-", new Exception("Unhandled event exception; (EXIT80.0301.EX01)", ex), "EX01", "NetClientBTnGService.RaiseOnDataReceived", netProcessId: e?.ReceivedData?.NetProcessId);
            }
        }


    }
}