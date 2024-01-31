using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket
{
    [Serializable]
    public class BoardingTicket
    {
        public string BusCompanyName { get; set; }
        public string BusCompanyCode { get; set; }
        public string BusCompanyLogoURL { get; set; }

        public DateTime? PassengerDepartDate { get; set; }
        public string PassengerName { get; set; }
        public string PassengerIC { get; set; }
        public string PassengerContact { get; set; }

        public bool IsValidToPay { get; set; }
        public string InvalidToPayMessage { get; set; }

        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ///// 3rd Party Ticket Information - 
        public string LastSubmittedTicketNo { get; set; }
        public string TicketNo { get; set; }
        //-------------------------------------------
        public string ExistingContact { get; set; }
        public string ExistingName { get; set; }
        public string ExistingIC { get; set; }
        public string OpeRouteId { get; set; }
        public string TripNo { get; set; }
        public string TripDate { get; set; }
        public string From { get; set; }
        public string Fromdesn { get; set; }
        public string To { get; set; }
        public string ToDesn { get; set; }
        public string SeatNo { get; set; }
        public string SeatType { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal FacilityCharge { get; set; }
        public decimal QRCharge { get; set; }
        public decimal TotalChargableAmount { get; set; }
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        public void Reset()
        {
            BusCompanyName = null;
            BusCompanyCode = null;
            BusCompanyLogoURL = null;

            PassengerDepartDate = null;
            PassengerName = null;
            PassengerIC = null;
            PassengerContact = null;

            IsValidToPay = false;
            InvalidToPayMessage = null;

            ///// 3rd Party Ticket Information - 
            LastSubmittedTicketNo = null;
            TicketNo = null;

            ExistingContact = null;
            ExistingName = null;
            ExistingIC = null;
            OpeRouteId = null;
            TripNo = null;
            TripDate = null;
            From = null;
            Fromdesn = null;
            To = null;
            ToDesn = null;
            SeatNo = null;
            SeatType = null;
            SellingPrice = 0.0M;
            FacilityCharge = 0.0M;
            QRCharge = 0.0M;
            TotalChargableAmount = 0.0M;
        }

        public BoardingTicket Duplicate()
        {
            return new BoardingTicket()
            {
                BusCompanyName = this.BusCompanyName,
                BusCompanyCode = this.BusCompanyCode,
                BusCompanyLogoURL = this.BusCompanyLogoURL,

                PassengerDepartDate = this.PassengerDepartDate,
                PassengerName = this.PassengerName,
                PassengerIC = this.PassengerIC,
                PassengerContact = this.PassengerContact,

                IsValidToPay = this.IsValidToPay,
                InvalidToPayMessage = this.InvalidToPayMessage,

                ///// 3rd Party Ticket Information - 
                LastSubmittedTicketNo = this.LastSubmittedTicketNo,
                TicketNo = this.TicketNo,

                ExistingContact = this.ExistingContact,
                ExistingName = this.ExistingName,
                ExistingIC = this.ExistingIC,
                OpeRouteId = this.OpeRouteId,
                TripNo = this.TripNo,
                TripDate = this.TripDate,
                From = this.From,
                Fromdesn = this.Fromdesn,
                To = this.To,
                ToDesn = this.ToDesn,
                SeatNo = this.SeatNo,
                SeatType = this.SeatType,
                SellingPrice = this.SellingPrice,
                FacilityCharge = this.FacilityCharge,
                QRCharge = this.QRCharge,
                TotalChargableAmount = this.TotalChargableAmount
            };
        }
    }
}
