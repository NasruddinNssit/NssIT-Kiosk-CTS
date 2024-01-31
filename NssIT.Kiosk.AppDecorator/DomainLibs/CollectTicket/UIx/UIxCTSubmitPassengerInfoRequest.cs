using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx
{
    [Serializable]
    public class UIxCTSubmitPassengerInfoRequest : UIxKioskDataRequestBase
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public string PassengerName { get; private set; }
        public string ICPassportNo { get; private set; }
        public string ContactNo { get; private set; }

        public UIxCTSubmitPassengerInfoRequest(string processId, string passengerName, string icPassportNo, string contactNo)
            : base(processId)
        {
            PassengerName = passengerName;
            ICPassportNo = icPassportNo;
            ContactNo = contactNo;
        }
    }
}
