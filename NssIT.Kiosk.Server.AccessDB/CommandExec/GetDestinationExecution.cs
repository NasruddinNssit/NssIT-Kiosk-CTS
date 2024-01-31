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
	public class GetDestinationExecution : IAccessCommandExec, IDisposable 
	{
        private const string LogChannel = "ServerAccess";

		private AccessCommandPack _commandPack = null;
        private DestinationListRequestCommand _command = null;

        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
		{
            _serverAccess = serverAccess;
			_commandPack = commPack;
            bool eventSent = false;
            UIDestinationListAck uiDest;

            try
            {
                _command = (DestinationListRequestCommand)_commandPack.Command;

                bool isNetworkTimeout = false;

                bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

                destination_status dest = null;

                if (isWebServiceDetected)
                    dest = GetDestination(_command.OriginStationCode, out isNetworkTimeout);
                else
                    dest = new destination_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (II)" };

                //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                if ((dest != null) && ((dest.code == 20 || dest.code == 21)))
                {
                    if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                    {
                        isNetworkTimeout = false;
                        dest = GetDestination(_command.OriginStationCode, out isNetworkTimeout);
                    }
                    else
                    {
                        if (networkTimeout == true)
                            dest = new destination_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (III)" };
                        else
                            dest = new destination_status() { code = ServerAccess.InvalidAuthentication, msg = "Invalid authentication" };
                    }
                }

                if (dest?.code == 0)
                {
                    uiDest = new UIDestinationListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, dest);
                    commPack.UpSertResult(false, uiDest);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, "GetDestinationExecution.Execute:A02", uiDest);
                }
                else
                {
                    if (dest == null)
                        dest = new destination_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (IV)" };
                    
                    uiDest = new UIDestinationListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, dest) 
                        { ErrorMessage = (string.IsNullOrWhiteSpace(dest.msg) == true ? "Unknown error when reading destination (EXIT21601)" : dest.msg)};

                    commPack.UpSertResult(true, uiDest, uiDest.ErrorMessage);

                    whenCompletedSendEvent((isNetworkTimeout == true ? ResultStatus.NetworkTimeout : ResultStatus.Fail), 
                        _commandPack.NetProcessId, _commandPack.ProcessId, "GetDestinationExecution.Execute:A05", uiDest, new Exception(uiDest.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiDest = new UIDestinationListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21603)" };
                commPack.UpSertResult(true, uiDest, errorMessage: uiDest.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, "GetDestinationExecution.Execute:A11", uiDest, new Exception(uiDest.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, UIDestinationListAck uIDestinationList,
                Exception error = null)
            {
                if (eventSent)
                    return;

                SendMessageEventArgs compEv;

                if ((error == null) && (resultState == ResultStatus.Success))
                {
                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uIDestinationList);

                    Log.LogText(LogChannel, processId, compEv,
                        "A01", "GetDestinationExecution.whenCompletedSendEvent", netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }
                else
                {
                    if (resultState == ResultStatus.Success)
                        resultState = ResultStatus.ErrorFound;

                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uIDestinationList)
                    {
                        Message = ((error?.Message) is null) ? "Unknown error found;" : error.Message
                    };

                    Log.LogText(LogChannel, processId, $@"Start - whenCompletedSendEvent; Error: {compEv.Message}",
                        "A02", "GetDestinationExecution.whenCompletedSendEvent", netProcessId: netProcessId);
                }
                
                _serverAccess.RaiseOnSendMessage(compEv, lineTag);
                eventSent = true;

                Log.LogText(LogChannel, processId, "End - whenCompletedSendEvent", "A10", "GetDestinationExecution.whenCompletedSendEvent", netProcessId: netProcessId);
            }
        }

        private destination_status GetDestination(string originCode, out bool isNetworkTimeout, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;

            destination_status resultDest = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                resultDest = null;
                Thread threadWorker = new Thread(new ThreadStart(GetDestinationThreadWorking));
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
                        resultDest = new destination_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout; (EXIT21610)" };
                    }
                }

                if (resultDest != null)
                    break;
                else
                    Task.Delay(ServerAccess.RetryIntervalSec * 1000).Wait();
            }

            return resultDest;

            void GetDestinationThreadWorking()
            {
                try
                {
                    string token = Security.AccessToken;

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        resultDest = null;
                    }
                    else
                    {
                        // CYA-DEBUG --> Temporary Hard Code From Origin as "MEL"
                        destination_status retDest = _serverAccess.Soap.Destination(token, originCode);
                        //destination_status retDest = _serverAccess.Soap.Destination(token, "");
                        //seat_status retSeat = _serverAccess.Soap.Seat((.Destination(token, "");
                        //seat_status retSeat = _serverAccess.Soap.SeatConfirm((.Destination(token, "");

                        //transcomplete_status tranCompState = _serverAccess.Soap.TransComplete(.Destination(token, "");
                        //transcomplete_status tranCompState = _serverAccess.Soap.UpdateKioskCash(.Destination(token, "");
                        //transcomplete_status tranCompState = _serverAccess.Soap.UpdateKioskCoin(.Destination(token, "");

                        resultDest = retDest;
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, classNMethodName: "GetDestinationExecution.GetDestinationThreadWorking");
                }
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
