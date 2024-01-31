using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx;
using NssIT.Kiosk.Server.AccessDB.AxCommand;
using NssIT.Kiosk.Server.AccessDB.AxCommand.CTCollectTicket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket;

namespace NssIT.Kiosk.Server.ServerApp.CustomApp.CollectTicketApp
{
    /// <summary>
    /// ClassCode:EXIT70.05
    /// </summary>
    public class MelTicketCollectionAppPlan : IServerAppPlan
    {
        private const string LogChannel = "ServerApplication";
        private AppModule _appModule = AppModule.UICollectTicket;
        private int _standardAccessWorkingTimeSec = 120;

        private List<RunThreadMan> _threadManList = new List<RunThreadMan>();

        /// <summary>
        /// FuncCode:EXIT70.0505
        /// </summary>
        public IKioskMsg DoProcess(string procId, Guid? netProcId, IKioskMsg svcMsg, UserSession session, AppCallBackDelg appCallBackDelHandle, out bool releaseSeatRequest)
        {
            releaseSeatRequest = false;
            if (svcMsg is UIReq<UIxCTResetUserSessionRequest>)
            {
                return new UIAckX<UIxCTResetUserSessionAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTResetUserSessionAck(netProcId, procId));
            }
            else if (svcMsg is UIReq<UIxCTStartCollectTicketRequest>)
            {
                return new UIAck<UIxCTSelectLanguageAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTSelectLanguageAck(netProcId, procId));
            }
            else if (svcMsg is UIReq<UIxCTSubmitLanguageRequest> uiReq3)
            {
                IAx ax = new AxCTGetBusCompany("*", uiReq3.BaseNetProcessId, appCallBackDelHandle);
                RunThreadMan tWorker = new RunThreadMan(new ThreadStart(ax.Execute), $@"{ax.GetType().Name}.Execute::MelTicketCollectionAppPlan.DoProcess; (EXIT70.0505.A02)", _standardAccessWorkingTimeSec, LogChannel);
                _threadManList.Add(tWorker);
            }
            else if (svcMsg is UIAck<UIxGnBTnGAck<boardingcompany_status>> uiAck3)
            {
                return uiAck3;
            }
            else if (svcMsg is UIReq<UIxCTSubmitBusCompanyRequest> uiReq4)
            {
                return new UIAck<UIxCTSelectDepartureDateAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTSelectDepartureDateAck(netProcId, procId));
            }
            else if (svcMsg is UIReq<UIxCTSubmitDepartureDateRequest> uiReq5)
            {
                return new UIAck<UIxCTAcquireTicketNoAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTAcquireTicketNoAck(netProcId, procId));
            }
            else if (svcMsg is UIReq<UIxCTSubmitTicketNoRequest> uiReq6)
            {
                // Note : session.TicketCollection.LastSubmittedTicketNo is empty should never occur.
                if (string.IsNullOrWhiteSpace(session.TicketCollection.LastSubmittedTicketNo))
                    return new UIAck<UIxCTTicketNoNotFoundAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTTicketNoNotFoundAck(netProcId, procId, "*****-*****", 99, "Ticket number cannot be blank"));

                else
                {
                    DateTime departDate = session.TicketCollection.PassengerDepartDate.HasValue ? session.TicketCollection.PassengerDepartDate.Value : DateTime.MaxValue;
                    IAx ax = new AxCTGetTicket(uiReq6.MsgData.TicketNo, uiReq6.BaseNetProcessId, uiReq6.MsgData.TicketNo, session.TicketCollection.BusCompanyCode, departDate, appCallBackDelHandle);
                    RunThreadMan tWorker = new RunThreadMan(new ThreadStart(ax.Execute), $@"{ax.GetType().Name}.Execute::MelTicketCollectionAppPlan.DoProcess; (EXIT70.0505.G02)", _standardAccessWorkingTimeSec, LogChannel);
                    _threadManList.Add(tWorker);
                }
            }
            else if (svcMsg is UIAck<UIxGnBTnGAck<boardingqueryticket_status>> uiAck6)
            {
                if (uiAck6.MsgData?.Data?.code == 0)
                {
                    return new UIAck<UIxCTAcquirePassengerInfoAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTAcquirePassengerInfoAck(netProcId, procId));
                }
                else
                {
                    if (uiAck6.MsgData?.Error is TicketNoFoundException ex6)
                    {
                        return new UIAck<UIxCTTicketNoNotFoundAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTTicketNoNotFoundAck(netProcId, procId, session.TicketCollection.LastSubmittedTicketNo, ex6.ErrorCode, ex6.Message));
                    }
                    else
                    {
                        return new UIAck<UIxCTTicketNoNotFoundAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTTicketNoNotFoundAck(netProcId, procId, session.TicketCollection.LastSubmittedTicketNo, 99, 
                            string.IsNullOrWhiteSpace(uiAck6.MsgData.Error?.Message) ? $@"Error when query ticket from website; (EXIT70.0505.X06)": uiAck6.MsgData.Error.Message));
                    }
                }
            }
            else if (svcMsg is UIReq<UIxCTSubmitPassengerInfoRequest> uiReq8)
            {
                if (session.TicketCollection.IsValidToPay)
                    return new UIAck<UIxCTAcquirePaymentAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTAcquirePaymentAck(netProcId, session.TicketCollection.TicketNo));

                else 
                {
                    string errorMsg = "";

                    if (session.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = string.IsNullOrWhiteSpace(session.TicketCollection.InvalidToPayMessage) ? $@"Tidak dapat menyelesaikan transaksi anda pada masa ini, sila cuba sebentar lagi; (EXIT70.0505.X8)" : session.TicketCollection.InvalidToPayMessage;
                    else
                        errorMsg = string.IsNullOrWhiteSpace(session.TicketCollection.InvalidToPayMessage) ? $@"Unable to complete your transaction at the moment, please try again later; (EXIT70.0505.X8)" : session.TicketCollection.InvalidToPayMessage;

                    return new UIAck<UIxCTReDoTransactionAck>(netProcId, procId, _appModule, DateTime.Now, new UIxCTReDoTransactionAck(netProcId, session.TicketCollection.TicketNo, errorMsg));
                }
            }
            else if (svcMsg is UIReq<UIxSubmitCollectTicketPaymentRequest> uiReq9)
            {
                DateTime departDate = session.TicketCollection.PassengerDepartDate.HasValue ? session.TicketCollection.PassengerDepartDate.Value : DateTime.MaxValue;

                IAx ax = new AxCompleteSaleCollectTicketTransaction(procId, netProcId,
                    session.TicketCollection.BusCompanyCode, session.TicketCollection.TicketNo, departDate, 
                    session.TicketCollection.TripDate, session.TicketCollection.OpeRouteId, session.TicketCollection.TripNo, session.TicketCollection.From, session.TicketCollection.To, 
                    session.TicketCollection.PassengerName, session.TicketCollection.PassengerContact, session.TicketCollection.PassengerIC, 
                    session.TicketCollection.SeatNo, session.TicketCollection.SeatType, session.TicketCollection.SellingPrice, session.TicketCollection.FacilityCharge, session.TicketCollection.QRCharge,
                    uiReq9.MsgData.Paymentmethod, uiReq9.MsgData.TypeOfPayment, uiReq9.MsgData.Cassette1NoteCount, uiReq9.MsgData.Cassette2NoteCount, uiReq9.MsgData.Cassette3NoteCount, uiReq9.MsgData.RefundCoinAmount, 
                    uiReq9.MsgData.PaymentRefNo, appCallBackDelHandle);

                RunThreadMan tWorker = new RunThreadMan(new ThreadStart(ax.Execute), $@"{ax.GetType().Name}.Execute::MelTicketCollectionAppPlan.DoProcess; (EXIT70.0505.K09)", _standardAccessWorkingTimeSec, LogChannel, isLogReq: true);
                _threadManList.Add(tWorker);
            }
            else if (svcMsg is UIAck<UIxGnBTnGAck<boardingcollectticket_status>> uiAck10)
            {
                return uiAck10;
            }
            return null;
        }

        public void ResetCleanUp()
        {
            while(_threadManList.Count > 0)
            {
                RunThreadMan tWorker = _threadManList[0];
                _threadManList.RemoveAt(0);

                tWorker?.AbortRequest(out _);
            }
        }

        /// <summary>
        /// FuncCode:EXIT70.0510
        /// </summary>
        public UserSession UpdateUserSession(UserSession userSession, IKioskMsg svcMsg)
        {
            if ((svcMsg is UIReq<UIxCTResetUserSessionRequest>) || (svcMsg is UIReq<UIxCTResetUserSessionSendOnlyRequest>))
            {
                userSession.SessionReset();
            }
            else if (svcMsg is UIReq<UIxCTStartCollectTicketRequest>)
            {
                userSession.NewSession(svcMsg.RefNetProcessId.Value);
            }
            else if ((svcMsg is UIReq<UIxCTSubmitLanguageRequest> uiReq3) && (uiReq3.MsgData != null))
            {
                userSession.Language = uiReq3.MsgData.Language;
            }
            else if ((svcMsg is UIReq<UIxCTSubmitBusCompanyRequest> uiReq4) && (uiReq4.MsgData != null))
            {
                userSession.TicketCollection.BusCompanyCode = uiReq4.MsgData.BusCompanyCode;
                userSession.TicketCollection.BusCompanyLogoURL = uiReq4.MsgData.BusCompanyLogoURL;
                userSession.TicketCollection.BusCompanyName = uiReq4.MsgData.BusCompanyName;
            }
            else if ((svcMsg is UIReq<UIxCTSubmitDepartureDateRequest> uiReq5) && (uiReq5.MsgData != null))
            {
                userSession.TicketCollection.PassengerDepartDate = uiReq5.MsgData.DepartureDate;
            }
            else if ((svcMsg is UIReq<UIxCTSubmitTicketNoRequest> uiReq6) && (uiReq6.MsgData != null))
            {
                userSession.TicketCollection.LastSubmittedTicketNo = uiReq6.MsgData.TicketNo;
            }
            else if ((svcMsg is UIAck<UIxGnBTnGAck<boardingqueryticket_status>> uiReq7) && (uiReq7.MsgData.Data?.code == 0)
                && (string.IsNullOrWhiteSpace(userSession.TicketCollection.LastSubmittedTicketNo) == false))
            {
                boardingqueryticket_status dt7 = uiReq7.MsgData.Data;

                userSession.TicketCollection.TicketNo = userSession.TicketCollection.LastSubmittedTicketNo;
                userSession.TicketCollection.ExistingContact = dt7.contact;
                userSession.TicketCollection.ExistingIC = dt7.ic;
                userSession.TicketCollection.ExistingName = dt7.name;
                userSession.TicketCollection.FacilityCharge = dt7.facilitycharge;
                userSession.TicketCollection.TripDate = dt7.tripdate;
                userSession.TicketCollection.TripNo = dt7.tripno;
                userSession.TicketCollection.OpeRouteId = dt7.operouteid;
                userSession.TicketCollection.From = dt7.from;
                userSession.TicketCollection.Fromdesn = dt7.fromdesn;
                userSession.TicketCollection.To = dt7.to;
                userSession.TicketCollection.ToDesn = dt7.todesn;
                userSession.TicketCollection.QRCharge = dt7.qrcharge;
                userSession.TicketCollection.SeatNo = dt7.seatno;
                userSession.TicketCollection.SeatType = dt7.seattype;
                userSession.TicketCollection.SellingPrice = dt7.sellingprice;
                userSession.TicketCollection.TotalChargableAmount = dt7.facilitycharge + dt7.qrcharge;

                if (string.IsNullOrWhiteSpace(dt7.name) == false)
                    userSession.TicketCollection.PassengerName = dt7.name;
                else
                {
                    userSession.TicketCollection.ExistingName = null;
                    userSession.TicketCollection.PassengerName = null;
                }

                if (string.IsNullOrWhiteSpace(dt7.ic) == false)
                    userSession.TicketCollection.PassengerIC = dt7.ic;
                else
                {
                    userSession.TicketCollection.ExistingIC = null;
                    userSession.TicketCollection.PassengerIC = null;
                }

                if (string.IsNullOrWhiteSpace(dt7.contact) == false)
                    userSession.TicketCollection.PassengerContact = dt7.contact;
                else
                {
                    userSession.TicketCollection.ExistingContact = null;
                    userSession.TicketCollection.PassengerContact = null;
                }
            }
            else if ((svcMsg is UIReq<UIxCTSubmitPassengerInfoRequest> uiReq8) && (uiReq8.MsgData is UIxCTSubmitPassengerInfoRequest uiX8))
            {
                userSession.TicketCollection.PassengerName = uiX8.PassengerName;
                userSession.TicketCollection.PassengerIC = uiX8.ICPassportNo;
                userSession.TicketCollection.PassengerContact = uiX8.ContactNo;

                userSession.TicketCollection.IsValidToPay = false;
                userSession.TicketCollection.InvalidToPayMessage = null;

                //-----------------------------------------
                //CYA-TEST .. test to fail  .. userSession.TicketCollection.BusCompanyName = null;
                //-----------------------------------------

                string errorMsg = "";
                //Data Validation
                if (string.IsNullOrWhiteSpace(userSession.TicketCollection.PassengerName))
                {
                    if (userSession.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = "Entri nama tidak sah; (EXIT70.0510.X01)";
                    else
                        errorMsg = "Invalid name entry; (EXIT70.0510.X01)";
                }
                else if (string.IsNullOrWhiteSpace(userSession.TicketCollection.PassengerIC))
                {
                    if (userSession.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = "Entri nombor IC/Pasport tidak sah; (EXIT70.0510.X01)";
                    else
                        errorMsg = "Invalid IC/Passport number entry; (EXIT70.0510.X01)";
                }
                else if (string.IsNullOrWhiteSpace(userSession.TicketCollection.PassengerContact))
                {
                    if (userSession.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = "Entri nombor telefon bimbit tidak sah; (EXIT70.0510.X01)";
                    else
                        errorMsg = "Invalid contact/mobile number entry; (EXIT70.0510.X01)";
                }
                else if (string.IsNullOrWhiteSpace(userSession.TicketCollection.TicketNo))
                {
                    if (userSession.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = "Entri nombor tiket tidak sah; (EXIT70.0510.X01)";
                    else
                        errorMsg = "Invalid ticket number entry; (EXIT70.0510.X01)";
                }
                else if (userSession.TicketCollection.TicketNo.Equals(userSession.TicketCollection.LastSubmittedTicketNo) == false)
                {
                    if (userSession.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = "Entri nombor tiket tidak sah (II); (EXIT70.0510.X01)";
                    else
                        errorMsg = "Invalid (II) ticket number entry; (EXIT70.0510.X01)";
                }
                else if (string.IsNullOrWhiteSpace(userSession.TicketCollection.TripNo))
                {
                    if (userSession.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = "Nombor perjalanan tidak sah; (EXIT70.0510.X01)";
                    else
                        errorMsg = "Invalid trip number; (EXIT70.0510.X01)";
                }
                else if (userSession.TicketCollection.TotalChargableAmount <= 0)
                {
                    if (userSession.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = "Caj tidak diperlukan; (EXIT70.0510.X01)";
                    else
                        errorMsg = "Charge not required; (EXIT70.0510.X01)";
                }
                else if (userSession.TicketCollection.PassengerDepartDate.HasValue == false)
                {
                    if (userSession.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = "Tiada tarikh berlepas; (EXIT70.0510.X01)";
                    else
                        errorMsg = "Missing departure date; (EXIT70.0510.X01)";
                }
                else if (string.IsNullOrWhiteSpace(userSession.TicketCollection.BusCompanyCode) || string.IsNullOrWhiteSpace(userSession.TicketCollection.BusCompanyName))
                {
                    if (userSession.Language == AppDecorator.Common.LanguageCode.Malay)
                        errorMsg = "Kod/nama syarikat bas tidak sah; (EXIT70.0510.X01)";
                    else
                        errorMsg = "Invalid bus company code/name; (EXIT70.0510.X01)";
                }

                if (string.IsNullOrWhiteSpace(errorMsg))
                {
                    userSession.TicketCollection.IsValidToPay = true;
                }
                else
                {
                    userSession.TicketCollection.IsValidToPay = false;
                    userSession.TicketCollection.InvalidToPayMessage = errorMsg;
                }
                //------------------------------------------------------------------------------
            }

            return userSession;
        }

        /// <summary>
        /// FuncCode:EXIT70.0520
        /// </summary>
        public UserSession SetEditingSession(UserSession userSession, TickSalesMenuItemCode detailItemCode)
        {
            return userSession;
        }

        /// <summary>
        /// FuncCode:EXIT70.0530
        /// </summary>
        public UserSession SetUIPageNavigateSession(UserSession userSession, UIPageNavigateRequest pageNav)
        {
            return userSession;
        }

        public UISalesInst NextInstruction(string procId, Guid? netProcId, IKioskMsg svcMsg, UserSession session, out bool releaseSeatRequest)
            => throw new NotImplementedException("IServerAppPlan.NextInstruction has obsolate");
    }
}
