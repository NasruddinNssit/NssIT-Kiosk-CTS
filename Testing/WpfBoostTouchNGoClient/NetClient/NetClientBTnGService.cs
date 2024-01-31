using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfBoostTouchNGoClient.NetClient
{
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

        public bool PaymentMakeNewPaymentRequest(
            string docNo,
            decimal price, string paymentGateway,
            string currency, string customerFirstName,
            string customerLastName, string contactNo,
            out bool isServerResponded, int waitDelaySec = 60)
        {
            isServerResponded = false;

            var uiReq = new UIReq<UIxBTnGPaymentMakeNewPaymentRequest>(docNo, _appModule, DateTime.Now,
                new UIxBTnGPaymentMakeNewPaymentRequest(docNo, docNo, price, paymentGateway, currency, customerFirstName, customerLastName, contactNo, "*"));

            UIAck<UIxBTnGPaymentNewTransStartedAck> uIAck = Request<UIAck<UIxBTnGPaymentNewTransStartedAck>>(
                uiReq, "NetClientBTnGService.PaymentMakeNewPaymentRequest", out bool isResponded, waitDelaySec);

            isServerResponded = isResponded;

            return isServerResponded;
        }

        public void CancelRefundPaymentRequest(
            string processId, string salesTransactionNo,
            string docNo, string currency,
            string paymentGateway, decimal amount)
        {
            var uiReq = new UIReq<UIxBTnGCancelRefundPaymentRequest>(docNo, _appModule, DateTime.Now,
                new UIxBTnGCancelRefundPaymentRequest(docNo, salesTransactionNo, docNo, currency, paymentGateway, amount));

            SendToServerOnly(uiReq, "NetClientBTnGService.CancelRefundPaymentRequest");
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
        /// Send to local server then expect at least one response.
        /// </summary>
        /// <param name="sendKioskMsg"></param>
        /// <param name="processTag"></param>
        /// <param name="waitDelaySec"></param>
        /// <returns></returns>
        private UIxTResult Request<UIxTResult>(IKioskMsg sendKioskMsg, string processTag, out bool isServerResponded, int waitDelaySec = 60)
        {
            isServerResponded = false;
            IKioskMsg kioskMsg = null;

            Guid lastNetProcessId;

            waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

            _log.LogText(_logChannel, "-", $@"Start - {processTag}", "A01", "NetClientBTnGService.Request");

            NetMessagePack msgPack = new NetMessagePack(sendKioskMsg) { DestinationPort = GetServerPort() };
            lastNetProcessId = msgPack.NetProcessId;

            _log.LogText(_logChannel, "-",
                msgPack, "A05", $@"{processTag} => NetClientBTnGService.Request", extraMsg: "MsgObject: NetMessagePack");

            _netInterface.SendMsgPack(msgPack);

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

            //CYA-DEBUG .. need to log here
            if ((isServerResponded) && (kioskMsg?.GetMsgData() is UIxTResult result))
            {
                return result;
            }
            else
                return default;
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

            _netInterface.SendMsgPack(msgPack);
        }

        private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.ReceivedData?.Module == AppModule.UIBTnG)
            {
                _recvedNetProcIdTracker.AddNetProcessId(e.ReceivedData.NetProcessId, e.ReceivedData.MsgObject);

                if (e.ReceivedData.MsgObject?.GetMsgData() is IUIxBTnGPaymentOngoingGroupAck uIAck)
                {
                    RaiseOnDataReceived(sender, e);
                }
            }
            else if (e.ReceivedData?.Module == AppModule.Unknown)
            {
                string errMsg = $@"Error : {e.ReceivedData.ErrorMessage}; NetProcessId: {e.ReceivedData.NetProcessId}";
                _log.LogText(_logChannel, "-", errMsg, "A02", "NetClientBTnGService._netInterface_OnDataReceived", NssIT.Kiosk.AppDecorator.Log.MessageType.Error, netProcessId: e.ReceivedData.NetProcessId);
            }
        }

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
                _log.LogError(_logChannel, "-", new Exception("Unhandled event exception; (EXITxx)", ex), "EX01", "NetClientBTnGService.RaiseOnDataReceived", netProcessId: e?.ReceivedData?.NetProcessId);
            }
        }

        public class ReceivedNetProcessIdTracker
        {
            private object _conCurrLock = new object();

            private (int ClearNetProcessIdListIntervalSec, DateTime NextClearListTime, ConcurrentDictionary<Guid, ResponseData> NetProcessIdList) _receivedNetProcess
                = (ClearNetProcessIdListIntervalSec: 60, NextClearListTime: DateTime.Now, NetProcessIdList: new ConcurrentDictionary<Guid, ResponseData>());

            public ReceivedNetProcessIdTracker() { }

            public void AddNetProcessId(Guid netProcessId, IKioskMsg kioskMsgResponded)
            {
                lock (_conCurrLock)
                {
                    // Clear NetProcessIdList -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
                    if (_receivedNetProcess.NextClearListTime.Ticks < DateTime.Now.Ticks)
                    {
                        Guid[] clearNetProcessIdArr = (from keyPair in _receivedNetProcess.NetProcessIdList
                                                       where (keyPair.Value.ReceivedTime.Ticks <= _receivedNetProcess.NextClearListTime.Ticks)
                                                       select keyPair.Key).ToArray();

                        foreach (Guid netProcId in clearNetProcessIdArr)
                        {
                            _receivedNetProcess.NetProcessIdList.TryRemove(netProcId, out ResponseData resp);
                            resp.Dispose();
                        }

                        _receivedNetProcess.NextClearListTime = DateTime.Now.AddSeconds(_receivedNetProcess.ClearNetProcessIdListIntervalSec);
                    }
                    // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

                    if (_receivedNetProcess.NetProcessIdList.TryGetValue(netProcessId, out _))
                    {
                        _receivedNetProcess.NetProcessIdList.TryRemove(netProcessId, out ResponseData resp2);
                        resp2.Dispose();
                    }

                    try
                    {
                        _receivedNetProcess.NetProcessIdList.TryAdd(netProcessId, new ResponseData() { KioskData = kioskMsgResponded, ReceivedTime = DateTime.Now });
                    }
                    catch { }
                }
            }

            /// <summary>
            /// Return true if NetProcessId has responded.
            /// </summary>
            /// <param name="netProcessId"></param>
            /// <returns></returns>
            public bool CheckReceivedResponded(Guid netProcessId, out IKioskMsg kioskMsg)
            {
                bool retFound = false;
                kioskMsg = null;
                ResponseData rData = null;

                Thread execThread = new Thread(new ThreadStart(new Action(() =>
                {
                    lock (_conCurrLock)
                    {
                        if (_receivedNetProcess.NetProcessIdList.TryGetValue(netProcessId, out ResponseData responsedData))
                        {
                            retFound = true;
                            rData = responsedData;
                        }
                    }
                })));
                execThread.IsBackground = true;
                execThread.Start();
                execThread.Join(5 * 1000);

                if ((retFound) && ((rData?.KioskData) != null))
                {
                    kioskMsg = rData.KioskData;
                    return true;
                }
                else
                    return false;
            }
        }

        class ResponseData : IDisposable
        {
            public DateTime ReceivedTime { get; set; }
            public IKioskMsg KioskData { get; set; }

            public void Dispose()
            {
                KioskData = null;
            }
        }
    }
}