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
    public class UIxBTnGPaymentGatewayLogSuccessRequest : UIxKioskDataRequestBase, IUIxBTnGPaymentGroupAck
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOnly;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public string BTnGSaleTransactionNo { get; private set; }

        public UIxBTnGPaymentGatewayLogSuccessRequest(string processId, string bTnGSaleTransactionNo)
            : base(processId)
        {
            BTnGSaleTransactionNo = bTnGSaleTransactionNo;
        }
    }
}
