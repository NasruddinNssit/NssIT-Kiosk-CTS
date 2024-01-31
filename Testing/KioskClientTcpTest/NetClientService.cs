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

namespace KioskClientTcpTest
{
	public class NetClientService : IDisposable 
	{
		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		public NetClientCashMachineService CashMachineService { get; private set; } = null;
		public NetClientSalesService SalesService { get; private set; } = null;

		public NetClientService()
		{
			_netInterface = new LocalTcpService(GetLocalServicePort());

			CashMachineService = new NetClientCashMachineService(_netInterface);
			SalesService = new NetClientSalesService(_netInterface);

			int GetLocalServicePort() => 9838;
		}

		public void Dispose()
		{
			if (CashMachineService != null)
				CashMachineService.Dispose();

			CashMachineService = null;

			_netInterface.ShutdownService();
			_netInterface.Dispose();
			_netInterface = null;
		}

		public void StartCashPayment(string processId, decimal amount, string docNo)
		{
			try
			{
				wfmCashPayment payUi = new wfmCashPayment(_netInterface, CashMachineService, processId, amount, docNo);
				payUi.ShowDialog();
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, processId, ex, "EX01-StartPayment", "wfmCashPayment.BtnCancelSales_Click");
				throw ex;
			}

			//	int GetServerPort() => 7385;
		}
	}
}
