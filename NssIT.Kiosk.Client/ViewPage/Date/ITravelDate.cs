using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Date
{
    public interface ITravelDate
    {
        void InitData(UserSession session);
        void QuerySelectedDate(out DateTime? departDate, out DateTime? returnDate);
    }
}
