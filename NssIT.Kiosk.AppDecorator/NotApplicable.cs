using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator
{
    public class NotApplicable
    {
        public static NotApplicable _obj = new NotApplicable();
        public static NotApplicable Object => _obj;
    }
}
