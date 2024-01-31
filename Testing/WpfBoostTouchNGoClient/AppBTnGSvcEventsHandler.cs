using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfBoostTouchNGoClient.NetClient;

namespace WpfBoostTouchNGoClient
{
    public class AppBTnGSvcEventsHandler
    {
        private string _logChannel = "AppSys";

        private NetClientService _netClientSvc = null;

        public AppBTnGSvcEventsHandler(NetClientService netClientSvc)
        {
            _netClientSvc = netClientSvc;

            _netClientSvc.BTnGService.OnDataReceived += BTnGService_OnDataReceived;
        }

        private void BTnGService_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.ReceivedData?.MsgObject?.GetMsgData() is IUIxBTnGPaymentOngoingGroupAck)
            {
                App.AppWindow?.BTnGShowPaymentInfo(e.ReceivedData.MsgObject);
            }
        }
    }
}
