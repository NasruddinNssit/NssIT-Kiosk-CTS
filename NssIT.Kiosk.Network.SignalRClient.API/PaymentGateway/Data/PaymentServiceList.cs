using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data
{
    public class PaymentServiceList
    {
        public string MerchantId { get; set; }
        public PaymentService[] PaymentGatewayList { get; set; }
    }
}
