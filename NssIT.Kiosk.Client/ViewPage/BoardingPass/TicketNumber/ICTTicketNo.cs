using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.TicketNumber
{
    public interface ICTTicketNo
    {
        void InitData(IKioskMsg kioskMsg);
        void ShowTicketNumberNotFound(IKioskMsg kioskMsg);
    }
}
