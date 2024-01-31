using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common
{
    public enum TravelMode
    {
        DepartOnly = 0,

        /// <summary>
        /// Depart is mandatory, return is optional
        /// </summary>
        DepartOrReturn = 1
    }
}
