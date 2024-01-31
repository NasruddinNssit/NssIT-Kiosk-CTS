using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx
{
    [Serializable]
    public class UIxBTnGPaymentErrorAck : UIxKioskDataAckBase, IUIxBTnGPaymentOngoingGroupAck, IUIxBTnGPaymentGroupAck
    {
        public string ErrorMsg { get; private set; } = null;
        public UIxBTnGPaymentErrorAck(Guid? refNetProcessId, string processId,
            string errMsg) : base()
        {
            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            //-------------------------------------
            ErrorMsg = errMsg;
        }
    }
}
