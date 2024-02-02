using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
	/// <summary>
	/// Common Instruction
	/// </summary>
	public enum CommInstruction
	{
		// Generic 0 - 1000
		Blank = 0,
		ReferToGenericsUIObj = 1000000,
		TransceiveTextMsg = 1,
		CheckConnection = 2,
		TransceiveErrorMsg = 3,
		TransceiveErrorMsgDetail = 4,
		NotChanged = 5,
		VisibleEnabled = 6,
		VisibleDisabled = 7,
		Enabled = 8,
		Disabled = 9,
		ShowErrorMessage = 10,
		Ignored = 11,

		// UIPaymInstruction 1200 - 1499
		UIPaymShowForm = 1201,
		UIPaymHideForm = 1202,
		UIPaymCloseForm = 1203,
		UIPaymInProgress = 1204,
		UIPaymCompleted = 1205,

		UIPaymShowBanknote = 1206,
		UIPaymShowOutstandingPayment = 1207,
		UIPaymShowRefundPayment = 1208,

		UIPaymShowCustomerMessage = 1209,
		UIPaymShowProcessingMessage = 1210,
		UIPaymShowCountdownMessage = 1211,

		UIPaymSetCancelPermission = 1212,
		UIPaymCancelPayment = 1213,
		UIPaymCreateNewPayment = 1214,

		// UICashMachineInst (UI All Cash Machine Instuction) 1500 - 2000
		UICashMachRequestMachineStatusSummary = 1501,
		UICashMachineStatusSummary = 1502,
		UICashMachRequestStartMakePayment = 1503,
		UICashMachRequestCancelPayment = 1504,
		UICashMachineStatus = 1505,

		// Below code used for B2B CashMachine -- 2001 - 2200
		UIB2BAllCassetteInfoRequest = 2001,
		UIB2BAllCassetteInfo = 2002,
        UISalesClientMaintenanceFinishedSubmission = 2205,
        // Below code used for Sales Application -- 2201 - 2500

        UISalesCountDownStartRequest = 2206,
		UISalesCountDownExpiredAck = 2207,

		UISalesLanguageSubmission = 2211,
		UISalesLanguageSelectionAck = 2212,

		UISalesWebServerLogonRequest = 2216,
		UISalesWebServerLogonStatusAck = 2217,

		UISalesStartNewSalesRequest = 2221,

		UISalesServerApplicationStatusRequest = 2226,
		UISalesServerApplicationStatusAck = 2227,

		UISalesOriginListRequest = 2290,
		UISalesOriginListAck = 2291,
		UISalesOriginSubmission = 2292,

		UISalesDestinationListRequest = 2231,
		UISalesDestinationListAck = 2232,
		UISalesDestinationSubmission = 2233,

		UISalesTravelDatesEnteringAck = 2236,
		UISalesTravelDatesSubmission = 2238,

		UISalesDetailEditRequest = 2241,

		UISalesDepartTripListRequest = 2246,
		UISalesDepartTripListAck = 2247,
		UISalesDepartTripSubmission = 2248,
		/// <summary>
		/// To Init a Depart Trip Page Without Trip Data
		/// </summary>
		UISalesDepartTripListInitAck = 2249,
		UISalesDepartTripSubmissionErrorAck = 2250,

		UISalesDepartSeatListRequest = 2251,
		UISalesDepartSeatListAck = 2252,
		UISalesDepartSeatSubmission = 2253,
		UISalesDepartSeatConfirmRequest = 2254,
		UISalesDepartSeatConfirmResult = 2255,
		UISalesDepartSeatConfirmFailAck = 2256,
		UISalesDepartSeatListErrorResult = 2257,

		UISalesDepartPickupNDropAck = 2261,
		UISalesDepartPickupNDropSubmission = 2262,

		UISalesInsuranceAck = 2264,
		UISalesInsuranceSubmission = 2265,

		UISalesCustInfoAck = 2266,
		UISalesCustInfoSubmission = 2267,
		UISalesCustInfoUpdateResult = 2268,
		/// <summary>
		/// Update Customer Info. Release Seat when failed to update
		/// </summary>
		UISalesCustInfoUpdateELSEReleaseSeatRequest = 2269,
		UISalesCustInfoUpdateFailAck = 2270,

		UISalesPaymentProceedAck = 2271,
		UISalesPaymentSubmission = 2272,

		UISalesSeatReleaseRequest = 2276,
		UISalesSeatReleaseResult = 2277,

		UISalesCompleteTransactionElseReleaseSeatRequest = 2281,
		UISalesCompleteTransactionResult = 2282,

		UISalesRedirectDataToClient = 2286,

		UISalesCountDownPausedRequest = 2287,
		UISalesPageNavigateRequest = 2288,
		UISalesTimeoutChangeRequest = 2289,

        // Below used for Credit Card Settlement
        UISalesCheckOutstandingCardSettlementRequest = 2360,
        UISalesCheckOutstandingCardSettlementAck = 2361,
        UISalesCardSettlementSubmission = 2362,
        UISalesCardSettlementStatusAck = 2363,
        //--------------------------------------

    }
}
