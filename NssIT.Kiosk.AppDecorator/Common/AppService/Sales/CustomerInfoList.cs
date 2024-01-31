using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class CustomerInfoList : IDisposable 
    {
        /// <summary>
        /// Refer to Seat No
        /// </summary>
        public CustSeatDetail[] CustSeatInfoList { get; set; } = null;
        //public decimal Insurance { get; private set; } = 0M;

        public CustomerInfoList(CustSeatDetail[] custSeatInfoList)
        {
            CustSeatInfoList = custSeatInfoList;
            //Insurance = insurance;
        }

        public void Dispose()
        {
            CustSeatInfoList = null;
        }
    }
}


