using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
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
    /// ClassCode:EXIT26.08
    /// </summary>
    public class AxCompleteSaleCollectTicketTransaction : IAxCommand<UIxGnBTnGAck<boardingcollectticket_status>>, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        //Note : Tag must be designed in verb sentence
        private string _domainEntityTag = "Complete Sale Collect Ticket Transaction from website";
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
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        private string _prmCompanycode = "";
        private string _prmTicketno = "";
        private DateTime _prmDepartDate = DateTime.MaxValue;
        private string _prmTripdate = "";
        private string _prmOperouteid = "";
        private string _prmTripno = "";
        private string _prmFrom = "";
        private string _prmTo = "";
        private string _prmName = "";
        private string _prmContact = "";
        private string _prmIcno = "";
        private string _prmSeatno = "";
        private string _prmSeattype = "";
        private decimal _prmSellingprice = 0.0M;
        private decimal _prmFacilitycharge = 0.0M;
        private decimal _prmQrCharge = 0.0M;
        private string _prmPaymentmethod = "";

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private PaymentType _prmTypeOfPayment = PaymentType.Unknown;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private int _prmCassette1NoteCount = 0;
        private int _prmCassette2NoteCount = 0;
        private int _prmCassette3NoteCount = 0;
        private int _prmRefundCoinAmount = 0;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        // BTnG & Credit Card
        private string _prmPaymentRefNo = "";
        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        /// <summary>
        /// FuncCode:EXIT26.0801
        /// </summary>
        public AxCompleteSaleCollectTicketTransaction(string processId, Guid? netProcessId,
            string companycode, string ticketno, DateTime departDate, 
            string tripdate, string operouteid, string tripno, 
            string from, string to, 
            string name, string contact, string icno, 
            string seatno, string seattype, 
            decimal sellingprice, decimal facilitycharge, decimal qrCharge, 
            string paymentmethod, PaymentType typeOfPayment, 
            int cassette1NoteCount, int cassette2NoteCount, int cassette3NoteCount, int refundCoinAmount, 
            string paymentRefNo, 
            AppCallBackDelg callBackEvent)
        {
            ProcessId = processId;
            NetProcessId = netProcessId;

            _prmCompanycode = companycode;
            _prmTicketno = ticketno;
            _prmDepartDate = departDate;
            _prmTripdate = tripdate;
            _prmOperouteid = operouteid;
            _prmTripno = tripno;
            _prmFrom = from;
            _prmTo = to;
            _prmName = name;
            _prmContact = contact;
            _prmIcno = icno;
            _prmSeatno = seatno;
            _prmSeattype = seattype;
            _prmSellingprice = sellingprice;
            _prmFacilitycharge = facilitycharge;
            _prmQrCharge = qrCharge;
            _prmPaymentmethod = paymentmethod;
            _prmTypeOfPayment = typeOfPayment;
            _prmCassette1NoteCount = cassette1NoteCount;
            _prmCassette2NoteCount = cassette2NoteCount;
            _prmCassette3NoteCount = cassette3NoteCount;
            _prmRefundCoinAmount = refundCoinAmount;
            _prmPaymentRefNo = paymentRefNo;

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
        /// FuncCode:EXIT26.0802
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
                /////RaiseCallBack(new UIxGnBTnGAck<boardingcollectticket_status>(NetProcessId, ProcessId, new Exception($@"Timeout, Operation aborted when -{_domainEntityTag}-; (EXIT26.0802.EX01)", ex)), CallBackEvent);

                // Note : Because Execute() will be run in a new thread, so throw ThreadAbortException when this exception found
                throw new Exception($@"ThreadAbortException when -{_domainEntityTag}-", ex);
            }
            catch (Exception ex)
            {
                _processStopped = true;
                _log?.LogError(LogChannel, "-", ex, "EX05", _exeMethodTag, netProcessId: NetProcessId);
                RaiseCallBack(new UIxGnBTnGAck<boardingcollectticket_status>(NetProcessId, ProcessId, new Exception($@"Error when -{_domainEntityTag}-; {ex.Message} ;(EXIT26.0802.EX05); Error Type: {ex.GetType().Name}", ex)), CallBackEvent);
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
        /// FuncCode:EXIT26.0805
        /// </summary>
        private void DoExe()
        {
            bool isNetworkTimeout = false;
            bool isWebServiceDetected = Security.GetTimeStamp(out string timeStampStr1);

            AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

            boardingcollectticket_status webResultData = null;

            if (isWebServiceDetected)
                webResultData = ProceedWebDataTransaction(out isNetworkTimeout, _maxWebAccessTimeSec);
            else
                webResultData = new boardingcollectticket_status() { code = ServerAccess.WebAccessError, msg = "Website not found" };

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
                        webResultData = new boardingcollectticket_status() { code = ServerAccess.NetworkTimeout, msg = $@"Fail login to web server; Web connection timeout; Fail to login web server when -{_domainEntityTag}-; (EXIT26.0805.X1)" };

                    else if (isValidAuthentication == false)
                        webResultData = new boardingcollectticket_status() { code = ServerAccess.InvalidAuthentication, msg = $@"Fail login to web server; Invalid authentication when -{_domainEntityTag}-; (EXIT26.0805.X2)" };

                    else
                        webResultData = new boardingcollectticket_status() { code = ServerAccess.WebAccessError, msg = "Fail login to web server; (EXIT26.0805.X3)" };
                }
            }

            if (webResultData?.code == 0)
            {
                if (string.IsNullOrWhiteSpace(webResultData.tripno) == false)
                { /*By Pass */ }
                else
                {
                    webResultData = new boardingcollectticket_status() { code = ServerAccess.NoDetailFound, msg = $@"Trip not found when -{_domainEntityTag}- (EXIT26.0805.X5)" };
                }
            }
            else
            {
                if (webResultData == null)
                {
                    if (isNetworkTimeout)
                        webResultData = new boardingcollectticket_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout(IV); (EXIT26.0805.X6)" };
                    else
                        webResultData = new boardingcollectticket_status() { code = ServerAccess.WebAccessError, msg = "Fail to read from web; (EXIT26.0805.X9)" };
                }
                if (string.IsNullOrWhiteSpace(webResultData.msg))
                    webResultData.msg = $@"Unknown error when -{_domainEntityTag}-; (EXIT26.0805.X9)";
            }

            if (webResultData.code == 0)
            {
                RaiseCallBack(new UIxGnBTnGAck<boardingcollectticket_status>(NetProcessId, ProcessId, webResultData), CallBackEvent);
            }
            else
            {
                int errCode = webResultData.code;
                string errMsg = webResultData?.msg ?? $@"Unknown error; (EXIT26.0805.X10)";

                RaiseCallBack(new UIxGnBTnGAck<boardingcollectticket_status>(NetProcessId, ProcessId, new Exception($@"Error when -{_domainEntityTag}-; Website Error; {errMsg}; Code: {errCode}")), CallBackEvent);
            }
        }

        /// <summary>
        /// FuncCode:EXIT26.0807
        /// </summary>
        private boardingcollectticket_status ProceedWebDataTransaction(out bool isNetworkTimeout, int waitSec = 60, int maxRetryTimes = 3)
        {
            isNetworkTimeout = false;
            waitSec = (waitSec < 5) ? 10 : waitSec;
            maxRetryTimes = (maxRetryTimes < 1) ? 1 : maxRetryTimes;
            Exception error = null;

            boardingcollectticket_status retData = null;

            for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
            {
                retData = null;

                _webDataTransWorker?.AbortRequest(out _, 500);
                _webDataTransWorker = new RunThreadMan(new ThreadStart(WebDataTransactionThreadWorking), "AxCompleteSaleCollectTicketTransaction.ProceedWebDataTransaction", waitSec, LogChannel, isLogReq: true);
                _webDataTransWorker.WaitUntilCompleted();

                if (retData is null)
                {
                    if ((retryInx + 1) == maxRetryTimes)
                    {
                        isNetworkTimeout = true;

                        if (error != null)
                        {
                            _log?.LogError(LogChannel, "-", new Exception($@"{error.Message}; (EXIT26.0807.X1)", error), "EX01", classNMethodName: "AxCompleteSaleCollectTicketTransaction.ProceedWebDataTransaction");
                            retData = new boardingcollectticket_status() { code = ServerAccess.WebAccessError, msg = $@"{error.Message}; (EXIT26.0807.X1)" };
                        }
                        else
                            retData = new boardingcollectticket_status() { code = ServerAccess.NetworkTimeout, msg = "Network Timeout(V); (EXIT26.0807.X2)" };
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
            /// FuncCode:EXIT26.0807A
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
                        error = new Exception("Invalid security token(empty); (EXIT26.0807A.X1)");
                    }
                    else
                    {
                        boardingcollectticket_status exeResult = null;

                        try
                        {
                            if (NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().IsDebugMode)
                            {
                                exeResult = GetDebugTestData();
                            }
                        }
                        catch { }

                        if (exeResult is null)
                        {
                            exeResult = Soap.BoardingCollectTicket(kioskId, token, _prmCompanycode, _prmTicketno, _prmDepartDate.ToString("dd/MM/yyyy"),
                                _prmTripdate, _prmOperouteid, _prmTripno, _prmFrom, _prmTo, _prmName, _prmContact, _prmIcno,
                                _prmSeatno, _prmSeattype, _prmSellingprice, _prmFacilitycharge, _prmQrCharge,
                                _prmPaymentmethod, _prmPaymentRefNo);
                        }

                        if ((exeResult.code == 0) && (_prmTypeOfPayment == PaymentType.Cash))
                        {
                            try
                            {
                                Soap.UpdateKioskCash(kioskId, token, _prmCassette1NoteCount, _prmCassette2NoteCount, _prmCassette3NoteCount);
                            }
                            catch (Exception ex)
                            {
                                _log?.LogError(LogChannel, _prmTicketno ?? "-", ex, "EX01", classNMethodName: "AxCompleteSaleCollectTicketTransaction.WebDataTransactionThreadWorking");
                            }

                            try
                            {
                                Soap.UpdateKioskCoin(kioskId, token, _prmRefundCoinAmount);
                            }
                            catch (Exception ex)
                            {
                                _log?.LogError(LogChannel, _prmTicketno ?? "-", ex, "EX02", classNMethodName: "AxCompleteSaleCollectTicketTransaction.WebDataTransactionThreadWorking");
                            }
                        }

                        retData = exeResult;
                    }
                }
                catch (ThreadAbortException) 
                {
                    string ttX56 = "*";
                }
                catch (Exception ex)
                {
                    error = new Exception($@"{ex.Message}; (EXIT26.0807A.EX01)", ex);
                    _log?.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "AxCompleteSaleCollectTicketTransaction.ProceedWebDataTransaction");
                }
            }
        }

        /// <summary>
        /// FuncCode:EXIT26.0810
        /// </summary>
        private void RaiseCallBack(UIxGnBTnGAck<boardingcollectticket_status> uixData, AppCallBackDelg callBackEvent)
        {
            if ((_callBackFlag == true) || (callBackEvent is null) || (_processStopped))
                return;

            try
            {
                callBackEvent.Invoke(uixData);

                _log?.LogText(LogChannel, ProcessId, uixData, "A01", $@"AxCompleteSaleCollectTicketTransaction.RaiseCallBack");
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, ProcessId, new WithDataException(ex.Message, ex, uixData), "EX01", "AxCompleteSaleCollectTicketTransaction.RaiseCallBack", netProcessId: NetProcessId);
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

        private boardingcollectticket_status GetDebugTestData()
        {
            /////CYA-DEBUG .. Must be remark upon deployment
            //return new boardingcollectticket_status()
            //{
            //    code = 0,
            //    companyname = "EKSPRES DELIMA",
            //    currency = "RM",
            //    departuredate = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy"),
            //    departuretime = "07:00 PM",
            //    from = "MELAKA SENTRAL",
            //    gateno = "G01",
            //    OnlineSurvey = @"https://eticketing.melakasentral.com.my/survey",
            //    platformno = "G01",
            //    msg = "OK",
            //    QRLink = @"https://cts.melakasentral.com.my/QRCodeGenerator/QRCodeHandler.ashx",
            //    to = "JB - LARKIN",
            //    tripno = "MLWC08",
            //    details = new tick_detail[] { new tick_detail() {
            //        ticketno = "CDELM211200000001",
            //        barcodevalue = "CDELM211200000001-33542",
            //        contact = "77777788877777",
            //        drop = "Tempat Drop",
            //        extra = "xtr",
            //        extraamount = 0.0M,
            //        ic = "777778888833",
            //        insurance = 0.0M,
            //        messagetocustomer = @"Buy your tickets online at www.melakasentral.com.my",
            //        name = "CYA-DEBUG .. Name ABCDEFG",
            //        onlineqrcharge = 0.06M,
            //        pick = "Tempat-Pick",
            //        pick_time = "06:30 PM", price = 18.00M,
            //        refn = "211203DA2E3-MYS",
            //        salesinfo = "EDELIMA  [CHONGKIOSK]  05/12/2021 19:36",
            //        seatdesn = "A",
            //        seattypedesn = "Adult",
            //        terminalcharge = 0.7M,
            //        vege = true
            //    } }
            //};
            /////---------------------------------------------------------------
            return null;
        }

        public void Dispose()
        {
            ShutDown();
        }
    }
}
