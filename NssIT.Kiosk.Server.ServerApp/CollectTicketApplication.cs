using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Server.ServerApp.CustomApp;
using NssIT.Kiosk.Server.ServerApp.CustomApp.CollectTicketApp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp
{
    public class CollectTicketApplication : IUIApplicationJob, IDisposable
    {
        private const string LogChannel = "ServerApplication";

        private int _standardCountDownIntervalSec = 30;
        private int _custInfoEntryIntervalSec = 60;

        //-------------------------------------------------------------

        private int _maxProcessPeriodSec = 180;
        private bool _disposed = false;

        private UserSession _session = new UserSession();

        private IServerAppPlan _svrAppPlan = null;
        private AppCallBackDelg _appCallBackHandle = null;
        private IAppAccessCallBackPlan _appCallBackPlan = null;

        //private List<Guid> _sessionNetProcessIDList = new List<Guid>();

        private SessionSupervisor _SessionSuper = new SessionSupervisor();

        public event EventHandler<UIMessageEventArgs> OnShowResultMessage;

        private DbLog _log = null;
        private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

        public CollectTicketApplication(AppGroup appGroup)
        {
            _appCallBackHandle = new AppCallBackDelg(AccessCallBackDelgWorking);
            _appCallBackPlan = new MelTicketCollectionApp_AccessCallBackPlan(this);
            _svrAppPlan = new MelTicketCollectionAppPlan();
        }

        private string _currProcessId = "-";
        public string CurrentProcessId
        {
            get => _currProcessId;
            private set => _currProcessId = string.IsNullOrEmpty(value) ? "-" : value.Trim();
        }

        private Setting _sysSetting = null;
        private Setting SysSetting
        {
            get
            {
                return _sysSetting ?? (_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting());
            }
        }

        public async void AccessCallBackDelgWorking(UIxKioskDataAckBase accessResult)
        {
            string processId = string.IsNullOrWhiteSpace(accessResult?.BaseProcessId) ? "*" : accessResult?.BaseProcessId;

            try
            {
                if (accessResult.BaseRefNetProcessId.HasValue == false)
                    throw new Exception($@"Unable to read NetProcessId from '{accessResult.GetType().Name}'");

                await _appCallBackPlan.DeliverAccessResult(accessResult);
            }
            catch (Exception ex)
            {
                string pTypeStr = (accessResult is null) ? "-" : accessResult.GetType().FullName;
                Log?.LogError(LogChannel, processId, new WithDataException($@"{ex.Message}; Parameter Type: {pTypeStr}", ex, accessResult), "EX01", "CollectTicketApplication.AccessCallBackDelgWorking");
            }
        }

        private SemaphoreSlim _asyncSendLock = new SemaphoreSlim(1);
        public async Task<bool> SendInternalCommand(string processId, Guid? netProcessId, IKioskMsg svcMsg)
        {
            bool lockSuccess = false;
            Guid instanceProcId = Guid.NewGuid();

            try
            {
                lockSuccess = await _asyncSendLock.WaitAsync(5 * 60 * 1000);

                if (lockSuccess == false)
                {
                    Log.LogText(LogChannel, "-", svcMsg, "X05", "CollectTicketApplication.SendInternalCommand", AppDecorator.Log.MessageType.Error, netProcessId: netProcessId, extraMsg: $@"lockSuccess: {lockSuccess}; instanceProcId: {instanceProcId}");
                    return false;
                }
                else
                    Log.LogText(LogChannel, "-", svcMsg, "A05", "CollectTicketApplication.SendInternalCommand", netProcessId: netProcessId, extraMsg: $@"lockSuccess: {lockSuccess}; instanceProcId: {instanceProcId}");

                Log.LogText(LogChannel, "-", svcMsg, "A01", "CollectTicketApplication.SendInternalCommand", netProcessId: netProcessId,
                    extraMsg: $@"Start - SendInternalCommand; MsgObj: {svcMsg.GetType().ToString()}");

                // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // Pre-Processing xx
                // xxxxxxxxxxxxxxxxx

                // Reset Session Supervisor
                if ((svcMsg is UIReq<UIxCTStartCollectTicketRequest>) 
                    || (svcMsg is UIReq<UIxCTResetUserSessionRequest>)
                    || (svcMsg is UIReq<UIxCTResetUserSessionSendOnlyRequest>)
                    )
                {
                    _svrAppPlan.ResetCleanUp();
                    _SessionSuper.CleanNetProcessId();
                    _SessionSuper.AddNetProcessId(netProcessId.Value);

                    if (_showResultMessageLock.CurrentCount == 0)
                        _showResultMessageLock.Release();
                }
                else if (svcMsg is IGnReq)
                {
                    _SessionSuper.AddNetProcessId(netProcessId.Value);
                }
                // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // Update Session
                _session = _svrAppPlan.UpdateUserSession(_session, svcMsg);

                // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // Process svcMsg
                IKioskMsg resultKioskMsg = _svrAppPlan.DoProcess(svcMsg.ProcessId, svcMsg.RefNetProcessId, svcMsg, _session, _appCallBackHandle, out _);
                
                // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // Send/Raise OnShowResultMessage Event
                if (resultKioskMsg != null)
                    RaiseOnShowResultMessage(netProcessId, resultKioskMsg);
                
                // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                Log.LogText(LogChannel, _currProcessId, $@"Done - SendInternalCommand; Net Process Id:{netProcessId}", $@"A03", classNMethodName: "ServerSalesApplication.SendInternalCommand");
            }
            catch (Exception ex)
            {
                Log.LogText(LogChannel, processId, svcMsg, "EX01", "ServerSalesApplication.SendInternalCommand", AppDecorator.Log.MessageType.Error, extraMsg: "MsgObj: IKioskMsg", netProcessId: netProcessId);
                Log.LogError(LogChannel, processId, ex, "E02", "ServerSalesApplication.SendInternalCommand", netProcessId: netProcessId);
            }
            finally
            {
                if ((lockSuccess == true) && (_asyncSendLock.CurrentCount == 0))
                {
                    _asyncSendLock.Release();
                    Log.LogText(LogChannel, "-", $@"End - SendInternalCommand; instanceProcId: {instanceProcId}", "K100", "ServerSalesApplication.SendInternalCommand", netProcessId: netProcessId);
                }
                else
                {
                    Log.LogText(LogChannel, "-", $@"End - SendInternalCommand; instanceProcId: {instanceProcId}", "XK101", "ServerSalesApplication.SendInternalCommand", AppDecorator.Log.MessageType.Error, netProcessId: netProcessId);
                }
            }

            return true;

            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void RedirectDataToClient(string procId, Guid? netProcId, IKioskMsg kioskMsg)
            {
                MessageType msgTyp = string.IsNullOrWhiteSpace(kioskMsg.ErrorMessage) ? MessageType.NormalType : MessageType.ErrorType;
                RaiseOnShowResultMessage(netProcId, kioskMsg, msgTyp);
            }
        }

        private TimeSpan _maxWaitSendMsg = new TimeSpan(0, 0, 20);
        private SemaphoreSlim _showResultMessageLock = new SemaphoreSlim(1);
        /// <summary>
        /// </summary>
        /// <param name="netProcessId"></param>
        /// <param name="kioskMsg">Must has a valid object in order to return a result to UI.</param>
        /// <param name="msgType"></param>
        /// <param name="message">Normally this is 2nd error message. 1st error message is in returnObj.ErrorMessage</param>
        public void RaiseOnShowResultMessage(Guid? netProcessId, IKioskMsg kioskMsg, MessageType? msgType = null, string message = null)
        {
            Guid _workToken = Guid.NewGuid();
            bool lockSuccess = false;
            try
            {
                lockSuccess = _showResultMessageLock.WaitAsync(_maxWaitSendMsg).GetAwaiter().GetResult();

                if (lockSuccess == false)
                    return;

                if (kioskMsg != null)
                {
                    bool proceed = false;

                    MessageType msgTy = (msgType.HasValue == false) ? MessageType.NormalType : msgType.Value;

                    if (_session.SessionId.Equals(Guid.Empty))
                    {
                        proceed = true;
                    }
                    else if (_session.Expired == false)
                    {
                        if (_SessionSuper.FindNetProcessId(netProcessId.Value))
                        {
                            proceed = true;
                        }
                        else
                        {
                            // Note : This mean netProcessId is refer to previous session and already expired.
                            proceed = false;
                        }
                    }
                    //--------------------------------------------------------------
                    if (proceed)
                    {
                        //----------------------------------------------------------
                        // Update Session info into kioskMsg
                        if (kioskMsg is IUserSession sess)
                            sess.UpdateSession(_session);
                        //----------------------------------------------------------

                        if ((string.IsNullOrWhiteSpace(kioskMsg.ErrorMessage)) && (msgTy == MessageType.NormalType || msgTy == MessageType.UnknownType))
                            OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.NormalType });
                        else
                            OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.ErrorType });
                    }
                }
                else
                {
                    message = (string.IsNullOrWhiteSpace(message) == true) ? "Result not available. (EXIT21318)" : message;
                    OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = null, MsgType = MessageType.ErrorType });
                }
            }
            catch (Exception ex)
            {
                if (kioskMsg != null)
                    Log.LogError(LogChannel, kioskMsg.ProcessId, new Exception($@"Unhandle event error OnShowCustomerMessage. Net Process Id: {netProcessId}", ex), "E01", "CollectTicketApplication.ShowCustomerMessage");
                else
                    Log.LogError(LogChannel, "-", new Exception($@"Unhandle event error OnShowCustomerMessage. Net Process Id: {netProcessId}", ex), "E02", "CollectTicketApplication.ShowCustomerMessage");
            }
            finally
            {
                if (lockSuccess && (_showResultMessageLock.CurrentCount == 0))
                    _showResultMessageLock.Release();
            }
        }

        public bool ShutDown()
        {
            _disposed = true;

            if (OnShowResultMessage != null)
            {
                Delegate[] delgList = OnShowResultMessage.GetInvocationList();
                foreach (EventHandler<UIMessageEventArgs> delg in delgList)
                {
                    OnShowResultMessage -= delg;
                }
            }

            return true;
        }

        public void Dispose()
        {
            _disposed = true;

            if (OnShowResultMessage != null)
            {
                Delegate[] delgList = OnShowResultMessage.GetInvocationList();
                foreach (EventHandler<UIMessageEventArgs> delg in delgList)
                {
                    OnShowResultMessage -= delg;
                }
            }

            /////_log = null;
        }
    }
}
