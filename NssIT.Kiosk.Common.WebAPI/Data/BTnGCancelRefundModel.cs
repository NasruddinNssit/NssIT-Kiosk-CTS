using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data
{
    [Serializable]
    public class BTnGCancelRefundModel
    {
        public string Status { get; set; }
        public string Description { get; set; }
        public string SalesTransactionNo { get; set; }
        public string MerchantTransactionNo { get; set; }
        public string Signature { get; set; }
    }
}
