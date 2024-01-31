using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public enum PaymentProgressStatus
	{
		[Description("New")]
		New = -1,				/* Mean this status is new and not yet assigned with any status */

		[Description("UnknownState")]
		UnknownState = 0,

		// ----- Normal Response -----
		[Description("StartPayment")]
		StartPayment = 1,

		[Description("DeviceInitiation")]
		DeviceInitiation = 5,   /* Cash & Card */

		[Description("MachineReadyToReceive")]
		ReadyToReceive = 8,     /* Cash & Card */

		[Description("Information")]
		Infor = 10,

		[Description("CashAcceptableInformation")]
		CashAcceptableInfor = 12,

		[Description("PaymentFound")]
		PaymentFound = 15,      /* For Card Tap OR Any Cash Found (Just Received) */

		[Description("BillNoteConfirm")]
		BillNoteConfirm = 20,	/* For Cash confirm received. */

		[Description("FirstPayment")]
		FirstPayment = 30,      /* For Cash */

		[Description("SubPayment")]
		SubPayment = 50,      /* For Cash */

		[Description("LastPayment")]
		LastPayment = 80,       /* For Cash */

		[Description("Refunding")]
		Refunding = 85,			/* For Cash */

		[Description("PaymentSuccess")]
		PaymentSuccess = 100,   /* For Cash & Card */

		// ----- Negative Response -----
		[Description("FailPayment")]
		FailPayment = 101,      /* For Cash & Card */

		[Description("FailRefundOnPaymentSuccess")]
		FailRefundOnPaymentSuccess = 103,      /* For Cash*/

		[Description("FailInternalTrans")]
		FailInternalTrans = 105,      /* For Cash & Card ; Fail DB transaction OR fail web server transaction.*/

		[Description("RefundOnPaymentCompleted")]
		RefundOnPaymentCompleted = 121,      /* For Cash*/

		[Description("PaymentCancelled")]
		PaymentCancelled = 201, /* For Cash & Card */

		[Description("RefundOnCancelPayment")]
		RefundOnCancelPayment = 221,		/* For Cash*/

		[Description("VoidPayment")]
		VoidPayment = 225,					/* For Card */

		[Description("PaymentTimeout")]
		PaymentTimeout = 301,  /* For Cash & Card */

		[Description("MachineInternalTimeout")]
		MachineInternalTimeout = 401,  /* For Card */

		// ----- Error Response -----
		[Description("Error")]
		Error = 9000,

		[Description("RefundError")]
		RefundError = 9010,

		[Description("MachineMalFunctionError")]
		MachineMalFunctionError = MachineState.MachineMalFunction,

		// ----- End -----
		[Description("PaymentDone")]
		PaymentDone = 10000000,     /* For Cash & Card */
	}
}
