using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp.CustomApp
{
    public interface IServerAppPlan
    {
        /// <summary>
        /// This method is obsolete (2021-11-23)
        /// </summary>
        /// <param name="procId"></param>
        /// <param name="netProcId"></param>
        /// <param name="svcMsg"></param>
        /// <param name="session"></param>
        /// <param name="releaseSeatRequest"></param>
        /// <returns></returns>
        UISalesInst NextInstruction(string procId, Guid? netProcId, IKioskMsg svcMsg, UserSession session, out bool releaseSeatRequest);
        IKioskMsg DoProcess(string procId, Guid? netProcId, IKioskMsg svcMsg, UserSession session, AppCallBackDelg appCallBackDelHandle, out bool releaseSeatRequest);
        UserSession UpdateUserSession(UserSession userSession, IKioskMsg svcMsg);
        UserSession SetEditingSession(UserSession userSession, TickSalesMenuItemCode detailItemCode);
        UserSession SetUIPageNavigateSession(UserSession userSession, UIPageNavigateRequest pageNav);
        void ResetCleanUp();
    }
}