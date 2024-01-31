using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    public class ReleaseSeatExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private TicketReleaseCommand _command = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UISeatReleaseResult uiTickRelResult;

            try
            {
                _command = (TicketReleaseCommand)commPack.Command;

                bool isNetworkTimeout = false;
                bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

                seatrelease_status relStt = null;

                if (isWebServiceDetected)
                    relStt = ReleaseSeat(_command.TransactionNo, out isNetworkTimeout, 90);
                else
                    relStt = new seatrelease_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (II)" };

                //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                if ((relStt != null) && ((relStt.code == 20 || relStt.code == 21)))
                {
                    if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                    {
                        isNetworkTimeout = false;
                        relStt = ReleaseSeat(_command.TransactionNo, out isNetworkTimeout, 90);
                    }
                    else
                    {
                        if (networkTimeout == true)
                            relStt = new seatrelease_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (III)" };
                        else
                            relStt = new seatrelease_status() { code = ServerAccess.InvalidAuthentication, msg = "Invalid authentication" };
                    }
                }

                if (relStt?.code == 0)
                {
                    uiTickRelResult = new UISeatReleaseResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, relStt);
                    commPack.UpSertResult(false, uiTickRelResult);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, "TicketReleaseExecution.Execute:A02", uiTickRelResult);
                }
                else
                {
                    if (relStt == null)
                        relStt = new seatrelease_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (IV)" };

                    uiTickRelResult = new UISeatReleaseResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, relStt)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(relStt.msg) == true ? "Unknown error when reading destination trip (EXIT21661)" : relStt.msg) };

                    commPack.UpSertResult(true, uiTickRelResult, uiTickRelResult.ErrorMessage);

                    whenCompletedSendEvent((isNetworkTimeout == true ? ResultStatus.NetworkTimeout : ResultStatus.Fail),
                        _commandPack.NetProcessId, _commandPack.ProcessId, "TicketReleaseExecution.Execute:A05", uiTickRelResult, new Exception(uiTickRelResult.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiTickRelResult = new UISeatReleaseResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21662)" };
                commPack.UpSertResult(true, uiTickRelResult, errorMessage: uiTickRelResult.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, "TicketReleaseExecution.Execute:A11", uiTickRelResult, new Exception(uiTickRelResult.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, UISeatReleaseResult uiTickRelResultX,
                Exception error = null)
            {
                if (eventSent)
                    return;

                SendMessageEventArgs compEv;

                if ((error == null) && (resultState == ResultStatus.Success))
                {
                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiTickRelResultX);

                    Log.LogText(LogChannel, processId, compEv,
                        "A01", "TicketReleaseExecution.whenCompletedSendEvent", netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }
                else
                {
                    if (resultState == ResultStatus.Success)
                        resultState = ResultStatus.ErrorFound;

                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiTickRelResultX)
                    {
                        Message = ((error?.Message) is null) ? "Unknown error found;" : error.Message
                    };

                    Log.LogText(LogChannel, processId, compEv,
                        "A02", "TicketReleaseExecution.whenCompletedSendEvent", AppDecorator.Log.MessageType.Error, netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }

                // Note Release Ticket no need to send back any response.
                ///// _serverAccess.RaiseOnSendMessage(compEv, lineTag);
                eventSent = true;

                Log.LogText(LogChannel, processId, "End - whenCompletedSendEvent", "A10", "TicketReleaseExecution.whenCompletedSendEvent", netProcessId: netProcessId);
            }
        }


        private seatrelease_status ReleaseSeat(string transactionNo, out bool isNetworkTimeout, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;

            seatrelease_status resultDest = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                resultDest = null;
                Thread threadWorker = new Thread(new ThreadStart(ReleaseSeatThreadWorking));
                threadWorker.IsBackground = true;

                DateTime timeOut = DateTime.Now.AddSeconds(waitSec);
                threadWorker.Start();

                while ((timeOut.Subtract(DateTime.Now).TotalMilliseconds > 0) && (resultDest is null) && (threadWorker.ThreadState.IsState(ThreadState.Stopped) == false))
                {
                    Task.Delay(100).Wait();
                }

                if (resultDest is null)
                {
                    try
                    {
                        threadWorker.Abort();
                        Task.Delay(500).Wait();
                    }
                    catch { }

                    if ((retryInx + 1) == maxRetryTimes)
                    {
                        isNetworkTimeout = true;
                        resultDest = new seatrelease_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout; (EXIT21663)" };
                    }
                }

                if (resultDest != null)
                    break;
                else
                    Task.Delay(ServerAccess.RetryIntervalSec * 1000).Wait();
            }

            return resultDest;

            void ReleaseSeatThreadWorking()
            {
                try
                {
                    string token = Security.AccessToken;
                    string kioskId = Security.KioskID;

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        resultDest = null;
                    }
                    else
                    {
                        seatrelease_status retRState = _serverAccess.Soap.SeatRelease(kioskId, token, (transactionNo ?? ""));
                        resultDest = retRState;
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, classNMethodName: "ServerAccess.ReleaseSeatThreadWorking");
                }
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