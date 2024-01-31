using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data
{
    public class PaymentReceipt
    {
		public string Status { get; set; }
		public string Description { get; set; }
		public string MerchantId { get; set; }
		public string SalesTransactionNo { get; set; }
		public string MerchantTransactionNo { get; set; }
		public decimal Amount { get; set; }
		public string CustomField { get; set; }
		public string Signature { get; set; }
	}
}
