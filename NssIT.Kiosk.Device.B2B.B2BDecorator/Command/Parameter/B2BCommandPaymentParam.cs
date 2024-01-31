using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Command.Parameter
{
	public class B2BCommandPaymentParam : IB2BCommandParameter
	{
		public string ProcessId { get; set; }
		public string Command { get; set; }
		public decimal Amount { get; set; }

		public string DocNumbers { get; set; }
		public bool ProcessDone { get; set; } = false;

		public int MaxWaitingSec { get; set; } = 300;

		public bool HasCommand { get { return (!string.IsNullOrWhiteSpace(Command)); } }

		public void Dispose()
		{
			
		}
	}
}
