using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
    public interface IStation
    {
        void InitOriginStationData(UIOriginListAck uiDest);
        void InitDestStationData(UIDestinationListAck uiDest);
    }
}
