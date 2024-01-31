using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.CollectTicket.UIx
{
    [Serializable]
    public class UIxCTTicketNoNotFoundAck : UIxKioskDataAckBase
    {
        public string LastSubmittedTicketNo { get; private set; }
        public int ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public UIxCTTicketNoNotFoundAck(Guid? refNetProcessId, string processId, string lastSubmittedTicketNo, int errorCode, string errorMessage)
            : base()
        {
            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            LastSubmittedTicketNo = lastSubmittedTicketNo;

            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}
