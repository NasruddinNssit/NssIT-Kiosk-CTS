using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus.UIx
{
    [Serializable]
    public class UIxStatusMonitorDataSendRequest : UIxKioskDataRequestBase
    {
        public override CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOnly;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public KioskStatusData StatusData { get; private set; }

        public UIxStatusMonitorDataSendRequest(string processId, KioskStatusData statusData)
            : base(processId)
        {
            StatusData = statusData.Duplicate();
        }
    }
}
