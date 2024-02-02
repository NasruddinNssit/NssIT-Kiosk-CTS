using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.Access;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sales.UIx;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access.Echo;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Log.DB;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.NetClient
{
    /// <summary>
    /// ClassCode:EXIT80.11
    /// </summary>
    public class NetClientSalesServiceV2
    {
        public const int WebServerTimeout = 9999999;
        public const int InvalidAuthentication = 9999998;

        private AppModule _appModule = AppModule.UIKioskSales;
        private string _logChannel = "NetClientService";

        private NssIT.Kiosk.Log.DB.DbLog _log = null;
        private INetMediaInterface _netInterface;

        private ReceivedNetProcessIdTracker _recvedNetProcIdTracker = new ReceivedNetProcessIdTracker();

        // OnDataReceived : New Implemention for UI Page change only and special event like Ongoing Payment process.
        public event EventHandler<DataReceivedEventArgs> OnDataReceived;

        public NetClientSalesServiceV2(INetMediaInterface netInterface)
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

        /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        #region Services Implementation

        public bool QueryKioskLastRebootTime(out KioskLastRebootTimeEcho kioskLastRebootTime,
            out bool isServerResponded, int waitDelaySec = 60)
        {
            isServerResponded = false;
            kioskLastRebootTime = null;

            var uiReq = new UIReq<UIxGetMachineLastRebootTimeRequest>("*", _appModule, DateTime.Now, new UIxGetMachineLastRebootTimeRequest("*"));

            UIxGnAppAck<KioskLastRebootTimeEcho> uIxAck = Request<UIxGnAppAck<KioskLastRebootTimeEcho>>(
                uiReq, "NetClientSalesServiceV2.QueryKioskLastRebootTime", out bool isResponded, waitDelaySec);

            isServerResponded = isResponded;

            if ((isResponded) && (uIxAck?.IsDataReadSuccess == true) && (uIxAck?.Data != null))
            {
                kioskLastRebootTime = uIxAck.Data;
                return true;
            }
            else
            {
                _log?.LogError(_logChannel, "*", 
                    new WithDataException($@"isResponded:{isResponded}; MsgObj: UIxGnAppAck<KioskLastRebootTimeEcho>", new Exception("Fail to read kiosk Last Reboot Time"), uIxAck), 
                    "X20", "NetClientSalesServiceV2.QueryKioskLastRebootTime");
                return false;
            }
        }

        public bool QueryLastCardSettlementSuccessTime(out CardSettlementSuccessTimeEcho kioskLastCardSettlementSuccessTime,
            out bool isServerResponded, int waitDelaySec = 60)
        {
            isServerResponded = false;
            kioskLastCardSettlementSuccessTime = null;

            var uiReq = new UIReq<UIxGetCardSettlementTimeRequest>("*", _appModule, DateTime.Now, new UIxGetCardSettlementTimeRequest("*"));

            UIxGnAppAck<CardSettlementSuccessTimeEcho> uIxAck = Request<UIxGnAppAck<CardSettlementSuccessTimeEcho>>(
                uiReq, "NetClientSalesServiceV2.QueryLastCardSettlementSuccessTime", out bool isResponded, waitDelaySec);

            isServerResponded = isResponded;

            if ((isResponded) && (uIxAck?.IsDataReadSuccess == true) && (uIxAck?.Data != null))
            {
                kioskLastCardSettlementSuccessTime = uIxAck.Data;
                return true;
            }
            else
            {
                _log?.LogError(_logChannel, "*",
                    new WithDataException($@"isResponded:{isResponded}; MsgObj: UIxGnAppAck<CardSettlementSuccessTimeEcho>", new Exception("Fail to read kiosk Last Card Settlement Success Time"), uIxAck),
                    "X20", "NetClientSalesServiceV2.QueryLastCardSettlementSuccessTime");
                return false;
            }
        }

        public bool UpdateCardSettlementSuccessTime(DateTime successTime, out bool isServerResponded, int waitDelaySec = 60)
        {
            isServerResponded = false;

            var uiReq = new UIReq<UIxUpdateCardSettlementSuccessTimeRequest>("*", _appModule, DateTime.Now, new UIxUpdateCardSettlementSuccessTimeRequest("*", successTime));

            UIxGnAppAck<SuccessXEcho> uIxAck = Request<UIxGnAppAck<SuccessXEcho>>(
                uiReq, "NetClientSalesServiceV2.UpdateCardSettlementSuccessTime", out bool isResponded, waitDelaySec);

            isServerResponded = isResponded;

            if ((isResponded) && (uIxAck?.IsDataReadSuccess == true) && (uIxAck?.Data != null))
            {
                return true;
            }
            else
            {
                _log?.LogError(_logChannel, "*",
                    new WithDataException($@"isResponded:{isResponded}; MsgObj: UIxGnAppAck<SuccessXEcho>", new Exception("Fail to Update Card Settlement Success Time"), uIxAck),
                    "X20", "NetClientSalesServiceV2.UpdateCardSettlementSuccessTime");
                return false;
            }
        }

        #endregion
        /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
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

            _netInterface.SendMsgPack(msgPack);
        }

        private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.ReceivedData?.Module == AppModule.UIKioskSales)
            {
                if ((e.ReceivedData?.MsgObject is IKioskMsg tMsg) && (tMsg.Instruction == CommInstruction.ReferToGenericsUIObj))
                {
                    _recvedNetProcIdTracker.AddNetProcessId(e.ReceivedData.NetProcessId, e.ReceivedData.MsgObject);

                    //if (e.ReceivedData.MsgObject?.GetMsgData() is IUIxBTnGPaymentOngoingGroupAck uIAck)
                    //{
                    //    RaiseOnDataReceived(sender, e);
                    //}
                }
            }
            else if (e.ReceivedData?.Module == AppModule.Unknown)
            {
                string errMsg = $@"Error : {e.ReceivedData.ErrorMessage}; NetProcessId: {e.ReceivedData.NetProcessId}";
                _log.LogText(_logChannel, "-", errMsg, "A02", "NetClientBTnGService._netInterface_OnDataReceived", NssIT.Kiosk.AppDecorator.Log.MessageType.Error, netProcessId: e.ReceivedData.NetProcessId);
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.1101
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
                _log.LogError(_logChannel, "-", new Exception("Unhandled event exception; (EXIT80.1101.EX01)", ex), "EX01", "NetClientBTnGService.RaiseOnDataReceived", netProcessId: e?.ReceivedData?.NetProcessId);
            }
        }
    }
}
