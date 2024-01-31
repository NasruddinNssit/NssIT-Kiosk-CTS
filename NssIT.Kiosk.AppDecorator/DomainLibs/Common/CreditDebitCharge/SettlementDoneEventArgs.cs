using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge
{
    public class SettlementDoneEventArgs : EventArgs
    {
        public bool RebootMachineRequest { get; private set; } = false;
        public bool StopSellingTicketRequest { get; private set; } = false;

        public SettlementDoneEventArgs(bool rebootMachineRequest = false, bool stopSellingTicketRequest = false)
        {
            RebootMachineRequest = rebootMachineRequest;
            StopSellingTicketRequest = stopSellingTicketRequest;
        }
    }
}