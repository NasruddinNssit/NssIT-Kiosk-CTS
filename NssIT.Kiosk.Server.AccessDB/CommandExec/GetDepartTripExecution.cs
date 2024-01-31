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
    public class GetDepartTripExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DepartTripListCommand _command = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIDepartTripListAck uiTripList;

            try
            {
                _command = (DepartTripListCommand)commPack.Command;

                bool isNetworkTimeout = false;
                bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

                trip_status trip = null;

                if (isWebServiceDetected)
                    trip = GetTrip(_command.PassengerTravelDate, _command.FromStationCode, _command.ToStationCode, out isNetworkTimeout, 90);
                else
                    trip = new trip_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (II)" };

                //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                if ((trip != null) && ((trip.code == 20 || trip.code == 21)))
                {
                    if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                    {
                        isNetworkTimeout = false;
                        trip = GetTrip(_command.PassengerTravelDate, _command.FromStationCode, _command.ToStationCode, out isNetworkTimeout, 90);
                    }
                    else
                    {
                        if (networkTimeout == true)
                            trip = new trip_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (III)" };
                        else
                            trip = new trip_status() { code = ServerAccess.InvalidAuthentication, msg = "Invalid authentication" };
                    }
                }

                if (trip?.code == 0)
                {
                    uiTripList = new UIDepartTripListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, trip, AppDecorator.Common.TravelMode.DepartOnly);
                    commPack.UpSertResult(false, uiTripList);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, "GetTripExecution.Execute:A02", uiTripList);
                }
                else
                {
                    if (trip == null)
                        trip = new trip_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (IV)" };

                    uiTripList = new UIDepartTripListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, trip, AppDecorator.Common.TravelMode.DepartOnly)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(trip.msg) == true ? "Unknown error when reading destination trip (EXIT21632)" : trip.msg) };

                    commPack.UpSertResult(true, uiTripList, uiTripList.ErrorMessage);

                    whenCompletedSendEvent((isNetworkTimeout == true ? ResultStatus.NetworkTimeout : ResultStatus.Fail),
                        _commandPack.NetProcessId, _commandPack.ProcessId, "GetTripExecution.Execute:A05", uiTripList, new Exception(uiTripList.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiTripList = new UIDepartTripListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null, AppDecorator.Common.TravelMode.DepartOnly) { ErrorMessage = $@"{ex.Message}; (EXIT21633)" };
                commPack.UpSertResult(true, uiTripList, errorMessage: uiTripList.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, "GetTripExecution.Execute:A11", uiTripList, new Exception(uiTripList.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, UIDepartTripListAck uiTripListX,
                Exception error = null)
            {
                if (eventSent)
                    return;

                SendMessageEventArgs compEv;

                if ((error == null) && (resultState == ResultStatus.Success))
                {
                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiTripListX);

                    Log.LogText(LogChannel, processId, compEv,
                        "A01", "GetTripExecution.whenCompletedSendEvent", netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }
                else
                {
                    if (resultState == ResultStatus.Success)
                        resultState = ResultStatus.ErrorFound;

                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiTripListX)
                    {
                        Message = ((error?.Message) is null) ? "Unknown error found;" : error.Message
                    };

                    Log.LogText(LogChannel, processId, $@"Start - whenCompletedSendEvent; Error: {compEv.Message}",
                        "A02", "GetTripExecution.whenCompletedSendEvent", netProcessId: netProcessId);
                }

                _serverAccess.RaiseOnSendMessage(compEv, lineTag);
                eventSent = true;

                Log.LogText(LogChannel, processId, "End - whenCompletedSendEvent", "A10", "GetTripExecution.whenCompletedSendEvent", netProcessId: netProcessId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tripDate"></param>
        /// <param name="fromStation"></param>
        /// <param name="toStation"></param>
        /// <param name="isNetworkTimeout"></param>
        /// <param name="waitSec"></param>
        /// <param name="maxRetryTimes"></param>
        /// <returns></returns>
        private trip_status GetTrip(DateTime tripDate, string fromStation, string toStation, out bool isNetworkTimeout, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;

            trip_status resultDest = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                resultDest = null;
                Thread threadWorker = new Thread(new ThreadStart(GetTripThreadWorking));
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
                        resultDest = new trip_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout; (EXIT21631)" };
                    }
                }

                if (resultDest != null)
                    break;
                else
                    Task.Delay(ServerAccess.RetryIntervalSec * 1000).Wait();
            }

            return resultDest;

            void GetTripThreadWorking()
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
                        trip_status retDest = _serverAccess.Soap.Trip(kioskId, token, tripDate.ToString("dd/MM/yyyy"), fromStation ?? "", toStation ?? "");
                        resultDest = retDest;
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, classNMethodName: "GetDepartTripExecution.GetTripThreadWorking");
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