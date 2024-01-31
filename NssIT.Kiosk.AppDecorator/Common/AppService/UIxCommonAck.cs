using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
    public class UIxCommonAck : UIxKioskDataAckBase
    {
        public bool IsSuccess { get; private set; }
        public Exception Error { get; private set; }

        public UIxCommonAck(Guid? refNetProcessId, string processId,
            bool isSuccess, Exception error = null)
            : base()
        {
            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            //-------------------------------------
            IsSuccess = isSuccess;

            if (IsSuccess == false)
            {
                Error = error ?? new Exception("Unknown error; (-#x#-)");
            }
        }
    }
}
