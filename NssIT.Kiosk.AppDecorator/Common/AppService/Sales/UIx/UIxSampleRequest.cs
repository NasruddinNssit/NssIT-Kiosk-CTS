using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UIx
{
    public class UIxSampleDateRequest : UIxKioskDataRequestBase
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public UIxSampleDateRequest(string processId)
            : base(processId)
        {
        }
    }
}
