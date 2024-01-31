using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
	public class EndOfPaymentEventArgs : EventArgs, IDisposable
	{
		public string ProcessId { get; } = null;
		public PaymentResult ResultState { get; private set; } = PaymentResult.Cancel;
		public PaymentType TypeOfPayment { get; set; } = PaymentType.Unknown;
		public string PaymentMethod { get; private set; }
		//====================================================================
		//For B2B Cash Payment Only
		public int Cassette1NoteCount { get; private set; }
		public int Cassette2NoteCount { get; private set; }
		public int Cassette3NoteCount { get; private set; }
		public int RefundCoinAmount { get; private set; }
		//==================================================================
		// For 'Boost / Touch n Go' Payment transaction
		public string BTnGSaleTransactionNo { get; private set; }
		//==================================================================

		/// <summary>
		/// Payment Gateway (eWallet)
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="resultState"></param>
		/// <param name="bTngSaleTransactionNo"></param>
		public EndOfPaymentEventArgs(string processId, PaymentResult resultState, string bTngSaleTransactionNo, string paymentMethod)
		{
			ProcessId = string.IsNullOrWhiteSpace(processId) ? "-" : processId;
			ResultState = resultState;
			TypeOfPayment = PaymentType.PaymentGateway;
			PaymentMethod = paymentMethod;

			//CYA-DEMO
			if (App.SysParam.PrmNoPaymentNeed == false)
			{
				if ((resultState == PaymentResult.Success) && (string.IsNullOrWhiteSpace(bTngSaleTransactionNo)))
				{
					throw new Exception("Invalid Payment Gateway Sale Transaction Number with successful transaction result !");
				}
			}
			BTnGSaleTransactionNo = bTngSaleTransactionNo;
		}

		/// <summary>
		/// Cash Payment
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="resultState"></param>
		/// <param name="cassette1NoteCount"></param>
		/// <param name="cassette2NoteCount"></param>
		/// <param name="cassette3NoteCount"></param>
		/// <param name="refundCoinAmount"></param>
		public EndOfPaymentEventArgs(string processId, PaymentResult resultState,
			int cassette1NoteCount, int cassette2NoteCount, int cassette3NoteCount, int refundCoinAmount)
		{
			ProcessId = string.IsNullOrWhiteSpace(processId) ? "-" : processId;
			ResultState = resultState;

			TypeOfPayment = PaymentType.Cash;
			PaymentMethod = "C";

			Cassette1NoteCount = cassette1NoteCount;
			Cassette2NoteCount = cassette2NoteCount;
			Cassette3NoteCount = cassette3NoteCount;
			RefundCoinAmount = refundCoinAmount;
		}

		public void Dispose()
		{ }
	}
}