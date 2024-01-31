using NssIT.Kiosk.AppDecorator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    public interface ICash
    {
        event EventHandler<EndOfPaymentEventArgs> OnEndPayment;
        void InitSalesPayment(string processId, decimal amount, string docNo, LanguageCode language, string currency);
        void ClearEvents();
    }
}
