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
    public class GetDepartSeatListExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DepartSeatListCommand _command = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            //UIDepartSeatListAck uiSeatList;
            IKioskMsg uiSeatList;

            try
            {
                _command = (DepartSeatListCommand)commPack.Command;

                bool isNetworkTimeout = false;
                bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

                seat_status seatStt = null;

                if (isWebServiceDetected)
                    seatStt = GetSeatList(_command.DepartTripId, _command.DepartTripNo, _command.DepartDate, _command.DepartVehicleTripDate, 
                        _command.DepartFromStationCode, _command.DepartToStationCode, _command.DepartTimePosi, out isNetworkTimeout, 50);
                else
                    seatStt = new seat_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (II)" };

                //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                if ((seatStt != null) && ((seatStt.code == 20 || seatStt.code == 21)))
                {
                    if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                    {
                        isNetworkTimeout = false;
                        seatStt = GetSeatList(_command.DepartTripId, _command.DepartTripNo, _command.DepartDate, _command.DepartVehicleTripDate,
                        _command.DepartFromStationCode, _command.DepartToStationCode, _command.DepartTimePosi, out isNetworkTimeout, 50);
                    }
                    else
                    {
                        if (networkTimeout == true)
                            seatStt = new seat_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (III)" };
                        else
                            seatStt = new seat_status() { code = ServerAccess.InvalidAuthentication, msg = "Invalid authentication" };
                    }
                }

                if (seatStt?.code == 0)
                {
                    uiSeatList = new UIDepartSeatListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, seatStt);
                    commPack.UpSertResult(false, uiSeatList);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, "GetDepartSeatListExecution.Execute:A02", uiSeatList);
                }
                else
                {
                    if (seatStt == null)
                        seatStt = new seat_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (IV)" };

                    //uiSeatList = new UIDepartSeatListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, seatStt)
                    //{ ErrorMessage = (string.IsNullOrWhiteSpace(seatStt.msg) == true ? "Unknown error when reading depart seat list (EXIT21638)" : seatStt.msg) };

                    uiSeatList = new UIDepartSeatListErrorResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, $@"Unable to read seat data from database; (EXIT21638)");

                    Log.LogText(LogChannel, commPack.ProcessId, $@"Seat API Return Code: {seatStt.code}; (EXIT21638); Unable to read seat from database ",
                        "EX05", "GetDepartSeatListExecution.Execute", AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);

                    commPack.UpSertResult(true, uiSeatList, uiSeatList.ErrorMessage);

                    whenCompletedSendEvent((isNetworkTimeout == true ? ResultStatus.NetworkTimeout : ResultStatus.Fail),
                        _commandPack.NetProcessId, _commandPack.ProcessId, "GetDepartSeatListExecution.Execute:EX05", uiSeatList, new Exception(uiSeatList.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                //uiSeatList = new UIDepartSeatListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21639)" };
                //commPack.UpSertResult(true, uiSeatList, errorMessage: uiSeatList.ErrorMessage);
                //whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, "GetDepartSeatListExecution.Execute:A11", uiSeatList, new Exception(uiSeatList.ErrorMessage));

                uiSeatList = new UIDepartSeatListErrorResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, $@"Unable to read seat data from database; (EXIT21639)") ;

                Log.LogText(LogChannel, commPack.ProcessId, $@"{ex.Message}; (EXIT21639); Unable to read seat from database ",
                        "EX05", "GetDepartSeatListExecution.Execute", AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);

                commPack.UpSertResult(true, uiSeatList, errorMessage: uiSeatList.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, "GetDepartSeatListExecution.Execute:A11", uiSeatList, new Exception(uiSeatList.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, IKioskMsg uiSeatListX,
                Exception error = null)
            {
                if (eventSent)
                    return;

                SendMessageEventArgs compEv;

                if ((error == null) && (resultState == ResultStatus.Success))
                {
                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiSeatListX);

                    Log.LogText(LogChannel, processId, compEv,
                        "A01", "GetDepartSeatListExecution.whenCompletedSendEvent", netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }
                else
                {
                    if (resultState == ResultStatus.Success)
                        resultState = ResultStatus.ErrorFound;

                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiSeatListX)
                    {
                        Message = ((error?.Message) is null) ? "Unknown error found;" : error.Message
                    };

                    Log.LogText(LogChannel, processId, $@"Start - whenCompletedSendEvent; With Error: {compEv.Message}",
                        "A05", "GetDepartSeatListExecution.whenCompletedSendEvent", netProcessId: netProcessId);
                }

                _serverAccess.RaiseOnSendMessage(compEv, lineTag);
                eventSent = true;

                Log.LogText(LogChannel, processId, "End - whenCompletedSendEvent", "A10", "GetDepartSeatListExecution.whenCompletedSendEvent", netProcessId: netProcessId);
            }
        }

        private seat_status GetSeatList(string tripId, string tripNo, string departDate, string tripDate, string fromStationCode, string toStationCode, short timePosi,
            out bool isNetworkTimeout, int waitSec = 50, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;

            seat_status resultSeat = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                resultSeat = null;
                Thread threadWorker = new Thread(new ThreadStart(GetSeatListThreadWorking));
                threadWorker.IsBackground = true;

                DateTime timeOut = DateTime.Now.AddSeconds(waitSec);
                threadWorker.Start();

                while ((timeOut.Subtract(DateTime.Now).TotalMilliseconds > 0) && (resultSeat is null) && (threadWorker.ThreadState.IsState(ThreadState.Stopped) == false))
                {
                    Task.Delay(100).Wait();
                }

                if (resultSeat is null)
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
                        resultSeat = new seat_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout; (EXIT21637)" };
                    }
                }

                if (resultSeat != null)
                    break;
                else
                    Task.Delay(ServerAccess.RetryIntervalSec * 1000).Wait();
            }

            return resultSeat;

            void GetSeatListThreadWorking()
            {
                try
                {
                    string token = Security.AccessToken;
                    string kioskId = Security.KioskID;

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        resultSeat = null;
                    }
                    else
                    {
                        seat_status retSeat = _serverAccess.Soap.Seat(kioskId ?? "", token ?? "", tripId ?? "", tripNo ?? "", departDate ?? "", tripDate ?? "", fromStationCode ?? "", toStationCode ?? "", timePosi);
                        resultSeat = retSeat;
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, classNMethodName: "GetDepartSeatListExecution.GetSeatListThreadWorking");
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
