using System;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Events
{
	public class InProgressEventArgs : EventArgs, IInProgressEventArgs
	{
		public Guid? NetProcessId { get; } = null;
		public string ProcessId { get; } = null;

		/// <summary>
		/// When PaymentStatus = PaymentFound, and 
		/// </summary>
		public string Message { get; set; } = "Work in progress";

		//public IEventMessageObj MsgObj { get; set; } = null;
		public IKioskMsg KioskMessage { get; set; } = null;

		public Exception Error { get; set; } = null;

		public DeviceProgressStatus PaymentStatus { get; set; } = DeviceProgressStatus.New;
		public string PaymentStatusDesc { get => Enum.GetName(typeof(DeviceProgressStatus), this.PaymentStatus); }

		public B2BModuleAppGroup ModuleAppGroup { get; set; } = B2BModuleAppGroup.Unknown;

		public InProgressEventArgs(Guid? netProcessId, string processId)
		{
			NetProcessId = netProcessId;
			ProcessId = string.IsNullOrWhiteSpace(processId) ? "-" : processId;
		}

		public void Dispose()
		{
			Error = null;
			KioskMessage = null;
		}
	}
}
