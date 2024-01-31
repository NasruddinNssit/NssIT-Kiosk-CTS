using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBoostTouchNGoClient.NetClient
{
	public class NetClientService : IDisposable
	{
		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		//public NetClientCashPaymentService CashPaymentService { get; private set; } = null;
		//public NetClientSalesService SalesService { get; private set; } = null;
		public NetClientBTnGService BTnGService { get; private set; } = null;

		public NetClientService()
		{
			_netInterface = new LocalTcpService(App.SysParam.PrmClientPort);

			//CashPaymentService = new NetClientCashPaymentService(_netInterface);
			//SalesService = new NetClientSalesService(_netInterface);

			BTnGService = new NetClientBTnGService(_netInterface);
		}

		public INetMediaInterface NetInterface { get => _netInterface; }

		private bool _disposed = false;
		public void Dispose()
		{
			if (_disposed == false)
			{
				_disposed = true;

				try
				{
					if (BTnGService != null)
						BTnGService?.Dispose();
					BTnGService = null;
				}
				catch { }

				if (_netInterface != null)
				{
					try
					{
						_netInterface.ShutdownService();
					}
					catch { }
					try
					{
						_netInterface.Dispose();
					}
					catch { }
					_netInterface = null;
				}
			}


		}
	}
}