using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI
{
	[Serializable]
	public class UIHideForm : IKioskMsg
	{
		public Guid? RefNetProcessId { get; private set; }
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIPayment;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;

		public CommInstruction Instruction { get; } = (CommInstruction)AppService.Instruction.UIPaymInstruction.HideForm;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public PaymentResult ResultState { get; private set; } = PaymentResult.Cancel;

		public PaymentType TypeOfPayment { get; private set; }
		public int Cassette1NoteCount { get; private set; }
		public int Cassette2NoteCount { get; private set; }
		public int Cassette3NoteCount { get; private set; }
		public int RefundCoinAmount { get; private set; }

		public UIHideForm(Guid? refNetProcessId, string processId, DateTime timeStamp, PaymentType paymentType)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			ResultState = PaymentResult.Unknown;
			Cassette1NoteCount = 0;
			Cassette2NoteCount = 0;
			Cassette3NoteCount = 0;
			RefundCoinAmount = 0;
			TypeOfPayment = paymentType;
		}

		public UIHideForm(Guid? refNetProcessId, string processId, DateTime timeStamp, PaymentResult resultState,
			int cassette1NoteCount, int cassette2NoteCount, int cassette3NoteCount, int refundCoinAmount)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			TypeOfPayment = PaymentType.Cash;

			ResultState = resultState;
			Cassette1NoteCount = cassette1NoteCount;
			Cassette2NoteCount = cassette2NoteCount;
			Cassette3NoteCount = cassette3NoteCount;
			RefundCoinAmount = refundCoinAmount;
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
