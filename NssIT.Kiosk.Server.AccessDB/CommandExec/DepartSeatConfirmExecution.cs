using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.Config;
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
    public class DepartSeatConfirmExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;
        private DepartSeatConfirmCommand _command = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIDepartSeatConfirmResult uiConfirmResult;

            try
            {
                _command = (DepartSeatConfirmCommand)commPack.Command;

                if ((_command.PassengerSeatDetail is null) || (_command.PassengerSeatDetail.Length == 0))
                {
                    throw new Exception("Invalid depart seat info; (EXIT21643)");
                }

                bool isNetworkTimeout = false;
                bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

                seatconfirm_status confirmResult = null;

                if (isWebServiceDetected)
                    confirmResult = ConfirmSeat(_command.TripId,
                        _command.TripDate, _command.DepartDate, 
                        _command.DepartTime, _command.BusType, 
                        _command.FromStationCode, _command.ToStationCode, 
                        _command.AdultPrice, _command.AdultExtra, 
                        _command.AdultDisc, _command.TerminalCharge, 
                        _command.OnlineQRCharge, _command.Insurance, 
                        _command.TripCode, _command.PickLocationCode, 
                        _command.PickLocationDesn, _command.PickTime, 
                        _command.DropLocationCode, _command.DropLocationDesn, 
                        _command.TotalAmount, _command.PassengerSeatDetail 
                        , out isNetworkTimeout, 50);
                else
                    confirmResult = new seatconfirm_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (II)" };

                //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                if ((confirmResult != null) && ((confirmResult.code == 20 || confirmResult.code == 21)))
                {
                    if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                    {
                        isNetworkTimeout = false;
                        confirmResult = ConfirmSeat(_command.TripId,
                            _command.TripDate, _command.DepartDate,
                            _command.DepartTime, _command.BusType,
                            _command.FromStationCode, _command.ToStationCode,
                            _command.AdultPrice, _command.AdultExtra,
                            _command.AdultDisc, _command.TerminalCharge,
                            _command.OnlineQRCharge, _command.Insurance,
                            _command.TripCode, _command.PickLocationCode,
                            _command.PickLocationDesn, _command.PickTime,
                            _command.DropLocationCode, _command.DropLocationDesn,
                            _command.TotalAmount, _command.PassengerSeatDetail
                            , out isNetworkTimeout, 50);
                    }
                    else
                    {
                        if (networkTimeout == true)
                            confirmResult = new seatconfirm_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (III)" };
                        else
                            confirmResult = new seatconfirm_status() { code = ServerAccess.InvalidAuthentication, msg = "Invalid authentication" };
                    }
                }

                if (confirmResult?.code == 0)
                {
                    uiConfirmResult = new UIDepartSeatConfirmResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, confirmResult);
                    commPack.UpSertResult(false, uiConfirmResult);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, "DepartSeatConfirmExecution.Execute:A02", uiConfirmResult);
                }
                else
                {
                    if (confirmResult == null)
                        confirmResult = new seatconfirm_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (IV)" };

                    uiConfirmResult = new UIDepartSeatConfirmResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, confirmResult)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(confirmResult.msg) == true ? "Unknown error when confirm depart seat (EXIT21640)" : confirmResult.msg) };

                    commPack.UpSertResult(true, uiConfirmResult, uiConfirmResult.ErrorMessage);

                    whenCompletedSendEvent((isNetworkTimeout == true ? ResultStatus.NetworkTimeout : ResultStatus.Fail),
                        _commandPack.NetProcessId, _commandPack.ProcessId, "DepartSeatConfirmExecution.Execute:A05", uiConfirmResult, new Exception(uiConfirmResult.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiConfirmResult = new UIDepartSeatConfirmResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21641)" };
                commPack.UpSertResult(true, uiConfirmResult, errorMessage: uiConfirmResult.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, "DepartSeatConfirmExecution.Execute:A11", uiConfirmResult, new Exception(uiConfirmResult.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, UIDepartSeatConfirmResult uiSeatConfirmResult,
                Exception error = null)
            {
                if (eventSent)
                    return;

                SendMessageEventArgs compEv;

                if ((error == null) && (resultState == ResultStatus.Success))
                {
                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiSeatConfirmResult);

                    Log.LogText(LogChannel, processId, compEv,
                        "A01", "DepartSeatConfirmExecution.whenCompletedSendEvent", netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }
                else
                {
                    if (resultState == ResultStatus.Success)
                        resultState = ResultStatus.ErrorFound;

                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiSeatConfirmResult)
                    {
                        Message = ((error?.Message) is null) ? "Unknown error found;" : error.Message
                    };

                    Log.LogText(LogChannel, processId, compEv,
                        "A02", "DepartSeatConfirmExecution.whenCompletedSendEvent", AppDecorator.Log.MessageType.Error, netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }

                _serverAccess.RaiseOnSendMessage(compEv, lineTag);
                eventSent = true;

                Log.LogText(LogChannel, processId, "End - whenCompletedSendEvent", "A10", "DepartSeatConfirmExecution.whenCompletedSendEvent", netProcessId: netProcessId);
            }
        }

        private seatconfirm_status ConfirmSeat(
                string tripId,
                string tripDate,
                string departDate,
                string departTime,
                string busType,
                string fromStationCode,
                string toStationCode,
                decimal adultPrice,
                string adultExtra,
                decimal adultDisc,
                decimal terminalCharge,
                decimal onlineQrCharge,
                decimal insurance,
                int tripCode,
                string pickLocationCode,
                string pickLocationDesn,
                string pickTime,
                string dropLocationCode,
                string dropLocationDesn,
                decimal totalAmount,
                CustSeatDetail[] custSeatDetailArr,
            out bool isNetworkTimeout, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;

            seatconfirm_status resultState = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                resultState = null;
                Thread threadWorker = new Thread(new ThreadStart(ConfirmSeatThreadWorking));
                threadWorker.IsBackground = true;

                DateTime timeOut = DateTime.Now.AddSeconds(waitSec);
                threadWorker.Start();

                while ((timeOut.Subtract(DateTime.Now).TotalMilliseconds > 0) && (resultState is null) && (threadWorker.ThreadState.IsState(ThreadState.Stopped) == false))
                {
                    Task.Delay(100).Wait();
                }

                if (resultState is null)
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
                        resultState = new seatconfirm_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout; (EXIT21642)" };
                    }
                }

                if (resultState != null)
                    break;
                else
                    Task.Delay(ServerAccess.RetryIntervalSec * 1000).Wait();
            }

            return resultState;

            void ConfirmSeatThreadWorking()
            {
                try
                {
                    string token = Security.AccessToken;
                    string kioskId = Security.KioskID;

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        resultState = null;
                    }
                    else
                    {
                        Setting sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                        cust_seat_detail[] seatDetailArr = new cust_seat_detail[custSeatDetailArr.Length];

                        int arrInx = -1;
                        foreach(CustSeatDetail row in custSeatDetailArr)
                        {
                            arrInx++;
                            seatDetailArr[arrInx] = new cust_seat_detail() { contact = row.Contact ?? "", ic = row.CustIC ?? "", name = row.CustName ?? "", seatid = row.SeatId, desn = row.Desn ?? "" };
                        }

                        seatconfirm_status confirmState = _serverAccess.Soap.SeatConfirm(kioskId, token, tripId ?? "", tripDate ?? "", departDate ?? "",
                            departTime ?? "", busType ?? "", fromStationCode ?? "", toStationCode ?? "", adultPrice, adultExtra ?? "", adultDisc, 
                            terminalCharge, onlineQrCharge, insurance, tripCode, pickLocationCode ?? "", pickLocationDesn ?? "",
                            pickTime ?? "", dropLocationCode ?? "", dropLocationDesn ?? "", totalAmount, seatDetailArr);

                        if (sysSetting.IsDebugMode)
                        {
                            if (confirmState.code != 0)
                            {
                                /////CYA-DEBUG
                                //confirmState = new seatconfirm_status() { code = 0, msg = "", transaction_no = $@"TXS{DateTime.Now.ToString("ddHHMMSS")}" };
                            }
                        }

                        resultState = confirmState;
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, classNMethodName: "DepartSeatConfirmExecution.ConfirmSeatThreadWorking");
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
