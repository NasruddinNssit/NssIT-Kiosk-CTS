using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data
{
    [Serializable]
    public class BTnGCancelRefundReqInfo
    {
        public string MerchantId { get; set; }
        public string PaymentGateway { get; set; }
        public string MerchantTransactionNo { get; set; }
        public string SalesTransactionNo { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Signature { get; set; }
    }
}
