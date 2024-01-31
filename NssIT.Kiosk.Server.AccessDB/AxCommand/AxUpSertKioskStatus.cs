using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.Common.WebAPI;
using NssIT.Kiosk.Common.WebAPI.Common.WebApi;
using NssIT.Kiosk.Common.WebAPI.Data;
using NssIT.Kiosk.Common.WebAPI.Data.PostRequestParam;
using NssIT.Kiosk.Common.WebAPI.Data.Response;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.AxCommand
{
    /// <summary>
    /// ClassCode:EXIT25.18
    /// </summary>
    public class AxUpSertKioskStatus : IAxCommand<UIxGnAppAck<UpdateKioskStatusResult<BaseCommonObj>>>, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private string _domainEntityTag = "UpSert Kiosk Status to Web Api";
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public string ProcessId { get; private set; }
        public Guid? NetProcessId { get; private set; }
        public AppCallBackDelg CallBackEvent { get; private set; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private bool _callBackFlag = false;
        private DbLog _log = DbLog.GetDbLog();
        private string _webApiUrl = @"KioskStatus/upSertKioskStatus";

        private string prmMachineId = "-*-";
        private KioskLatestStatusModel[] prmLatestStatusList = null;
        private bool prmIsCleanupExistingMachineStatus = false;

        /// <summary>
        /// FuncCode:EXIT25.1801
        /// </summary>
        public AxUpSertKioskStatus(string processId, Guid? netProcessId, KioskLatestStatusModel[] latestStatusList, bool isCleanupExistingMachineStatus, AppCallBackDelg callBackEvent)
        {
            ProcessId = processId;
            NetProcessId = netProcessId;
            CallBackEvent = callBackEvent;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            prmMachineId = Setting.GetSetting().KioskId;
            prmIsCleanupExistingMachineStatus = isCleanupExistingMachineStatus;

            List<KioskLatestStatusModel> statusList = new List<KioskLatestStatusModel>();
            foreach (KioskLatestStatusModel stt in latestStatusList)
            {
                KioskLatestStatusModel newStt = new KioskLatestStatusModel()
                {
                    CheckingCode = stt.CheckingCode, 
                    CheckingDescription = stt.CheckingDescription, 
                    CheckingName = stt.CheckingName, 
                    LastUpdateDateTime = stt.LastUpdateDateTime,
                    MachineId = stt.MachineId, 
                    MachineLocalDateTime = DateTime.MinValue,
                    MachineLocalDateTimeTicks = stt.MachineLocalDateTime.Ticks, 
                    Remark = stt.Remark, 
                    RemarkType = stt.RemarkType, 
                    Status = stt.Status, 
                    StatusName = stt.StatusName
                };
                statusList.Add(newStt);
            }
            prmLatestStatusList = statusList.ToArray();
        }

        /// <summary>
        /// FuncCode:EXIT25.1802
        /// </summary>
        public void Execute()
        {
            string exeMethodTag = $@"{this.GetType().Name}.Execute()-AxExeTag";
            
            Guid workId = Guid.NewGuid();
            try
            {
                DoExe();
            }
            catch (ThreadAbortException ex)
            {
                _log?.LogError(LogChannel, "-", ex, "EX01", exeMethodTag, netProcessId: NetProcessId);

                RaiseCallBack(new UIxGnAppAck<UpdateKioskStatusResult<BaseCommonObj>>(NetProcessId, ProcessId, new Exception($@"Operation aborted in '{_domainEntityTag}' (access); {ex.Message} ;(EXIT25.1802.EX01)", ex)), CallBackEvent);
                throw ex;
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, "-", ex, "EX05", exeMethodTag, netProcessId: NetProcessId);

                RaiseCallBack(new UIxGnAppAck<UpdateKioskStatusResult<BaseCommonObj>>(NetProcessId, ProcessId, new Exception($@"Error when {_domainEntityTag} (access); {ex.Message} ;(EXIT25.1802.EX05); Error Type: {ex.GetType().Name}", ex)), CallBackEvent);
            }
            finally
            {
                ShutDown();
            }
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            /// <summary>
            /// FuncCode:EXIT25.188A
            /// </summary>
            void DoExe()
            {
                string doExeMethodTag = $@"{this.GetType().Name}.DoExe()-AxDoExeTag";

                // AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
                //------------------------------------------------------------------------------------------------
                using (WebAPIAgent webAPI = new WebAPIAgent(AppDecorator.Config.Setting.GetSetting().WebApiURL))
                {

                    //---------------------------------
                    // Web API Parameter
                    KioskStatusUpdateRequest param = new KioskStatusUpdateRequest() 
                    { 
                        MachineId = prmMachineId,
                        IsCleanupExistingMachineStatus = prmIsCleanupExistingMachineStatus, 
                        LatestStatusList = prmLatestStatusList
                    };

                    dynamic apiRes = webAPI.Post<UpdateKioskStatusResult<BaseCommonObj>>(param, _webApiUrl, $@"{doExeMethodTag}").GetAwaiter().GetResult();

                    if ((apiRes is UpdateKioskStatusResult<BaseCommonObj> xResult) && (xResult.Code.Equals(WebAPIAgent.ApiCodeOK)))
                    {
                        if (xResult.Status == true)
                        {
                            RaiseCallBack(new UIxGnAppAck<UpdateKioskStatusResult<BaseCommonObj>>(NetProcessId, ProcessId, xResult), CallBackEvent);
                        }
                        else
                        {
                            _log?.LogText(LogChannel, "-", xResult, "A03", doExeMethodTag, AppDecorator.Log.MessageType.Error,
                                netProcessId: NetProcessId, extraMsg: $@"No valid data found; (EXIT25.188A.X02); Error Code : {xResult.Code}; MsgObj: PaymentGatewayResult");

                            RaiseCallBack(new UIxGnAppAck<UpdateKioskStatusResult<BaseCommonObj>>(NetProcessId, ProcessId, new Exception($@"No valid data found; (EXIT25.188A.X15); Error Code : {xResult.Code}")), CallBackEvent);
                        }
                    }
                    else
                    {
                        string errorMsg = "";

                        if (apiRes is UpdateKioskStatusResult<BaseCommonObj> errResult)
                        {
                            if ((errResult.Status == false) && (string.IsNullOrWhiteSpace(errResult.MessageString()) == false))
                                errorMsg = $@"{errResult.MessageString()} (when {_domainEntityTag}); (EXIT25.188A.X04) ";
                            else
                                errorMsg = $@"No valid data found (when {_domainEntityTag}); (EXIT25.188A.X06)";

                            _log?.LogText(LogChannel, "-", errResult, "A05", doExeMethodTag, AppDecorator.Log.MessageType.Error,
                                netProcessId: NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: PaymentSubmissionResult");
                        }
                        else
                        {
                            if (apiRes is WebApiException wex)
                                errorMsg = $@"{wex.MessageString() ?? "Web process error"}; (when {_domainEntityTag}); (EXIT25.188A.X08)";
                            else
                                errorMsg = $@"Unexpected error occurred; ({_domainEntityTag}); (EXIT25.188A.X10)";

                            _log?.LogText(LogChannel, "-", errorMsg, "A09", doExeMethodTag, AppDecorator.Log.MessageType.Error, netProcessId: NetProcessId);
                        }

                        RaiseCallBack(new UIxGnAppAck<UpdateKioskStatusResult<BaseCommonObj>>(NetProcessId, ProcessId, new Exception(errorMsg)), CallBackEvent);
                    }
                }
            }
        }

        // <summary>
        // FuncCode:EXIT25.1803
        // </summary>
        private void RaiseCallBack(UIxGnAppAck<UpdateKioskStatusResult<BaseCommonObj>> uixData, AppCallBackDelg callBackEvent)
        {
            if ((_callBackFlag == true) || (callBackEvent is null))
                return;

            try
            {
                callBackEvent?.Invoke(uixData);

                _log?.LogText(LogChannel, ProcessId, uixData, "A01", $@"AxUpSertKioskStatus.RaiseCallBack");
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, ProcessId, new WithDataException(ex.Message, ex, uixData), "EX01", "AxUpSertKioskStatus.RaiseCallBack", netProcessId: NetProcessId);
            }
            finally
            {
                _callBackFlag = true;
            }
        }

        private void ShutDown()
        {
            CallBackEvent = null;
            _log = null;
        }

        public void Dispose()
        {
            ShutDown();
        }
    }
}
