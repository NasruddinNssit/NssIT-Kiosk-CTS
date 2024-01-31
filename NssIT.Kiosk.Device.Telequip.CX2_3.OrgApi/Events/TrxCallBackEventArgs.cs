using NssIT.Kiosk.AppDecorator.Devices.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.CX2_3Decorator.Events
{
	public class TrxCallBackEventArgs : EventArgs, ITrxCallBackEventArgs
	{
		public string Remark { get; set; } = null;
		public Exception Error { get; set; } = null;
		public PaymentResultStatus ResultStatus { get; set; } = PaymentResultStatus.InitNewState;

		public bool IsSuccess { get { return (ResultStatus == PaymentResultStatus.Success); } }
		public string ErrorMsg { get { return Error?.Message; } }

		public void ResetInfo()
		{
			ResultStatus = PaymentResultStatus.InitNewState;
			Error = null;
		}

		public void SetEmptyTo(bool setNull = true)
		{ }

		public void AddErrorMessage(string errorMsg)
		{
			if (string.IsNullOrWhiteSpace(errorMsg) == false)
			{
				if (Error == null)
					Error = new Exception(errorMsg);
				else
					Error = new Exception($@"{errorMsg}; {Error.Message};", Error);
			}
		}

		public ITrxCallBackEventArgs Duplicate()
		{
			TrxCallBackEventArgs ev = new TrxCallBackEventArgs() { Error = this.Error, ResultStatus = this.ResultStatus, Remark = this.Remark };
			return ev;
		}

		public void Dispose()
		{
			Error = null;
		}
	}
}
