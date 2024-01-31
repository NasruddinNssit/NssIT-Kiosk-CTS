using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus
{
    /// <summary>
    /// Extended Status infor only for local process; This info will not sent to WebAPI
    /// </summary>
    public interface IStatusLocalExtension
    {
        IStatusLocalExtension Duplicate();
    }
}
