using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.Common.WebAPI;
using NssIT.Kiosk.Common.WebAPI.Common.WebApi;
using NssIT.Kiosk.Common.WebAPI.Data;
using NssIT.Kiosk.Common.WebAPI.Data.PostRequestParam.BTnG;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Network.SignalRClient.API.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.AxCommand.BTnG
{
    /// <summary>
    /// ClassCode:EXIT26.04
    /// </summary>
    public class AxGetAvailablePaymentGateway : IAxCommand<UIxGnBTnGAck<BTnGGetPaymentGatewayResult>>, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private string _domainEntityTag = "Reading available Payment Gateway from web";
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public string ProcessId { get; private set; }
        public Guid? NetProcessId { get; private set; }
        public AppCallBackDelg CallBackEvent { get; private set; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private bool _callBackFlag = false;
        private DbLog _log = DbLog.GetDbLog();
        private string _webApiUrl = @"PaymentGateway/getAvailablePaymentGateway";

        private string _deviceId = "-*-";
        /// <summary>
        /// FuncCode:EXIT26.0401
        /// </summary>
        public AxGetAvailablePaymentGateway(string processId, Guid? netProcessId, AppCallBackDelg callBackEvent)
        {
            ProcessId = processId;
            NetProcessId = netProcessId;
            CallBackEvent = callBackEvent;

            _deviceId = RegistrySetup.GetRegistrySetting().DeviceId;
        }

        /// <summary>
        /// FuncCode:EXIT26.0402
        /// </summary>
        public void Execute()
        {
            string _exeMethodTag = $@"{this.GetType().Name}.DoExe()-AxExeTag";
            Guid workId = Guid.NewGuid();
            try
            {
                DoExe();
            }
            catch (ThreadAbortException ex)
            {
                _log?.LogError(LogChannel, "-", ex, "EX01", _exeMethodTag, netProcessId: NetProcessId);

                RaiseCallBack(new UIxGnBTnGAck<BTnGGetPaymentGatewayResult>(NetProcessId, ProcessId, new Exception($@"Operation aborted in '{_domainEntityTag}'(access); {ex.Message} ;(EXIT26.0402.EX01)", ex)), CallBackEvent);
                throw ex;
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, "-", ex, "EX05", _exeMethodTag, netProcessId: NetProcessId);

                RaiseCallBack(new UIxGnBTnGAck<BTnGGetPaymentGatewayResult>(NetProcessId, ProcessId, new Exception($@"Error when {_domainEntityTag}; {ex.Message} ;(EXIT26.0402.EX05); Error Type: {ex.GetType().Name}", ex)), CallBackEvent);
            }
            finally
            {
                ShutDown();
            }
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            /// <summary>
            /// FuncCode:EXIT26.048A
            /// </summary>
            void DoExe()
            {
                string bTngWebApiVerStr = PaymentGuard.SectionVersion;

                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                NssIT.Kiosk.AppDecorator.Common.AppStationCode.GetStationInfo(AppDecorator.Common.AppGroup.MelakaSentral, out string stationCode, out _);

                if (string.IsNullOrWhiteSpace(bTngWebApiVerStr) == false)
                {
                    // AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
                    //------------------------------------------------------------------------------------------------
                    using (WebAPIAgent webAPI = new WebAPIAgent(AppDecorator.Config.Setting.GetSetting().WebApiURL))
                    {
                        //---------------------------------
                        // Web API Parameter
                        AvailablePaymentGatewayRequest param = new AvailablePaymentGatewayRequest() { MerchantId = RegistrySetup.GetRegistrySetting().BTnGMerchantId, DeviceId = _deviceId, StationCode = stationCode };

                        dynamic apiRes = webAPI.Post<BTnGGetPaymentGatewayResult>(param, _webApiUrl, $@"{_exeMethodTag}").GetAwaiter().GetResult();

                        if ((apiRes is BTnGGetPaymentGatewayResult xResult) && (xResult.Code.Equals(WebAPIAgent.ApiCodeOK)))
                        {
                            if ((xResult.Status == true)
                                && (xResult.Data is BTnGPaymentGatewayModel data)
                                && (data.PaymentGatewayList?.Length > 0)
                                )
                            {
                                RaiseCallBack(new UIxGnBTnGAck<BTnGGetPaymentGatewayResult>(NetProcessId, ProcessId, xResult), CallBackEvent);
                            }
                            else
                            {
                                _log?.LogText(LogChannel, "-", xResult, "A03", _exeMethodTag, AppDecorator.Log.MessageType.Error,
                                    netProcessId: NetProcessId, extraMsg: $@"No valid data found; (EXIT26.048A.X02); Error Code : {xResult.Code}; MsgObj: PaymentGatewayResult");

                                RaiseCallBack(new UIxGnBTnGAck<BTnGGetPaymentGatewayResult>(NetProcessId, ProcessId, new Exception($@"No valid data found; (EXIT26.048A.X15); Error Code : {xResult.Code}")), CallBackEvent);
                            }
                        }
                        else
                        {
                            string errorMsg = "";

                            if (apiRes is BTnGGetPaymentGatewayResult errResult)
                            {
                                if ((errResult.Status == false) && (string.IsNullOrWhiteSpace(errResult.MessageString()) == false))
                                    errorMsg = $@"{errResult.MessageString()} (when {_domainEntityTag}); (EXIT26.048A.X04) ";
                                else
                                    errorMsg = $@"No valid data found (when {_domainEntityTag}); (EXIT26.048A.X06)";

                                _log?.LogText(LogChannel, "-", errResult, "A05", _exeMethodTag, AppDecorator.Log.MessageType.Error,
                                    netProcessId: NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: PaymentSubmissionResult");
                            }
                            else
                            {
                                if (apiRes is WebApiException wex)
                                    errorMsg = $@"{wex.MessageString() ?? "Web process error"}; (when {_domainEntityTag}); (EXIT26.048A.X08)";
                                else
                                    errorMsg = $@"Unexpected error occurred; ({_domainEntityTag}); (EXIT26.048A.X10)";

                                _log?.LogText(LogChannel, "-", errorMsg, "A09", _exeMethodTag, AppDecorator.Log.MessageType.Error, netProcessId: NetProcessId);
                            }

                            RaiseCallBack(new UIxGnBTnGAck<BTnGGetPaymentGatewayResult>(NetProcessId, ProcessId, new Exception(errorMsg)), CallBackEvent);
                        }
                    }
                }
                else
                {
                    BTnGGetPaymentGatewayResult res = new BTnGGetPaymentGatewayResult()
                    {
                        Code = "0",
                        Status = true,
                        Data = new BTnGPaymentGatewayModel()
                        {
                            MerchantId = RegistrySetup.GetRegistrySetting().BTnGMerchantId,
                            PaymentGatewayList = new BTnGPaymentGatewayDetailModel[0]
                        }
                    };
                    RaiseCallBack(new UIxGnBTnGAck<BTnGGetPaymentGatewayResult>(NetProcessId, ProcessId, res), CallBackEvent);
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT26.0403
        /// </summary>
        private void RaiseCallBack(UIxGnBTnGAck<BTnGGetPaymentGatewayResult> uixData, AppCallBackDelg callBackEvent)
        {
            if ((_callBackFlag == true) || (callBackEvent is null))
                return;

            try
            {
                callBackEvent.Invoke(uixData);

                _log?.LogText(LogChannel, ProcessId, uixData, "A01", $@"AxGetAvailablePaymentGateway.RaiseCallBack");
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, ProcessId, new WithDataException(ex.Message, ex, uixData), "EX01", "AxGetAvailablePaymentGateway.RaiseCallBack", netProcessId: NetProcessId);
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
