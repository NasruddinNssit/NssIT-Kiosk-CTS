using System;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public class OutstandingPaymentStatusEvtMessage : IKioskMsg
	{
		public Guid? RefNetProcessId { get; set; }
		public string ProcessId { get; set; }
		public DateTime TimeStamp { get; private set; }

		public AppModule Module { get; } = AppModule.UIPayment;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }

		public CommInstruction Instruction { get; } = CommInstruction.Blank;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		public WorkProgressStatus PaymentStatus { get; }

		public decimal LastBillInsertedAmount { get; set; } = 0.00M;

		public string CustmerMsg { get; set; } = null;
		public string ProcessMsg { get; set; } = null;

		public decimal Price { get; set; } = 0.00M;
		public decimal PaidAmount { get; set; } = 0.00M;
		public decimal OutstandingAmount { get; set; } = 0.00M;
		public decimal RefundAmount { get; set; } = 0M;

		public bool IsRefundRequest { get; set; } = false;
		public bool IsPaymentDone { get; set; } = false;

		public OutstandingPaymentStatusEvtMessage()
		{
			TimeStamp = DateTime.Now;
		}

		public void Dispose()
		{  }
	}
}
