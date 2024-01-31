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
    /// <summary>
    /// Customer Info Update will release seat in case fail to update info.
    /// </summary>
    public class DepartCustInfoUpdateELSEReleaseSeatExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;
        private DepartCustInfoUpdateELSEReleaseSeatCommand _command = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIDepartCustInfoUpdateResult uiUpdateResult;

            try
            {
                _command = (DepartCustInfoUpdateELSEReleaseSeatCommand)commPack.Command;

                if ((_command.PassengerSeatDetail is null) || (_command.PassengerSeatDetail.Length == 0))
                {
                    throw new Exception("Invalid depart customer/seat info; (EXIT21621)");
                }

                bool isNetworkTimeout = false;
                bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

                changeticketname_status changeStatus = null;

                if (isWebServiceDetected)
                    changeStatus = UpdateCustomerInfo(
                        _command.TransactionNo, _command.TripDate, _command.DepartDate,
                        _command.PassengerSeatDetail,
                        out isNetworkTimeout, 120);
                else
                    changeStatus = new changeticketname_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (II)" };

                //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                if ((changeStatus != null) && ((changeStatus.code == 20 || changeStatus.code == 21)))
                {
                    if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                    {
                        isNetworkTimeout = false;
                        changeStatus = UpdateCustomerInfo(
                        _command.TransactionNo, _command.TripDate, _command.DepartDate,
                        _command.PassengerSeatDetail,
                        out isNetworkTimeout, 120);
                    }
                    else
                    {
                        if (networkTimeout == true)
                            changeStatus = new changeticketname_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (III)" };
                        else
                            changeStatus = new changeticketname_status() { code = ServerAccess.InvalidAuthentication, msg = "Invalid authentication" };
                    }
                }

                if (changeStatus?.code == 0)
                {
                    uiUpdateResult = new UIDepartCustInfoUpdateResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, changeStatus);
                    commPack.UpSertResult(false, uiUpdateResult);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, "DepartCustInfoUpdateExecution.Execute:A02", uiUpdateResult);
                }
                else
                {
                    if (changeStatus == null)
                        changeStatus = new changeticketname_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout (IV)" };

                    uiUpdateResult = new UIDepartCustInfoUpdateResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, changeStatus)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(changeStatus.msg) == true ? "Unknown error when update customer list (EXIT21622)" : changeStatus.msg) };

                    commPack.UpSertResult(true, uiUpdateResult, uiUpdateResult.ErrorMessage);

                    whenCompletedSendEvent((isNetworkTimeout == true ? ResultStatus.NetworkTimeout : ResultStatus.Fail),
                        _commandPack.NetProcessId, _commandPack.ProcessId, "DepartCustInfoUpdateExecution.Execute:A05", uiUpdateResult, new Exception(uiUpdateResult.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiUpdateResult = new UIDepartCustInfoUpdateResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21623)" };
                commPack.UpSertResult(true, uiUpdateResult, errorMessage: uiUpdateResult.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, "DepartCustInfoUpdateExecution.Execute:A11", uiUpdateResult, new Exception(uiUpdateResult.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, UIDepartCustInfoUpdateResult uiCustUpdateResult,
                Exception error = null)
            {
                if (eventSent)
                    return;

                SendMessageEventArgs compEv;

                if ((error == null) && (resultState == ResultStatus.Success))
                {
                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiCustUpdateResult);

                    Log.LogText(LogChannel, processId, compEv,
                        "A01", "DepartCustInfoUpdateExecution.whenCompletedSendEvent", netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }
                else
                {
                    if (resultState == ResultStatus.Success)
                        resultState = ResultStatus.ErrorFound;

                    compEv = new SendMessageEventArgs(netProcessId, processId, resultState, uiCustUpdateResult)
                    {
                        Message = ((error?.Message) is null) ? "Unknown error found;" : error.Message
                    };

                    Log.LogText(LogChannel, processId, compEv,
                        "A02", "DepartCustInfoUpdateExecution.whenCompletedSendEvent", AppDecorator.Log.MessageType.Error, netProcessId: netProcessId,
                        extraMsg: $@"Start - whenCompletedSendEvent; MsgObj: SendMessageEventArgs");
                }

                _serverAccess.RaiseOnSendMessage(compEv, lineTag);
                eventSent = true;

                Log.LogText(LogChannel, processId, "End - whenCompletedSendEvent", "A10", "DepartCustInfoUpdateExecution.whenCompletedSendEvent", netProcessId: netProcessId);
            }
        }

        private changeticketname_status UpdateCustomerInfo(
                string transactionNo, string tripDate, string departDate,
                CustSeatDetail[] custSeatDetailArr,
                out bool isNetworkTimeout, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;

            changeticketname_status resultState = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                resultState = null;
                Thread threadWorker = new Thread(new ThreadStart(UpdateCustThreadWorking));
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
                        resultState = new changeticketname_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout; (EXIT21624)" };
                    }
                }

                if (resultState != null)
                    break;
                else
                    Task.Delay(ServerAccess.RetryIntervalSec * 1000).Wait();
            }

            return resultState;

            void UpdateCustThreadWorking()
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

                        cust_name_detail[] custDetailArr = new cust_name_detail[custSeatDetailArr.Length];

                        int arrInx = -1;
                        foreach (CustSeatDetail row in custSeatDetailArr)
                        {
                            arrInx++;
                            custDetailArr[arrInx] = new cust_name_detail() { contact = row.Contact ?? "", ic = row.CustIC ?? "", name = row.CustName ?? "", seatid = row.SeatId };
                        }

                        changeticketname_status changeStatus = _serverAccess.Soap.ChangeTicketName(token,
                            transactionNo, tripDate, departDate, custDetailArr);

                        //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
                        if ((changeStatus.code != 0) && (changeStatus.code != 20) && (changeStatus.code != 21))
                        {
                            // Release Seat on Fail Update Customer Info
                            seatrelease_status retRState = _serverAccess.Soap.SeatRelease(kioskId, token, (transactionNo ?? ""));
                            string debugBlockStr = "";
                        }

                        resultState = changeStatus;
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "-", ex, classNMethodName: "DepartCustInfoUpdateExecution.UpdateCustThreadWorking");
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
