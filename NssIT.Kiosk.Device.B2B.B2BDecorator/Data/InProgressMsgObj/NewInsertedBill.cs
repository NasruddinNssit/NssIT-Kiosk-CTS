using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using System;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Data.InProgressMsgObj
{
	/// <summary>
	/// Normally Work with PaymentStatus = AppDecorator.Devices.Payment.PaymentProgressStatus.BillNoteConfirm
	/// </summary>
	public class NewInsertedBill : IKioskMsg
	{
		public Guid? RefNetProcessId { get; set; }
		public string ProcessId { get; set; }
		public DateTime TimeStamp { get; private set; }

		public AppModule Module { get; } = AppModule.UIPayment;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }

		public WorkProgressStatus PaymentStatus { get; } = WorkProgressStatus.BillNoteConfirm;

		public CommInstruction Instruction { get; } = CommInstruction.Blank;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		public int DigitBillType { get; set; } = 0;

		public decimal Price { get; set; } = 0M;
		public decimal InsertedAmount { get; set; } = 0M;
		public decimal OutstandingAmount { get; set; } = 0M;
		public decimal RefundAmount { get; set; } = 0M;

		public bool IsPaymentDone { get; set; } = false;
		public bool IsRefundRequest { get; set; } = false;

		public NewInsertedBill()
		{
			TimeStamp = DateTime.Now;
		}

		public void Dispose()
		{ }
	}
}
