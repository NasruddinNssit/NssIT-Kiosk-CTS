using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx
{
    [Serializable]
    public class UIxBTnGPaymentEndAck : UIxKioskDataAckBase, IUIxBTnGPaymentOngoingGroupAck, IUIxBTnGPaymentGroupAck
    {
        public string Message { get; private set; } = null;
        public string ErrorMsg { get; private set; } = null;
        public PaymentResult ResultState { get; private set; } = PaymentResult.Unknown;

        public UIxBTnGPaymentEndAck(Guid? refNetProcessId, string processId, 
            PaymentResult resultState, string message = null, string errorMsg = null)
            : base()
        {
            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            //-------------------------------------
            ResultState = resultState;
            Message = message;
            ErrorMsg = errorMsg;
        }
    }
}
