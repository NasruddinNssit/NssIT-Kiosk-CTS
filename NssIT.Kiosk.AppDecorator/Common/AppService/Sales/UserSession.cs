using NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket;
using NssIT.Kiosk.AppDecorator.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class UserSession : IDisposable 
    {
        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        /// <summary>
        /// Normally refer to NetProcessID of UICountDownStartRequest
        /// </summary>
        public Guid SessionId { get; set; }
        public bool Expired { get; set; }
        public LanguageCode Language { get; set; }
        //-----------------------------------------------
        public TravelMode TravelMode { get; set; }
        //-----------------------------------------------
        public string OriginStationCode { get; set; }
        public string OriginStationName { get; set; }
        public string DestinationStationCode { get; set; }
        public string DestinationStationName { get; set; }
        public CustSeatDetail[] PassengerSeatDetailList { get; set; }
        public ProcessResult PassengerInfoUpdateStatus { get; set; } = ProcessResult.Fail;
        public PaymentResult PaymentState { get; set; } = PaymentResult.Unknown;
        public TickSalesMenuItemCode? CurrentEditMenuItemCode { get; set; } = null;

        /// <summary>
        /// State result refer to a response after the payment transaction has sent to web service. 
        /// </summary>
        public ProcessResult PaymentTransactionWebCompletedState { get; private set; } = ProcessResult.Fail;

        public string PaymentTypeDesc { get; set; }

        // OriginStationName DestinationStationName

        /// <summary>
        /// Customer Departure Date; The Date refer to customer checkin into a bus.
        /// </summary>
        public DateTime? DepartPassengerDate { get; private set; }
        public string DepartTripId { get; set; }
        /// <summary>
        /// Vehicle First station Departure Date; The Date refer to vehicle left the first station/terminal.
        /// </summary>
        public string DepartVehicleTripDate { get; set; }
        public string DepartBusType { get; set; }
        public decimal DepartAdultPrice { get; set; }
        /// <summary>
        /// Additional Ticket Information; Like aditional meal/Food
        /// </summary>
        public string DepartAdultExtra { get; set; }
        public decimal DepartAdultDisc { get; set; }
        public decimal DepartTerminalCharge { get; set; }
        public decimal DepartOnlineQrCharge { get; set; }
        public decimal DepartInsurance { get; set; }
        public decimal DepartSkyWayAmount { get; set; } 
        public decimal SkyWayTicketPrice { get; set; }
        public int DepartTripCode { get; set; }
        public string DepartPick { get; set; }
        public string DepartPickDesn { get; set; }
        public string DepartPickTime { get; set; }
        public string DepartDrop { get; set; }
        public string DepartDropDesn { get; set; }
        public decimal DepartTotalAmount { get;  set; }
        public string DepartCompanyDesc { get; set; }
        public string DepartCompanyLogoUrl { get; set; }
        public string DepartPassengerDepartTime { get; set; }
        //public string DepartTripDate { get; set; }
        public string DepartTripNo { get; set; }
        public short DepartTimeposi { get; set; }
        public string DepartRouteDetail { get; set; }
        public string DepartCurrency { get; set; }
        /// <summary>
        /// Pickup/Drop Switch => 0:Disable; 1:Enable
        /// </summary>
        public string DepartEmbed { get; set; }
        public string DepartPassengerActualFromStationsCode { get; set; }
        public string DepartPassengerActualToStationsCode { get; set; }
        public PickupNDropList DepartPickupNDropList { get; set; }
        public string DepartSeatConfirmCode { get; set; }
        public string DepartSeatConfirmTransNo { get; set; }
        public string DepartSeatConfirmMessage { get; set; }
        public string DepartPendingReleaseTransNo { get; set; }

        /// <summary>
        /// Customer Return Date; The Date refer to customer checkin into a bus.
        /// </summary>
        public DateTime? ReturnPassengerDate { get; private set; }
        public string ReturnTripId { get; set; }
        /// <summary>
        /// Vehicle First station Return Date; The Date refer to vehicle left the first station/terminal.
        /// </summary>
        public DateTime? ReturnVehicleTripDate { get; set; }
        public string ReturnBusType { get; set; }
        public decimal ReturnAdultPrice { get; set; }
        /// <summary>
        /// Additional Ticket Information; Like aditional meal/Food
        /// </summary>
        public string ReturnAdultExtra { get; set; }
        public decimal ReturnAdultDisc { get; set; }
        public decimal ReturnTerminalCharge { get; set; }
        public decimal ReturnOnlineQrCharge { get; set; }
        public decimal ReturnInsurance { get; set; }
        public int ReturnTripCode { get; set; }
        public string ReturnPick { get; set; }
        public string ReturnPickDesn { get; set; }
        public string ReturnPickTime { get; set; }
        public string ReturnDrop { get; set; }
        public string ReturnDropDesn { get; set; }
        public CustSeatDetail[] ReturnCustSeatDetailList { get; set; }
        public decimal ReturnTotalAmount { get; set; }

        public string ReturnCompanyDesc { get; set; }
        public string ReturnPassengerDepartTime { get; set; }
        public string ReturnTripDate { get; set; }
        public string ReturnTripNo { get; set; }
        public short ReturnTimeposi { get; set; }
        public string ReturnRouteDetail { get; set; }
        public string ReturnCurrency { get; set; }
        /// <summary>
        /// Pickup/Drop Switch => 0:Disable; 1:Enable
        /// </summary>
        public string ReturnEmbed { get; set; }
        public string ReturnVehicleFromStationsCode { get; set; }
        public string ReturnVehicleToStationsCode { get; set; }

        public PickupNDropList ReturnPickupNDropList { get; set; }
        public string ReturnSeatConfirmCode { get; set; }
        public string ReturnSeatConfirmTransNo { get; set; }
        public string ReturnSeatConfirmMessage { get; set; }

        public bool IsIncludeSkyWayTicket { get; set; }

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ///// Collection Boarding Pass    
        public BoardingTicket TicketCollection { get; set; } = new BoardingTicket();

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ///// Payment       xxxxx
        /////xxxxxxxxxxxxxxxxxxxx
        public PaymentType TypeOfPayment { get; set; } = PaymentType.Unknown;
        public string PaymentMethodCode { get; set; } = "C";

        // Cash Machine Status
        public int Cassette1NoteCount { get; set; }
        public int Cassette2NoteCount { get; set; }
        public int Cassette3NoteCount { get; set; }
        public int RefundCoinAmount { get; set; }
        //----------------------
        // BTnG & Credit Card
        public string PaymentRefNo { get; set; }
        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        public UserSession() { SessionReset(); }

        public void NewSession(Guid sessionId)
        {
            SessionReset();

            SessionId = sessionId;
            Expired = false;
        }

        public void SessionReset()
        {
            SessionId = Guid.Empty;
            Expired = true;
            Language = LanguageCode.English;
            TravelMode = TravelMode.DepartOnly;
            CurrentEditMenuItemCode = null;

            TicketCollection.Reset();

            OriginStationCode = null;
            OriginStationName = null;
            DestinationStationCode = null;
            DestinationStationName = null;
            PaymentState = PaymentResult.Unknown;
            PaymentTransactionWebCompletedState = ProcessResult.Fail;
            PaymentTypeDesc = null;

            PaymentRefNo = null;
            PaymentMethodCode = "C";
            TypeOfPayment = PaymentType.Unknown;
            Cassette1NoteCount = -1;
            Cassette2NoteCount = -1;
            Cassette3NoteCount = -1;

            DepartPassengerDate = null;
            DepartTripId = null;
            DepartBusType = null;
            DepartAdultPrice = 0M;
            DepartAdultExtra = null;
            SkyWayTicketPrice = 0M;
            DepartAdultDisc = 0M;
            DepartTerminalCharge = 0M;
            DepartOnlineQrCharge = 0M;
            DepartInsurance = 0M;
            DepartSkyWayAmount = 0M;
            DepartTripCode = -1;
            DepartPick = null;
            DepartPickDesn = null;
            DepartPickTime = null;
            DepartDrop = null;
            DepartDropDesn = null;
            PassengerSeatDetailList = null;
            PassengerInfoUpdateStatus = ProcessResult.Fail;
            DepartTotalAmount = 0M;
            DepartCompanyDesc = null;
            DepartCompanyLogoUrl = null;
            DepartPassengerDepartTime = null;
            //DepartTripDate = null;
            DepartTripNo = null;
            DepartTimeposi = -1;
            DepartRouteDetail = null;
            DepartCurrency = null;
            DepartEmbed = null;
            DepartVehicleTripDate = null;
            DepartPassengerActualFromStationsCode = null;
            DepartPassengerActualToStationsCode = null;
            DepartPickupNDropList = null;
            DepartSeatConfirmCode = null;
            DepartSeatConfirmTransNo = null;
            DepartSeatConfirmMessage = null;
            DepartPendingReleaseTransNo = null;

            ReturnPassengerDate = null;
            ReturnTripId = null;
            ReturnVehicleTripDate = null;
            ReturnBusType = null;
            ReturnAdultPrice = 0M;
            ReturnAdultExtra = null;
            ReturnAdultDisc = 0M;
            ReturnTerminalCharge = 0M;
            ReturnOnlineQrCharge = 0M;
            ReturnInsurance = 0M;
            ReturnTripCode = -1;
            ReturnPick = null;
            ReturnPickDesn = null;
            ReturnPickTime = null;
            ReturnDrop = null;
            ReturnDropDesn = null;
            ReturnCustSeatDetailList = null;
            ReturnTotalAmount = 0M;
            ReturnCompanyDesc = null;
            ReturnPassengerDepartTime = null;
            ReturnTripDate = null;
            ReturnTripNo = null;
            ReturnTimeposi = -1;
            ReturnRouteDetail = null;
            ReturnCurrency = null;
            ReturnEmbed = null;
            ReturnVehicleFromStationsCode = null;
            ReturnVehicleToStationsCode = null;
            ReturnPickupNDropList = null;
            ReturnSeatConfirmCode = null;
            ReturnSeatConfirmTransNo = null;
            ReturnSeatConfirmMessage = null;

            
        }

        public void SetTravelDate(TravelMode travelMode, DateTime? passengerDepartDate, DateTime? passengerReturnDate)
        {
            TravelMode = travelMode;
            DepartPassengerDate = passengerDepartDate;

            if (TravelMode == TravelMode.DepartOrReturn)
                ReturnPassengerDate = passengerReturnDate;
        }

        public void Dispose()
        {
            PassengerSeatDetailList = null; 
            ReturnCustSeatDetailList = null;
        }
    }
}
