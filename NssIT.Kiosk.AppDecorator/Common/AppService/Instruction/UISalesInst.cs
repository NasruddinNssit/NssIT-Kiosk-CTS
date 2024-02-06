using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Instruction
{
	public enum UISalesInst
	{
		Unknown = CommInstruction.Blank,

		CountDownStartRequest = CommInstruction.UISalesCountDownStartRequest,
		CountDownExpiredAck = CommInstruction.UISalesCountDownExpiredAck,

		LanguageSelectionAck = CommInstruction.UISalesLanguageSelectionAck,
		LanguageSubmission = CommInstruction.UISalesLanguageSubmission,

		StartNewSalesRequest = CommInstruction.UISalesStartNewSalesRequest,

		ServerApplicationStatusRequest = CommInstruction.UISalesServerApplicationStatusRequest,
		ServerApplicationStatusAck = CommInstruction.UISalesServerApplicationStatusAck,

		OriginListRequest = CommInstruction.UISalesOriginListRequest,
		OriginListAck = CommInstruction.UISalesOriginListAck,
		OriginSubmission = CommInstruction.UISalesOriginSubmission,

		DestinationListRequest = CommInstruction.UISalesDestinationListRequest,
		DestinationListAck = CommInstruction.UISalesDestinationListAck,
		DestinationSubmission = CommInstruction.UISalesDestinationSubmission,

		SkyWaySubmission = CommInstruction.UISkyWaySubmission,
		SkyWayAck = CommInstruction.SkyWayAck,

		TravelDatesEnteringAck = CommInstruction.UISalesTravelDatesEnteringAck,
		TravelDatesSubmission = CommInstruction.UISalesTravelDatesSubmission,

		WebServerLogonRequest = CommInstruction.UISalesWebServerLogonRequest,
		WebServerLogonStatusAck = CommInstruction.UISalesWebServerLogonStatusAck,

		DetailEditRequest = CommInstruction.UISalesDetailEditRequest,

		DepartTripListRequest = CommInstruction.UISalesDepartTripListRequest,
		DepartTripListAck = CommInstruction.UISalesDepartTripListAck,
		DepartTripSubmission = CommInstruction.UISalesDepartTripSubmission,
		DepartTripListInitAck = CommInstruction.UISalesDepartTripListInitAck,
		DepartTripSubmissionErrorAck = CommInstruction.UISalesDepartTripSubmissionErrorAck,

		DepartSeatListRequest = CommInstruction.UISalesDepartSeatListRequest,
		DepartSeatListAck = CommInstruction.UISalesDepartSeatListAck,
		DepartSeatSubmission = CommInstruction.UISalesDepartSeatSubmission,
		DepartSeatConfirmRequest = CommInstruction.UISalesDepartSeatConfirmRequest,
		DepartSeatConfirmResult = CommInstruction.UISalesDepartSeatConfirmResult,
		DepartSeatConfirmFailAck = CommInstruction.UISalesDepartSeatConfirmFailAck,
		DepartSeatListErrorResult = CommInstruction.UISalesDepartSeatListErrorResult,

		DepartPickupNDropAck = CommInstruction.UISalesDepartPickupNDropAck,
		DepartPickupNDropSubmission = CommInstruction.UISalesDepartPickupNDropSubmission,

		InsuranceAck = CommInstruction.UISalesInsuranceAck,
		InsuranceSubmission = CommInstruction.UISalesInsuranceSubmission,

		CustInfoAck = CommInstruction.UISalesCustInfoAck,
		CustInfoSubmission = CommInstruction.UISalesCustInfoSubmission,
		CustInfoUpdateResult = CommInstruction.UISalesCustInfoUpdateResult,
		CustInfoUpdateFailAck = CommInstruction.UISalesCustInfoUpdateFailAck,
		/// /// <summary>
		/// Update Customer Info. Release Seat when failed to update
		/// </summary>
		CustInfoUpdateELSEReleaseSeatRequest = CommInstruction.UISalesCustInfoUpdateELSEReleaseSeatRequest,

		SalesPaymentProceedAck = CommInstruction.UISalesPaymentProceedAck,
		SalesPaymentSubmission = CommInstruction.UISalesPaymentSubmission,

		SeatReleaseRequest = CommInstruction.UISalesSeatReleaseRequest,
		SeatReleaseResult = CommInstruction.UISalesSeatReleaseResult,

		CompleteTransactionElseReleaseSeatRequest = CommInstruction.UISalesCompleteTransactionElseReleaseSeatRequest,
		CompleteTransactionResult = CommInstruction.UISalesCompleteTransactionResult,

		RedirectDataToClient = CommInstruction.UISalesRedirectDataToClient,

		CountDownPausedRequest = CommInstruction.UISalesCountDownPausedRequest,
		PageNavigateRequest = CommInstruction.UISalesPageNavigateRequest,
		TimeoutChangeRequest = CommInstruction.UISalesTimeoutChangeRequest
	}
}
