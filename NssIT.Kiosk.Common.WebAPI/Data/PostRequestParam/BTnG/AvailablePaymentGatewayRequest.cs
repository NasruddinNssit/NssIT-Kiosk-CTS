using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data.PostRequestParam.BTnG
{
    public class AvailablePaymentGatewayRequest : IPostRequestParam
    {
        //@"{""merchantId"": ""KTMB""}";
        public string MerchantId { get; set; }
        public string DeviceId { get; set; }
        public string StationCode { get; set; }
    }
}
