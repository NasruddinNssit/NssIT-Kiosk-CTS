using System.ComponentModel;
using NssIT.Kiosk.AppDecorator.Devices;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public enum RefundType
	{
		[Description("New")]
		New = DeviceProgressStatus.New,

		[Description("RefundForCancelPayment")]
		CancelPayment = DeviceProgressStatus.RefundOnCancelPayment,

		//[Description("FailRefundOnPaymentSuccess")]
		//FailRefundOnPaymentSuccess = PaymentProgressStatus.FailRefundOnPaymentSuccess,

		[Description("RefundForCompletePayment")]
		CompletePayment = DeviceProgressStatus.RefundOnPaymentCompleted
	}
}
