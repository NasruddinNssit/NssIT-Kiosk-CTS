using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UIx;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using NssIT.Kiosk.Server.AccessDB.AxCommand;
using NssIT.Kiosk.Server.AccessDB.AxCommand.BTnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.PaymentGatewayApp.CustomApp.KTMBApp
{
    public class KTMBAppPlan : IServerAppPlan
    {
        private AppCallBackDelg _callBackEventHandle = null;
        public KTMBAppPlan(AppCallBackDelg callBackEventHandle)
        {
            _callBackEventHandle = callBackEventHandle;
        }

        public void PreProcess(IKioskMsg refSvcMsg)
        {

        }

        public dynamic GetInstruction(IKioskMsg refSvcMsg)
        {
            if (refSvcMsg is UIReq<UIxBTnGGetAvailablePaymentGatewayRequest> uiReq1)
            {
                return new AxGetAvailablePaymentGateway("*", uiReq1.BaseNetProcessId, _callBackEventHandle);
            }
            else if (refSvcMsg is UIReq<UIxBTnGPaymentMakeNewPaymentRequest> uiReq2)
            {
                return AppBTnGPayingCode.RequestPayment;
            }
            else if (refSvcMsg is UIAck<UIxGnBTnGAck<BTnGGetPaymentGatewayResult>>)
            {
                return AppInstructionCode.SendPaymentGatewayListResult;
            }
            else if (refSvcMsg is UIReq<UIxSampleDateRequest>)
            {
                return AppInstructionCode.DoSampleJob;
            }

            return AppInstructionCode.Unknown;
        }
    }
}
