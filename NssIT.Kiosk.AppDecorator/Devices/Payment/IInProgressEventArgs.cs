using System;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	/// <summary>
	/// Used for a Payment Device for sending a In Progress Events
	/// </summary>
	public interface IInProgressEventArgs
	{
		Guid? NetProcessId { get; }
		string ProcessId { get; }

		string Message { get; set; }
		IKioskMsg KioskMessage { get; set; }

		Exception Error { get; set; }
		DeviceProgressStatus PaymentStatus { get; set; }
	}
}
