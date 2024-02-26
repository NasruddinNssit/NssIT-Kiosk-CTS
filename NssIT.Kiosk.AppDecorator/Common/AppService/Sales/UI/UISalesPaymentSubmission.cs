using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UISalesPaymentSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.SalesPaymentSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		public string TransactionNo { get; private set; } = null;
		public decimal TotalAmount { get; private set; } = 0M;
		public string PaymentMethodCode { get; private set; }
		//public PaymentResult PaymentState { get; private set; } = PaymentResult.Unknown;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		//Temporary Solution for capturing cash status
		public PaymentType TypeOfPayment { get; set; } = PaymentType.Unknown;

		// Cash Status
		public int Cassette1NoteCount { get; set; }
		public int Cassette2NoteCount { get; set; }
		public int Cassette3NoteCount { get; set; }
		public int RefundCoinAmount { get; set; }
		//-----------------------------------------------------------
		// For 'Boost / Touch n Go' Payment transaction

		// BTnG & Credit Card; Unique sale number.  
		public string PaymentRefNo { get; private set; } = null;
		/// <summary>
		/// Refer to KTMBCTS table PaymentGatewayMappings.PaymentMethod; Or NssIT.Train.Kiosk.Common.Constants.FinancePaymentMethod.
		/// </summary>
		/// 

		//For Credit Card Payment transaction
		public string BankReferenceNo { get; private set; }
		public CreditCardResponse CreditCardAnswer { get; private set; }

		//==================================================================

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public UISalesPaymentSubmission(string processId, DateTime timeStamp, string transactionNo, decimal totalAmount, 
			int cassette1NoteCount, int cassette2NoteCount, int cassette3NoteCount, int refundCoinAmount)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			TransactionNo = transactionNo;
			TotalAmount = totalAmount;
			//PaymentState = paymentState;
			PaymentMethodCode = "C";
			TypeOfPayment = PaymentType.Cash;
			Cassette1NoteCount = cassette1NoteCount;
			Cassette2NoteCount = cassette2NoteCount;
			Cassette3NoteCount = cassette3NoteCount;
			RefundCoinAmount = refundCoinAmount;
		}

		/// <summary>
		/// Submission for Payment Gateway (eWallet)
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="timeStamp"></param>
		/// <param name="transactionNo"></param>
		/// <param name="totalAmount"></param>
		/// <param name="paymentRefNo">BTnG Sale Transaction No. Or Unique number sale transaction for credit/debit Card </param>
		/// <param name="paymentMethod">'C' for cash; 'D' for credit/debit card; 'B' / 'T' for Payment Gateway (Boost / TnG)</param>
		/// <param name="paymentType"></param>
		public UISalesPaymentSubmission(string processId, DateTime timeStamp, string transactionNo, decimal totalAmount,
			string paymentRefNo, string paymentMethod, PaymentType paymentType)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			TransactionNo = transactionNo;
			TotalAmount = totalAmount;
			//TradeCurrency = tradeCurrency;
			PaymentRefNo = paymentRefNo;
			PaymentMethodCode = paymentMethod;
			TypeOfPayment = paymentType;
		}

		public UISalesPaymentSubmission(string processId, DateTime timeStamp, string transactionNo, decimal totalAmount, 
			string bankReferenceNo, CreditCardResponse creditCardAnswer)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			TransactionNo = transactionNo;
			TotalAmount = totalAmount;
			PaymentMethodCode = "D";
			TypeOfPayment = PaymentType.CreditCard;
			BankReferenceNo = bankReferenceNo;
			CreditCardAnswer = creditCardAnswer;
		}

		public void Dispose()
		{ }
	}
}
