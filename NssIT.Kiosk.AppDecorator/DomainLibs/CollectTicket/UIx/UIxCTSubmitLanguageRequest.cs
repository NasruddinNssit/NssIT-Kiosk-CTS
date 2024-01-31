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
    public class UIxCTSubmitLanguageRequest : UIxKioskDataRequestBase
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public LanguageCode Language { get; private set; }

        public UIxCTSubmitLanguageRequest(string processId, LanguageCode language)
            : base(processId)
        {
            Language = language;
        }
    }
}
