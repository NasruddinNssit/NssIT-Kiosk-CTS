using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    public class WebServerLogonExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIWebServerLogonStatusAck uiState;

            try
            {
                //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                {
                    uiState = new UIWebServerLogonStatusAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, true, false, true);
                    commPack.UpSertResult(false, uiState);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId,
                          "WebServerLogonExecution.Execute:A05", uiState);
                }
                else
                {
                    uiState = new UIWebServerLogonStatusAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, false, networkTimeout, isValidAuthentication);
                    commPack.UpSertResult(true, uiState, $@"Fail logon; NetworkTimeout: {networkTimeout}; IsValidAuthentication: {isValidAuthentication}");
                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId,
                          "WebServerLogonExecution.Execute:A07", uiState, new Exception($@"Fail logon; NetworkTimeout: {networkTimeout}; IsValidAuthentication: {isValidAuthentication}"));
                }
            }
            catch (Exception ex)
            {
                uiState = new UIWebServerLogonStatusAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, false, false, false, true) { ErrorMessage = $@"Error when logon to web server; (EXIT21653), {ex.Message};" };
                commPack.UpSertResult(true, uiState, uiState.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId,
                          "WebServerLogonExecution.Execute:A09", uiState, new Exception(uiState.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, UIWebServerLogonStatusAck uiWebServerLogonStatus, Exception error = null)
            {
                if (eventSent)
                    return;

                SendMessageEventArgs compEv;

                if ((error == null) && (resultState == ResultStatus.Success))
                {
                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiWebServerLogonStatus);

                    Log.LogText(LogChannel, processId, compEv,
                        "A01", "WebServerLogonExecution.whenCompletedSendEvent", netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }
                else
                {
                    if (resultState == ResultStatus.Success)
                        resultState = ResultStatus.ErrorFound;

                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiWebServerLogonStatus)
                    {
                        Message = ((error?.Message) is null) ? "Unknown error found;" : error.Message
                    };

                    Log.LogText(LogChannel, processId, $@"Start - whenCompletedSendEvent; Error: {compEv.Message}",
                        "A02", "WebServerLogonExecution.whenCompletedSendEvent", netProcessId: netProcessId);
                }

                _serverAccess.RaiseOnSendMessage(compEv, lineTag);
                eventSent = true;

                Log.LogText(LogChannel, processId, "End - whenCompletedSendEvent", "A10", "WebServerLogonExecution.whenCompletedSendEvent", netProcessId: netProcessId);
            }
        }

        public void Dispose()
        {
            _commandPack = null;
            _log = null;
            _serverAccess = null;
        }
    }
}
