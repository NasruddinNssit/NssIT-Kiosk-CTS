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
using System.Collections.Concurrent;
using System.Threading;

namespace KioskClientTcpTest
{
    public class NetClientSalesService
    {
		public const int WebServerTimeout = 9999999;
		public const int InvalidAuthentication = 9999998;

		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		private UIServerApplicationStatusAck _serverApplicationStatus = null;
		private UIWebServerLogonStatusAck _webServerLogonStatus = null;

		private ReceivedNetProcessIdTracker _recvedNetProcIdTracker = new ReceivedNetProcessIdTracker();

		private string _currProcessId = null;

		public event EventHandler<DataReceivedEventArgs> OnDataReceived;

		public NetClientSalesService(INetMediaInterface netInterface)
		{
			_netInterface = netInterface;

			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			if (_netInterface != null)
				_netInterface.OnDataReceived += _netInterface_OnDataReceived;
		}

		private int GetServerPort() => 7385;

		public void QuerySalesServerStatus(out bool isServerResponded, out bool serverAppHasDisposed, out bool serverAppHasShutdown, out bool serverWebServiceIsDetected, int waitDelaySec = 60)
		{
			serverAppHasDisposed = false;
			serverAppHasShutdown = false;
			serverWebServiceIsDetected = false;
			isServerResponded = false;

			_serverApplicationStatus = null;
			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - get CheckSalesServerIsReady", "A01", "NetClientSalesService.QuerySalesServerStatus");

			UIServerApplicationStatusRequest res = new UIServerApplicationStatusRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.QuerySalesServerStatus", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if ((_serverApplicationStatus == null) && (_recvedNetProcIdTracker.CheckNetProcessIdHasReceived(lastNetProcessId) == false))
					Task.Delay(300).Wait();
				else
					break;
			}

			bool alreadyExpired = false;

			if (_serverApplicationStatus == null)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
					"NetClientSalesService.QuerySalesServerStatus");
			}
			else if (_serverApplicationStatus == null)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT(9000001); Adnormal result !!;", "A21",
					"NetClientSalesService.QuerySalesServerStatus");
			}
			else
			{
				isServerResponded = true;
				serverAppHasDisposed = _serverApplicationStatus.ServerAppHasDisposed;
				serverAppHasShutdown = _serverApplicationStatus.ServerAppHasShutdown;
				serverWebServiceIsDetected = _serverApplicationStatus.ServerWebServiceIsDetected;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded}; ServerAppHasDisposed: {serverAppHasDisposed}; ServerAppHasShutdown: {serverAppHasShutdown}; ServerWebServiceIsDetected: {serverWebServiceIsDetected}",
				"A100",
				"NetClientSalesService.QuerySalesServerStatus");

		}

		public void WebServerLogon(out bool isServerResponded, out bool isLogonSuccess, out bool isNetworkTimeout, out bool isValidAuthentication, out bool isLogonErrorFound, out string errorMessage, int waitDelaySec = 120)
		{
			isServerResponded = false;
			isLogonSuccess = false;
			isNetworkTimeout = true;
			isValidAuthentication = false;
			isLogonErrorFound = true;
			errorMessage = null;

			_webServerLogonStatus = null;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 60 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - get WebServerLogon", "A01", "NetClientSalesService.WebServerLogon");

			UIWebServerLogonRequest res = new UIWebServerLogonRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.WebServerLogon", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_webServerLogonStatus == null)
					Task.Delay(300).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
					"NetClientSalesService.WebServerLogon");

				isLogonSuccess = false;
				isNetworkTimeout = false;
				isValidAuthentication = false;
				isLogonErrorFound = true;
				errorMessage = $@"Unable to read from Local Server; (EXIT9000002)";
			}
			else if (_webServerLogonStatus == null)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000003); Adnormal result !!;", "A21",
					"NetClientSalesService.WebServerLogon");

				isLogonSuccess = false;
				isNetworkTimeout = false;
				isValidAuthentication = false;
				isLogonErrorFound = true;
				errorMessage = $@"Unable to read from Local Server; (EXIT9000003)";
			}
			else
			{
				if (_webServerLogonStatus.LogonSuccess)
				{
					isLogonSuccess = true;
					isNetworkTimeout = _webServerLogonStatus.NetworkTimeout;
					isValidAuthentication = _webServerLogonStatus.IsValidAuthentication;
					isLogonErrorFound = _webServerLogonStatus.LogonErrorFound;
				}
				else
				{
					isLogonSuccess = false;
					isNetworkTimeout = _webServerLogonStatus.NetworkTimeout;
					isValidAuthentication = _webServerLogonStatus.IsValidAuthentication;
					isLogonErrorFound = _webServerLogonStatus.LogonErrorFound;
					errorMessage = _webServerLogonStatus.ErrorMessage;
				}
			}

			_log.LogText(_logChannel, "-",
				$@"End - isLogonSuccess: {isLogonSuccess}; isNetworkTimeout: {isNetworkTimeout}; isValidAuthentication: {isValidAuthentication}; isLogonErrorFound: {isLogonErrorFound}; errorMessage: {errorMessage}",
				"A100",
				"NetClientSalesService.WebServerLogon");

		}

		public void StartSales(out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.StartSales");

			UIStartNewSalesRequest res = new UIStartNewSalesRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.StartSales", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);


			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckNetProcessIdHasReceived(lastNetProcessId) == false)
					Task.Delay(300).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
					"NetClientSalesService.StartSales", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000001); Adnormal result !!;", "A21",
					"NetClientSalesService.StartSales", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.StartSales");
		}

		private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.ReceivedData?.Module == AppModule.UIKioskSales)
			{
				if (e.ReceivedData.MsgObject is UIServerApplicationStatusAck appStt)
				{
					_serverApplicationStatus = appStt;
				}
				else if (e.ReceivedData.MsgObject is UIWebServerLogonStatusAck logStt)
				{
					_webServerLogonStatus = logStt;
				}
				else
				{
					if (e.ReceivedData.MsgObject is UIDestinationListAck)
					{
						_recvedNetProcIdTracker.AddNetProcessId(e.ReceivedData.NetProcessId);
						RaiseOnDataReceived(sender, e);
					}
				}
			}
			else if (e.ReceivedData?.Module == AppModule.Unknown)
			{
				string errMsg = $@"Error : {e.ReceivedData.ErrorMessage}; NetProcessId: {e.ReceivedData.NetProcessId}";
				_log.LogText(_logChannel, "-", errMsg, "A02", "NetClientSalesService._netInterface_OnDataReceived", NssIT.Kiosk.AppDecorator.Log.MessageType.Error, netProcessId: e.ReceivedData.NetProcessId) ;
			}
		}

		private void RaiseOnDataReceived(object sender, DataReceivedEventArgs e)
		{
			try
			{
				if (OnDataReceived != null)
				{
					OnDataReceived.Invoke(sender, e);
				}
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, "-", new Exception("Unhandled event exception; (EXIT9000010)", ex), "EX01", "NetClientSalesService.RaiseOnDataReceived", netProcessId: e?.ReceivedData?.NetProcessId);
			}
		}

		public class ReceivedNetProcessIdTracker
		{
			private (int ClearNetProcessIdListIntervalSec, DateTime NextClearListTime, ConcurrentDictionary<Guid, DateTime> NetProcessIdList) _receivedNetProcess
				= (ClearNetProcessIdListIntervalSec: 60, NextClearListTime: DateTime.Now, NetProcessIdList: new ConcurrentDictionary<Guid, DateTime>());

			public ReceivedNetProcessIdTracker() { }

			public void AddNetProcessId(Guid netProcessId)
			{
				lock(_receivedNetProcess.NetProcessIdList)
				{
					// Clear NetProcessIdList -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
					if (_receivedNetProcess.NextClearListTime.Ticks < DateTime.Now.Ticks)
					{
						Guid[] clearNetProcessIdArr = (from keyPair in _receivedNetProcess.NetProcessIdList
														where (keyPair.Value.Ticks <= _receivedNetProcess.NextClearListTime.Ticks)
														select keyPair.Key).ToArray();

						foreach (Guid netProcId in clearNetProcessIdArr)
							_receivedNetProcess.NetProcessIdList.TryRemove(netProcId, out DateTime receivedTime);
						_receivedNetProcess.NextClearListTime = DateTime.Now.AddSeconds(_receivedNetProcess.ClearNetProcessIdListIntervalSec);
					}
					// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

					if (_receivedNetProcess.NetProcessIdList.TryGetValue(netProcessId, out DateTime receivedTime2))
						_receivedNetProcess.NetProcessIdList.TryRemove(netProcessId, out DateTime receivedTime3);

					try
					{
						_receivedNetProcess.NetProcessIdList.TryAdd(netProcessId, DateTime.Now);
					}
					catch { }
				}
			}

			/// <summary>
			/// Return true if NetProcessId has responded.
			/// </summary>
			/// <param name="netProcessId"></param>
			/// <returns></returns>
			public bool CheckNetProcessIdHasReceived(Guid netProcessId)
			{
				bool retFound = false;

				Thread execThread = new Thread(new ThreadStart(new Action(() =>
				{
					lock (_receivedNetProcess.NetProcessIdList)
					{
						if (_receivedNetProcess.NetProcessIdList.TryGetValue(netProcessId, out DateTime receivedTime2))
							retFound = true;
					}
				})));
				execThread.IsBackground = true;
				execThread.Start();
				execThread.Join();

				return retFound;
			}
		}
	}
}
