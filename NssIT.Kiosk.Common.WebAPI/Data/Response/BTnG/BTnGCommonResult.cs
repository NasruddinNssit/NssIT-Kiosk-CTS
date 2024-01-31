using NssIT.Kiosk.Common.WebAPI.Common.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG
{
    [Serializable]
    public class BTnGCommonResult : iWebApiResult, IDisposable
    {
        public bool Status { get; set; }
        public IList<string> Messages { get; set; }

        public string Code { get; set; }

        public dynamic Data { get; set; }

        public void Dispose()
        {
            Messages = null;
            Data = null;
        }

        public string MessageString()
        {
            string msgStr = null;
            foreach (string msg in Messages)
                msgStr += $@"{msg}; ";
            return msgStr;
        }
    }
}
