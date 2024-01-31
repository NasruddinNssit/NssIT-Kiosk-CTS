using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.PaymentGatewayApp.CustomApp.KTMBApp
{
    /// <summary>
    /// ClassCode:EXIT60.02
    /// </summary>
    public class KTMBApp_AccessCallBackPlan : IAppAccessCallBackPlan
    {
        private IUIApplicationJob _uiApp = null;
        private AppModule _appModule = AppModule.UIBTnG;

        public KTMBApp_AccessCallBackPlan(IUIApplicationJob uiApp)
        {
            _uiApp = uiApp;
        }

        /// <summary>
        /// FuncCode:EXIT60.0205
        /// </summary>
        public async Task DeliverAccessResult(UIxKioskDataAckBase accessResult)
        {
            if (accessResult is UIxGnBTnGAck<BTnGGetPaymentGatewayResult> uix1)
            {
                await _uiApp.SendInternalCommand(accessResult.BaseProcessId, accessResult.BaseRefNetProcessId, new UIAck<UIxGnBTnGAck<BTnGGetPaymentGatewayResult>>(uix1.BaseRefNetProcessId, uix1.BaseProcessId, _appModule, DateTime.Now, uix1));
            }
            else
            {
                throw new Exception($@"Unrecognized Kiosk Data Type to deliver access result; (EXIT60.0205.X01)");
            }
        }
    }
}