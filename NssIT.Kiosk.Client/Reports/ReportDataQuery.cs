using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Reports
{
    public class ReportDataQuery
    {
        public static transcomplete_status GetTicketTestData01()
        {
            // Field Length
            //---------------
            // companyname = 50 ; tripno = 20; 
            // platformno = 50 ;  gateno = 20
            transcomplete_status ticketList = new transcomplete_status()
            {
                code = 0,
                companyname = "Operator Company Name",
                currency = "RM",
                departuredate = DateTime.Now.ToString("dd/MM/yyyy"),
                departuretime = "10:30 PM",
                from = "TBS",
                to = "Melaka Sentral",
                platformno = "2",
                gateno = "7",
                msg = "msg : Be Early Bird",
                tripno = "TRIP12345678",
                QRLink = $@"http://127.0.0.1/QRCodeGenerator/QRCodeHandler.ashx"
            };

            ticketList.details = new tick_detail[2] {

                // Field Length
                //---------------
                // ticketno = 20 ;      seatdesn = 8;       seattypedesn = "ADULT"; ic = 14 
                // name = 50 ;          contact = 14;       refn = 20;              pick = 50
                // pick_time = 10;      drop = 50;          messagetocustomer = ;   salesinfo = 
                // extra = 500;         vege = ;            price = -;              insurance = -
                // terminalcharge = -;  onlineqrcharge = -; extraamount = -;      
                // barcodevalue = -;

                new tick_detail(){ ticketno = "TICK12345678", seatdesn = "E1", seattypedesn = "ADULT",
                                    ic = "IC1234567890",name = "AHMAD", contact = "1234567890111", refn = "REF12345611",
                                    pick = "PickA", pick_time = "104520", drop = "DropA",
                                    messagetocustomer = "Happy Travel", salesinfo = "SalesInfo01", extra = "ExtraA", vege = true,
                                    price = 5.20M, insurance = 0.40M, terminalcharge = 1.50M, onlineqrcharge = 0.50M, extraamount = 0.20M,
                                    barcodevalue = "TICK12345678"},

                new tick_detail(){ ticketno = "TICK22345677", seatdesn = "E2", seattypedesn = "ADULT",
                                    ic = "IC1234567888",name = "TAN", contact = "1234567890122", refn = "",
                                    pick = "", pick_time = "", drop = "",
                                    messagetocustomer = "Happy Travel", salesinfo = "SalesInfo01", extra = "ExtraA", vege = false,
                                    price = 5.20M, insurance = 0.0M, terminalcharge = 0.0M, onlineqrcharge = 0.0M, extraamount = 0.20M,
                                    barcodevalue = "TICK22345677"}
                //new tick_detail(){ ticketno = "TICK32345666", seatdesn = "E3", seattypedesn = "ADULT",
                //                    ic = "IC1234567877",name = "Ali", contact = "1234567890133", refn = "REF12345633",
                //                    pick = "PickA", pick_time = "104520", drop = "DropA",
                //                    messagetocustomer = "Happy Travel", salesinfo = "SalesInfo01", extra = "ExtraA", vege = false,
                //                    price = 5.20M, insurance = 0.40M, terminalcharge = 1.50M, onlineqrcharge = 0.50M, extraamount = 0.20M,
                //                    barcodevalue = "http://127.0.0.1/QRCodeGenerator/QRCodeHandler.ashx?t=TICK32345666&e=M&q=Two&s=4"},
                //new tick_detail(){ ticketno = "TICK42345655", seatdesn = "E4", seattypedesn = "ADULT",
                //                    ic = "IC1234567866",name = "Muto", contact = "1234567890144", refn = "REF12345644",
                //                    pick = "PickA", pick_time = "104520", drop = "DropA",
                //                    messagetocustomer = "Happy Travel", salesinfo = "SalesInfo01", extra = "ExtraA", vege = true,
                //                    price = 5.20M, insurance = 0.40M, terminalcharge = 1.50M, onlineqrcharge = 0.50M, extraamount = 0.20M,
                //                    barcodevalue = "http://127.0.0.1/QRCodeGenerator/QRCodeHandler.ashx?t=TICK42345655&e=M&q=Two&s=4"},
                //new tick_detail(){ ticketno = "TICK52345644", seatdesn = "F1", seattypedesn = "ADULT",
                //                    ic = "IC1234567855",name = "Kuma", contact = "1234567890155", refn = "REF12345655",
                //                    pick = "PickA", pick_time = "104520", drop = "DropA",
                //                    messagetocustomer = "Happy Travel", salesinfo = "SalesInfo01", extra = "ExtraA", vege = false,
                //                    price = 5.20M, insurance = 0.40M, terminalcharge = 1.50M, onlineqrcharge = 0.50M, extraamount = 0.20M,
                //                    barcodevalue = "http://127.0.0.1/QRCodeGenerator/QRCodeHandler.ashx?t=TICK52345644&e=M&q=Two&s=4"},
            };


            return ticketList;
        }

        public static DsMelakaCentralTicket ReadToTicketDataSet(string transactionNo, transcomplete_status webTransCompletedStatus, string terminalLogoPath, string bcImagePath, out bool errorFound)
        {
            errorFound = false;
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                DsMelakaCentralTicket ds = new DsMelakaCentralTicket();
                DsMelakaCentralTicket.TicketInfoDataTable dt = (DsMelakaCentralTicket.TicketInfoDataTable)ds.Tables["TicketInfo"];
                dt.AcceptChanges();
                ds.AcceptChanges();

                int testCount = -1;
                foreach (tick_detail tkRow in webTransCompletedStatus.details)
                {
                    testCount++;
                    DsMelakaCentralTicket.TicketInfoRow rw = dt.NewTicketInfoRow();
                    dt.Rows.Add(rw);

                    rw.Code = webTransCompletedStatus.code;
                    rw.Msg = webTransCompletedStatus.msg;
                    rw.CompanyName = webTransCompletedStatus.companyname;
                    rw.TripNo = webTransCompletedStatus.tripno;
                    rw.FromStation = webTransCompletedStatus.from;
                    rw.ToStation = webTransCompletedStatus.to;
                    rw.DepartureDate = webTransCompletedStatus.departuredate;
                    //if (string.IsNullOrWhiteSpace(webTransCompletedStatus.departuretime) == false)
                    //    rw.DepartureTime = DateTime.ParseExact($@"01/01/1900{webTransCompletedStatus.departuretime}", "dd/MM/yyyyHHmmss", provider).ToString("hh:mm tt");
                    //else
                    //    rw.DepartureTime = "";
                    rw.DepartureTime = webTransCompletedStatus.departuretime;
                    rw.Currency = webTransCompletedStatus.currency;
                    rw.GateNo = webTransCompletedStatus.gateno;
                    rw.PlatformNo = webTransCompletedStatus.platformno;
                    rw.QRLink = webTransCompletedStatus.QRLink;

                    rw.TicketNo = tkRow.ticketno;
                    rw.SeatDesn = tkRow.seatdesn;
                    rw.Name = tkRow.name;
                    rw.IC = tkRow.ic;
                    rw.Contact = tkRow.contact;
                    rw.Refn = tkRow.refn;
                    rw.SeatTypeDesn = tkRow.seattypedesn;
                    rw.Price = tkRow.price;
                    rw.Insurance = tkRow.insurance;
                    rw.TerminalCharge = tkRow.terminalcharge;
                    rw.OnlineQRCharge = tkRow.onlineqrcharge;
                    rw.BarcodeValue = tkRow.barcodevalue;
                    
                    rw.QRNoUrl = $@"{webTransCompletedStatus.QRLink}?t={tkRow.barcodevalue?.Trim()}&e=M&q=Two&s=4";
                    rw.QRTicketData = QRGen.GetQRCodeData(tkRow.barcodevalue);

                    ///// By refer to QR size (0.60974in width x 0.60974in height), webTransCompletedStatus.OnlineSurvey should not has string longer than 180 characters, else 2D barcode may not able to be read by scanner. 
                    if (string.IsNullOrWhiteSpace(webTransCompletedStatus.OnlineSurvey) == false)
                        rw.QRSurveyData = QRGen.GetQRCodeData(webTransCompletedStatus.OnlineSurvey);
                    else
                        rw.QRSurveyData = null;
                    /////-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                    rw.ExtraAmount = tkRow.extraamount;
                    rw.Extra = tkRow.extra;
                    rw.Vege = tkRow.vege;
                    rw.PickLocation = tkRow.pick;
                    //if (string.IsNullOrWhiteSpace(tkRow.pick_time) == false)
                    //    rw.PickTime = DateTime.ParseExact($@"01/01/1900{tkRow.pick_time}", "dd/MM/yyyyHHmmss", provider).ToString("hh:mm tt");
                    //else
                    //    rw.PickTime = "";
                    rw.PickTime = tkRow.pick_time;
                    rw.Drop = tkRow.drop;
                    rw.MessageToCustomer = tkRow.messagetocustomer;
                    rw.SalesInfo = tkRow.salesinfo;
                    rw.TicketTotalAmount = tkRow.price + tkRow.insurance + tkRow.terminalcharge + tkRow.onlineqrcharge + tkRow.extraamount;
                    rw.TerminalLogoPath = terminalLogoPath;
                    rw.BCImagePath = bcImagePath;

                    rw.AcceptChanges();
                    dt.AcceptChanges();

                    //if (testCount == 1)
                    //    throw new Exception("CYA-DEBUG .. QA Fail Testing");
                }

                dt.AcceptChanges();
                ds.AcceptChanges();
                return (DsMelakaCentralTicket)ds;
            }
            catch (Exception ex)
            {
                errorFound = true;
                //CYA-DEBUG .. Handle Error Here
                errorFound = true;
                return new DsMelakaCentralTicket();
            }
        }

        public static DsMelakaCentralTicket ReadToTicketDataSet(string transactionNo, boardingcollectticket_status webTransCompletedStatus, string terminalLogoPath, string bcImagePath, out bool errorFound)
        {
            errorFound = false;
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                DsMelakaCentralTicket ds = new DsMelakaCentralTicket();
                DsMelakaCentralTicket.TicketInfoDataTable dt = (DsMelakaCentralTicket.TicketInfoDataTable)ds.Tables["TicketInfo"];
                dt.AcceptChanges();
                ds.AcceptChanges();

                int testCount = -1;
                foreach (tick_detail tkRow in webTransCompletedStatus.details)
                {
                    testCount++;
                    DsMelakaCentralTicket.TicketInfoRow rw = dt.NewTicketInfoRow();
                    dt.Rows.Add(rw);

                    rw.Code = webTransCompletedStatus.code;
                    rw.Msg = webTransCompletedStatus.msg;
                    rw.CompanyName = webTransCompletedStatus.companyname;
                    rw.TripNo = webTransCompletedStatus.tripno;
                    rw.FromStation = webTransCompletedStatus.from;
                    rw.ToStation = webTransCompletedStatus.to;
                    rw.DepartureDate = webTransCompletedStatus.departuredate;
                    //if (string.IsNullOrWhiteSpace(webTransCompletedStatus.departuretime) == false)
                    //    rw.DepartureTime = DateTime.ParseExact($@"01/01/1900{webTransCompletedStatus.departuretime}", "dd/MM/yyyyHHmmss", provider).ToString("hh:mm tt");
                    //else
                    //    rw.DepartureTime = "";
                    rw.DepartureTime = webTransCompletedStatus.departuretime;
                    rw.Currency = webTransCompletedStatus.currency;
                    rw.GateNo = webTransCompletedStatus.gateno;
                    rw.PlatformNo = webTransCompletedStatus.platformno;
                    rw.QRLink = webTransCompletedStatus.QRLink;

                    rw.TicketNo = tkRow.ticketno;
                    rw.SeatDesn = tkRow.seatdesn;
                    rw.Name = tkRow.name;
                    rw.IC = tkRow.ic;
                    rw.Contact = tkRow.contact;
                    rw.Refn = tkRow.refn;
                    rw.SeatTypeDesn = tkRow.seattypedesn;
                    rw.Price = tkRow.price;
                    rw.Insurance = tkRow.insurance;
                    rw.TerminalCharge = tkRow.terminalcharge;
                    rw.OnlineQRCharge = tkRow.onlineqrcharge;
                    rw.BarcodeValue = tkRow.barcodevalue;

                    rw.QRNoUrl = $@"{webTransCompletedStatus.QRLink}?t={tkRow.barcodevalue?.Trim()}&e=M&q=Two&s=4";
                    rw.QRTicketData = QRGen.GetQRCodeData(tkRow.barcodevalue);

                    ///// By refer to QR size (0.60974in width x 0.60974in height), webTransCompletedStatus.OnlineSurvey should not has string longer than 180 characters, else 2D barcode may not able to be read by scanner. 
                    if (string.IsNullOrWhiteSpace(webTransCompletedStatus.OnlineSurvey) == false)
                        rw.QRSurveyData = QRGen.GetQRCodeData(webTransCompletedStatus.OnlineSurvey);
                    else
                        rw.QRSurveyData = null;
                    /////-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                    rw.ExtraAmount = tkRow.extraamount;
                    rw.Extra = tkRow.extra;
                    rw.Vege = tkRow.vege;
                    rw.PickLocation = tkRow.pick;
                    //if (string.IsNullOrWhiteSpace(tkRow.pick_time) == false)
                    //    rw.PickTime = DateTime.ParseExact($@"01/01/1900{tkRow.pick_time}", "dd/MM/yyyyHHmmss", provider).ToString("hh:mm tt");
                    //else
                    //    rw.PickTime = "";
                    rw.PickTime = tkRow.pick_time;
                    rw.Drop = tkRow.drop;
                    rw.MessageToCustomer = tkRow.messagetocustomer;
                    rw.SalesInfo = tkRow.salesinfo;
                    rw.TicketTotalAmount = tkRow.price + tkRow.insurance + tkRow.terminalcharge + tkRow.onlineqrcharge + tkRow.extraamount;
                    rw.TerminalLogoPath = terminalLogoPath;
                    rw.BCImagePath = bcImagePath;

                    rw.AcceptChanges();
                    dt.AcceptChanges();

                    //if (testCount == 1)
                    //    throw new Exception("CYA-DEBUG .. QA Fail Testing");
                }

                dt.AcceptChanges();
                ds.AcceptChanges();
                return (DsMelakaCentralTicket)ds;
            }
            catch (Exception ex)
            {
                errorFound = true;
                //CYA-DEBUG .. Handle Error Here
                errorFound = true;
                return new DsMelakaCentralTicket();
            }
        }

        public static DsMelakaCentralErrorTicketMessage GetTicketErrorDataSet(string transactionNo, string terminalVerticalLogoPath)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                DsMelakaCentralErrorTicketMessage ds = new DsMelakaCentralErrorTicketMessage();
                DsMelakaCentralErrorTicketMessage.ErrorInfoDataTable dt = (DsMelakaCentralErrorTicketMessage.ErrorInfoDataTable)ds.Tables["ErrorInfo"];
                dt.AcceptChanges();
                ds.AcceptChanges();

                DsMelakaCentralErrorTicketMessage.ErrorInfoRow rw = dt.NewErrorInfoRow();
                dt.Rows.Add(rw);

                rw.TerminalLogoPath = terminalVerticalLogoPath;
                rw.TransactionNo = transactionNo;
                rw.TimeStr = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss tt");
                rw.ErrorMsg = $@"Error encountered When Ticket Printing;";

                rw.AcceptChanges();
                dt.AcceptChanges();
                ds.AcceptChanges();
                return ds;
            }
            catch (Exception ex)
            {
                //CYA-DEBUG .. Handle Error Here
                return new DsMelakaCentralErrorTicketMessage();
            }
        }
    }
}
