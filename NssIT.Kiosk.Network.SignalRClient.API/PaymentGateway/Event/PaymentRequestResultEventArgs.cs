using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Event
{
    public class PaymentRequestResultEventArgs : EventArgs
    {
        public CreateSaleResponse PaymentRequestResult { get; private set; }

        public PaymentRequestResultEventArgs(CreateSaleResponse paymentRequestResult)
        {
            PaymentRequestResult = paymentRequestResult;
        }
    }
}