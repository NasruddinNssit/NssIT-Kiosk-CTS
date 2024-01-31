using System;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;

namespace NssIT.Kiosk.Device.B2B.AccessSDK.Instruction.Command
{
	public class B2BPaymentCommand : IB2BCommand
	{
		public B2BCommandCode CommandCode => B2BCommandCode.StartMakePayment;
				
		public string ProcessId { get; private set; }
		public string DocNumbers { get; set; }

		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != B2BCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;
		public decimal Amount { get; set; }
		public int MaxWaitingSec { get; set; } = 390;

		public B2BPaymentCommand(string processId, Guid? netProcessId, decimal amount, string docNumbers = null, int maxWaitingSec = 390)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
			Amount = amount;
			DocNumbers = string.IsNullOrWhiteSpace(docNumbers) ? null : docNumbers;
			MaxWaitingSec = (maxWaitingSec < 0) ? 390 : maxWaitingSec;
		}

		public void Dispose()
		{
			
		}
	}
}
