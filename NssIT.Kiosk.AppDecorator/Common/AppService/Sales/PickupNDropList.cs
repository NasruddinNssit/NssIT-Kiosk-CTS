using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class PickupNDropList
    {
        public PickDetail[] PickDetailList { get; set; }
        public DropDetail[] DropDetailList { get; set; }
    }
}
