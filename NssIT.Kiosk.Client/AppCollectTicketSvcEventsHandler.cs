using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx;
using NssIT.Kiosk.Client.NetClient;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.Client.Base.Time;
using Newtonsoft.Json;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;

namespace NssIT.Kiosk.Client
{
    /// <summary>
    /// ClassCode:EXIT80.20
    /// </summary>
    public class AppCollectTicketSvcEventsHandler
    {
        private string _logChannel = "AppSys";

        private NetClientService _netClientSvc = null;
        private RunThreadMan _commonTMan = null;

        /// <summary>
        /// FuncCode:EXIT80.2002
        /// </summary>
        public AppCollectTicketSvcEventsHandler(NetClientService netClientSvc)
        {
            _netClientSvc = netClientSvc;

            _netClientSvc.CollectTicketService.OnDataReceived += CollectTicketService_OnDataReceived;
        }

        /// <summary>
        /// FuncCode:EXIT80.2003
        /// </summary>
        private void CollectTicketService_OnDataReceived(object sender, AppDecorator.Common.AppService.Network.DataReceivedEventArgs e)
        {
            /////App.ShowDebugMsg(JsonConvert.SerializeObject(e.ReceivedData.MsgObject, Formatting.Indented));
            
            if ((e.ReceivedData?.MsgObject?.GetMsgData() is UIxCTSelectLanguageAck)
                && App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.Language, 30))
            {
                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Found showing UI Language Instruction");
                

                _commonTMan?.AbortRequest(out _, 300);
                _commonTMan = new RunThreadMan(new Action(() => {
                    App.MainScreenControl.ChooseLanguage(AppModule.UICollectTicket);
                }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxCTSelectLanguageAck##", 30, _logChannel);
            }
            else if ((e.ReceivedData?.MsgObject?.GetMsgData() is UIxGnBTnGAck<boardingcompany_status> ack2) 
                && App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.BusCompany, 30))
            {
                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Found Bus Company List");
                if (ack2.IsDataReadSuccess == false)
                {
                    if (string.IsNullOrWhiteSpace(ack2.Error?.Message) == false)
                    {
                        _commonTMan?.AbortRequest(out _, 300);
                        _commonTMan = new RunThreadMan(new Action(() => {
                            App.MainScreenControl.Alert(detailMsg: ack2.Error.Message);
                        }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxGnBTnGAck<boardingcompany_status>1##", 30, _logChannel);
                    }
                    else
                    {
                        _commonTMan?.AbortRequest(out _, 300);
                        _commonTMan = new RunThreadMan(new Action(() => {
                            App.MainScreenControl.Alert(detailMsg: "Unknown error when try to query bus company list; (EXIT80.2003.X03)");
                        }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxGnBTnGAck<boardingcompany_status>2##", 30, _logChannel);
                    }
                }
                else
                {
                    _commonTMan?.AbortRequest(out _, 300);
                    _commonTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.CTChooseBusCompany(e.ReceivedData.MsgObject);
                    }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxGnBTnGAck<boardingcompany_status>###", 30, _logChannel);
                }
            }
            else if ((e.ReceivedData?.MsgObject?.GetMsgData() is UIxCTSelectDepartureDateAck ack3)
                && App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.DepartureDate, 30))
            {
                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Enter Deperture Date for the Boarding Pass");
                
                _commonTMan?.AbortRequest(out _, 300);
                _commonTMan = new RunThreadMan(new Action(() => {
                    App.MainScreenControl.CTChooseDepartureDate(e.ReceivedData.MsgObject);
                }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxCTSelectDepartureDateAck##", 30, _logChannel);
            }
            else if ((e.ReceivedData?.MsgObject?.GetMsgData() is UIxCTAcquireTicketNoAck ack4)
                && App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.TicketNumberEntry, 30))
            {
                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Ticket Number Entry for the Boarding Pass");
                
                _commonTMan?.AbortRequest(out _, 300);
                _commonTMan = new RunThreadMan(new Action(() => {
                    App.MainScreenControl.CTEnterTicketNo(e.ReceivedData.MsgObject);
                }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxCTAcquireTicketNoAck##", 30, _logChannel);
            }
            else if ((e.ReceivedData?.MsgObject?.GetMsgData() is UIxCTTicketNoNotFoundAck ack5)
                && App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.TicketNumberEntry, 30))
            {
                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> The submitted Ticket Number is not found the Boarding Pass");
                
                _commonTMan?.AbortRequest(out _, 300);
                _commonTMan = new RunThreadMan(new Action(() => {
                    App.MainScreenControl.CTShowTicketNumberNotFound(e.ReceivedData.MsgObject);
                }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxCTTicketNoNotFoundAck##", 30, _logChannel);
            }
            else if ((e.ReceivedData?.MsgObject?.GetMsgData() is UIxCTAcquirePassengerInfoAck ack6)
                && App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.TicketInfo, 90))
            {
                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Passenger Info Entry for the Boarding Pass");
                
                _commonTMan?.AbortRequest(out _, 300);
                _commonTMan = new RunThreadMan(new Action(() => {
                    App.MainScreenControl.CTEnterPassengerInfo(e.ReceivedData.MsgObject);
                }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxCTAcquirePassengerInfoAck##", 30, _logChannel);
            }
            else if ((e.ReceivedData?.MsgObject?.GetMsgData() is UIxCTReDoTransactionAck ack7)
                && App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.TicketInfo, 90))
            {
                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> ReDo Transaction for the Boarding Pass");

                if (string.IsNullOrWhiteSpace(ack7.Message) == false)
                {
                    
                    _commonTMan?.AbortRequest(out _, 300);
                    _commonTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.Alert(detailMsg: ack7.Message);
                    }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxCTReDoTransactionAck##1", 30, _logChannel);
                }
                else
                {
                    
                    _commonTMan?.AbortRequest(out _, 300);
                    _commonTMan = new RunThreadMan(new Action(() => {
                        App.MainScreenControl.Alert(detailMsg: "We are sorry. Unable to complete transaction. Please try again later. (EXIT80.2003.X07)");
                    }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxCTReDoTransactionAck##2", 30, _logChannel);
                }
            }
            else if ((e.ReceivedData?.MsgObject?.GetMsgData() is UIxCTAcquirePaymentAck ack8)
                && App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.Payment, 30 * 60))
            {
                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Payment the Boarding Pass");
                
                _commonTMan?.AbortRequest(out _, 300);
                _commonTMan = new RunThreadMan(new Action(() => {
                    App.MainScreenControl.CTMakeTicketPayment(e.ReceivedData.MsgObject);
                }), "AppCollectTicketSvcEventsHandler.CollectTicketService_OnDataReceived##UIxCTAcquirePaymentAck##", 45, _logChannel);
            }
            else if (e.ReceivedData?.MsgObject?.GetMsgData() is UIxGnBTnGAck<boardingcollectticket_status> ack9)
            {
                //Note : Only set CountDown. No MainScreenControl method needed to be called here
                App.CollectBoardingPassCountDown.ForceResetCounter();
                App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.Printing, 30 * 60);
                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Print ticket in progress for the Boarding Pass");
            }
            else
            {
                //CYA-DEBUG
                DebugShowUnhandledDataType(e);
            }

            return;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void DebugShowUnhandledDataType(AppDecorator.Common.AppService.Network.DataReceivedEventArgs eX)
            {
                App.ShowDebugMsg("AppCollectTicketSvcEventsHandler -> Start - ********** ********** !!!!! AppCollectTicketSvcEventsHandler Received Unrecognized Acknowledge !!!!! ********** **********");
                App.CollectBoardingPassCountDown.ChangeCountDown("CollectTicketService_OnDataReceived", CollectTicketCountDown.ColTickCountDownCode.Language, 30);

                string unKnTypeStr = "Unknown";
                dynamic msgData = eX.ReceivedData?.MsgObject;

                if (msgData != null)
                    try
                    {
                        unKnTypeStr = msgData.GetType().ToString();
                        unKnTypeStr = string.IsNullOrWhiteSpace(unKnTypeStr) ? "Unknown" : unKnTypeStr.Trim();
                    }
                    catch { }

                App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Unregconize Received DataType; Data Type: {unKnTypeStr}");

                if (msgData != null)
                {
                    App.ShowDebugMsg(JsonConvert.SerializeObject(msgData, Formatting.Indented));

                    try
                    {
                        dynamic unKnownObj = eX.ReceivedData?.MsgObject?.GetMsgData();

                        if (unKnownObj != null)
                            App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Received unregconize Detail Data Type : {unKnownObj.GetType().ToString()}");

                        else
                            App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Detail Data Type has not found from Unregconize Received DataType");
                    }
                    catch (Exception ex)
                    {
                        App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Error; Unable to read the received unregconize data type in more accurate detail; {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        App.ShowDebugMsg(JsonConvert.SerializeObject(e, Formatting.Indented));
                    }
                    catch (Exception ex)
                    {

                        App.ShowDebugMsg($@"AppCollectTicketSvcEventsHandler -> Error when read unregconize receive data; {ex.ToString()}");
                    }
                }

                App.ShowDebugMsg("AppCollectTicketSvcEventsHandler -> End - ********** ********** !!!!! AppCollectTicketSvcEventsHandler Received Unrecognized Acknowledge !!!!! ********** **********");
            }
        }
    }
}
