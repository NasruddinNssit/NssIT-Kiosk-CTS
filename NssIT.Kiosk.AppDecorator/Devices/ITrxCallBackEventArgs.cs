using System;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;

namespace NssIT.Kiosk.AppDecorator.Devices
{
	/// <summary>
	/// Used for a Payment Device for sending a Completed Events
	/// </summary>
	public interface ITrxCallBackEventArgs : IDisposable
	{
		Guid? NetProcessId { get; }
		string ProcessId { get; }

		bool IsSuccess { get; }
		string Remark { get; set; }
		string ErrorMsg { get; }
		Exception Error { get; set; }
		DeviceProgressStatus ResultStatus { get; set; }

		//IEventMessageObj EventMessageObj { get; set; }
		IKioskMsg KioskMessage { get; set; }

		void AddErrorMessage(string errorMsg);
		void SetEmptyTo(bool setNull = true);
		void ResetInfo();

		ITrxCallBackEventArgs Duplicate();
	}
}
