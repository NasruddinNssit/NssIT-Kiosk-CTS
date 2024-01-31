using System;
using NssIT.Kiosk.AppDecorator.Devices;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public enum PaymentResultStatus
	{
		InitNewState = DeviceProgressStatus.New,
		Unknown = DeviceProgressStatus.UnknownState,
		Success = DeviceProgressStatus.PaymentSuccess,
		Fail = DeviceProgressStatus.FailPayment,
		FailInternalTrans = DeviceProgressStatus.FailInternalTrans,
		FailRefundOnPaymentSuccess = DeviceProgressStatus.FailRefundOnPaymentSuccess,
		Cancel = DeviceProgressStatus.PaymentCancelled,
		Timeout = DeviceProgressStatus.PaymentTimeout,
		Error = DeviceProgressStatus.Error,
		MachineMalfunction = DeviceProgressStatus.MachineMalFunctionError
	}
}
