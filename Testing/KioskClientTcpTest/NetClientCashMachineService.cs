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
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;

namespace KioskClientTcpTest
{
	public class NetClientCashMachineService : IDisposable 
	{
		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		private UICashMachineStatusSummary _cashMachineStatusSummary = null;

		private string _currProcessId = null;

		public NetClientCashMachineService(INetMediaInterface netInterface)
		{
			_netInterface = netInterface;

			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			if (_netInterface != null)
				_netInterface.OnDataReceived += _netInterface_OnDataReceived;
		}

		private int GetServerPort() => 7385;

		public bool CheckCashMachineIsReady(string processId, out bool isLowCoin, out string errorMsg, int waitDelaySec = 60)
		{
			isLowCoin = false;
			processId = string.IsNullOrWhiteSpace(processId) ? "-" : processId.Trim();
			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - get CheckCashMachineIsReady", "A01", "NetClientCashMachineService.CheckCashMachineIsReady");
			errorMsg = null;
			_cashMachineStatusSummary = null;

			UIRequestCashMachingStatusSummary res = new UIRequestCashMachingStatusSummary(processId, DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res)  { DestinationPort = GetServerPort() };
			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_cashMachineStatusSummary == null)
					Task.Delay(300).Wait();
				else
					break;
			}

			if (_cashMachineStatusSummary == null)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Cash Machine", "A20",
					"NetClientCashMachineService.CheckCashMachineIsReady");
				throw new Exception("Unable to read from Cash Machine ");
			}

			if (_cashMachineStatusSummary.IsCashMachineReady == false)
				errorMsg = _cashMachineStatusSummary.ErrorMessage;

			_log.LogText(_logChannel, "-", $@"End - IsCashMachineReady: {_cashMachineStatusSummary.IsCashMachineReady}", "A100", 
				"NetClientCashMachineService.CheckCashMachineIsReady");

			isLowCoin = _cashMachineStatusSummary.IsLowCoin;
			return _cashMachineStatusSummary.IsCashMachineReady;
		}

		public bool StartPayment(string processId, decimal amount, string docNo)
		{
			bool isSuccess = false;
			try
			{
				UIMakeNewPayment newPay = new UIMakeNewPayment(processId, DateTime.Now)
				{
					Price = amount,
					DocNo = docNo
				};
				NetMessagePack msgPack = new NetMessagePack(newPay) { DestinationPort = GetServerPort() };

				_netInterface.SendMsgPack(msgPack);
				_currProcessId = processId;
				isSuccess = true;
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, processId, ex, "EX01-StartPayment", "NetClientCashMachineService.BtnCancelSales_Click");
				throw ex;
			}

			return isSuccess;
		}

		private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.ReceivedData?.Module == AppModule.UICashMachine)
			{
				switch (e.ReceivedData.Instruction)
				{
					case CommInstruction.UICashMachineStatusSummary:
						_cashMachineStatusSummary = (UICashMachineStatusSummary)e.ReceivedData.MsgObject;
						break;
				}
			}
		}

		public void Dispose()
		{
			try
			{
				if (_netInterface != null)
					_netInterface.OnDataReceived -= _netInterface_OnDataReceived;

				_netInterface = null;
			} catch { }
		}
	}
}
