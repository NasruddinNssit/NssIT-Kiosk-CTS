using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public interface ICustInfo
    {
        void InitPassengerInfo(UICustInfoAck uiCustInfo);
    }
}
