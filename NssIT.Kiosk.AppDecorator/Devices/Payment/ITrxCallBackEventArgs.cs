using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public interface ITrxCallBackEventArgs : IDisposable
	{
		bool IsSuccess { get; }
		string Remark { get; set; }
		string ErrorMsg { get; }
		Exception Error { get; set; }
		PaymentResultStatus ResultStatus { get; set; }

		void AddErrorMessage(string errorMsg);
		void SetEmptyTo(bool setNull = true);
		void ResetInfo();

		ITrxCallBackEventArgs Duplicate();
	}
}
