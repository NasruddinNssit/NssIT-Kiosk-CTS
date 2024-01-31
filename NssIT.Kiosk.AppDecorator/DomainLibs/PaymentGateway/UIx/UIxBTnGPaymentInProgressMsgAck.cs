using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx
{
    [Serializable]
    public class UIxBTnGPaymentInProgressMsgAck : UIxKioskDataAckBase, IUIxBTnGPaymentOngoingGroupAck, IUIxBTnGPaymentGroupAck
    {
        public string Message { get; private set; } = null;
        public bool? IsCancelAllowed { get; private set; } = null;

        /// <summary>
        /// </summary>
        /// <param name="msg">If value is null means no change</param>
        /// <param name="isCancelAllowed">If value is null means no change</param>
        public UIxBTnGPaymentInProgressMsgAck(Guid? refNetProcessId, string processId, 
            string msg, bool? isCancelAllowed = null) : base()
        {
            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            //-------------------------------------
            Message = msg;
            IsCancelAllowed = isCancelAllowed;
        }        
    }
}
