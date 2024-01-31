using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data
{
    [Serializable]
    public class BTnGPaymentGatewayModel
    {
        public string MerchantId { get; set; }
        public BTnGPaymentGatewayDetailModel[] PaymentGatewayList { get; set; }
    }
}
