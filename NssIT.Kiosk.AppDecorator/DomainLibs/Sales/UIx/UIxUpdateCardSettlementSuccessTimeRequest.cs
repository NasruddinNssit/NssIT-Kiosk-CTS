using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sales.UIx
{
    [Serializable]
    public class UIxUpdateCardSettlementSuccessTimeRequest : UIxKioskDataRequestBase
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public string ProcessId { get; private set; } = null;
        public DateTime SuccessTime { get; private set; } = DateTime.Now;

        public UIxUpdateCardSettlementSuccessTimeRequest(
            string processId, DateTime successTime)
            : base(processId)
        {
            ProcessId = processId;
            SuccessTime = successTime;
        }
    }
}