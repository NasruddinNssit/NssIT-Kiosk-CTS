using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    public interface IBTnG
    {
        event EventHandler<EndOfPaymentEventArgs> OnEndPayment;

        void InitPaymentData(string currency, decimal amount, string refNo, string paymentGateway, string firstName, string lastName, string contactNo,
            string paymentGatewayLogoUrl, string financePaymentMethod,
            ResourceDictionary languageResource);

        void BTnGShowPaymentInfo(IKioskMsg kioskMsg);
        void ClearEvents();
    }
}
