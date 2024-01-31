using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp.CustomApp.CollectTicketApp
{
    /// <summary>
    /// ClassCode:EXIT70.06
    /// </summary>
    public class MelTicketCollectionApp_AccessCallBackPlan : IAppAccessCallBackPlan
    {
        private IUIApplicationJob _uiApp = null;
        private AppModule _appModule = AppModule.UICollectTicket;

        /// <summary>
        /// FuncCode:EXIT70.0601
        /// </summary>
        public MelTicketCollectionApp_AccessCallBackPlan(IUIApplicationJob uiApp)
        {
            _uiApp = uiApp;
        }

        /// <summary>
        /// FuncCode:EXIT70.0603
        /// </summary>
        public async Task DeliverAccessResult(UIxKioskDataAckBase accessResult)
        {
            if (accessResult is UIxGnBTnGAck<boardingcompany_status> uixAck1)
            {
                await _uiApp.SendInternalCommand(accessResult.BaseProcessId, accessResult.BaseRefNetProcessId, new UIAck<UIxGnBTnGAck<boardingcompany_status>>(uixAck1.BaseRefNetProcessId, uixAck1.BaseProcessId, _appModule, DateTime.Now, uixAck1));
            }
            else if (accessResult is UIxGnBTnGAck<boardingqueryticket_status> uixAck2)
            {
                await _uiApp.SendInternalCommand(accessResult.BaseProcessId, accessResult.BaseRefNetProcessId, new UIAck<UIxGnBTnGAck<boardingqueryticket_status>>(uixAck2.BaseRefNetProcessId, uixAck2.BaseProcessId, _appModule, DateTime.Now, uixAck2));
            }
            else if (accessResult is UIxGnBTnGAck<boardingcollectticket_status> uixAck3)
            {
                await _uiApp.SendInternalCommand(accessResult.BaseProcessId, accessResult.BaseRefNetProcessId, new UIAck<UIxGnBTnGAck<boardingcollectticket_status>>(uixAck3.BaseRefNetProcessId, uixAck3.BaseProcessId, _appModule, DateTime.Now, uixAck3));
            }
            else
            {
                throw new Exception($@"Unrecognized Kiosk Data Type to deliver; (EXIT70.0603.X01)");
            }
        }
    }
}
