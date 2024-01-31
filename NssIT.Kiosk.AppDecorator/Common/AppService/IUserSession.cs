using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
    public interface IUserSession
    {
        UserSession Session { get; }
        void UpdateSession(UserSession session);
    }
}
