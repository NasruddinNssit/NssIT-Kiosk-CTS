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
using NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Machine.UI;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Instruction;

namespace CashMachineTCPTest
{
	public class NetB2BService : IDisposable
	{
		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		public NetB2BService(INetMediaInterface netInterface)
		{
			_netInterface = netInterface;

			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			if (_netInterface != null)
				_netInterface.OnDataReceived += _netInterface_OnDataReceived;
		}

		private int GetServerPort() => 7385;

		private UIB2BAllCassetteInfo _b2bAllCassetteInfo = null;
		public async Task<B2BCassetteInfoCollection> GetAllCassetteInfo(string processId, int waitDelaySec = 20)
		{
			processId = string.IsNullOrWhiteSpace(processId) ? "-" : processId.Trim();
			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - GetAllCassetteInfo", "A01", "NetClientCashMachineService.GetAllCassetteInfo");
			_b2bAllCassetteInfo = null;

			UIB2BAllCassetteInfoRequest res = new UIB2BAllCassetteInfoRequest(processId, DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_b2bAllCassetteInfo == null)
					await Task.Delay(300);
				else
					break;
			}

			if (_b2bAllCassetteInfo == null)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Cash Machine", "A20",
					"NetClientCashMachineService.CheckCashMachineIsReady");
				throw new Exception("Unable to read from Cash Machine ");
			}

			_log.LogText(_logChannel, "-", $@"End - GetAllCassetteInfo", "A100",
				"NetClientCashMachineService.CheckCashMachineIsReady");

			return _b2bAllCassetteInfo.CassetteInfoCollection ;
		}

		public UIB2BAllCassetteInfo AllCassetteInfo { get => _b2bAllCassetteInfo; }

		private UICashMachineStatusSummary _cashMachineStatusSummary = null;
		public bool CheckCashMachineIsReady(string processId, out string errorMsg, int waitDelaySec = 20)
		{
			errorMsg = null;

			processId = string.IsNullOrWhiteSpace(processId) ? "-" : processId.Trim();
			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - get CheckCashMachineIsReady", "A01", "NetClientCashMachineService.CheckCashMachineIsReady");
			
			_cashMachineStatusSummary = null;

			UIRequestCashMachingStatusSummary res = new UIRequestCashMachingStatusSummary(processId, DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
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
				errorMsg = _cashMachineStatusSummary.ErrorMessage; ;

			_log.LogText(_logChannel, "-", $@"End - IsCashMachineReady: {_cashMachineStatusSummary.IsCashMachineReady}", "A100",
				"NetClientCashMachineService.CheckCashMachineIsReady");

			return _cashMachineStatusSummary.IsCashMachineReady;
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
			else if (e.ReceivedData?.Module == AppModule.UIB2B)
			{
				switch (e.ReceivedData.Instruction)
				{
					case (CommInstruction)UIB2BInstruction.AllCassetteInfo:
						_b2bAllCassetteInfo = (UIB2BAllCassetteInfo)e.ReceivedData.MsgObject;
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
			}
			catch { }
		}
	}
}
