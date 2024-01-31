using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.PaymentGatewayApp.Base
{
    public class BTnGEventArg<T> : EventArgs, IDisposable
        where T : IUIxBTnGPaymentGroupAck
    {
        public T EventData { get; private set; }
        public BTnGEventArg(T dataObj)
        {
            EventData = dataObj;
        }

        public void Dispose()
        {
            EventData = default(T);
        }
    }
}
