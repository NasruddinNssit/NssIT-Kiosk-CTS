using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Ticketing.UIx
{
    [Serializable]
    public class UIxResetUserSessionSendOnlyRequest : UIxKioskDataRequestBase
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOnly;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        public UIxResetUserSessionSendOnlyRequest(string processId)
            : base(processId)
        { }
    }
}
