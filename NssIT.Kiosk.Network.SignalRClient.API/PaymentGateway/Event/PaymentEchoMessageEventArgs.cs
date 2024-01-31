using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Event
{
    public class PaymentEchoMessageEventArgs
    {
        public string EchoMessage { get; private set; }

        public PaymentEchoMessageEventArgs(string echoMessage)
        {
            EchoMessage = echoMessage?.Trim();
        }
    }
}
