using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command
{
	public enum AccessDBCommandCode
	{
		UnKnown = UISalesInst.Unknown,

		OriginListRequest = UISalesInst.OriginListRequest,
		DestinationListRequest = UISalesInst.DestinationListRequest,

		WebServerLogonRequest = UISalesInst.WebServerLogonRequest,
		DepartTripListRequest = UISalesInst.DepartTripListRequest,
		DepartSeatListRequest = UISalesInst.DepartSeatListRequest,
		DepartSeatConfirmRequest = UISalesInst.DepartSeatConfirmRequest,
		DepartCustInfoUpdateELSEReleaseSeatRequest = UISalesInst.CustInfoUpdateELSEReleaseSeatRequest,
		CompleteTransactionElseReleaseSeatRequest = UISalesInst.CompleteTransactionElseReleaseSeatRequest,

		TicketReleaseRequest = UISalesInst.SeatReleaseRequest,
        CheckOutstandingCardSettlementRequest = UISalesInst.CheckOutstandingCardSettlementRequest,



    }
}
