using NssIT.Kiosk.Common.WebAPI.Common.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG
{
    [Serializable]
    public class BTnGCleanUpSalesResult : iWebApiResult, IDisposable
    {
        public bool Status { get; set; } = false;
        public BTnGCleanUpSalesResultCollectionModel[] Data { get; set; } = null;
        public IList<string> Messages { get; set; }
        public string Code { get; set; } = "99";

        public BTnGCleanUpSalesResult()
        {
            Messages = new List<string>();
            Code = "99";
            Status = false;
            Data = null;
        }

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
