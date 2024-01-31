using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data.Request
{
    public class CreatePaymentRequest
    {
        public string PaymentMethod { get; set; }
        public NewPayment BTnGPayment { get; set; }
    }
}
