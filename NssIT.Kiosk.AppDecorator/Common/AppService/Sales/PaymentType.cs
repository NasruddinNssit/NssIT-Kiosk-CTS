using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    public enum PaymentType
    {
        Unknown = 0,

        /// <summary>
        /// PaymentMethod 'C'
        /// </summary>
        Cash = 1,

        /// <summary>
        /// PaymentMethod 'D'
        /// </summary>
        CreditCard = 2,

        /// <summary>
        /// PaymentMethod Boost:'B' ; TnG:'T'
        /// </summary>
        PaymentGateway = 3
    }
}
