using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx
{
    [Serializable]
    public class UIxBTnGPaymentCountDownAck : UIxKioskDataAckBase, IUIxBTnGPaymentOngoingGroupAck, IUIxBTnGPaymentGroupAck, IAgreeLog
    {
        public int CountDown { get; private set; } = 0;

        public bool IsAgreedLog
        {
            get 
            {
                if ((CountDown % 20) == 1)
                    return true;
                else
                    return false;
            }
        }

        public UIxBTnGPaymentCountDownAck(Guid? refNetProcessId, string processId, int countDown) 
            : base()
        {
            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            //-------------------------------------
            CountDown = countDown;
        }
    }
}
