using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge
{
    public class TrxCallBackEventArgs : EventArgs
    {
        public string ProcessId { get; set; } = null;
        public bool IsSuccess { get; set; } = false;
        public Exception Error { get; set; } = null;
        public ResponseInfo Result { get; set; } = null;

        public TrxCallBackEventArgs Duplicate()
        {
            TrxCallBackEventArgs ev = new TrxCallBackEventArgs() { Error = this.Error, IsSuccess = this.IsSuccess, Result = this.Result };
            return ev;
        }
    }
}
