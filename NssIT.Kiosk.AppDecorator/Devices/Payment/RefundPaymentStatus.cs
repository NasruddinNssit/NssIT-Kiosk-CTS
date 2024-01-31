using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public class RefundPaymentStatus : IInProgressMsgObj
	{
		public string CustmerMsg { get; set; } = null;

		public string ProcessMsg { get; set; } = null;

		public decimal Price { get; set; }

		public decimal PaidAmount { get; set; }

		public decimal RefundAmount { get; set; }

		public RefundType TypeOfRefund { get; set; } = RefundType.New;

		public void Dispose()
		{  }
	}
}
