using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.NetClient
{
	public class NetClientService : IDisposable
	{
		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		public NetClientCashPaymentService CashPaymentService { get; private set; } = null;
		public NetClientBTnGService BTnGService { get; private set; } = null;
		public NetClientSalesService SalesService { get; private set; } = null;

		public NetClientSalesServiceV2 SalesServiceV2 { get; private set; } = null;
		public NetClientCollectTicketService CollectTicketService { get; private set; } = null;

		public NetClientService()
		{
			_netInterface = new LocalTcpService(App.SysParam.PrmClientPort);

			CashPaymentService = new NetClientCashPaymentService(_netInterface);
			BTnGService = new NetClientBTnGService(_netInterface);
			SalesService = new NetClientSalesService(_netInterface);
            SalesServiceV2 = new NetClientSalesServiceV2(_netInterface);

            CollectTicketService = new NetClientCollectTicketService(_netInterface);
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
					CashPaymentService?.Dispose();
				}
				catch { }

				try
				{
					BTnGService?.Dispose();
				}
				catch { }

				try
				{
					CollectTicketService?.Dispose();
				}
				catch { }

				CashPaymentService = null;
				BTnGService = null;
				CollectTicketService = null;

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
