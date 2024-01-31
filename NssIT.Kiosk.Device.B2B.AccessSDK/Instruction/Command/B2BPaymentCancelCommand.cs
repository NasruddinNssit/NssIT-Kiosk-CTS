using System;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;

namespace NssIT.Kiosk.Device.B2B.AccessSDK.Instruction.Command
{
	public class B2BPaymentCancelCommand : IB2BCommand
	{
		public B2BCommandCode CommandCode => B2BCommandCode.CancelPayment;

		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != B2BCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		public B2BPaymentCancelCommand(string processId, Guid? netProcessId)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
		}

		public void Dispose()
		{
			
		}
	}
}
