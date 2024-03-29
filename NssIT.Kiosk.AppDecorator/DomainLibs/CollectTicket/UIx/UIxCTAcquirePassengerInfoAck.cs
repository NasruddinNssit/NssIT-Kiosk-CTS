﻿using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx
{
    [Serializable]
    public class UIxCTAcquirePassengerInfoAck : UIxKioskDataAckBase
    {
        public UIxCTAcquirePassengerInfoAck(Guid? refNetProcessId, string processId)
            : base()
        {
            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
        }
    }
}
