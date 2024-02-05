using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.Ticketing.UIx;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Server.AccessDB;
using NssIT.Kiosk.Server.ServerApp.CustomApp;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp
{
    public class ServerSalesApplication : IUIApplicationJob, IDisposable
    {
        private int _standardCountDownIntervalSec = 30;
        private int _custInfoEntryIntervalSec = 60;

        //CYA-DEBUG -- DEMO ONLY
        //private int _standardCountDownIntervalSec = 30 * 10;
        //private int _custInfoEntryIntervalSec = 60 * 3;
        //-------------------------------------------------------------

        private int _maxProcessPeriodSec = 180;

        private const string LogChannel = "ServerApplication";

        private bool _disposed = false;

        private UserSession _session = new UserSession();
        private ServerAccess _svrAccess = null;
        private AppCountDown _appCountDown = null;
        private IServerAppPlan _svrAppPlan = null;

        //private List<Guid> _sessionNetProcessIDList = new List<Guid>();

        private SessionSupervisor _SessionSuper = new SessionSupervisor();

        public event EventHandler<UIMessageEventArgs> OnShowResultMessage;

        public ServerSalesApplication(AppGroup appGroup)
        {
            _svrAccess = ServerAccess.GetAccessServer();
            _appCountDown = new AppCountDown(this);

            if ((appGroup == AppGroup.Larkin) || (appGroup == AppGroup.Klang))
                _svrAppPlan = new LarkinAppPlan();

            else if (appGroup == AppGroup.Gombak)
                _svrAppPlan = new MelakaSentralAppPlan();
            else if(appGroup == AppGroup.Genting)
                _svrAppPlan = new GentingAppPlan(); 
            else
                _svrAppPlan = new MelakaSentralAppPlan();

            _svrAccess.OnSendMessage += _b2bAccess_OnSendMessage;

        }

        private DbLog _log = null;
        private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

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

        private void _b2bAccess_OnSendMessage(object sender, SendMessageEventArgs e)
        {
            if (e.Module != AppDecorator.Common.AppService.AppModule.UIKioskSales)
                return;

            if (_disposed)
            {
                Log.LogText(LogChannel, _currProcessId, $@"Application server has shutdowned. Net Process Id: {e?.NetProcessId};",
                    $@"A01", classNMethodName: "ServerSalesApplication._b2bAccess_OnSendMessage");
                return;
            }

            Log.LogText(LogChannel, _currProcessId, e, $@"A02", classNMethodName: "ServerSalesApplication._b2bAccess_OnSendMessage",
                extraMsg: $@"Start - _b2bAccess_OnCompleted; Net Process Id:{e?.NetProcessId}; MsgObj: SendMessageEventArgs");

            // e.EventMessageObj must has valid for returning a result.
            if (e.KioskMessage != null)
            {
                if (e.KioskMessage is UIWebServerLogonStatusAck uiLogonStt)
                {
                    SendInternalCommand(uiLogonStt.ProcessId, uiLogonStt.RefNetProcessId, uiLogonStt);
                }
                else if (e.KioskMessage is UIOriginListAck uiOriginList)
                {
                    SendInternalCommand(uiOriginList.ProcessId, uiOriginList.RefNetProcessId, uiOriginList);
                }
                else if (e.KioskMessage is UIDestinationListAck uiDestList)
                {
                    SendInternalCommand(uiDestList.ProcessId, uiDestList.RefNetProcessId, uiDestList);
                }
                else if (e.KioskMessage is UIDepartTripListAck uiTripList)
                {
                    SendInternalCommand(uiTripList.ProcessId, uiTripList.RefNetProcessId, uiTripList);
                }
                else if (e.KioskMessage is UIDepartSeatListAck uiSeatList)
                {
                    SendInternalCommand(uiSeatList.ProcessId, uiSeatList.RefNetProcessId, uiSeatList);
                }
                else if (e.KioskMessage is UIDepartSeatConfirmResult uiResult)
                {
                    SendInternalCommand(uiResult.ProcessId, uiResult.RefNetProcessId, uiResult);
                }
                else if (e.KioskMessage is UIDepartCustInfoUpdateResult uiCIUpdResult)
                {
                    SendInternalCommand(uiCIUpdResult.ProcessId, uiCIUpdResult.RefNetProcessId, uiCIUpdResult);
                }
                else if (e.KioskMessage is UICompleteTransactionResult uiCompltResult)
                {
                    SendInternalCommand(uiCompltResult.ProcessId, uiCompltResult.RefNetProcessId, uiCompltResult);
                }
                else if (e.KioskMessage is UIDepartSeatListErrorResult uiSeatErrResult)
                {
                    SendInternalCommand(uiSeatErrResult.ProcessId, uiSeatErrResult.RefNetProcessId, uiSeatErrResult);
                }
                else
                {
                    //CYA-DEBUG -- Need to avoid running this area. -- Pending to remove
                    MessageType msgTyp = string.IsNullOrWhiteSpace(e.KioskMessage.ErrorMessage) ? MessageType.NormalType : MessageType.ErrorType;
                    RaiseOnShowResultMessage(e.NetProcessId, e.KioskMessage, msgTyp);
                }
            }
            else
                RaiseOnShowResultMessage(e.NetProcessId, null, MessageType.ErrorType, e.Message);

            Log.LogText(LogChannel, _currProcessId, $@"End - _b2bAccess_OnSendMessage; Net Process Id:{e?.NetProcessId}", "A10", classNMethodName: "ServerSalesApplication._b2bAccess_OnSendMessage");
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
                    Log.LogText(LogChannel, "-", svcMsg, "X05", "ServerSalesApplication.SendInternalCommand", AppDecorator.Log.MessageType.Error, netProcessId: netProcessId, extraMsg: $@"lockSuccess: {lockSuccess}; instanceProcId: {instanceProcId}");
                    return false;
                }
                else
                    Log.LogText(LogChannel, "-", svcMsg, "A05", "ServerSalesApplication.SendInternalCommand", netProcessId: netProcessId, extraMsg: $@"lockSuccess: {lockSuccess}; instanceProcId: {instanceProcId}");

                Log.LogText(LogChannel, "-", svcMsg, "A01", "ServerSalesApplication.SendInternalCommand", netProcessId: netProcessId,
                    extraMsg: $@"Start - SendInternalCommand; MsgObj: {svcMsg.GetType().ToString()}");

                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
                // Setup Session & Count Down
                {
                    if (svcMsg.Instruction == (CommInstruction)UISalesInst.CountDownStartRequest)
                    {
                        if (netProcessId.HasValue == false)
                            throw new Exception("Fail to start CountDown; NetProcessID Not found; (EXIT21321)");

                        _session.NewSession(netProcessId.Value);

                        _appCountDown.SetNewCountDown(_maxProcessPeriodSec, _session.SessionId);
                        _SessionSuper.CleanNetProcessId();
                        _SessionSuper.AddNetProcessId(netProcessId.Value);
                    }
                    else if (svcMsg is UIReq<UIxResetUserSessionSendOnlyRequest>)
                    {
                        _appCountDown.Abort();
                        _session.SessionReset();
                        _SessionSuper.CleanNetProcessId();
                    }
                    else if (
                            (svcMsg.Instruction == (CommInstruction)UISalesInst.ServerApplicationStatusRequest)
                            || (svcMsg.Instruction == (CommInstruction)UISalesInst.WebServerLogonRequest)
                            || (svcMsg.Instruction == (CommInstruction)UISalesInst.WebServerLogonStatusAck)
                            )
                    {
                        /*By Pass*/
                    }
                    else if (svcMsg is UITimeoutChangeRequest)
                    {
                        /*By Pass*/
                    }
                    else if ((_session.SessionId.Equals(Guid.Empty) == false))
                    {
                        if ((_session.Expired == false))
                        {
                            if (_appCountDown.UpdateCountDown(_maxProcessPeriodSec, _session.SessionId, isMandatoryExtensionChange: false, out bool isMatchedSession1) == false)
                            {
                                if (isMatchedSession1 == false)
                                    throw new Exception($@"Unable to update Count Down; (EXIT21330); Current App Session ID {_session.SessionId.ToString("D")} is not matched with count down.");
                                else
                                    throw new Exception($@"Unable to update Count Down; (EXIT21331); Session ID {_session.SessionId.ToString("D")} already expired.");
                            }
                            else
                            {
                                _SessionSuper.AddNetProcessId(netProcessId.Value);
                            }
                        }
                        else
                            throw new Exception($@"Unable to update Count Down; (EXIT21332); Session ID {_session.SessionId.ToString("D")} already expired.");
                    }
                }
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

                if (svcMsg.Instruction == (CommInstruction)UISalesInst.ServerApplicationStatusRequest)
                    GetServerStatus(processId, netProcessId);

                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

                else if (svcMsg.Instruction == (CommInstruction)UISalesInst.WebServerLogonRequest)
                    ReLogon(processId, netProcessId);

                else if (svcMsg.Instruction == (CommInstruction)UISalesInst.WebServerLogonStatusAck)
                    RedirectDataToClient(processId, netProcessId, svcMsg);

                else if (svcMsg is UIReq<UIxResetUserSessionSendOnlyRequest>)
                {
                    /*By Pass*/
                    string ttx1 = "*";
                }
                else
                {
                    // Below is User Session Processing // Note : For non-user data process, do not put on the below. 

                    if (_session.SessionId.Equals(Guid.Empty))
                        throw new Exception("Empty (EXIT21322).");

                    else if (_session.Expired)
                        throw new Exception("Session Expired (EXIT21323).");

                    if (svcMsg is UIDetailEditRequest uiEdit)
                    {
                        _session = _svrAppPlan.SetEditingSession(_session, uiEdit.EditItemCode);
                    }
                    else if (svcMsg is UIPageNavigateRequest uiPgNav)
                    {
                        _session = _svrAppPlan.SetUIPageNavigateSession(_session, uiPgNav);
                    }
                    else
                        _session = _svrAppPlan.UpdateUserSession(_session, svcMsg);

                    Log.LogText(LogChannel, _session.SessionId.ToString("D"), _session, "SESSION_1", "ServerSalesApplication.SendInternalCommand",
                        extraMsg: $@"Inst: {svcMsg.Instruction}; Inst.Desc.:{svcMsg.InstructionDesc} ;MsgObj: UserSession");

                    UISalesInst inst = _svrAppPlan.NextInstruction(processId, netProcessId, svcMsg, _session, out bool releaseSeatRequestOnEdit);

                    if (releaseSeatRequestOnEdit)
                    {
                        ReleaseSeatOnEdit(processId, netProcessId, _session);
                    }

                    if (inst == UISalesInst.LanguageSelectionAck)
                    {
                        _session.CurrentEditMenuItemCode = null;
                        SelectLanguageSendAck(processId, netProcessId);
                    }

                    else if (inst == UISalesInst.TimeoutChangeRequest)
                    {
                        ChangeTimeout(_session, (UITimeoutChangeRequest)svcMsg);
                    }

                    //GetOriginList

                    else if (inst == UISalesInst.OriginListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.FromStation;
                        GetOriginList(processId, netProcessId);
                    }

                    else if (inst == UISalesInst.DestinationListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ToStation;
                        GetDestinationList(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.TravelDatesEnteringAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartDate;
                        SelectTravelDatesSendAck(processId, netProcessId);
                    }

                    else if (inst == UISalesInst.DepartTripListInitAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartOperator;
                        InitDepartTripSendAck(processId, netProcessId, _session.DepartPassengerDate.Value);
                    }

                    else if (inst == UISalesInst.DepartTripListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartOperator;
                        GetDepartTripList(processId, netProcessId, _session.DepartPassengerDate.Value, _session.OriginStationCode, _session.DestinationStationCode);
                    }

                    else if (inst == UISalesInst.DepartTripSubmissionErrorAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartOperator;
                        DepartTripSubmissionErrorAck(processId, netProcessId, svcMsg.ErrorMessage);
                    }

                    else if (inst == UISalesInst.DepartSeatListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                        GetDepartSeatList(processId, netProcessId, _session.DepartPassengerDate.Value, _session.DepartTripId, _session.DepartTripNo, _session.DepartVehicleTripDate,
                            _session.DepartPassengerActualFromStationsCode, _session.DepartPassengerActualToStationsCode, _session.DepartTimeposi);
                    }

                    else if (inst == UISalesInst.DepartPickupNDropAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                        SelectPickupNDropSendAck(processId, netProcessId, _session.DepartPickupNDropList);
                    }

                    else if (inst == UISalesInst.InsuranceAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                        SelectInsuranceAck(processId, netProcessId);
                    }

                    else if (inst == UISalesInst.DepartSeatConfirmRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                        DepartConfirmSeat(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.DepartSeatConfirmFailAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                        DepartConfirmSeatFailSendAck(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.CustInfoAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        PassengerInfoEntrySendAck(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.CustInfoUpdateELSEReleaseSeatRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        UpdateCustInfoUpdateV2(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.CustInfoUpdateFailAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        CustInfoUpdateFailSendAck(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.SalesPaymentProceedAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Payment;
                        MakeSalesPaymentSendAck(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.CompleteTransactionElseReleaseSeatRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.AfterPayment;
                        CompleteTransactionRequest(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.SeatReleaseRequest)
                    {
                        ReleaseSeat(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.RedirectDataToClient)
                    {
                        if (svcMsg.Instruction == (CommInstruction)UISalesInst.CompleteTransactionResult)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.AfterPayment;
                        }

                        RedirectDataToClient(processId, netProcessId, svcMsg);
                    }

                    else if (inst == UISalesInst.CountDownPausedRequest)
                    {
                        PauseCountDown(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.CountDownExpiredAck)
                    {
                        TimeoutSession(processId, netProcessId, _session);
                    }

                    else
                    {
                        Log.LogText(LogChannel, processId,
                            svcMsg,
                            $@"EXXX01",
                            classNMethodName: "ServerSalesApplication.SendInternalCommand",
                            messageType: AppDecorator.Log.MessageType.Error,
                            extraMsg: $@"Unregconized Instruction Code; Net Process Id: {netProcessId}; Next Sales Instruction: {Enum.GetName(typeof(UISalesInst), inst)}; MsgObj: IUISvcMsg");

                        bool ret1 = _appCountDown.UpdateCountDown(_maxProcessPeriodSec, _session.SessionId, isMandatoryExtensionChange: false, out bool isMatchedSession2);
                    }
                }

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

            void GetServerStatus(string procId, Guid? netProcId)
            {
                UIServerApplicationStatusAck stt = null;
                try
                {
                    _svrAccess.QueryServerStatus(out bool isServerDisposed, out bool isServerShutdown, out bool isBusyDetectingWebService, out bool isWebServiceDetected);
                    stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, isServerDisposed, isServerShutdown, isWebServiceDetected, SysSetting.ApplicationGroup, 
                        SysSetting.ApplicationVersion, SysSetting.AvailablePaymentTypeList, SysSetting.IsBoardingPassEnabled);
                    RaiseOnShowResultMessage(netProcId, stt, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    string err = $@"{ex.Message}; (EXIT21305)";
                    stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, false, false, false, SysSetting.ApplicationGroup, 
                        SysSetting.ApplicationVersion, SysSetting.AvailablePaymentTypeList, SysSetting.IsBoardingPassEnabled) 
                            { ErrorMessage = err };
                    Log.LogError(LogChannel, processId, new Exception(err, ex), "E01", "ServerSalesApplication.GetServerStatus");
                    RaiseOnShowResultMessage(netProcId, stt, MessageType.ErrorType, err);
                }
            }

            void DepartTripSubmissionErrorAck(string procId, Guid? netProcId, string errorMessage)
            {
                try
                {
                    IKioskMsg msg = new UIDepartTripSubmissionErrorAck(netProcId, procId, DateTime.Now, errorMessage);
                    RaiseOnShowResultMessage(netProcId, msg, MessageType.ErrorType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, ex, "E01", "ServerSalesApplication.DepartTripSubmissionErrorAck");
                }
            }

            void ReLogon(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21313);");

                    LogonCommand command = new LogonCommand(procId, netProcId);

                    Log.LogText(LogChannel, procId, $@"Start - ReLogon; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.ReLogon");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21314).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Logon again). " + ex.Message);
                }
            }

            void SelectLanguageSendAck(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21315);");

                    UILanguageSelectionAck lang = new UILanguageSelectionAck(netProcId, procId, DateTime.Now);
                    RaiseOnShowResultMessage(netProcId, lang, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.SelectLanguage");
                }
            }

            void GetDestinationList(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21311);");

                    DestinationListRequestCommand command = new DestinationListRequestCommand(procId, netProcId, session.OriginStationCode);

                    Log.LogText(LogChannel, procId, $@"Start - GetDestinationList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetDestinationList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21312).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Destination List). " + ex.Message);
                }
            }

            void GetOriginList(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21311);");

                    OriginListRequestCommand command = new OriginListRequestCommand(procId, netProcId);

                    Log.LogText(LogChannel, procId, $@"Start - GetDestinationList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetDestinationList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21312).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Destination List). " + ex.Message);
                }
            }

            void InitDepartTripSendAck(string procId, Guid? netProcId, DateTime tripDate)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21345);");

                    UIDepartTripInitAck tripInit = new UIDepartTripInitAck(netProcId, procId, DateTime.Now, AppDecorator.Common.TravelMode.DepartOnly);
                    RaiseOnShowResultMessage(netProcId, tripInit, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.InitDepartTrip");
                }
            }

            void GetDepartTripList(string procId, Guid? netProcId, DateTime tripDate, string fromStationCode, string toStationCode)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21341);");

                    DepartTripListCommand command = new DepartTripListCommand(procId, netProcId, tripDate, fromStationCode, toStationCode);

                    Log.LogText(LogChannel, procId, $@"Start - GetDepartTripList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetDepartTripList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21342).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Depart Trip List). " + ex.Message);
                }
            }

            void GetDepartSeatList(string procId, Guid? netProcId,
                DateTime departPassengerDate, string departTripId, string departTripNo, string departVehicleTripDate,
                string departFromStationCode, string departToStationCode, short departTimePosi)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21346);");

                    DepartSeatListCommand command = new DepartSeatListCommand(procId, netProcId,
                        departPassengerDate, departTripId, departTripNo, departPassengerDate.ToString("dd/MM/yyyy"), departVehicleTripDate,
                        departFromStationCode, departToStationCode, departTimePosi);

                    Log.LogText(LogChannel, procId, $@"Start - GetDepartSeatList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetDepartSeatList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21347).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Depart Seat List). " + ex.Message);
                }
            }

            void SelectTravelDatesSendAck(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21316);");

                    UITravelDatesEnteringAck tvDates = new UITravelDatesEnteringAck(netProcId, procId, DateTime.Now);
                    RaiseOnShowResultMessage(netProcId, tvDates, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.SelectTravelDates");
                }
            }

            void SelectPickupNDropSendAck(string procId, Guid? netProcId, PickupNDropList pickupNDropList)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21316);");

                    UIDepartPickupNDropAck uiPnD = new UIDepartPickupNDropAck(netProcId, procId, DateTime.Now, pickupNDropList);
                    RaiseOnShowResultMessage(netProcId, uiPnD, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.SelectPickupNDrop");
                }
            }

            void DepartConfirmSeat(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21346);");

                    DepartSeatConfirmCommand command = new DepartSeatConfirmCommand(procId, netProcId,
                        session.DepartTripId,
                        session.DepartVehicleTripDate,
                        (session.DepartPassengerDate.HasValue ? session.DepartPassengerDate.Value.ToString("dd/MM/yyyy") : "01/01/1900"),
                        session.DepartPassengerDepartTime,
                        session.DepartBusType,
                        session.DepartPassengerActualFromStationsCode,
                        session.DepartPassengerActualToStationsCode,
                        session.DepartAdultPrice,
                        session.DepartAdultExtra,
                        session.DepartAdultDisc,
                        session.DepartTerminalCharge,
                        session.DepartOnlineQrCharge,
                        session.DepartInsurance,
                        session.DepartTotalAmount,
                        session.DepartTripCode,
                        session.DepartPick,
                        session.DepartPickDesn,
                        session.DepartPickTime,
                        session.DepartDrop,
                        session.DepartDropDesn,
                        session.PassengerSeatDetailList);

                    Log.LogText(LogChannel, procId, $@"Start - GetDepartSeatList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetDepartSeatList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21347).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Depart Seat List). " + ex.Message);
                }
            }

            void SelectInsuranceAck(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21367);");

                    UIInsuranceAck uiInsurn = new UIInsuranceAck(netProcId, procId, DateTime.Now);
                    RaiseOnShowResultMessage(netProcId, uiInsurn, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.PassengerInfoEntry");
                }
            }

            void DepartConfirmSeatFailSendAck(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21316);");

                    UIDepartSeatConfirmFailAck uiFail = new UIDepartSeatConfirmFailAck(netProcId, procId, DateTime.Now);
                    RaiseOnShowResultMessage(netProcId, uiFail, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.SelectPickupNDrop");
                }
            }

            void PassengerInfoEntrySendAck(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21317);");

                    // Note : Will set to 0.00 because 
                    CustomerInfoList custInfoList = new CustomerInfoList(session.PassengerSeatDetailList);

                    UICustInfoAck uiCust = new UICustInfoAck(netProcId, procId, DateTime.Now, custInfoList);
                    RaiseOnShowResultMessage(netProcId, uiCust, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.PassengerInfoEntry");
                }
            }

            void UpdateCustInfoUpdateV2(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21351);");

                    string departDate = (session.DepartPassengerDate.HasValue) ? session.DepartPassengerDate.Value.ToString("dd/MM/yyyy") : "01/01/1900";

                    DepartCustInfoUpdateELSEReleaseSeatCommand command =
                        new DepartCustInfoUpdateELSEReleaseSeatCommand(procId, netProcId, session.PassengerSeatDetailList,
                        session.DepartSeatConfirmTransNo, session.DepartVehicleTripDate, departDate);

                    Log.LogText(LogChannel, procId, $@"Start - UpdateCustInfoUpdateV2; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.UpdateCustInfoUpdateV2");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21352).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Depart Seat List). " + ex.Message);
                }
            }

            void CustInfoUpdateFailSendAck(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21356);");

                    UICustInfoUpdateFailAck uiFail = new UICustInfoUpdateFailAck(netProcId, procId, DateTime.Now, "Unabled to update passenger info");
                    RaiseOnShowResultMessage(netProcId, uiFail, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.CustInfoUpdateFailSendAck");
                }
            }

            void MakeSalesPaymentSendAck(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21356);");

                    //SalesPaymentData salesData = new SalesPaymentData() { TransactionNo = session.DepartSeatConfirmTransNo, Amount = session.DepartTotalAmount };

                    UISalesPaymentProceedAck uiPay = new UISalesPaymentProceedAck(netProcId, procId, DateTime.Now);
                    RaiseOnShowResultMessage(netProcId, uiPay, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.MakeSalesPaymentSendAck");
                }
            }

            void ReleaseSeatOnEdit(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21357);");

                    if (string.IsNullOrWhiteSpace(session.DepartPendingReleaseTransNo))
                        throw new Exception("No Transaction Number found when release seat (EXIT21358);");

                    TicketReleaseCommand command = new TicketReleaseCommand(procId, netProcId, session.DepartPendingReleaseTransNo);

                    Log.LogText(LogChannel, procId, $@"Start - ReleaseSeat; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.ReleaseSeat");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21359).");
                    }
                }
                catch (Exception ex)
                {
                    //RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Release Seat). " + ex.Message);
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ReleaseSeat");
                }
            }

            void CompleteTransactionRequest(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21360);");

                    CompleteTransactionElseReleaseSeatCommand command = new CompleteTransactionElseReleaseSeatCommand(procId, netProcId,
                        session.DepartSeatConfirmTransNo, session.DepartTotalAmount,
                        session.Cassette1NoteCount, session.Cassette2NoteCount, session.Cassette3NoteCount, session.RefundCoinAmount, 
                        session.TypeOfPayment, session.PaymentMethodCode, session.PaymentRefNo);

                    Log.LogText(LogChannel, procId, $@"Start - CompleteTransactionRequest; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.CompleteTransactionRequest");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21361).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Completing Transaction Request). " + ex.Message);
                }
            }

            void ChangeTimeout(UserSession session, UITimeoutChangeRequest changeReq)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21370);");

                    if (changeReq.ChangeMode == TimeoutChangeMode.ResetNormalTimeout)
                        _appCountDown.ClientChangeTimeout(changeReq.ChangeMode, _standardCountDownIntervalSec, session.SessionId);

                    else if (changeReq.ChangeMode == TimeoutChangeMode.MandatoryExtension)
                        _appCountDown.ClientChangeTimeout(changeReq.ChangeMode, changeReq.ExtensionTimeSec, session.SessionId);

                    else if (changeReq.ChangeMode == TimeoutChangeMode.RemoveMandatoryTimeout)
                        _appCountDown.ClientChangeTimeout(changeReq.ChangeMode, 0, session.SessionId);

                    string tt1 = "";
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ChangeTimeout");
                }
            }

            void ReleaseSeat(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21362);");

                    if (string.IsNullOrWhiteSpace(session.DepartPendingReleaseTransNo))
                        throw new Exception("No Transaction Number found when release seat (EXIT21363);");

                    TicketReleaseCommand command = new TicketReleaseCommand(procId, netProcId, session.DepartPendingReleaseTransNo);

                    Log.LogText(LogChannel, procId, $@"Start - ReleaseSeatOnFailPayment; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.ReleaseSeatOnFailPayment");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21364).");
                    }
                }
                catch (Exception ex)
                {
                    //RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Release Seat). " + ex.Message);
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ReleaseSeatOnFailPayment");
                }
            }

            void PauseCountDown(string procId, Guid? netProcId, UserSession session)
            {
                bool retRes = _appCountDown.Pause(session.SessionId);
            }

            void TimeoutSession(string procId, Guid? netProcId, UserSession session)
            {
                RaiseOnShowResultMessage(_session.SessionId, new UICountDownExpiredAck(_session.SessionId, "-", DateTime.Now));
            }
        }

        private Guid? _raiseOnShowResultMessage_currentWorkingToken = null;
        private ConcurrentQueue<Guid> _workQ = new ConcurrentQueue<Guid>();
        private SemaphoreSlim _showResultMessageLock = new SemaphoreSlim(1);
        private SemaphoreSlim _showResultMessageLock2 = new SemaphoreSlim(1);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="netProcessId"></param>
        /// <param name="kioskMsg">Must has a valid object in order to return a result to UI.</param>
        /// <param name="msgType"></param>
        /// <param name="message">Normally this is 2nd error message. 1st error message is in returnObj.ErrorMessage</param>
        public void RaiseOnShowResultMessage(Guid? netProcessId, IKioskMsg kioskMsg, MessageType? msgType = null, string message = null)
        {
            bool isEventDispatched = false;
            bool updateCountDownUponDispatched = false;
            Guid _workToken = Guid.NewGuid();

            try
            {

                _showResultMessageLock.WaitAsync().Wait();


                if (kioskMsg != null)
                {
                    bool proceed = false;

                    MessageType msgTy = (msgType.HasValue == false) ? MessageType.NormalType : msgType.Value;

                    if ((kioskMsg is UIServerApplicationStatusAck) || (kioskMsg is UIWebServerLogonStatusAck) || (kioskMsg is UICountDownExpiredAck))
                    {
                        proceed = true;

                        //----------------------------------------------------------
                        // Clear previous NetProcessID if already expired
                        if (kioskMsg is UICountDownExpiredAck)
                        {
                            ReleaseSeatOnTimeout();
                            _session.Expired = true;
                            _SessionSuper.CleanNetProcessId();
                        }
                    }
                    else if (_session.SessionId.Equals(Guid.Empty))
                    {
                        proceed = true;
                    }
                    else if (_session.Expired == false)
                    {
                        if (_SessionSuper.FindNetProcessId(netProcessId.Value))
                        {
                            proceed = true;
                            updateCountDownUponDispatched = true;
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

                        isEventDispatched = true;
                        _workQ.Enqueue(_workToken);
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
                    Log.LogError(LogChannel, kioskMsg.ProcessId, new Exception($@"Unhandle event error OnShowCustomerMessage. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ShowCustomerMessage");
                else
                    Log.LogError(LogChannel, "-", new Exception($@"Unhandle event error OnShowCustomerMessage. Net Process Id: {netProcessId}", ex), "E02", "ServerSalesApplication.ShowCustomerMessage");
            }
            finally
            {
                _showResultMessageLock.Release();

                //Note : Re-set Count Down; Below statement put here to avoid death-lock between _showResultMessageLock and _appCountDown's threadLock;
                if (isEventDispatched)
                {
                    bool workDone = false;

                    while (workDone == false)
                    {
                        try
                        {
                            _showResultMessageLock2.WaitAsync().Wait();

                            if (_raiseOnShowResultMessage_currentWorkingToken.HasValue == false)
                            {
                                if (_workQ.TryDequeue(out Guid nextToken))
                                    _raiseOnShowResultMessage_currentWorkingToken = nextToken;
                            }

                            if ((_raiseOnShowResultMessage_currentWorkingToken.HasValue == false) || (_raiseOnShowResultMessage_currentWorkingToken.Value.Equals(_workToken)))
                            {
                                if (updateCountDownUponDispatched)
                                {
                                    int countDownInterval = _standardCountDownIntervalSec + 3;

                                    if (kioskMsg is UICustInfoAck uICustInfo)
                                    {
                                        if ((uICustInfo.MessageData is CustomerInfoList custInfoList) && (custInfoList.CustSeatInfoList?.Length > 0))
                                        {
                                            //countDownInterval = (_custInfoEntryIntervalSec * custInfoList.CustSeatInfoList.Length) + 3;
                                            countDownInterval = (_custInfoEntryIntervalSec * 5) + 3;
                                        }
                                    }
                                    else if (kioskMsg is UISalesPaymentProceedAck)
                                    {
                                        countDownInterval = 60 * 20;
                                    }
                                    else if ((kioskMsg is UICompleteTransactionResult uiComplt))
                                    {
                                        if ((uiComplt.ProcessState == ProcessResult.Success))
                                        {
                                            countDownInterval = 60 * 4;
                                        }
                                        else
                                        {
                                            countDownInterval = 60 * 2;
                                        }
                                    }

                                    if ((_appCountDown.UpdateCountDown(countDownInterval, _session.SessionId, isMandatoryExtensionChange: false, out bool isMatchedSession2) == false))
                                    {
                                        if (isMatchedSession2 == false)
                                            throw new Exception($@"Unable to update Count Down; (EXIT21325); Current App Session ID {_session.SessionId.ToString("D")} is not matched with count down.");
                                        else
                                            throw new Exception($@"Unable to update Count Down; (EXIT21327); Session ID {_session.SessionId.ToString("D")} already expired.");
                                    }


                                }

                                workDone = true;
                                _raiseOnShowResultMessage_currentWorkingToken = null;

                            }
                        }
                        catch (Exception ex)
                        {
                            workDone = true;
                            _raiseOnShowResultMessage_currentWorkingToken = null;

                            Log.LogText(LogChannel, "-", kioskMsg, "EX11", "ServerSalesApplication.ShowCustomerMessage", AppDecorator.Log.MessageType.Error, extraMsg: "MsgObj: IKioskMsg", netProcessId: netProcessId);
                            Log.LogError(LogChannel, "-", ex, "EX12", "ServerSalesApplication.ShowCustomerMessage", netProcessId: netProcessId);
                        }
                        finally
                        {
                            _showResultMessageLock2.Release();

                            if (workDone == false)
                                Task.Delay(GetJumpNextDelay()).Wait();
                        }
                    }
                }
            }

            int GetJumpNextDelay()
            {
                string aVal = Guid.NewGuid().ToString("D").Substring(0, 1);
                int intDelay = Convert.ToInt32(aVal, 16) * 3;
                return intDelay;
            }

            void ReleaseSeatOnTimeout()
            {
                string transactionNo = null;
                if (string.IsNullOrWhiteSpace(_session.DepartSeatConfirmTransNo) == false)
                {
                    transactionNo = _session.DepartSeatConfirmTransNo;
                }
                else
                    return;

                Thread tWorker = new Thread(new ThreadStart(new Action(() =>
                {
                    Guid dummyGuid = Guid.NewGuid();
                    try
                    {
                        if (_disposed)
                            throw new Exception("System is shutting down (EXIT21365);");

                        TicketReleaseCommand command = new TicketReleaseCommand("-", dummyGuid, transactionNo);
                        Log.LogText(LogChannel, "-", $@"Start - ReleaseSeatOnTimeout; Net Process Id:{dummyGuid}", $@"A02", classNMethodName: "ServerSalesApplication.ReleaseSeatOnTimeout");
                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception(errorMsg);
                            else
                                throw new Exception("Unknown error (EXIT21366).");
                        }
                    }
                    catch (Exception ex)
                    {
                        //RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Release Seat). " + ex.Message);
                        Log.LogError(LogChannel, "-", new Exception($@"Error found. Net Process Id: {dummyGuid}", ex), "E01", "ServerSalesApplication.ReleaseSeatOnTimeout");
                    }
                })));
                tWorker.IsBackground = true;
                tWorker.Priority = ThreadPriority.AboveNormal;
                tWorker.Start();
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

            _log = null;
            _svrAccess = null;
        }

    }
}
