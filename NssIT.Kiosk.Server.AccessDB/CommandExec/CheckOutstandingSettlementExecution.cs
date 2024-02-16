using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.PostRequestParam;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    public class CheckOutstandingSettlementExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private CheckOutstandingCardSettlementCommand _command = null;
        private string _webApiUrl = @"Kiosk/getOutstandingCardSettlement";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "CheckOutstandingSettlementExecution.Execute";
            string domainEntityTag = "Check Outstanding Card Settlement";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UISalesCheckOutstandingCardSettlementAck uiOutStdHost;

            try
            {
                // KTM Trip List XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (CheckOutstandingCardSettlementCommand)commPack.Command;

                IPostRequestParam param = new CheckOutstandingCardSettlementRequest()
                {
                    MachineId = setting.KioskId
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<OutstandingCardSettlementResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is OutstandingCardSettlementResult outSettX) && (outSettX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    OutstandingCardSettlementResult outSett = outSettX;

                    if ((outSett?.Data.Host is null))
                    {
                        outSett.Data.Host = new string[0];
                    }

                    //if ((tick?.Data.Host.Length > 0))
                    //{
                    uiOutStdHost = new UISalesCheckOutstandingCardSettlementAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, outSett);
                    commPack.UpSertResult(false, uiOutStdHost);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiOutStdHost);
                    validDataFound = true;
                    //}
                    //else
                    //{
                    //    apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21811); Error Code : {tick?.Code}"));

                    //    Log.LogText(LogChannel, "-", tickX, "A03", methodTag, AppDecorator.Log.MessageType.Error,
                    //        netProcessId: commPack.NetProcessId, extraMsg: $@"No valid data found; (EXIT21811); Error Code : {tick?.Code}; MsgObj: OutstandingCardSettlementResult");
                    //}
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is OutstandingCardSettlementResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21812)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: OutstandingCardSettlementResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21813)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21814)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    OutstandingCardSettlementResult errData = new OutstandingCardSettlementResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiOutStdHost = new UISalesCheckOutstandingCardSettlementAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21815)" : errorMsg) };

                    commPack.UpSertResult(true, uiOutStdHost, uiOutStdHost.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiOutStdHost, new Exception(uiOutStdHost.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21816)" });
                OutstandingCardSettlementResult errData = new OutstandingCardSettlementResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiOutStdHost = new UISalesCheckOutstandingCardSettlementAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiOutStdHost, errorMessage: uiOutStdHost.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}.Execute:A11", uiOutStdHost, new Exception(uiOutStdHost.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag,
                IKioskMsg uiKioskMsg, Exception error = null)
            {
                if (eventSent)
                    return;
                _serverAccess.RaiseOnSendMessage(resultState, netProcessId, processId, lineTag, uiKioskMsg, error);
                eventSent = true;
            }
        }

        public void Dispose()
        {
            _commandPack = null;
            _log = null;
            _serverAccess = null;
            _command = null;
        }
    }
}