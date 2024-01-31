using NssIT.Kiosk.AppDecorator.Common;
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
    public class UIxCTSubmitBusCompanyRequest : UIxKioskDataRequestBase
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public string BusCompanyLogoURL { get; private set; }
        public string BusCompanyCode { get; private set; }
        public string BusCompanyName { get; private set; }

        public UIxCTSubmitBusCompanyRequest(string processId, string busCompanyLogoURL, string busCompanyCode, string busCompanyName)
            : base(processId)
        {
            BusCompanyLogoURL = busCompanyLogoURL;
            BusCompanyCode = busCompanyCode;
            BusCompanyName = busCompanyName;
        }
    }
}
