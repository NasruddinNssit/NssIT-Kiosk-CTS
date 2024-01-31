using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    public interface IPayment
    {
        void InitPayment(UserSession session);
        void UpdateTransCompleteStatus(UICompleteTransactionResult uiCompltResult);
        void BTnGShowPaymentInfo(IKioskMsg kioskMsg);
        ICash GetCashierPage();
        IBTnG GetBTnGCounterPage();
    }
}
