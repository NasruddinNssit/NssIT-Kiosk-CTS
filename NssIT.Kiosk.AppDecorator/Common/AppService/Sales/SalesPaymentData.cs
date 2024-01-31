using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class SalesPaymentData
    {
        public string TransactionNo { set; get; } = null;
        public decimal Amount { set; get; } = 0M;
    }
}
