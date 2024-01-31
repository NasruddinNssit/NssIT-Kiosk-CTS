using System;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.AppDecorator.Common.AppService;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Events
{
	public class InProgressEventArgs : EventArgs, IInProgressEventArgs
	{
		public Guid? NetProcessId { get; } = null;
		public string ProcessId { get; } = null;

		public InProgressEventArgs() { }
		/// <summary>
		/// When PaymentStatus = PaymentFound, and 
		/// </summary>
		public string Message { get; set; } = "Work in progress";

		//public IEventMessageObj MsgObj { get; set; } = null;
		public IKioskMsg KioskMessage { get; set; } = null;

		public Exception Error { get; set; } = null;

		public DeviceProgressStatus PaymentStatus { get; set; } = DeviceProgressStatus.New;
		public string PaymentStatusDesc { get => Enum.GetName(typeof(DeviceProgressStatus), this.PaymentStatus); }

		public void Dispose()
		{
			Error = null;
		}
	}
}
