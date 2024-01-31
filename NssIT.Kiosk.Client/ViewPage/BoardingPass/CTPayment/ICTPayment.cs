using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.CTPayment
{
    public interface ICTPayment
    {
        void InitData(IKioskMsg kioskMsg);
        void BTnGShowPaymentInfo(IKioskMsg kioskMsg);

        //void UpdateTransCompleteStatus(IKioskMsg kioskMsg);
    }
}
