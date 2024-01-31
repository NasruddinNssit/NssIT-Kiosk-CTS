using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket
{
    [Serializable]
    public class TicketNoFoundException : Exception
    {
        /// <summary>
        /// 99: Error refer to web server; 6: Ticket not found; 61: 3rd party remoted server not found; 20: Invalid Token; 21: Token Expired
        /// </summary>
        public int ErrorCode { get; private set; }
        public TicketNoFoundException(string errorMsg, int errorCode)
            : base(errorMsg)
        {
            ErrorCode = errorCode;
        }
    }
}
