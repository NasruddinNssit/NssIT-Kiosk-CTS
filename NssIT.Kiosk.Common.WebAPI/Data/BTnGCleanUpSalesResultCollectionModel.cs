using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data
{
    [Serializable]
    public class BTnGCleanUpSalesResultCollectionModel
    {
        public int OutStandingSalesCount { get; set; } = 0;
        public int ErrorCount { get; set; } = 0;
        public string FirstErrorMessage { get; set; } = null;
        public BTnGCleanUpSalesResultModel[] CleanUpSalesResultList { get; set; }
    }
}
