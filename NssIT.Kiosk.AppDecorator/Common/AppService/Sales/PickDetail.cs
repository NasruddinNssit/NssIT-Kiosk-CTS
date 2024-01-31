using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class PickDetail
    {
        public string Pick { get; set; }
        public string PickDesn { get; set; }
        public string PickTime { get; set; }
    }
}
