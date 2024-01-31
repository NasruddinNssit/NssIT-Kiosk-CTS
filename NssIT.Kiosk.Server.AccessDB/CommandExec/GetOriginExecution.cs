using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
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
	public class GetOriginExecution : IAccessCommandExec, IDisposable 
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
            UIOriginListAck uiOrig;

            try
            {
                bool isNetworkTimeout = false;

                bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

                destination_status orig = null;

                if (isWebServiceDetected)
                    orig = GetOrigin(out isNetworkTimeout);
                else
                    orig = new destination_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (II)" };

                //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                if ((orig != null) && ((orig.code == 20 || orig.code == 21)))
                {
                    if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                    {
                        isNetworkTimeout = false;
                        orig = GetOrigin(out isNetworkTimeout);
                    }
                    else
                    {
                        if (networkTimeout == true)
                            orig = new destination_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (III)" };
                        else
                            orig = new destination_status() { code = ServerAccess.InvalidAuthentication, msg = "Invalid authentication" };
                    }
                }

                if (orig?.code == 0)
                {
                    uiOrig = new UIOriginListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, orig);
                    commPack.UpSertResult(false, uiOrig);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, "GetOriginExecution.Execute:A02", uiOrig);
                }
                else
                {
                    if (orig == null)
                        orig = new destination_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (IV)" };
                    
                    uiOrig = new UIOriginListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, orig) 
                        { ErrorMessage = (string.IsNullOrWhiteSpace(orig.msg) == true ? "Unknown error when reading origin (EXIT21671)" : orig.msg)};

                    commPack.UpSertResult(true, uiOrig, uiOrig.ErrorMessage);

                    whenCompletedSendEvent((isNetworkTimeout == true ? ResultStatus.NetworkTimeout : ResultStatus.Fail), 
                        _commandPack.NetProcessId, _commandPack.ProcessId, "GetOriginExecution.Execute:A05", uiOrig, new Exception(uiOrig.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiOrig = new UIOriginListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21672)" };
                commPack.UpSertResult(true, uiOrig, errorMessage: uiOrig.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, "GetOriginExecution.Execute:A11", uiOrig, new Exception(uiOrig.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, UIOriginListAck uIOriginList,
                Exception error = null)
            {
                if (eventSent)
                    return;

                SendMessageEventArgs compEv;

                if ((error == null) && (resultState == ResultStatus.Success))
                {
                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uIOriginList);

                    Log.LogText(LogChannel, processId, compEv,
                        "A01", "GetOriginExecution.whenCompletedSendEvent", netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }
                else
                {
                    if (resultState == ResultStatus.Success)
                        resultState = ResultStatus.ErrorFound;

                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uIOriginList)
                    {
                        Message = ((error?.Message) is null) ? "Unknown error found;" : error.Message
                    };

                    Log.LogText(LogChannel, processId, $@"Start - whenCompletedSendEvent; Error: {compEv.Message}",
                        "A02", "GetOriginExecution.whenCompletedSendEvent", netProcessId: netProcessId);
                }
                
                _serverAccess.RaiseOnSendMessage(compEv, lineTag);
                eventSent = true;

                Log.LogText(LogChannel, processId, "End - whenCompletedSendEvent", "A10", "GetOriginExecution.whenCompletedSendEvent", netProcessId: netProcessId);
            }
        }

        private destination_status GetOrigin(out bool isNetworkTimeout, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;

            destination_status resultOrig = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                resultOrig = null;
                Thread threadWorker = new Thread(new ThreadStart(GetOriginThreadWorking));
                threadWorker.IsBackground = true;

                DateTime timeOut = DateTime.Now.AddSeconds(waitSec);
                threadWorker.Start();

                while ((timeOut.Subtract(DateTime.Now).TotalMilliseconds > 0) && (resultOrig is null) && (threadWorker.ThreadState.IsState(ThreadState.Stopped) == false))
                {
                    Task.Delay(100).Wait();
                }

                if (resultOrig is null)
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
                        resultOrig = new destination_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout; (EXIT21673)" };
                    }
                }

                if (resultOrig != null)
                    break;
                else
                    Task.Delay(ServerAccess.RetryIntervalSec * 1000).Wait();
            }

            return resultOrig;

            void GetOriginThreadWorking()
            {
                try
                {
                    string token = Security.AccessToken;

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        resultOrig = null;
                    }
                    else
                    {
                        // CYA-DEBUG --> Temporary Hard Code From Origin as "MEL"
                        destination_status retOrig = _serverAccess.Soap.Destination(token, "");
                        //destination_status retDest = _serverAccess.Soap.Destination(token, "");
                        //seat_status retSeat = _serverAccess.Soap.Seat((.Destination(token, "");
                        //seat_status retSeat = _serverAccess.Soap.SeatConfirm((.Destination(token, "");

                        //transcomplete_status tranCompState = _serverAccess.Soap.TransComplete(.Destination(token, "");
                        //transcomplete_status tranCompState = _serverAccess.Soap.UpdateKioskCash(.Destination(token, "");
                        //transcomplete_status tranCompState = _serverAccess.Soap.UpdateKioskCoin(.Destination(token, "");

                        resultOrig = retOrig;
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, classNMethodName: "GetOriginExecution.GetOriginThreadWorking");
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
