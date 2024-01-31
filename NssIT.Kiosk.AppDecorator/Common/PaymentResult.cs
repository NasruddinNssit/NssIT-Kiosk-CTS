using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common
{
    public enum PaymentResult
    {
        Unknown = 0,
        Success = 1,
        Cancel = 2,
        Timeout = 3,
        Fail = 4
    }
}
