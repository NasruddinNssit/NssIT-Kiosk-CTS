using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public class RefundPaymentStatusEvtMessage : IKioskMsg
	{
		public Guid? RefNetProcessId { get; set; }
		public string ProcessId { get; set; }
		public DateTime TimeStamp { get; private set; }

		public AppModule Module { get; } = AppModule.UIPayment;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }

		public CommInstruction Instruction { get; } = CommInstruction.Blank;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		public string CustmerMsg { get; set; } = null;

		public string ProcessMsg { get; set; } = null;

		public decimal Price { get; set; }

		public decimal PaidAmount { get; set; }

		public decimal RefundAmount { get; set; }

		public RefundType TypeOfRefund { get; set; } = RefundType.New;

		public RefundPaymentStatusEvtMessage()
		{
			TimeStamp = DateTime.Now;
		}

		public void Dispose()
		{  }
	}
}
