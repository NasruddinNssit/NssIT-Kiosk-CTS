
using NssIT.Kiosk.Common.Data;
using NssIT.Kiosk.Common.WebAPI.Common.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.Response
{
    [Serializable]
    public class OutstandingCardSettlementResult : iWebApiResult, IDisposable
    {
        public bool Status { get; set; }
        public OutstandingCardSettlementModel Data { get; set; } = new OutstandingCardSettlementModel();
        public IList<string> Messages { get; set; } = new   List<string>(); 
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

