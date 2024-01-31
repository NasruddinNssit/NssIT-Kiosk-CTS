using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx
{
    [Serializable]
    public class UIxBTnGPaymentCustomerMsgAck : UIxKioskDataAckBase, IUIxBTnGPaymentOngoingGroupAck, IUIxBTnGPaymentGroupAck
    {
        public string Message { get; private set; } = null;

        public UIxBTnGPaymentCustomerMsgAck(Guid? refNetProcessId, string processId, string msg)
            : base()
        {
            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            //-------------------------------------
            Message = msg;
        }
    }
}
