using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
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
    /// ClassCode:EXIT26.05
    /// </summary>
    public class AxUpdateSuccessTransLog : IAxCommand<UIxCommonAck>, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private string _domainEntityTag = "Update Success Transaction Log to Payment Gateway website";
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public string ProcessId { get; private set; }
        public Guid? NetProcessId { get; private set; }
        public AppCallBackDelg CallBackEvent { get; private set; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private bool _callBackFlag = false;
        private DbLog _log = DbLog.GetDbLog();
        private string _webApiUrl = @"PaymentGateway/updateSuccessTransactionLog";

        private string _bTnGSaleTransactionNo = "-*-";

        /// <summary>
        /// FuncCode:EXIT26.0501
        /// </summary>
        public AxUpdateSuccessTransLog(string processId, Guid? netProcessId, string bTnGSaleTransactionNo)
        {
            ProcessId = processId;
            NetProcessId = netProcessId;
            _bTnGSaleTransactionNo = bTnGSaleTransactionNo;
            CallBackEvent = null;
        }


        /// <summary>
        /// FuncCode:EXIT26.0502
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

                throw ex;
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, "-", ex, "EX05", _exeMethodTag, netProcessId: NetProcessId);
            }
            finally
            {
                ShutDown();
            }
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            /// <summary>
            /// FuncCode:EXIT26.0502A
            /// </summary>
            void DoExe()
            {
                string bTngWebApiVerStr = PaymentGuard.SectionVersion;

                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                NssIT.Kiosk.AppDecorator.Common.AppStationCode.GetStationInfo(setting.ApplicationGroup, out string stationCode, out _);

                if (string.IsNullOrWhiteSpace(bTngWebApiVerStr) == false)
                {
                    // AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
                    //------------------------------------------------------------------------------------------------
                    using (WebAPIAgent webAPI = new WebAPIAgent(AppDecorator.Config.Setting.GetSetting().WebApiURL))
                    {
                        //---------------------------------
                        // Web API Parameter
                        PaymentGatewaySuccessLogRequest param = new PaymentGatewaySuccessLogRequest()
                        {
                            SalesTransactionNo = _bTnGSaleTransactionNo,
                            HeaderStatus = ((int)BTnGHeaderStatus.SUCCESS).ToString(),
                            DetailStatus = ((int)BTnGDetailStatus.w_paid_ack).ToString(),

                            LineTag = "CTS_Kiosk_Transaction_A01",
                            User = setting.KioskId,
                            Remark = "Transaction successful done in Kiosk"
                        }; ;

                        dynamic apiRes = webAPI.Post<BTnGCommonResult>(param, _webApiUrl, $@"{_exeMethodTag}").GetAwaiter().GetResult();

                        if ((apiRes is BTnGCommonResult xResult) && (xResult.Code.Equals(WebAPIAgent.ApiCodeOK)))
                        {
                            _log?.LogText(LogChannel, _bTnGSaleTransactionNo, apiRes, "A70", _exeMethodTag, netProcessId: NetProcessId, extraMsg: "MsgObj: BTnGCommonResult");
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
                        }
                    }
                }
                else
                {
                    _log?.LogText(LogChannel, "-", "Not valid window register setting", "A09", _exeMethodTag, AppDecorator.Log.MessageType.Error, netProcessId: NetProcessId);
                }
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
