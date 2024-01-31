using NssIT.Kiosk.AppDecorator.Devices.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.OrgApi
{
	/// <summary>
	/// Nornally Work with PaymentStatus = AppDecorator.Devices.Payment.PaymentProgressStatus.BillNoteConfirm
	/// </summary>
	public class NewInsertedBill : IInProgressMsgObj
	{
		public int DigitBillType { get; set; } = 0;

		public decimal Price { get; set; } = 0M;
		public decimal InsertedAmount { get; set; } = 0M;
		public decimal OutstandingAmount { get; set; } = 0M;
		public decimal RefundAmount { get; set; } = 0M;

		public bool IsPaymentDone { get; set; } = false;
		public bool IsRefundRequest { get; set; } = false;

		public void Dispose()
		{ }
	}
}
