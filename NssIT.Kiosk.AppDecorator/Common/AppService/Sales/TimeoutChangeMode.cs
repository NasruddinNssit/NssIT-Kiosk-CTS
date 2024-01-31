using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    public enum TimeoutChangeMode
    {
        /// <summary>
        /// Set timeout to back to normal delay again
        /// </summary>
        ResetNormalTimeout = 0,

        ///// <summary>
        ///// Add extra time delay to existing available delay.
        ///// </summary>
        //ExtendTimeout = 1,

        /// <summary>
        /// Used for Extend Customer Info Entry
        /// </summary>
        MandatoryExtension = 2,

        /// <summary>
        /// Used to reset/remove  MandatoryExtension if Customer exit "Passenger Info Entry" page
        /// </summary>
        RemoveMandatoryTimeout = 3
    }
}