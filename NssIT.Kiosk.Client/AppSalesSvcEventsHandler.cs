using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.Client.NetClient;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client
{
    public class AppSalesSvcEventsHandler
    {
        private string _logChannel = "AppSys";

        private NetClientService _netClientSvc = null;

        private RunThreadMan _commonSalesTMan = null;
        private RunThreadMan _updateDepartTripListTMan = null;
        private RunThreadMan _updateTransactionCompleteStatusTMan = null;
        private RunThreadMan _bTnGTMan = null;

        public AppSalesSvcEventsHandler(NetClientService netClientSvc)
        {
            _netClientSvc = netClientSvc;

            _netClientSvc.SalesService.OnDataReceived += SalesService_OnDataReceived;
            _netClientSvc.BTnGService.OnDataReceived += BTnGService_OnDataReceived;
        }

        private void BTnGService_OnDataReceived(object sender, AppDecorator.Common.AppService.Network.DataReceivedEventArgs e)
        {
            if (e.ReceivedData?.MsgObject?.GetMsgData() is IUIxBTnGPaymentOngoingGroupAck)
            {
                _bTnGTMan = new RunThreadMan(new Action(() => 
                {
                    App.MainScreenControl?.BTnGShowPaymentInfo(e.ReceivedData.MsgObject);
                }), "AppSalesSvcEventsHandler.BTnGService_OnDataReceived##IUIxBTnGPaymentOngoingGroupAck##", 60, _logChannel, System.Threading.ThreadPriority.Highest);
            }
        }

        private void SalesService_OnDataReceived(object sender, AppDecorator.Common.AppService.Network.DataReceivedEventArgs e)
        {
            if (App.IsClientReady == false)
                return;

            string relatedStage = "";
            try
            {
                if (e?.ReceivedData?.MsgObject is UILanguageSelectionAck uiLang)
                {
                    relatedStage = "*Language Selection*";
                    App.ShowDebugMsg($@"Found showing UI Language Instruction");
                
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.ChooseLanguage(AppModule.UIKioskSales);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UILanguageSelectionAck##", 30, _logChannel);
                }

                if (e?.ReceivedData?.MsgObject is UICountDownExpiredAck uiCntExp)
                {
                    relatedStage = "*Count Down Expired*";
                    App.LatestUserSession = new AppDecorator.Common.AppService.Sales.UserSession() { Expired = true };
                    App.ShowDebugMsg($@"Working Count Down Expired");

                    // Delay 10 * 1000 milliseconds to allow printing.
                    _commonSalesTMan?.AbortRequest(out _, 10 * 1000);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.ShowWelcome();
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UICountDownExpiredAck##", 60, _logChannel);
                }
                else if (e?.ReceivedData?.MsgObject is UIOriginListAck uiOrig)
                {
                    relatedStage = "*Origin List*";
                    if (uiOrig.MessageData is destination_status origList)
                    {
                        if (origList.code == 0)
                        {
                            App.LatestUserSession = uiOrig.Session;
                            App.ShowDebugMsg($@"destination_status (Origin) has received at SalesService");
                            
                            _commonSalesTMan?.AbortRequest(out _, 300);
                            _commonSalesTMan = new RunThreadMan(new Action(() => {
                                App.MainScreenControl.ChooseOriginStation(uiOrig, uiOrig.Session);
                            }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UIOriginListAck##", 30, _logChannel);
                        }
                        else
                        {
                            App.ShowDebugMsg($@"Unabled to read Origin station list; (EXIT500032); WebCode: {origList.code}; ErrMsg: {origList.msg}");
                            throw new Exception($@"Unabled to read Origin station list; (EXIT500032); WebCode: {origList.code}; ErrMsg: {origList.msg}");
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(uiOrig.ErrorMessage) == false)
                    {
                        App.ShowDebugMsg($@"Error Reading Origin Data; (EXIT500033); {uiOrig.ErrorMessage}");
                        throw new Exception($@"Error Reading Origin Data; (EXIT500033); {uiOrig.ErrorMessage}");
                    }
                    else
                    {
                        App.ShowDebugMsg($@"Unable to read origin data (EXIT500034)");
                        throw new Exception($@"Unable to read origin data (EXIT500034)");
                    }
                }
                else if (e?.ReceivedData?.MsgObject is UIDestinationListAck uiDest)
                {
                    relatedStage = "*Destination List*";
                    if (uiDest.MessageData is destination_status destList)
                    {
                        if (destList.code == 0)
                        {
                            App.LatestUserSession = uiDest.Session;
                            App.ShowDebugMsg($@"destination_status has received at SalesService");
                            
                            _commonSalesTMan?.AbortRequest(out _, 300);
                            _commonSalesTMan = new RunThreadMan(new Action(() => {
                                App.MainScreenControl.ChooseDestinationStation(uiDest, uiDest.Session);
                            }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UIDestinationListAck##", 30, _logChannel);
                        }
                        else
                        {
                            App.ShowDebugMsg($@"Unabled to read Destination station list; (EXIT500023); WebCode: {destList.code}; ErrMsg: {destList.msg}");
                            throw new Exception($@"Unabled to read Destination station list; (EXIT500023); WebCode: {destList.code}; ErrMsg: {destList.msg}");
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(uiDest.ErrorMessage) == false)
                    {
                        App.ShowDebugMsg($@"Error Reading Destination Data; (EXIT500024); {uiDest.ErrorMessage}");
                        throw new Exception($@"Error Reading Destination Data; (EXIT500024); {uiDest.ErrorMessage}");
                    }
                    else
                    {
                        App.ShowDebugMsg($@"Unable to read destination data (EXIT500021)");
                        throw new Exception($@"Unable to read destination data (EXIT500021)");
                    }
                }
                else if (e?.ReceivedData?.MsgObject is UITravelDatesEnteringAck uiTravelDates)
                {
                    relatedStage = "*Travel Dates Enter*";
                    App.LatestUserSession = uiTravelDates.Session;
                    App.ShowDebugMsg($@"Show Travel Dates Entering");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.ChooseTravelDates(uiTravelDates, uiTravelDates.Session);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UITravelDatesEnteringAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartTripInitAck uiTripInit)
                {
                    relatedStage = "*Depart Trip Init*";
                    App.LatestUserSession = uiTripInit.Session;
                    App.ShowDebugMsg($@"Show Depart Trip Selection Page");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.ShowInitDepartTrip(uiTripInit, uiTripInit.Session);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UIDepartTripInitAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartTripListAck uiDTrip)
                {
                    relatedStage = "*Depart Trip List*";
                    trip_status ts = null;

                    if (uiDTrip.MessageData is null)
                    {
                        throw new Exception("Unabled to read Depart Trip list; (EXIT500022)");
                    }
                    else
                    {
                        ts = (trip_status)uiDTrip.MessageData;

                        // Code = 4 mean no record found
                        if (ts.code == 0)
                        {
                            App.LatestUserSession = uiDTrip.Session;
                        } 
                        else if (ts.code == 4)
                        {
                            App.LatestUserSession = uiDTrip.Session;
                            ts.details = new trip_detail[0];
                            ts.companyLogoURL = "";
                        }
                        else if (ts.code != 0)
                        {
                            throw new Exception($@"Unabled to read Depart Trip data; (EXIT500024); WebCode: {ts.code}; ErrMsg: {ts.msg}");
                        }
                    }

                    App.ShowDebugMsg($@"Show Depart Trip Selection Page");

                    _updateDepartTripListTMan?.AbortRequest(out _, 500);
                    _updateDepartTripListTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.UpdateDepartTripList(uiDTrip, uiDTrip.Session);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UIDepartTripListAck##", 60, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartTripSubmissionErrorAck uiTripSubErr)
                {
                    App.ShowDebugMsg($@"Error When submit Trip");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.UpdateDepartTripSubmitError(uiTripSubErr, uiTripSubErr.Session);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UIDepartTripSubmissionErrorAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartSeatListAck uiDSeat)
                {
                    relatedStage = "*Depart Seat List*";
                    seat_status sStt = null;

                    if (uiDSeat.MessageData is null)
                    {
                        throw new Exception("Unabled to read Depart Seat list; (EXIT500025)");
                    }
                    else
                    {
                        sStt = (seat_status)uiDSeat.MessageData;

                        // Code = 4 mean no record found
                        if (sStt.code == 0)
                        {
                            App.LatestUserSession = uiDSeat.Session;

                            if (sStt.details is null)
                                sStt.details = new seat_detail[0];
                        }
                        else if (sStt.code == 4)
                        {
                            App.LatestUserSession = uiDSeat.Session;
                            sStt.details = new seat_detail[0];
                        }
                        else if (sStt.code != 0)
                        {
                            throw new Exception($@"Unabled to read Depart Seat data; (EXIT500026); WebCode: {sStt.code}; ErrMsg: {sStt.msg}");
                        }
                    }

                    App.ShowDebugMsg($@"Show Depart Trip Selection Page");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.ChooseDepartSeat(uiDSeat, uiDSeat.Session);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UIDepartSeatListAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartPickupNDropAck uiDepPnD)
                {
                    relatedStage = "*Depart Pickup & Drop*";
                    PickupNDropList pnd = null;

                    if (uiDepPnD.MessageData is null)
                    {
                        throw new Exception("Unabled to read Pickup and Drop Off options; (EXIT500027)");
                    }
                    else
                    {
                        pnd = (PickupNDropList)uiDepPnD.MessageData;

                        if ((pnd.PickDetailList is null) || (pnd.PickDetailList.Length == 0))
                        {
                            throw new Exception($@"Unabled to read Pickup options; (EXIT500028);");
                        }
                        else if ((pnd.DropDetailList is null) || (pnd.DropDetailList.Length == 0))
                        {
                            throw new Exception($@"Unabled to read Drop Off options; (EXIT500029);");
                        }
                    }

                    App.ShowDebugMsg($@"Show Pickup and Drop Off Selection Page");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.ChoosePickupNDrop(uiDepPnD);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UIDepartPickupNDropAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UIInsuranceAck uiInsn)
                {
                    relatedStage = "*Select Insurance*";

                    App.LatestUserSession = uiInsn.Session;
                    App.ShowDebugMsg($@"Insurance Selection Page");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.ChooseInsurance(uiInsn);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UIInsuranceAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UISkyWayAck uISkyWay)
                {
                    relatedStage = "*Select Sky Way*";
                    App.LatestUserSession = uISkyWay.Session;
                    App.ShowDebugMsg($@"Sky Way Selection Page");


                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() =>
                    {
                        App.MainScreenControl.ChooseSkyWay(uISkyWay);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UISkyWayAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartSeatConfirmFailAck uiDpStCfmFail)
                {
                    relatedStage = "*Depart Seat Confirm Fail*";
                    string errMsg = uiDpStCfmFail.Session?.DepartSeatConfirmMessage ?? "";
                    errMsg = $@"{errMsg}; (EXIT500030); Please try again";

                    App.ShowDebugMsg($@"Fail Depart Seat Confirm; {errMsg}");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.Alert("TIDAK MENGESAHKAN", "UNABLE CONFIRM", detailMsg: $@"{errMsg}");
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UIDepartSeatConfirmFailAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UICustInfoAck uiCust)
                {
                    relatedStage = "*Cust. Info*";
                    CustomerInfoList custInfoList = (CustomerInfoList)uiCust.MessageData;

                    if ((custInfoList is null) || (custInfoList.CustSeatInfoList is null) || (custInfoList.CustSeatInfoList.Length == 0))
                        throw new Exception("Invalid passenger seat info");

                    App.ShowDebugMsg($@"Show Passenger Info Entry Page");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.EnterPassengerInfo(uiCust);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UICustInfoAck##", 40, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UICustInfoUpdateFailAck uiFailUpdCustInfo)
                {
                    relatedStage = "*Cust Info Update Fail*";
                    string errMsg = (string.IsNullOrWhiteSpace(uiFailUpdCustInfo.ErrorMessage) == false) ? uiFailUpdCustInfo.ErrorMessage.Trim() : "Unable to update your information at the moment";
                    errMsg = $@"{errMsg}; (EXIT500031); Please try again";

                    App.ShowDebugMsg($@"Fail Update Customer Info; {errMsg}");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.Alert("TIDAK DIKEMAS KINI", "UNABLE UPDATE", detailMsg: $@"{errMsg}");
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UICustInfoUpdateFailAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UISalesPaymentProceedAck uiSalesPayment)
                {
                    

                    relatedStage = "*Make Sales Payment*";
                    App.ShowDebugMsg($@"Received UIMakeSalesPaymentAck");
                    
                    _commonSalesTMan?.AbortRequest(out _, 300);
                    _commonSalesTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.MakeTicketPayment(uiSalesPayment);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UISalesPaymentProceedAck##", 30, _logChannel);
                }

                else if (e?.ReceivedData?.MsgObject is UICompleteTransactionResult uiCompltResult)
                {
                    relatedStage = "*Complete Transaction Result*";
                    App.ShowDebugMsg($@"Received UICompleteTransactionResult");

                    //NOte : Delay 10 * 1000 miliseconds to allow previous printing
                    _updateTransactionCompleteStatusTMan?.AbortRequest(out _, 10 * 1000);
                    _updateTransactionCompleteStatusTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.UpdateTransactionCompleteStatus(uiCompltResult);
                    }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##UICompleteTransactionResult##", 100, _logChannel, isLogReq: true);
                }
            }
            catch (Exception ex)
            {
                App.Log?.LogText(_logChannel, "-", e, "EX01", "AppSalesSvcEventsHandler.SalesService_OnDataReceived", NssIT.Kiosk.AppDecorator.Log.MessageType.Error,
                    extraMsg: "Error : " + ex.ToString() + "; \r\n\r\n MsgObj: DataReceivedEventArgs",
                    netProcessId: e?.ReceivedData?.NetProcessId);

                _commonSalesTMan?.AbortRequest(out _);
                _commonSalesTMan = new RunThreadMan(new Action(() => {
                    App.MainScreenControl?.Alert(detailMsg: $@"{ex.Message}; (EXIT500199); Related : {relatedStage}");
                }), "AppSalesSvcEventsHandler.SalesService_OnDataReceived##~Exception~(EXIT500199)~##", 30, _logChannel);
            }
        }
    }
}
