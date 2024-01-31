using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;

namespace CashMachineTCPTest
{
	public class NetClientService : IDisposable
	{
		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;

		private INetMediaInterface NetInterface { get; set; } = null;
		public NetB2BService BanknoteSvcClient { get; private set; } = null;

		public NetClientService()
		{
			NetInterface = new LocalTcpService(GetLocalServicePort());
			BanknoteSvcClient = new NetB2BService(NetInterface);

			//int GetLocalServicePort() => 9838;
			int GetLocalServicePort() => 9839;
		}

		public void Dispose()
		{
			if (BanknoteSvcClient != null)
				BanknoteSvcClient.Dispose();

			BanknoteSvcClient = null;

			NetInterface.ShutdownService();
			NetInterface.Dispose();
			NetInterface = null;
		}
	}
}
