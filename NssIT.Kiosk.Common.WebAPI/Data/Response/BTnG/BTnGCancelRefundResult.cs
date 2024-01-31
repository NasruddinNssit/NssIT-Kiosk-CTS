using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Common.WebAPI.Common.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG
{
    [Serializable]
    public class BTnGCancelRefundResult : iWebApiResult, IDisposable
    {
        public bool Status { get; set; }
        public BTnGCancelRefundModel Data { get; set; }
        public IList<string> Messages { get; set; }
        public string Code { get; set; }

        public string MessageString()
        {
            string msgStr = null;
            foreach (string msg in Messages)
                msgStr += $@"{msg}; ";
            return msgStr;
        }

        public void Dispose()
        {
            Data = null;
            Messages = null;
        }
    }
}
