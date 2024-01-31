using System;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public class StartCashPaymentEvtMessage : IKioskMsg
	{
		public Guid? RefNetProcessId { get; set; }
		public string ProcessId { get; set; }
		public DateTime TimeStamp { get; private set; }

		public AppModule Module { get; } = AppModule.UIPayment;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }

		public CommInstruction Instruction { get; } = CommInstruction.Blank;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		public WorkProgressStatus PaymentStatus { get; } = WorkProgressStatus.StartPayment;

		public string NewProcessId { get; private set; }
		public decimal Price { get; set; }

		public StartCashPaymentEvtMessage(string newProcessId)
		{
			NewProcessId = newProcessId;
			TimeStamp = DateTime.Now;
		}

		public void Dispose()
		{  }
	}
}