using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
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
    /// ClassCode:EXIT26.06
    /// </summary>
    public class AxCTGetBusCompany : IAxCommand<UIxGnBTnGAck<boardingcompany_status>>, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        //Note : Tag must be designed in verb sentence
        private string _domainEntityTag = "Get(ticket collection) bus company list from web";
        private int _maxWebAccessTimeSec = 112;
        private TimeSpan _webSendTimeout = new TimeSpan(0,0,30);
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

        /// <summary>
        /// FuncCode:EXIT26.0601
        /// </summary>
        public AxCTGetBusCompany(string processId, Guid? netProcessId, AppCallBackDelg callBackEvent)
        {
            ProcessId = processId;
            NetProcessId = netProcessId;
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
        /// FuncCode:EXIT26.0602
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
                /////RaiseCallBack(new UIxGnBTnGAck<boardingcompany_status>(NetProcessId, ProcessId, new Exception($@"Timeout, Operation aborted when -{_domainEntityTag}-; (EXIT26.0602.EX01)", ex)), CallBackEvent);

                // Note : Because Execute() will be run in a new thread, so throw ThreadAbortException when this exception found
                throw new Exception($@"ThreadAbortException when -{_domainEntityTag}-", ex);
            }
            catch (Exception ex)
            {
                _processStopped = true;
                _log?.LogError(LogChannel, "-", ex, "EX05", _exeMethodTag, netProcessId: NetProcessId);
                RaiseCallBack(new UIxGnBTnGAck<boardingcompany_status>(NetProcessId, ProcessId, new Exception($@"Error when -{_domainEntityTag}-; {ex.Message} ;(EXIT26.0602.EX05); Error Type: {ex.GetType().Name}", ex)), CallBackEvent);
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
        /// FuncCode:EXIT26.0605
        /// </summary>
        private void DoExe()
        {
            bool isNetworkTimeout = false;
            bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

            AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

            boardingcompany_status webResultData = null;

            if (isWebServiceDetected)
                webResultData = ProceedWebDataTransaction(out isNetworkTimeout, _maxWebAccessTimeSec);
            else
                webResultData = new boardingcompany_status() { code = ServerAccess.WebAccessError, msg = "Website not found" };

            //For Data Execution Code => 0: Success; 20: Invalid Token; 21: Token Expired
            if ((webResultData != null) && ((webResultData.code == 20 || webResultData.code == 21)))
            {
                if (Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication) == true)
                {
                    isNetworkTimeout = false;
                    webResultData = ProceedWebDataTransaction(out isNetworkTimeout, _maxWebAccessTimeSec);
                }
                else
                {
                    if (networkTimeout == true)
                        webResultData = new boardingcompany_status() { code = ServerAccess.NetworkTimeout, msg = $@"Fail login to web server; Web connection timeout; Fail to login web server when -{_domainEntityTag}-; (EXIT26.0605.X1)" };

                    else if (isValidAuthentication == false)
                        webResultData = new boardingcompany_status() { code = ServerAccess.InvalidAuthentication, msg = $@"Fail login to web server; Invalid authentication when -{_domainEntityTag}-; (EXIT26.0605.X2)" };

                    else
                        webResultData = new boardingcompany_status() { code = ServerAccess.WebAccessError, msg = "Fail login to web server; (EXIT26.0605.X3)" };
                }
            }

            if (webResultData?.code == 0)
            {
                if (webResultData.details?.Length > 0)
                { /*By Pass */ }
                else
                {
                    webResultData = new boardingcompany_status() { code = ServerAccess.NoDetailFound, msg = $@"Record not found when -{_domainEntityTag}- (EXIT26.0605.X5)" };
                }
            }
            else
            {
                if (webResultData == null)
                {
                    if (isNetworkTimeout)
                        webResultData = new boardingcompany_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout(IV); (EXIT26.0605.X6)" };
                    else
                        webResultData = new boardingcompany_status() { code = ServerAccess.WebAccessError, msg = "Fail to read from web; (EXIT26.0605.X9)" };
                }
                if (string.IsNullOrWhiteSpace(webResultData.msg))
                    webResultData.msg = $@"Unknown error when -{_domainEntityTag}-; (EXIT26.0605.X9)";
            }

            if (webResultData.code == 0)
            {
                RaiseCallBack(new UIxGnBTnGAck<boardingcompany_status>(NetProcessId, ProcessId, webResultData), CallBackEvent);
            }
            else
            {
                RaiseCallBack(new UIxGnBTnGAck<boardingcompany_status>(NetProcessId, ProcessId, new Exception($@"Error when -{_domainEntityTag}-; {webResultData?.msg}")), CallBackEvent);
            }
        }

        /// <summary>
        /// FuncCode:EXIT26.0607
        /// </summary>
        private boardingcompany_status ProceedWebDataTransaction(out bool isNetworkTimeout, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;
            Exception error = null;

            boardingcompany_status retData = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                retData = null;

                _webDataTransWorker?.AbortRequest(out _, 500);
                _webDataTransWorker = new RunThreadMan(new ThreadStart(WebDataTransactionThreadWorking), "AxCTGetBusCompany.ProceedWebDataTransaction", waitSec, LogChannel);
                _webDataTransWorker.WaitUntilCompleted();

                if (retData is null)
                {
                    if ((retryInx + 1) == maxRetryTimes)
                    {
                        isNetworkTimeout = true;

                        if (error != null)
                        {
                            _log?.LogError(LogChannel, "-", new Exception($@"{error.Message}; (EXIT26.0607.X1)", error), "EX01", classNMethodName: "AxCTGetBusCompany.ProceedWebDataTransaction");
                            retData = new boardingcompany_status() { code = ServerAccess.WebAccessError, msg = $@"{error.Message}; (EXIT26.0607.X1)" };
                        }
                        else
                            retData = new boardingcompany_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout(V); (EXIT26.0607.X2)" };
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
            /// FuncCode:EXIT26.0607A
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
                        error = new Exception("Invalid security token(empty); (EXIT26.0607A.X1)");
                    }
                    else
                    {
                        boardingcompany_status retDest = Soap.BoardingCompany(token);
                        retData = retDest;
                    }
                }
                catch (ThreadAbortException) 
                {
                    string tt2 = "*";
                }
                catch (Exception ex)
                {
                    error = new Exception($@"{ex.Message}; (EXIT26.0607A.EX01)", ex);
                    _log?.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "AxCTGetBusCompany.ProceedWebDataTransaction");
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT26.0610
        /// </summary>
        private void RaiseCallBack(UIxGnBTnGAck<boardingcompany_status> uixData, AppCallBackDelg callBackEvent)
        {
            if ((_callBackFlag == true) || (callBackEvent is null) || (_processStopped))
                return;

            try
            {
                callBackEvent.Invoke(uixData);

                _log?.LogText(LogChannel, ProcessId, uixData, "A01", $@"AxCTGetBusCompany.RaiseCallBack");
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, ProcessId, new WithDataException(ex.Message, ex, uixData), "EX01", "AxCTGetBusCompany.RaiseCallBack", netProcessId: NetProcessId);
            }
            finally
            {
                _callBackFlag = true;
            }
        }

        private void ShutDown()
        {
            _webDataTransWorker?.AbortRequest(out _, 500);
            
            try
            {
                _soap?.Close();
            }
            catch { }
            CallBackEvent = null;
            _soap = null;
            _log = null;
        }

        public void Dispose()
        {
            ShutDown();
        }
    }
}
