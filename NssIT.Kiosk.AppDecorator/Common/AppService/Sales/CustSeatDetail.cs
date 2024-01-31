using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class CustSeatDetail
    {
        /// <summary>
        /// Refer to Seat No
        /// </summary>
        public string Desn { get; set; }
        public long SeatId { get; set; }
        public string CustName { get; set; }
        public string CustIC { get; set; }
        public string Contact { get; set; }
        public string SeatType { get; set; }
    }
}
