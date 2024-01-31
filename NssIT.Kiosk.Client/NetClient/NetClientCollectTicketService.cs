using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.Base.Time;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NssIT.Kiosk.Client.Base.NetServiceAnswerMan;

namespace NssIT.Kiosk.Client.NetClient
{
    /// <summary>
    /// ClassCode:EXIT80.13
    /// </summary>
    public class NetClientCollectTicketService
    {
        public const int WebServerTimeout = 9999999;
        public const int InvalidAuthentication = 9999998;

        private AppModule _appModule = AppModule.UICollectTicket;
        private string _logChannel = "NetClientService";

        private NssIT.Kiosk.Log.DB.DbLog _log = null;
        private INetMediaInterface _netInterface;

        private ReceivedNetProcessIdTracker _recvedNetProcIdTracker = new ReceivedNetProcessIdTracker();

        // OnDataReceived : New Implemention for UI Page change only and special event like Ongoing Payment process.
        public event EventHandler<DataReceivedEventArgs> OnDataReceived;

        public NetClientCollectTicketService(INetMediaInterface netInterface)
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
        /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        public void ResetUserSession(out bool isServerResponded, int waitDelaySec = 60)
        {
            isServerResponded = false;

            var uiReq = new UIReq<UIxCTResetUserSessionRequest>("*", _appModule, DateTime.Now, new UIxCTResetUserSessionRequest("*"));

            UIxCTResetUserSessionAck uIAck = Request<UIxCTResetUserSessionAck>(
                uiReq, "NetClientCollectTicketService.ResetUserSession", out bool isResponded, waitDelaySec);

            isServerResponded = isResponded;
        }

        public void ResetUserSessionSendOnly(string processId)
        {
            try
            {
                var uiReq = new UIReq<UIxCTResetUserSessionSendOnlyRequest>(processId, _appModule, DateTime.Now,
                    new UIxCTResetUserSessionSendOnlyRequest(processId));
                SendToServerOnly(uiReq, "NetClientSalesService.ResetUserSessionSendOnly");
            }
            catch { }
        }

        public NetServiceAnswerMan StartCollectTicket(string noResponseErrorMessage,
            FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle)
        {
            Guid netProcessId = Guid.Empty;
            NetServiceAnswerMan retMan = null;
            string runningTag = "COLTICK-Start-Collect-Ticket-(OPR0001)";
            int maxSvrReqTimeSec = 20;

            try
            {
                if (App.CollectBoardingPassCountDown.ChangeCountDown("NetClientCollectTicketService.StartCollectTicket", CollectTicketCountDown.ColTickCountDownCode.SERVER_REQUEST, maxSvrReqTimeSec + 5) == false)
                    throw new Exception("User access expired");

                _log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientCollectTicketService.StartCollectTicket");

                var uiReq = new UIReq<UIxCTStartCollectTicketRequest>("*", _appModule, DateTime.Now, new UIxCTStartCollectTicketRequest("*"));

                NetMessagePack msgPack = new NetMessagePack(uiReq) { DestinationPort = GetServerPort() };

                retMan = new NetServiceAnswerMan(msgPack, runningTag,
                    noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
                    _netInterface, _recvedNetProcIdTracker, processId: "*", waitDelaySec: maxSvrReqTimeSec, threadPriority: ThreadPriority.AboveNormal);

                return retMan;
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error when -{runningTag}-; (EXIT.OPR0001)", ex);
            }
        }

        public NetServiceAnswerMan SubmitLanguage(LanguageCode language, 
            string noResponseErrorMessage,
            FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle)
        {
            Guid netProcessId = Guid.Empty;
            NetServiceAnswerMan retMan = null;
            string runningTag = "COLTICK-Submit-Language-(OPR0001)";
            int maxSvrReqTimeSec = 180;

            try
            {
                if (App.CollectBoardingPassCountDown.ChangeCountDown("NetClientCollectTicketService.SubmitLanguage", CollectTicketCountDown.ColTickCountDownCode.SERVER_REQUEST, maxSvrReqTimeSec + 5) == false)
                    throw new Exception("User access expired");

                _log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientCollectTicketService.SubmitLanguage");

                var uiReq = new UIReq<UIxCTSubmitLanguageRequest>("*", _appModule, DateTime.Now, new UIxCTSubmitLanguageRequest("*", language));

                NetMessagePack msgPack = new NetMessagePack(uiReq) { DestinationPort = GetServerPort() };

                retMan = new NetServiceAnswerMan(msgPack, runningTag,
                    noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
                    _netInterface, _recvedNetProcIdTracker, processId: "*", waitDelaySec: maxSvrReqTimeSec, threadPriority: ThreadPriority.AboveNormal);

                return retMan;
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error when -{runningTag}-; (EXIT.OPR0001)", ex);
            }
        }

        public NetServiceAnswerMan SubmitBusCompany(string busCompanyLogoURL, string busCompanyCode, string busCompanyName,
            string noResponseErrorMessage,
            FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle)
        {
            Guid netProcessId = Guid.Empty;
            NetServiceAnswerMan retMan = null;
            string runningTag = "COLTICK-Submit-Bus-Company-(OPR0001)";
            int maxSvrReqTimeSec = 180;

            try
            {
                if (App.CollectBoardingPassCountDown.ChangeCountDown("NetClientCollectTicketService.SubmitBusCompany", CollectTicketCountDown.ColTickCountDownCode.SERVER_REQUEST, maxSvrReqTimeSec + 5) == false)
                    throw new Exception("User access expired");

                _log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientCollectTicketService.SubmitBusCompany");

                var uiReq = new UIReq<UIxCTSubmitBusCompanyRequest>("*", _appModule, DateTime.Now, new UIxCTSubmitBusCompanyRequest("*", busCompanyLogoURL, busCompanyCode, busCompanyName));

                NetMessagePack msgPack = new NetMessagePack(uiReq) { DestinationPort = GetServerPort() };

                retMan = new NetServiceAnswerMan(msgPack, runningTag,
                    noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
                    _netInterface, _recvedNetProcIdTracker, processId: "*", waitDelaySec: maxSvrReqTimeSec, threadPriority: ThreadPriority.AboveNormal);

                return retMan;
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error when -{runningTag}-; (EXIT.OPR0001)", ex);
            }
        }

        public NetServiceAnswerMan SubmitDepartureDate(DateTime departureDate,
            string noResponseErrorMessage,
            FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle)
        {
            Guid netProcessId = Guid.Empty;
            NetServiceAnswerMan retMan = null;
            string runningTag = "COLTICK-Submit-Departure-Date-(OPR0001)";
            int maxSvrReqTimeSec = 180;

            try
            {
                if (App.CollectBoardingPassCountDown.ChangeCountDown("NetClientCollectTicketService.SubmitDepartureDate", CollectTicketCountDown.ColTickCountDownCode.SERVER_REQUEST, maxSvrReqTimeSec + 5) == false)
                    throw new Exception("User access expired");

                _log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientCollectTicketService.SubmitDepartureDate");

                var uiReq = new UIReq<UIxCTSubmitDepartureDateRequest>("*", _appModule, DateTime.Now, new UIxCTSubmitDepartureDateRequest("*", departureDate));

                NetMessagePack msgPack = new NetMessagePack(uiReq) { DestinationPort = GetServerPort() };

                retMan = new NetServiceAnswerMan(msgPack, runningTag,
                    noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
                    _netInterface, _recvedNetProcIdTracker, processId: "*", waitDelaySec: maxSvrReqTimeSec, threadPriority: ThreadPriority.AboveNormal);

                return retMan;
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error when -{runningTag}-; (EXIT.OPR0001)", ex);
            }
        }

        public NetServiceAnswerMan SubmitTicketNo(string ticketNo,
            string noResponseErrorMessage,
            FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle)
        {
            Guid netProcessId = Guid.Empty;
            NetServiceAnswerMan retMan = null;
            string runningTag = "COLTICK-Submit Ticket No-(OPR0001)";
            int maxSvrReqTimeSec = 180;

            try
            {
                if (App.CollectBoardingPassCountDown.ChangeCountDown("NetClientCollectTicketService.SubmitTicketNo", CollectTicketCountDown.ColTickCountDownCode.SERVER_REQUEST, maxSvrReqTimeSec + 5) == false)
                    throw new Exception("User access expired");

                _log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientCollectTicketService.SubmitTicketNo");

                var uiReq = new UIReq<UIxCTSubmitTicketNoRequest>(ticketNo, _appModule, DateTime.Now, new UIxCTSubmitTicketNoRequest(ticketNo, ticketNo));

                NetMessagePack msgPack = new NetMessagePack(uiReq) { DestinationPort = GetServerPort() };

                retMan = new NetServiceAnswerMan(msgPack, runningTag,
                    noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
                    _netInterface, _recvedNetProcIdTracker, processId: ticketNo, waitDelaySec: maxSvrReqTimeSec, threadPriority: ThreadPriority.AboveNormal);

                return retMan;
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error when -{runningTag}-; (EXIT.OPR0001)", ex);
            }
        }

        public NetServiceAnswerMan SubmitPassengerInfo(string processId, string passengerName, string icPassportNo, string contactNo,
            string noResponseErrorMessage,
            FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle)
        {
            Guid netProcessId = Guid.Empty;
            NetServiceAnswerMan retMan = null;
            string runningTag = "COLTICK-Submit Passenger Info-(OPR0001)";
            int maxSvrReqTimeSec = 180;

            try
            {
                if (App.CollectBoardingPassCountDown.ChangeCountDown("NetClientCollectTicketService.SubmitPassengerInfo", CollectTicketCountDown.ColTickCountDownCode.SERVER_REQUEST, maxSvrReqTimeSec + 5) == false)
                    throw new Exception("User access expired");

                _log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientCollectTicketService.SubmitPassengerInfo");

                var uiReq = new UIReq<UIxCTSubmitPassengerInfoRequest>(processId, _appModule, DateTime.Now, new UIxCTSubmitPassengerInfoRequest(processId, passengerName, icPassportNo, contactNo));

                NetMessagePack msgPack = new NetMessagePack(uiReq) { DestinationPort = GetServerPort() };

                retMan = new NetServiceAnswerMan(msgPack, runningTag,
                    noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
                    _netInterface, _recvedNetProcIdTracker, processId: processId, waitDelaySec: maxSvrReqTimeSec, threadPriority: ThreadPriority.AboveNormal);

                return retMan;
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error when -{runningTag}-; (EXIT.OPR0001)", ex);
            }
        }

        public bool SubmitEWalletPayment(string processId,
            string paymentmethod, PaymentType typeOfPayment, string paymentRefNo,
            out boardingcollectticket_status payGateResult,
            out string errorMsg,
            out bool isServerResponded, int waitDelaySec = 60)
        {
            errorMsg = null;
            int maxSvrReqTimeSec = 180;
            string runningTag = "COLTICK-Submit EWallet Payment-(OPR0001)";

            try
            {
                if (App.CollectBoardingPassCountDown.ChangeCountDown("NetClientCollectTicketService.SubmitEWalletPayment", CollectTicketCountDown.ColTickCountDownCode.SERVER_REQUEST, maxSvrReqTimeSec + 5) == false)
                    throw new Exception("User access expired");

                isServerResponded = false;
                payGateResult = null;

                var uiReq = new UIReq<UIxSubmitCollectTicketPaymentRequest>("*", _appModule, DateTime.Now,
                    new UIxSubmitCollectTicketPaymentRequest(processId, paymentmethod, typeOfPayment, paymentRefNo));

                _log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientCollectTicketService.SubmitEWalletPayment");

                UIxGnBTnGAck<boardingcollectticket_status> uIAck = Request<UIxGnBTnGAck<boardingcollectticket_status>>(
                    uiReq, "NetClientCollectTicketService.SubmitEWalletPayment", out bool isResponded, waitDelaySec);

                isServerResponded = isResponded;

                if ((isResponded) && (uIAck?.Data?.code == 0))
                {
                    payGateResult = uIAck.Data;
                    return true;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(uIAck?.Error?.Message) == false)
                        errorMsg = uIAck.Error.Message;

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error when -{runningTag}-; (EXIT.OPR0001)", ex);
            }
        }

        public bool SubmitCashPayment(string processId,
            int cassette1NoteCount, int cassette2NoteCount, int cassette3NoteCount, int refundCoinAmount,
            out boardingcollectticket_status payGateResult,
            out string errorMsg,
            out bool isServerResponded, int waitDelaySec = 60)
        {
            errorMsg = null;
            int maxSvrReqTimeSec = 180;
            string runningTag = "COLTICK-Submit Cash Payment-(OPR0001)";

            try
            {
                if (App.CollectBoardingPassCountDown.ChangeCountDown("NetClientCollectTicketService.SubmitCashPayment", CollectTicketCountDown.ColTickCountDownCode.SERVER_REQUEST, maxSvrReqTimeSec + 5) == false)
                    throw new Exception("User access expired");

                isServerResponded = false;
                payGateResult = null;

                var uiReq = new UIReq<UIxSubmitCollectTicketPaymentRequest>("*", _appModule, DateTime.Now,
                    new UIxSubmitCollectTicketPaymentRequest(processId, cassette1NoteCount, cassette2NoteCount, cassette3NoteCount, refundCoinAmount));

                _log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientCollectTicketService.SubmitCashPayment");

                UIxGnBTnGAck<boardingcollectticket_status> uIAck = Request<UIxGnBTnGAck<boardingcollectticket_status>>(
                    uiReq, "NetClientCollectTicketService.SubmitEWalletPayment", out bool isResponded, waitDelaySec);

                isServerResponded = isResponded;

                if ((isResponded) && (uIAck?.Data?.code == 0))
                {
                    payGateResult = uIAck.Data;
                    return true;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(uIAck?.Error?.Message) == false)
                        errorMsg = uIAck.Error.Message;

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error when -{runningTag}-; (EXIT.OPR0001)", ex);
            }
        }

        /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
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

            _log?.LogText(_logChannel, pId, $@"Start - {processTag}", "A01", "NetClientCollectTicketService.Request");

            NetMessagePack msgPack = new NetMessagePack(sendKioskMsg) { DestinationPort = GetServerPort() };
            lastNetProcessId = msgPack.NetProcessId;

            _log?.LogText(_logChannel, pId,
                msgPack, "A05", $@"{processTag} => NetClientCollectTicketService.Request", extraMsg: "MsgObj: NetMessagePack");

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

            _log?.LogText(_logChannel, pId, kioskMsg, "A10", "NetClientCollectTicketService.Request", extraMsg: $@"MsgObj: {kioskMsg?.GetType().Name}");

            if (isServerResponded && (kioskMsg?.GetMsgData() is UIxTResult result))
            {
                return result;
            }
            else
            {
                _log?.LogText(_logChannel, pId, $@"Problem; isServerResponded : {isServerResponded}", "B01", "NetClientCollectTicketService.Request");
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

            _log.LogText(_logChannel, "-", $@"Start - {processTag}", "A01", "NetClientCollectTicketService.SendToServerOnly");

            NetMessagePack msgPack = new NetMessagePack(sendKioskMsg) { DestinationPort = GetServerPort() };
            lastNetProcessId = msgPack.NetProcessId;

            _log.LogText(_logChannel, "-",
                msgPack, "A05", $@"{processTag} => NetClientCollectTicketService.SendToServerOnly", extraMsg: "MsgObject: NetMessagePack");

            _netInterface.SendMsgPack(msgPack);
        }

        private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.ReceivedData?.Module == AppModule.UICollectTicket)
            {
                _recvedNetProcIdTracker.AddNetProcessId(e.ReceivedData.NetProcessId, e.ReceivedData?.MsgObject);
                RaiseOnDataReceived(sender, e);
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.1301
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
                _log.LogError(_logChannel, "-", new Exception("Unhandled event exception; (EXIT80.1301.EX01)", ex), "EX01", "NetClientCollectTicketService.RaiseOnDataReceived", netProcessId: e?.ReceivedData?.NetProcessId);
            }
        }
    }
}