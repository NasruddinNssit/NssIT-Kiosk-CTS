using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge
{
    public class InProgressEventArgs : EventArgs
    {
        public string Message { get; set; } = "Work in progress";
        /////public int? NewTimeRequestSec { get; set; } = null;
    }
}
