using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Server.AccessDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp.CustomApp
{
    public class GentingAppPlan : IServerAppPlan
    {
        private CultureInfo _provider = CultureInfo.InvariantCulture;

        public UISalesInst NextInstruction(string procId, Guid? netProcId, IKioskMsg svcMsg, UserSession session, out bool releaseSeatRequestOnEdit)
        {
            releaseSeatRequestOnEdit = false;
            if (svcMsg != null)
            {
                if (svcMsg is UIDetailEditRequest uiEdit)
                {
                    if (string.IsNullOrWhiteSpace(session.DepartPendingReleaseTransNo) == false)
                        releaseSeatRequestOnEdit = true;

                    if (uiEdit.EditItemCode == TickSalesMenuItemCode.FromStation)
                    {
                        return UISalesInst.OriginListRequest;
                    }
                    else if(uiEdit.EditItemCode == TickSalesMenuItemCode.ToStation)
                    {
                        return UISalesInst.DestinationListRequest;
                    }
                    else if (uiEdit.EditItemCode == TickSalesMenuItemCode.DepartDate)
                    {
                        return UISalesInst.TravelDatesEnteringAck;
                    }
                    else if (uiEdit.EditItemCode == TickSalesMenuItemCode.DepartOperator)
                    {
                        return UISalesInst.DepartTripListInitAck;
                    }
                    else if (uiEdit.EditItemCode == TickSalesMenuItemCode.DepartSeat)
                    {
                        return UISalesInst.DepartSeatListRequest;
                    }
                }
                else if (svcMsg is UIPageNavigateRequest uiPgNav)
                {
                    if (string.IsNullOrWhiteSpace(session.DepartPendingReleaseTransNo) == false)
                        releaseSeatRequestOnEdit = true;

                    if (uiPgNav.NavigateDirection == PageNavigateDirection.Previous)
                    {
                        if (session.CurrentEditMenuItemCode.HasValue == false)
                        {
                            return UISalesInst.CountDownExpiredAck;
                        }
                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.Passenger)
                        {
                            return UISalesInst.DepartSeatListRequest;
                        }
                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.DepartSeat)
                        {
                            return UISalesInst.DepartTripListInitAck;
                        }
                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.DepartOperator)
                        {
                            return UISalesInst.TravelDatesEnteringAck;
                        }
                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.DepartDate)
                        {
                            return UISalesInst.DestinationListRequest;
                        }
                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.ToStation)
                        {
                            return UISalesInst.OriginListRequest;
                        }
                        else
                            return UISalesInst.CountDownExpiredAck;
                    }
                    else if (uiPgNav.NavigateDirection == PageNavigateDirection.Exit)
                    { }

                    return UISalesInst.CountDownExpiredAck;
                }
                else
                {
                    // Intruction redirection
                    if (svcMsg.Instruction == (CommInstruction)UISalesInst.CountDownStartRequest)
                        return UISalesInst.LanguageSelectionAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.LanguageSubmission)
                        return UISalesInst.OriginListRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.OriginListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.OriginSubmission)
                        return UISalesInst.DestinationListRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DestinationListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DestinationSubmission)
                        return UISalesInst.TravelDatesEnteringAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.TravelDatesSubmission)
                        return UISalesInst.DepartTripListInitAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartTripListRequest)
                        return UISalesInst.DepartTripListRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartTripListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartTripSubmission)
                        return UISalesInst.DepartSeatListRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatListErrorResult)
                        return UISalesInst.DepartTripSubmissionErrorAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatSubmission)
                    {
                        if (session.DepartSkyWayAmount > 0)
                        {
                            return UISalesInst.SkyWayAck;
                        }
                        else
                        {
                            if (session.DepartEmbed?.Trim().Equals("1") == true)
                                return UISalesInst.DepartPickupNDropAck;
                            else
                            {
                                if (session.DepartInsurance > 0)
                                    return UISalesInst.InsuranceAck;
                                else
                                    return UISalesInst.DepartSeatConfirmRequest;
                            }
                        }

                    }

                    else if(svcMsg.Instruction == (CommInstruction)UISalesInst.SkyWaySubmission)
                    {
                        if (session.DepartEmbed?.Trim().Equals("1") == true)
                            return UISalesInst.DepartPickupNDropAck;
                        else
                        {
                            if (session.DepartInsurance > 0)
                                return UISalesInst.InsuranceAck;
                            else
                                return UISalesInst.DepartSeatConfirmRequest;
                        }
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartPickupNDropSubmission)
                    {
                        if (session.DepartInsurance > 0)
                            return UISalesInst.InsuranceAck;
                        else
                            return UISalesInst.DepartSeatConfirmRequest;
                    }

                    //else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatSubmission)
                    //{
                    //    if (session.DepartEmbed?.Trim().Equals("1") == true)
                    //        return UISalesInst.DepartPickupNDropAck;
                    //    else
                    //        return UISalesInst.DepartSeatConfirmRequest;
                    //}

                    //else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartPickupNDropSubmission)
                    //    return UISalesInst.DepartSeatConfirmRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.InsuranceSubmission)
                        return UISalesInst.DepartSeatConfirmRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatConfirmResult)
                    {
                        if (session.DepartSeatConfirmCode?.Equals("0") == true)
                        {
                            // DepartSeatConfirmCode = "0" for confirm success
                            return UISalesInst.CustInfoAck;
                        }
                        else
                            return UISalesInst.DepartSeatConfirmFailAck;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustInfoSubmission)
                        return UISalesInst.CustInfoUpdateELSEReleaseSeatRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustInfoUpdateResult)
                    {
                        if (session.PassengerInfoUpdateStatus == ProcessResult.Success)
                            return UISalesInst.SalesPaymentProceedAck;
                        else
                            return UISalesInst.CustInfoUpdateFailAck;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.SalesPaymentSubmission)
                    {
                        return UISalesInst.CompleteTransactionElseReleaseSeatRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.SeatReleaseRequest)
                    {
                        return UISalesInst.SeatReleaseRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CompleteTransactionResult)
                    {
                        return UISalesInst.RedirectDataToClient;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CountDownPausedRequest)
                    {
                        return UISalesInst.CountDownPausedRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.TimeoutChangeRequest)
                    {
                        return UISalesInst.TimeoutChangeRequest;
                    }

                    //UISalesPaymentSubmission
                }
            }
            return UISalesInst.Unknown;
        }

        public void ResetCleanUp()
        {

        }

        public UserSession UpdateUserSession(UserSession userSession, IKioskMsg svcMsg)
        {
            if (svcMsg is UILanguageSubmission uiLang)
            {
                userSession.CurrentEditMenuItemCode = null;
                userSession.Language = uiLang.Language;

                userSession.TravelMode = AppDecorator.Common.TravelMode.DepartOnly;
            }

            else if (svcMsg is UIOriginSubmission uiOrig)
            {
                userSession.OriginStationCode = uiOrig.OriginCode;
                userSession.OriginStationName = uiOrig.OriginName;
            }

            else if (svcMsg is UIDestinationSubmission uiDest)
            {
                userSession.DestinationStationCode = uiDest.DestinationCode;
                userSession.DestinationStationName = uiDest.DestinationName;
            }

            else if (svcMsg is UITravelDateSubmission uiTvDate)
            {
                userSession.SetTravelDate(userSession.TravelMode, uiTvDate.DepartDate, uiTvDate.ReturnDate);
            }

            else if (svcMsg is UIDepartTripListRequest uiTrpReq)
            {
                userSession.SetTravelDate(userSession.TravelMode, uiTrpReq.PassengerDepartDate, userSession.ReturnPassengerDate);
            }

            else if (svcMsg is UIDepartTripSubmission uiDTripSubm)
            {
                userSession.SetTravelDate(userSession.TravelMode, uiDTripSubm.DepartPassengerDate, userSession.ReturnPassengerDate);
                userSession.DepartCompanyDesc = uiDTripSubm.DepartOperatorDesc;
                userSession.DepartCompanyLogoUrl = uiDTripSubm.DepartOperatorLogoUrl;
                userSession.DepartCurrency = uiDTripSubm.DepartCurrency;
                userSession.DepartEmbed = uiDTripSubm.DepartEmbed;
                userSession.DepartInsurance = uiDTripSubm.DepartInsurance;
                userSession.DepartPassengerDepartTime = uiDTripSubm.DepartPassengerDepartTime;
                userSession.DepartRouteDetail = uiDTripSubm.DepartRouteDetail;
                userSession.DepartTimeposi = uiDTripSubm.DepartTimePosi;
                userSession.DepartSkyWayAmount = uiDTripSubm.DepartSkyWayAmount;
                //userSession.DepartTotalAmount = 
                userSession.DepartTripId = uiDTripSubm.DepartTripId;
                userSession.DepartTripNo = uiDTripSubm.DepartTripNo;
                userSession.DepartPassengerActualFromStationsCode = uiDTripSubm.DepartPassengerActualFromStationCode;
                userSession.DepartPassengerActualToStationsCode = uiDTripSubm.DepartPassengerActualToStationCode;
                userSession.DepartVehicleTripDate = uiDTripSubm.DepartVehicleTripDate;
            }

            else if (svcMsg is UIDepartSeatListErrorResult uiSeatError)
            {
                userSession = SetEditingSession(userSession, TickSalesMenuItemCode.DepartOperator);
            }

            else if (svcMsg is UIDepartSeatSubmission uiDSeatSubm)
            {
                userSession.DepartAdultDisc = uiDSeatSubm.DepartAdultDisc;
                userSession.DepartAdultExtra = uiDSeatSubm.DepartAdultExtra;
                userSession.DepartAdultPrice = uiDSeatSubm.DepartAdultPrice;
                userSession.DepartBusType = uiDSeatSubm.DepartBusType;
                userSession.SetTravelDate(userSession.TravelMode, DateTime.ParseExact(uiDSeatSubm.DepartDate, "dd/MM/yyyy", _provider), userSession.ReturnPassengerDate);
                userSession.DepartInsurance = uiDSeatSubm.DepartInsurance;
                userSession.DepartSkyWayAmount = uiDSeatSubm.DepartSkyWayAmount;
                userSession.DepartOnlineQrCharge = uiDSeatSubm.DepartOnlineQrCharge;
                userSession.DepartTerminalCharge = uiDSeatSubm.DepartTerminalCharge;
                userSession.DepartTripCode = uiDSeatSubm.DepartTripCode;
                userSession.PassengerSeatDetailList = uiDSeatSubm.PassengerSeatDetailList;
                userSession.DepartPickupNDropList = uiDSeatSubm.PickupAndDropList;
                userSession.DepartTotalAmount = (uiDSeatSubm.DepartAdultPrice + userSession.DepartInsurance + uiDSeatSubm.DepartTerminalCharge + uiDSeatSubm.DepartOnlineQrCharge)
                    * uiDSeatSubm.PassengerSeatDetailList.Length;
            }

            else if (svcMsg is UIDepartPickupNDropSubmission uiPupDpSubm)
            {
                userSession.DepartDrop = uiPupDpSubm.DepartDrop;
                userSession.DepartDropDesn = uiPupDpSubm.DepartDropDesn;
                userSession.DepartPick = uiPupDpSubm.DepartPick;
                userSession.DepartPickDesn = uiPupDpSubm.DepartPickDesn;
                userSession.DepartPickTime = uiPupDpSubm.DepartPickTime;
            }

            else if (svcMsg is UIInsuranceSubmission uiInsurnSubm)
            {
                if (uiInsurnSubm.IsIncludeInsurance == false)
                {
                    userSession.DepartInsurance = 0M;
                    userSession.DepartTotalAmount = (userSession.DepartAdultPrice + userSession.DepartTerminalCharge + userSession.DepartOnlineQrCharge)
                        * userSession.PassengerSeatDetailList.Length;

                    userSession.DepartTotalAmount = 1M;
                }
                else
                {
                    userSession.DepartTotalAmount = (userSession.DepartAdultPrice + userSession.DepartInsurance + userSession.DepartTerminalCharge + userSession.DepartOnlineQrCharge)
                        * userSession.PassengerSeatDetailList.Length;

                    userSession.DepartTotalAmount = 1M;
                }
            }
            else if(svcMsg is UISkyWaySubmission uISkyWaySubm)
            {

                if(uISkyWaySubm.IsIncludeSkyWay == false)
                {
                    userSession.DepartSkyWayAmount = 0M;
                    userSession.DepartTotalAmount = (userSession.DepartAdultPrice + userSession.DepartTerminalCharge + userSession.DepartOnlineQrCharge)
                         * userSession.PassengerSeatDetailList.Length;
                }
                else
                {
                    userSession.DepartTotalAmount = (userSession.DepartAdultPrice + userSession.DepartSkyWayAmount + userSession.DepartTerminalCharge + userSession.DepartOnlineQrCharge)
                        * userSession.PassengerSeatDetailList.Length;
                }
            }

            else if (svcMsg is UIDepartSeatConfirmResult uiDepSeatConfResult)
            {
                if (uiDepSeatConfResult.MessageData != null)
                {
                    seatconfirm_status stt = (seatconfirm_status)uiDepSeatConfResult.MessageData;

                    userSession.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                    userSession.DepartSeatConfirmCode = stt.code.ToString().Trim();
                    userSession.DepartSeatConfirmTransNo = stt.transaction_no;
                    userSession.DepartSeatConfirmMessage = stt.msg;
                }
            }

            else if (svcMsg is UICustInfoSubmission uiCustSubm)
            {
                userSession.PassengerSeatDetailList = uiCustSubm.PassengerSeatDetailList;
            }

            else if (svcMsg is UIDepartCustInfoUpdateResult uiCustUpdRes)
            {
                if (uiCustUpdRes.MessageData is changeticketname_status updStt)
                {

                    if (updStt.code == 0)
                    {
                        userSession.PassengerInfoUpdateStatus = ProcessResult.Success;
                    }
                    else
                    {
                        userSession.PassengerInfoUpdateStatus = ProcessResult.Fail;
                    }
                }
                else
                    userSession.PassengerInfoUpdateStatus = ProcessResult.Fail;
            }

            else if (svcMsg is UISalesPaymentSubmission uiPaymSubm)
            {
                userSession.PaymentState = AppDecorator.Common.PaymentResult.Success;

                userSession.TypeOfPayment = uiPaymSubm.TypeOfPayment;

                userSession.Cassette1NoteCount = uiPaymSubm.Cassette1NoteCount;
                userSession.Cassette2NoteCount = uiPaymSubm.Cassette2NoteCount;
                userSession.Cassette3NoteCount = uiPaymSubm.Cassette3NoteCount;
                userSession.RefundCoinAmount = uiPaymSubm.RefundCoinAmount;

                userSession.PaymentRefNo = uiPaymSubm.PaymentRefNo;
                userSession.PaymentMethodCode = uiPaymSubm.PaymentMethodCode;

                if (uiPaymSubm.TypeOfPayment == PaymentType.CreditCard)
                    userSession.PaymentTypeDesc = "Credit/Debit Card";
                else if (uiPaymSubm.TypeOfPayment == PaymentType.PaymentGateway)
                    userSession.PaymentTypeDesc = "eWallet";
                else
                    userSession.PaymentTypeDesc = "Cash";
            }

            else if (svcMsg is UISeatReleaseRequest uiStRelReq)
            {
                userSession.DepartPendingReleaseTransNo = uiStRelReq.TransactionNo;

                userSession.DepartSeatConfirmCode = null;
                userSession.DepartSeatConfirmTransNo = null;
                userSession.DepartSeatConfirmMessage = null;
            }

            return userSession;
        }

        public UserSession SetUIPageNavigateSession(UserSession userSession, UIPageNavigateRequest pageNav)
        {
            if (pageNav.NavigateDirection == PageNavigateDirection.Previous)
            {
                if (userSession.CurrentEditMenuItemCode.HasValue == false)
                { }
                else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.Passenger)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.DepartSeat);
                }
                else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.DepartSeat)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.DepartOperator);
                }
                else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.DepartOperator)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.DepartDate);
                }
                else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.DepartDate)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.ToStation);
                }
                else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.ToStation)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.FromStation);
                }
            }
            //else if (pageNav.NavigateDirection == PageNavigateDirection.Exit)
            //{ }

            return userSession;
        }

        public UserSession SetEditingSession(UserSession userSession, TickSalesMenuItemCode detailItemCode)
        {
            if (detailItemCode == TickSalesMenuItemCode.DepartSeat)
            {
                userSession = ResetDepartSeat(userSession);
            }
            if (detailItemCode == TickSalesMenuItemCode.DepartOperator)
            {
                userSession = ResetDepartSeat(userSession);
                userSession = ResetDepartOperator(userSession);
            }
            else if (detailItemCode == TickSalesMenuItemCode.DepartDate)
            {
                userSession = ResetDepartSeat(userSession);
                userSession = ResetDepartOperator(userSession);
                userSession = ResetTravelDates(userSession);
            }
            else if (detailItemCode == TickSalesMenuItemCode.ToStation)
            {
                userSession = ResetDepartSeat(userSession);
                userSession = ResetDepartOperator(userSession);
                userSession = ResetTravelDates(userSession);
                userSession = ResetDestStation(userSession);
            }
            else if (detailItemCode == TickSalesMenuItemCode.FromStation)
            {
                userSession = ResetDepartSeat(userSession);
                userSession = ResetDepartOperator(userSession);
                userSession = ResetTravelDates(userSession);
                userSession = ResetDestStation(userSession);
                userSession = ResetOriginStation(userSession);
            }

            return userSession;

            UserSession ResetOriginStation(UserSession session)
            {
                session.OriginStationCode = null;
                session.OriginStationName = null;

                return session;
            }

            UserSession ResetDestStation(UserSession session)
            {
                session.DestinationStationCode = null;
                session.DestinationStationName = null;

                return session;
            }

            UserSession ResetTravelDates(UserSession session)
            {
                session.SetTravelDate(session.TravelMode, null, null);
                return session;
            }

            UserSession ResetDepartOperator(UserSession session)
            {
                session.DepartCompanyDesc = null;
                session.DepartCompanyLogoUrl = null;
                session.DepartCurrency = null;
                session.DepartEmbed = null;
                session.DepartInsurance = 0M;
                session.DepartPassengerDepartTime = null;
                session.DepartRouteDetail = null;
                session.DepartTimeposi = -1;
                session.DepartTripId = null;
                session.DepartTripNo = null;
                session.DepartPassengerActualFromStationsCode = null;
                session.DepartPassengerActualToStationsCode = null;
                session.DepartVehicleTripDate = null;
                return session;
            }

            UserSession ResetDepartSeat(UserSession session)
            {
                session.DepartAdultDisc = 0M;
                session.DepartAdultExtra = null;
                session.DepartAdultPrice = 0M;
                session.DepartBusType = null;
                session.DepartInsurance = 0M;
                session.DepartOnlineQrCharge = 0M;
                session.DepartTerminalCharge = 0M;
                session.DepartTripCode = -1;
                session.PassengerSeatDetailList = null;
                session.DepartPickupNDropList = null;

                if ((string.IsNullOrWhiteSpace(session.DepartSeatConfirmTransNo) == false) &&
                    (session.DepartSeatConfirmCode?.Trim().Equals("0") == true))
                    session.DepartPendingReleaseTransNo = session.DepartSeatConfirmTransNo;
                else
                    session.DepartPendingReleaseTransNo = null;

                session.DepartSeatConfirmCode = null;
                session.DepartSeatConfirmTransNo = null;
                session.DepartSeatConfirmMessage = null;

                session.DepartDrop = "";
                session.DepartDropDesn = "";
                session.DepartPick = "";
                session.DepartPickDesn = "";
                session.DepartPickTime = "";

                userSession.PassengerSeatDetailList = null;
                session.PassengerInfoUpdateStatus = ProcessResult.Fail;

                return session;
            }
        }

        public IKioskMsg DoProcess(string procId, Guid? netProcId, IKioskMsg svcMsg, UserSession session, AppCallBackDelg appCallBackEvent, out bool releaseSeatRequest)
        {
            releaseSeatRequest = false;
            return null;
        }
    }
}
