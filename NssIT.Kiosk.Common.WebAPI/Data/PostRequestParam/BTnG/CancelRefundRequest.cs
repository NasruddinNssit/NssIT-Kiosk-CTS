using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data.PostRequestParam.BTnG
{
    [Serializable]
    public class CancelRefundRequest : IPostRequestParam
    {
        public string DeviceId { get; set; }
        public string StationCode { get; set; } = "*#*";
        public BTnGCancelRefundReqInfo CancelRefundRequestInfo { get; set; }
    }
}