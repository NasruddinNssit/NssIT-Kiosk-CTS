using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx
{
    [Serializable]
    public class UIxBTnGGetAvailablePaymentGatewayRequest : UIxKioskDataRequestBase, IUIxBTnGPaymentGroupAck
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        public UIxBTnGGetAvailablePaymentGatewayRequest(string processId)
            : base(processId)
        { }
    }
}
