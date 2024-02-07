using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.UI
{
    public class OnCloseOperationTimeEventArgs
    {
        public DateTime StartOperationTime { get; set; }

        public DateTime EndOperationTime { get; set;}
    }
}
