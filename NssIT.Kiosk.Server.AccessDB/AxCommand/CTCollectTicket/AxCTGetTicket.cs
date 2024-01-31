using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.AxCommand.CTCollectTicket
{
    /// <summary>
    /// ClassCode:EXIT26.07
    /// </summary>
    public class AxCTGetTicket : IAxCommand<UIxGnBTnGAck<boardingqueryticket_status>>, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        //Note : Tag must be designed in verb sentence
        private string _domainEntityTag = "Get (ticket collection) ticket from website";
        private int _maxWebAccessTimeSec = 112;
        private TimeSpan _webSendTimeout = new TimeSpan(0, 0, 30);
        private TimeSpan _webReceiveTimeout = new TimeSpan(0, 1, 20);
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public string ProcessId { get; private set; }
        public Guid? NetProcessId { get; private set; }
        public AppCallBackDelg CallBackEvent { get; private set; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private bool _callBackFlag = false;
        private DbLog _log = DbLog.GetDbLog();
        private bool _processStopped = false;
        private static RunThreadMan _webDataTransWorker = null;

        private string _prmTicketNo = "";
        private string _prmCompanyCode = "";
        private DateTime _prmDepartDate = DateTime.MaxValue;

        /// <summary>
        /// FuncCode:EXIT26.0701
        /// </summary>
        public AxCTGetTicket(string processId, Guid? netProcessId, string ticketNo, string companyCode, DateTime departDate,
            AppCallBackDelg callBackEvent)
        {
            ProcessId = processId;
            NetProcessId = netProcessId;
            _prmTicketNo = (ticketNo ?? "").Trim();
            _prmCompanyCode = (companyCode ?? "").Trim();
            _prmDepartDate = departDate;
            CallBackEvent = callBackEvent;
        }

        private ServiceSoapClient _soap = null;
        public ServiceSoapClient Soap
        {
            get
            {
                if (_soap == null)
                {
                    BasicHttpBinding binding = new BasicHttpBinding();
                    binding.MaxReceivedMessageSize = 2097150;
                    binding.SendTimeout = _webSendTimeout;
                    binding.ReceiveTimeout = _webReceiveTimeout;
                    EndpointAddress address = new EndpointAddress(AppDecorator.Config.Setting.GetSetting().WebServiceURL);
                    _soap = new ServiceSoapClient(binding, address);
                }
                return _soap;
            }
        }

        /// <summary>
        /// FuncCode:EXIT26.0702
        /// </summary>
        public void Execute()
        {
            string _exeMethodTag = $@"{this.GetType().Name}.Execute()-AxExeTag";
            try
            {
                DoExe();
            }
            catch (ThreadAbortException ex)
            {
                _processStopped = true;
                _log?.LogError(LogChannel, "-", ex, "EX01", _exeMethodTag, netProcessId: NetProcessId);
                /////RaiseCallBack(new UIxGnBTnGAck<boardingqueryticket_status>(NetProcessId, ProcessId, new Exception($@"Timeout, Operation aborted when -{_domainEntityTag}-; (EXIT26.0702.EX01)", ex)), CallBackEvent);

                // Note : Because Execute() will be run in a new thread, so throw ThreadAbortException when this exception found
                throw new Exception($@"ThreadAbortException when -{_domainEntityTag}-", ex);
            }
            catch (Exception ex)
            {
                _processStopped = true;
                _log?.LogError(LogChannel, "-", ex, "EX05", _exeMethodTag, netProcessId: NetProcessId);
                RaiseCallBack(new UIxGnBTnGAck<boardingqueryticket_status>(NetProcessId, ProcessId, new Exception($@"Error when -{_domainEntityTag}-; {ex.Message} ;(EXIT26.0702.EX05); Error Type: {ex.GetType().Name}", ex)), CallBackEvent);
            }
            finally
            {
                _processStopped = true;
                ShutDown();
            }
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        }

        /// <summary>
        /// FuncCode:EXIT26.0705
        /// </summary>
        private void DoExe()
        {
            bool isNetworkTimeout = false;
            bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

            AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

            boardingqueryticket_status webResultData = null;

            if (isWebServiceDetected)
                webResultData = ProceedWebDataTransaction(out isNetworkTimeout, _maxWebAccessTimeSec);
            else
                webResultData = new boardingqueryticket_status() { code = ServerAccess.WebAccessError, msg = "Website not found" };

            //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
            if ((webResultData != null) && (webResultData.code == 20 || webResultData.code == 21))
            {
                if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                {
                    isNetworkTimeout = false;
                    webResultData = ProceedWebDataTransaction(out isNetworkTimeout, _maxWebAccessTimeSec);
                }
                else
                {
                    if (networkTimeout == true)
                        webResultData = new boardingqueryticket_status() { code = ServerAccess.NetworkTimeout, msg = $@"Fail login to web server; Web connection timeout; Fail to login web server when -{_domainEntityTag}-; (EXIT26.0705.X1)" };

                    else if (isValidAuthentication == false)
                        webResultData = new boardingqueryticket_status() { code = ServerAccess.InvalidAuthentication, msg = $@"Fail login to web server; Invalid authentication when -{_domainEntityTag}-; (EXIT26.0705.X2)" };

                    else
                        webResultData = new boardingqueryticket_status() { code = ServerAccess.WebAccessError, msg = "Fail login to web server; (EXIT26.0705.X3)" };
                }
            }

            if (webResultData?.code == 0)
            {
                if (string.IsNullOrWhiteSpace(webResultData.tripno) == false)
                { /*By Pass */ }
                else
                {
                    webResultData = new boardingqueryticket_status() { code = ServerAccess.NoDetailFound, msg = $@"Trip not found when -{_domainEntityTag}- (EXIT26.0705.X5)" };
                }
            }
            else
            {
                if (webResultData == null)
                {
                    if (isNetworkTimeout)
                        webResultData = new boardingqueryticket_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout(IV); (EXIT26.0705.X6)" };
                    else
                        webResultData = new boardingqueryticket_status() { code = ServerAccess.WebAccessError, msg = "Fail to read from web; (EXIT26.0705.X9)" };
                }
                if (string.IsNullOrWhiteSpace(webResultData.msg))
                    webResultData.msg = $@"Unknown error when -{_domainEntityTag}-; (EXIT26.0705.X9)";
            }

            if (webResultData.code == 0)
            {
                RaiseCallBack(new UIxGnBTnGAck<boardingqueryticket_status>(NetProcessId, ProcessId, webResultData), CallBackEvent);
            }
            else
            {
                int errCode = webResultData.code;
                string errMsg = webResultData?.msg ?? $@"Unknown error when -{_domainEntityTag}-; (EXIT26.0705.X10)";

                if (errMsg.Contains("(404) Not Found") || errMsg.Contains("(503) Server Unavailable"))
                    errCode = 61;

                RaiseCallBack(new UIxGnBTnGAck<boardingqueryticket_status>(NetProcessId, ProcessId, new TicketNoFoundException($@"Error when -{_domainEntityTag}-; Website Error; {webResultData?.msg}", errCode)), CallBackEvent);
            }
        }

        /// <summary>
        /// FuncCode:EXIT26.0707
        /// </summary>
        private boardingqueryticket_status ProceedWebDataTransaction(out bool isNetworkTimeout, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;
            Exception error = null;

            boardingqueryticket_status retData = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                retData = null;

                _webDataTransWorker?.AbortRequest(out _, 500);
                _webDataTransWorker = new RunThreadMan(new ThreadStart(WebDataTransactionThreadWorking), "AxCTGetTicket.ProceedWebDataTransaction", waitSec, LogChannel);
                _webDataTransWorker.WaitUntilCompleted();

                //while ((retData is null) && (_webDataTransWorker.IsEnd == false))
                //    Thread.Sleep(100);

                if (retData is null)
                {
                    if ((retryInx + 1) == maxRetryTimes)
                    {
                        isNetworkTimeout = true;

                        if (error != null)
                        {
                            _log?.LogError(LogChannel, "-", new Exception($@"{error.Message}; (EXIT26.0707.X1)", error), "EX01", classNMethodName: "AxCTGetTicket.ProceedWebDataTransaction");
                            retData = new boardingqueryticket_status() { code = ServerAccess.WebAccessError, msg = $@"{error.Message}; (EXIT26.0707.X1)" };
                        }
                        else
                            retData = new boardingqueryticket_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout(V); (EXIT26.0707.X2)" };
                    }
                }

                if (retData != null)
                    break;

                else
                    Thread.Sleep(ServerAccess.RetryIntervalSec * 1000);

            }

            return retData;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            /// <summary>
            /// FuncCode:EXIT26.0707A
            /// </summary>
            void WebDataTransactionThreadWorking()
            {
                try
                {
                    string token = Security.AccessToken;
                    string kioskId = Security.KioskID;

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        retData = null;
                        error = new Exception("Invalid security token(empty); (EXIT26.0707A.X1)");
                    }
                    else
                    {
                        boardingqueryticket_status exeResult = null;

                        try
                        {
                            if (NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().IsDebugMode)
                            {
                                exeResult = GetDebugTestData();
                            }
                        }
                        catch { }
                        
                        if (exeResult is null)
                            exeResult = Soap.BoardingQueryTicket(kioskId, token, _prmCompanyCode, _prmDepartDate.ToString("dd/MM/yyyy"), _prmTicketNo);

                        retData = exeResult;
                    }
                }
                catch (ThreadAbortException) 
                {
                    string ttX2 = "*";
                }
                catch (Exception ex)
                {
                    error = new Exception($@"{ex.Message}; (EXIT26.0707A.EX01)", ex);
                    _log?.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "AxCTGetTicket.ProceedWebDataTransaction");
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT26.0710
        /// </summary>
        private void RaiseCallBack(UIxGnBTnGAck<boardingqueryticket_status> uixData, AppCallBackDelg callBackEvent)
        {
            if ((_callBackFlag == true) || (callBackEvent is null) || (_processStopped))
                return;

            try
            {
                callBackEvent.Invoke(uixData);

                _log?.LogText(LogChannel, ProcessId, uixData, "A01", $@"AxCTGetTicket.RaiseCallBack");
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, ProcessId, new WithDataException(ex.Message, ex, uixData), "EX01", "AxCTGetTicket.RaiseCallBack", netProcessId: NetProcessId);
            }
            finally
            {
                _callBackFlag = true;
            }
        }

        private void ShutDown()
        {
            _webDataTransWorker?.AbortRequest(out _, 300);

            try
            {
                _soap?.Close();
            }
            catch { }
            CallBackEvent = null;
            _soap = null;
            _log = null;
        }

        private boardingqueryticket_status GetDebugTestData()
        {
            //CYA-DEBUG .. Must be remark upon deployment
            return new boardingqueryticket_status()
            {
                code = 0,
                contact = "777666555222",
                facilitycharge = 0.7M,
                from = "MEL",
                fromdesn = "MELAKA SENTRAL",
                ic = "888777666555",
                name = "CYA_DEBUG -- .....",
                operouteid = "MEL-LAR-WLD-CTY",
                qrcharge = 0.6M,
                seatno = "35",
                seattype = "A",
                sellingprice = 18.0M,
                to = "LAR",
                todesn = "JB - LARKIN",
                tripdate = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy"),
                tripno = "MLWC08"
            };
            //---------------------------------------------------------------
            return null;
        }

        public void Dispose()
        {
            ShutDown();
        }
    }
}