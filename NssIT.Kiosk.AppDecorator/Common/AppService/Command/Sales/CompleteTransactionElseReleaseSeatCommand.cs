using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class CompleteTransactionElseReleaseSeatCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.CompleteTransactionElseReleaseSeatRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string TransactionNo { get; private set; }
		public decimal TotalAmount { get; private set; }

		public PaymentType TypeOfPayment { get; set; } = PaymentType.Unknown;
		public int Cassette1NoteCount { get; private set; }
		public int Cassette2NoteCount { get; private set; }
		public int Cassette3NoteCount { get; private set; }
		public int RefundCoinAmount { get; private set; }

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		// BTnG & Credit Card
		public string PaymentRefNo { get; set; }
		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

		public string PaymentMethodCode { get; set; } = "C";

		public CompleteTransactionElseReleaseSeatCommand(string processId, Guid? netProcessId, string transactionNo, decimal totalAmount,
			int cassette1NoteCount, int cassette2NoteCount, int cassette3NoteCount, int refundCoinAmount, 
			PaymentType paymentType, string paymentMethodCode, string paymentRefNo)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			TransactionNo = transactionNo;
			TotalAmount = totalAmount;

			Cassette1NoteCount = cassette1NoteCount;
			Cassette2NoteCount = cassette2NoteCount;
			Cassette3NoteCount = cassette3NoteCount;
			RefundCoinAmount = refundCoinAmount;

			TypeOfPayment = paymentType;
			PaymentMethodCode = paymentMethodCode;
			PaymentRefNo = paymentRefNo;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}