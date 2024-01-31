using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data.Response
{
    public class BaseResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
    }
}
