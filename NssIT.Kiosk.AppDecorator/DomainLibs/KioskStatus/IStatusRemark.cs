using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus
{
    public interface IStatusRemark
    {
        string TypeDesc { get; set; }

        /// <summary>
        /// To duplicated the StatusRemark instance
        /// </summary>
        /// <returns></returns>
        IStatusRemark Duplicate();
    }
}