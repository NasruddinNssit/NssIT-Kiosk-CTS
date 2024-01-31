using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public class OutstandingPaymentStatus : IInProgressMsgObj
	{
		public decimal LastBillInsertedAmount { get; set; } = 0.00M;

		public string CustmerMsg { get; set; } = null;
		public string ProcessMsg { get; set; } = null;

		public decimal Price { get; set; } = 0.00M;
		public decimal PaidAmount { get; set; } = 0.00M;
		public decimal OutstandingAmount { get; set; } = 0.00M;
		public decimal RefundAmount { get; set; } = 0M;

		public bool IsRefundRequest { get; set; } = false;
		public bool IsPaymentDone { get; set; } = false;

		public void Dispose()
		{  }
	}
}
