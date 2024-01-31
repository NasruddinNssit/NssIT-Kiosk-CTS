using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public enum PaymentResultStatus
	{
		InitNewState = PaymentProgressStatus.New,
		Unknown = PaymentProgressStatus.UnknownState,
		Success = PaymentProgressStatus.PaymentSuccess,
		Fail = PaymentProgressStatus.FailPayment,
		FailInternalTrans = PaymentProgressStatus.FailInternalTrans,
		FailRefundOnPaymentSuccess = PaymentProgressStatus.FailRefundOnPaymentSuccess,
		Cancel = PaymentProgressStatus.PaymentCancelled,
		Timeout = PaymentProgressStatus.PaymentTimeout,
		Error = PaymentProgressStatus.Error,
		MachineMalfunction = PaymentProgressStatus.MachineMalFunctionError
	}
}
