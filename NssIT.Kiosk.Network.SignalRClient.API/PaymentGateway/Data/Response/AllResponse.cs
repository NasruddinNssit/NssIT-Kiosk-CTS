using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data.Response
{
    /// <summary>
    /// Create/New Sales Response
    /// </summary>
    public class CreateSaleResponse : BaseResponse
    {
        public MerchantSaleInfo Data { get; set; }
    }
}
