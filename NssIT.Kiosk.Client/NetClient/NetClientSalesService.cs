using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.Ticketing.UIx;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NssIT.Kiosk.Client.Base.NetServiceAnswerMan;

namespace NssIT.Kiosk.Client.NetClient
{
	public class NetClientSalesService
	{
		public const int WebServerTimeout = 9999999;
		public const int InvalidAuthentication = 9999998;

		private string _logChannel = "NetClientService";
		private AppModule _appModule = AppModule.UIKioskSales;

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		private UIServerApplicationStatusAck _serverApplicationStatus = null;
		private UIWebServerLogonStatusAck _webServerLogonStatus = null;

		private ReceivedNetProcessIdTracker _recvedNetProcIdTracker = new ReceivedNetProcessIdTracker();

		public event EventHandler<DataReceivedEventArgs> OnDataReceived;

		public NetClientSalesService(INetMediaInterface netInterface)
		{
			_netInterface = netInterface;

			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			if (_netInterface != null)
				_netInterface.OnDataReceived += _netInterface_OnDataReceived;
		}

		private int GetServerPort() => App.SysParam.PrmLocalServerPort;

		public void QuerySalesServerStatus(out bool isServerResponded, out bool serverAppHasDisposed, out bool serverAppHasShutdown, out bool serverWebServiceIsDetected, int waitDelaySec = 60)
		{
			serverAppHasDisposed = false;
			serverAppHasShutdown = false;
			serverWebServiceIsDetected = false;
			isServerResponded = false;

			_serverApplicationStatus = null;
			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - QuerySalesServerStatus", "A01", "NetClientSalesService.QuerySalesServerStatus");

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
				if ((_serverApplicationStatus == null) && (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false))
					Task.Delay(100).Wait();
				else
					break;
			}

			bool alreadyExpired = false;

			if (_serverApplicationStatus == null)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Already expired; (EXIT9000201) ", "A20",
					"NetClientSalesService.QuerySalesServerStatus");
			}
			else if (_serverApplicationStatus == null)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000202); Adnormal result !!;", "A21",
					"NetClientSalesService.QuerySalesServerStatus");
			}
			else
			{
				isServerResponded = true;
				serverAppHasDisposed = _serverApplicationStatus.ServerAppHasDisposed;
				serverAppHasShutdown = _serverApplicationStatus.ServerAppHasShutdown;
				serverWebServiceIsDetected = _serverApplicationStatus.ServerWebServiceIsDetected;
				App.ServerAppGroup = _serverApplicationStatus.ApplicationGroup;
				App.ServerVersion = _serverApplicationStatus.ServerSystemVersion;
				App.AvailablePaymentTypeList = _serverApplicationStatus.AvailablePaymentTypeList;
				App.IsBoardingPassEnabled = _serverApplicationStatus.IsBoardingPassEnabled;
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

			_log.LogText(_logChannel, "-", "Start - WebServerLogon", "A01", "NetClientSalesService.WebServerLogon");

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
					Task.Delay(100).Wait();
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
				errorMessage = $@"Unable to read from Local Server; (EXIT9000001)";
			}
			else if (_webServerLogonStatus == null)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000002); Adnormal result !!;", "A21",
					"NetClientSalesService.WebServerLogon");

				isLogonSuccess = false;
				isNetworkTimeout = false;
				isValidAuthentication = false;
				isLogonErrorFound = true;
				errorMessage = $@"Unable to read from Local Server; (EXIT9000003)";
			}
			else
			{
				isLogonSuccess = _webServerLogonStatus.LogonSuccess;
				isNetworkTimeout = _webServerLogonStatus.NetworkTimeout;
				isValidAuthentication = _webServerLogonStatus.IsValidAuthentication;
				isLogonErrorFound = _webServerLogonStatus.LogonErrorFound;
			}

			_log.LogText(_logChannel, "-",
				$@"End - isLogonSuccess: {isLogonSuccess}; isNetworkTimeout: {isNetworkTimeout}; isValidAuthentication: {isValidAuthentication}; isLogonErrorFound: {isLogonErrorFound}; errorMessage: {errorMessage}",
				"A100",
				"NetClientSalesService.WebServerLogon");

		}

		public void ResetUserSessionSendOnly(string processId)
		{
			try
			{
				var uiReq = new UIReq<UIxResetUserSessionSendOnlyRequest>(processId, _appModule, DateTime.Now,
				new UIxResetUserSessionSendOnlyRequest(processId));

				SendToServerOnly(uiReq, "NetClientSalesService.ResetUserSessionSendOnly");
			}
			catch { }
		}

		public NetServiceAnswerMan StartNewSessionCountDown(string noResponseErrorMessage,
			FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle,
			int waitDelaySec = 60)
		{
			Guid netProcessId = Guid.Empty;
			NetServiceAnswerMan retMan = null;
			string runningTag = "Start-New-Session-Count-Down-(OPR0001)";

			try
			{
				_log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientSalesService.QueryKomuterTicketTypePackage");

				UICountDownStartRequest res = new UICountDownStartRequest("-", DateTime.Now, waitDelaySec);
				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

				retMan = new NetServiceAnswerMan(msgPack, runningTag,
					noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
					_netInterface, _recvedNetProcIdTracker, processId: "*", waitDelaySec: waitDelaySec, threadPriority: ThreadPriority.AboveNormal);

				return retMan;
			}
			catch (Exception ex)
			{
				throw new Exception($@"Error when {runningTag}; (EXIT.OPR0001)", ex);
			}
		}

		public void StartNewSessionCountDownXXXXX(out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - StartNewSessionCountDown", "A01", "NetClientSalesService.StartNewSessionCountDown");

			UICountDownStartRequest res = new UICountDownStartRequest("-", DateTime.Now, 30);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.StartNewSessionCountDown", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);
			
			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
					"NetClientSalesService.StartNewSessionCountDown", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000006); Adnormal result !!;", "A21",
						"NetClientSalesService.StartNewSessionCountDown", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				else
					isServerResponded = true;
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.StartNewSessionCountDown");
		}

		//public void SubmitLanguage(LanguageCode language, out bool isServerResponded, int waitDelaySec = 60)
		//{
		//	isServerResponded = false;

		//	Guid lastNetProcessId;

		//	waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

		//	_log.LogText(_logChannel, "-", "Start - SubmitLanguage", "A01", "NetClientSalesService.StartSales");

		//	UILanguageSubmission res = new UILanguageSubmission("-", DateTime.Now, language);

		//	NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
		//	lastNetProcessId = msgPack.NetProcessId;

		//	_log.LogText(_logChannel, "-",
		//		msgPack, "A05", "NetClientSalesService.SubmitLanguage", extraMsg: "MsgObject: NetMessagePack");

		//	_netInterface.SendMsgPack(msgPack);

		//	DateTime startTime = DateTime.Now;
		//	DateTime endTime = startTime.AddSeconds(waitDelaySec);
			
		//	while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
		//	{
		//		if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
		//			Task.Delay(100).Wait();
		//		else
		//		{
		//			isServerResponded = true;
		//			break;
		//		}
		//	}

		//	bool alreadyExpired = false;

		//	if (isServerResponded == false)
		//		alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

		//	if (alreadyExpired)
		//	{
		//		_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
		//			"NetClientSalesService.SubmitLanguage", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
		//	}
		//	else if (isServerResponded == false)
		//	{
		//		_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000011); Adnormal result !!;", "A21",
		//			"NetClientSalesService.SubmitLanguage", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
		//	}
		//	else
		//	{
		//		isServerResponded = true;
		//	}

		//	_log.LogText(_logChannel, "-",
		//		$@"End - IsLocalServerResponded: {isServerResponded};",
		//		"A100",
		//		"NetClientSalesService.SubmitLanguage");
		//}

		public NetServiceAnswerMan SubmitLanguage(LanguageCode language, string noResponseErrorMessage,
			FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle,
			int waitDelaySec = 60)
		{
			Guid netProcessId = Guid.Empty;
			NetServiceAnswerMan retMan = null;
			string runningTag = "Submit-Language-(OPR0001)";

			try
			{
				_log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientSalesService.QueryKomuterTicketTypePackage");

				UILanguageSubmission res = new UILanguageSubmission("-", DateTime.Now, language);
				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

				retMan = new NetServiceAnswerMan(msgPack, runningTag,
					noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
					_netInterface, _recvedNetProcIdTracker, processId: "*", waitDelaySec: waitDelaySec, threadPriority: ThreadPriority.AboveNormal);

				return retMan;
			}
			catch (Exception ex)
			{
				throw new Exception($@"Error when {runningTag}; (EXIT.OPR0001)", ex);
			}
		}

		public void SubmitDestination(string destinationCode, string destinationName, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitDestination", "A01", "NetClientSalesService.SubmitDestination");

			UIDestinationSubmission res = new UIDestinationSubmission("-", DateTime.Now, destinationCode, destinationName);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitDestination", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Already expired; (EXIT9000016)", "A20",
					"NetClientSalesService.SubmitDestination", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000017); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitDestination", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitDestination");
		}

		public void SubmitOrigin(string originCode, string originName, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitOrigin", "A01", "NetClientSalesService.SubmitOrigin");

			UIOriginSubmission res = new UIOriginSubmission("-", DateTime.Now, originCode, originName);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitOrigin", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Already expired; (EXIT9000004)", "A20",
					"NetClientSalesService.SubmitOrigin", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000005); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitOrigin", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitOrigin");
		}

		public void SubmitTravelDates(DateTime? departDate, DateTime? returnDate, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitTravelDates", "A01", "NetClientSalesService.SubmitTravelDates");

			UITravelDateSubmission res = new UITravelDateSubmission("-", DateTime.Now, departDate, returnDate);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitTravelDates", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000021)", "A20",
					"NetClientSalesService.SubmitTravelDates", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000022); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitTravelDates", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitTravelDates");
		}

		public void QueryDepartTripList(DateTime passengerDepartDate, string fromStationCode, string toStationCode, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;
			Guid? netProcessId = null;

			try
			{
				waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

				_log.LogText(_logChannel, "-", "Start - QueryDepartTripList", "A01", "NetClientSalesService.QueryDepartTripList");

				UIDepartTripListRequest res = new UIDepartTripListRequest("-", DateTime.Now, passengerDepartDate, fromStationCode, toStationCode);

				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
				netProcessId = msgPack.NetProcessId;

				_log.LogText(_logChannel, "-",
					msgPack, "A05", "NetClientSalesService.QueryDepartTripList", extraMsg: "MsgObject: NetMessagePack");

				_netInterface.SendMsgPack(msgPack);

				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(waitDelaySec);

				while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
				{
					if (_recvedNetProcIdTracker.CheckReceivedResponded(netProcessId.Value, out _) == false)
						Task.Delay(100).Wait();
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
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000026)", "A20",
						"NetClientSalesService.QueryDepartTripList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else if (isServerResponded == false)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000027); Adnormal result !!;", "A21",
						"NetClientSalesService.QueryDepartTripList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else
				{
					isServerResponded = true;
				}

				_log.LogText(_logChannel, "-",
					$@"End - IsLocalServerResponded: {isServerResponded};",
					"A100",
					"NetClientSalesService.QueryDepartTripList");
			}
			catch (ThreadAbortException)
			{
				try
				{
					_log.LogText(_logChannel, "-",
					$@"ThreadAbortException",
					"A101",
					"NetClientSalesService.QueryDepartTripList");
				}
				catch { }

				if (netProcessId.HasValue)
				{
					try
					{
						_netInterface.SetExpiredNetProcessId(netProcessId.Value);
					}
					catch { }
				}
			}
		}


		public void SubmitDepartTrip(DateTime departPassengerDate,
			string departPassengerDepartTime, string departCurrency,
			string departOperatorDesc, string departOperatorLogoUrl, 
			decimal departPrice,
			string departTripId, string departVehicleTripDate,
			string departTripNo, short departTimePosi,
			string departRouteDetail, string departEmbed,
			string departPassengerActualFromStationCode, string departPassengerActualToStationCode,
			decimal departInsurance, 
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitDepartTrip", "A01", "NetClientSalesService.SubmitTravelDates");

			UIDepartTripSubmission res = new UIDepartTripSubmission("-", DateTime.Now, departPassengerDate,
						departPassengerDepartTime, departCurrency,
						departOperatorDesc, departOperatorLogoUrl, departPrice,
						departTripId, departVehicleTripDate,
						departTripNo, departTimePosi,
						departRouteDetail, departEmbed,
						departPassengerActualFromStationCode, departPassengerActualToStationCode,
						departInsurance);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitDepartTrip", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000031)", "A20",
					"NetClientSalesService.SubmitDepartTrip", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000032); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitDepartTrip", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitDepartTrip");
		}

		public void SubmitSeatList(CustSeatDetail[] custSeatDetailList, PickupNDropList pickupAndDropList,
			string departDate, string departBusType,
			decimal departInsurance, decimal departTerminalCharge,
			int departTripCode, decimal departAdultPrice,
			string departAdultExtra, decimal departAdultDisc,
			decimal departOnlineQrCharge, 
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitSeatList", "A01", "NetClientSalesService.SubmitDestination");

			UIDepartSeatSubmission res = new UIDepartSeatSubmission("-", DateTime.Now, 
				custSeatDetailList, pickupAndDropList,
				departDate, departBusType,
				departInsurance, departTerminalCharge,
				departTripCode, departAdultPrice,
				departAdultExtra, departAdultDisc,
				departOnlineQrCharge);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitDestination", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000036)", "A20",
					"NetClientSalesService.SubmitDestination", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000037); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitDestination", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitDestination");
		}

		public void SubmitPickupNDrop(string departPick, string departPickDesn, string departPickTime, string departDrop, string departDropDesn,
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitPickupNDrop", "A01", "NetClientSalesService.SubmitDestination");

			UIDepartPickupNDropSubmission res = new UIDepartPickupNDropSubmission("-", DateTime.Now,
				departPick, departPickDesn, departPickTime, departDrop, departDropDesn);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitPickupNDrop", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000041)", "A20",
					"NetClientSalesService.SubmitPickupNDrop", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000042); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitPickupNDrop", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitPickupNDrop");
		}

		public void SubmitSkyWay(bool isIncludeSkyWay, out bool isServerResponded, int waitDelaySec = 60)
		{
            isServerResponded = false;

            Guid lastNetProcessId;
            waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

            _log.LogText(_logChannel, "-", "Start - SubmitSkyWayBuyTicketRequest", "A01", "NetClientSalesService.SubmitSkyWayBuyTicketRequest");
			UISkyWaySubmission res = new UISkyWaySubmission("-", DateTime.Now, isIncludeSkyWay);
            NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
            lastNetProcessId = msgPack.NetProcessId;

            _log.LogText(_logChannel, "-",
                msgPack, "A05", "NetClientSalesService.SubmitSkyWay", extraMsg: "MsgObject: NetMessagePack");
            _netInterface.SendMsgPack(msgPack);
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(waitDelaySec);

            while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
            {
                if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
                    Task.Delay(100).Wait();
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
                _log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000007)", "A201",
                    "NetClientSalesService.SubmitSkyWay", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
            }
            else if (isServerResponded == false)
            {
                _log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000008); Adnormal result !!;", "A211",
                    "NetClientSalesService.SubmitSkyWay", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
            }
            else
            {
                isServerResponded = true;
            }

            _log.LogText(_logChannel, "-",
                $@"End - IsLocalServerResponded: {isServerResponded};",
                "A100",
                "NetClientSalesService.SubmitSkyWay");
        }

        public void SubmitInsurance(bool isIncludeInsurance, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitInsurance", "A01", "NetClientSalesService.SubmitDestination");

			UIInsuranceSubmission res = new UIInsuranceSubmission("-", DateTime.Now, isIncludeInsurance);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitInsurance", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000007)", "A20",
					"NetClientSalesService.SubmitInsurance", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000008); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitInsurance", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitInsurance");
		}

		public void SubmitPassengerInfo(CustSeatDetail[] custSeatDetailList,  
			out bool isServerResponded, int waitDelaySec = 120)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitPassengerInfo", "A01", "NetClientSalesService.SubmitPassengerInfo");

			UICustInfoSubmission res = new UICustInfoSubmission("-", DateTime.Now, custSeatDetailList);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitPassengerInfo", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000036)", "A20",
					"NetClientSalesService.SubmitPassengerInfo", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000037); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitPassengerInfo", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitPassengerInfo");
		}

		/// <summary>
		/// Cash Sale Payment Submission
		/// </summary>
		/// <param name="transactionNo"></param>
		/// <param name="totalAmount"></param>
		/// <param name="cassette1NoteCount"></param>
		/// <param name="cassette2NoteCount"></param>
		/// <param name="cassette3NoteCount"></param>
		/// <param name="refundCoinAmount"></param>
		/// <param name="isServerResponded"></param>
		/// <param name="waitDelaySec"></param>
		public void SubmitSalesPayment(string transactionNo, decimal totalAmount,
			int cassette1NoteCount, int cassette2NoteCount, int cassette3NoteCount, int refundCoinAmount,
			out bool isServerResponded, int waitDelaySec = 120)
		{
			isServerResponded = false;

			UISalesPaymentSubmission res = new UISalesPaymentSubmission("-", DateTime.Now, transactionNo, totalAmount,
				cassette1NoteCount, cassette2NoteCount, cassette3NoteCount, refundCoinAmount);

			DoSubmitSalesPayment(res, out bool isServerRespondedX, waitDelaySec);
			isServerResponded = isServerRespondedX;
		}

		/// <summary>
		/// Payment Gateway Sale Payment Submission
		/// </summary>
		/// <param name="transactionNo"></param>
		/// <param name="totalAmount"></param>
		/// <param name="bTnGSaleTransactionNo"></param>
		/// <param name="paymentMethod"></param>
		/// <param name="isServerResponded"></param>
		/// <param name="waitDelaySec"></param>
		public void SubmitSalesPayment(string transactionNo, decimal totalAmount,
			string bTnGSaleTransactionNo, string paymentMethod, PaymentType paymentType,
			out bool isServerResponded, int waitDelaySec = 120)
		{
			isServerResponded = false;

			UISalesPaymentSubmission res = new UISalesPaymentSubmission("-", DateTime.Now, transactionNo, totalAmount,
				bTnGSaleTransactionNo, paymentMethod, paymentType);

			DoSubmitSalesPayment(res, out bool isServerRespondedX, waitDelaySec);
			isServerResponded = isServerRespondedX;
		}

		private void DoSubmitSalesPayment(UISalesPaymentSubmission submission,
			out bool isServerResponded, int waitDelaySec = 120)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			string payDesc = $@"Trans.No.: {submission.TransactionNo}; PayType: {submission.TypeOfPayment.ToString()} - {submission.PaymentMethodCode}";

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, submission.TransactionNo, $@"Start - DoSubmitSalesPayment; {payDesc}", "A01", "NetClientSalesService.DoSubmitSalesPayment");

			NetMessagePack msgPack = new NetMessagePack(submission) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, submission.TransactionNo,
				msgPack, "A05", "NetClientSalesService.DoSubmitSalesPayment", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, submission.TransactionNo, $@"Unable to read from Local Server; (EXIT9000056); {payDesc}", "A20",
					"NetClientSalesService.DoSubmitSalesPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, submission.TransactionNo, $@"Unable to read from Local Server; (EXIT9000057); Adnormal result !!; {payDesc}", "A21",
					"NetClientSalesService.DoSubmitSalesPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, submission.TransactionNo,
				$@"End - IsLocalServerResponded: {isServerResponded}; {payDesc}",
				"A100",
				"NetClientSalesService.DoSubmitSalesPayment");
		}



		public void RequestSeatRelease(string transactionNo)
		{
			//isServerResponded = false;

			Guid lastNetProcessId;

			//waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitSalesPayment", "A01", "NetClientSalesService.SubmitSalesPayment");

			UISeatReleaseRequest res = new UISeatReleaseRequest("-", DateTime.Now, transactionNo);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitSalesPayment", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			//DateTime startTime = DateTime.Now;
			//DateTime endTime = startTime.AddSeconds(waitDelaySec);

			//while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			//{
			//	if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
			//		Task.Delay(100).Wait();
			//	else
			//	{
			//		isServerResponded = true;
			//		break;
			//	}
			//}

			//bool alreadyExpired = false;

			//if (isServerResponded == false)
			//	alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			//if (alreadyExpired)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000051)", "A20",
			//		"NetClientSalesService.RequestSeatRelease", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else if (isServerResponded == false)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000052); Adnormal result !!;", "A21",
			//		"NetClientSalesService.RequestSeatRelease", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else
			//{
			//	isServerResponded = true;
			//}

			//_log.LogText(_logChannel, "-",
			//	$@"End - IsLocalServerResponded: {isServerResponded};",
			//	"A100",
			//	"NetClientSalesService.RequestSeatRelease");
		}

		public void PauseCountDown(out bool isServerResponded, int waitDelaySec = 5)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - PauseCountDown", "A01", "NetClientSalesService.StartSales");

			UICountDownPausedRequest res = new UICountDownPausedRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.PauseCountDown", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			//DateTime startTime = DateTime.Now;
			//DateTime endTime = startTime.AddSeconds(waitDelaySec);

			//while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			//{
			//	if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
			//		Task.Delay(100).Wait();
			//	else
			//	{
			//		isServerResponded = true;
			//		break;
			//	}
			//}

			//bool alreadyExpired = false;

			//if (isServerResponded == false)
			//	alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			//if (alreadyExpired)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
			//		"NetClientSalesService.PauseCountDown", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else if (isServerResponded == false)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000011); Adnormal result !!;", "A21",
			//		"NetClientSalesService.PauseCountDown", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else
			//{
			//	isServerResponded = true;
			//}

			//_log.LogText(_logChannel, "-",
			//	$@"End - IsLocalServerResponded: {isServerResponded};",
			//	"A100",
			//	"NetClientSalesService.PauseCountDown");
		}

		public void EditSalesDetail(TickSalesMenuItemCode editDetailItem, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			RemoveCustomerInfoEntryTimeoutExtension();

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.EditSalesDetail");

			UIDetailEditRequest res = new UIDetailEditRequest("-", DateTime.Now, editDetailItem);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.EditSalesDetail", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000046)", "A20",
					"NetClientSalesService.EditSalesDetail", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000047); Adnormal result !!;", "A21",
					"NetClientSalesService.EditSalesDetail", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.EditSalesDetail");
		}


		//public void NavigateToPage(PageNavigateDirection navigateDirection, out bool isServerResponded, int waitDelaySec = 60)
		public void NavigateToPage(PageNavigateDirection navigateDirection, int waitDelaySec = 60)
		{
			//isServerResponded = false;
			RemoveCustomerInfoEntryTimeoutExtension();

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.NavigateToPage");

			UIPageNavigateRequest res = new UIPageNavigateRequest("-", DateTime.Now, navigateDirection);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.NavigateToPage", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			//DateTime startTime = DateTime.Now;
			//DateTime endTime = startTime.AddSeconds(waitDelaySec);

			//while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			//{
			//	if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
			//		Task.Delay(100).Wait();
			//	else
			//	{
			//		isServerResponded = true;
			//		break;
			//	}
			//}

			//bool alreadyExpired = false;

			//if (isServerResponded == false)
			//	alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			//if (alreadyExpired)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000061)", "A20",
			//		"NetClientSalesService.NavigateToPage", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else if (isServerResponded == false)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000062); Adnormal result !!;", "A21",
			//		"NetClientSalesService.NavigateToPage", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else
			//{
			//	isServerResponded = true;
			//}

			//_log.LogText(_logChannel, "-",
			//	$@"End - IsLocalServerResponded: {isServerResponded};",
			//	"A100",
			//	"NetClientSalesService.NavigateToPage");
		}

		public void ResetTimeout()
		{
			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.ResetTimeout");

			UITimeoutChangeRequest res = new UITimeoutChangeRequest("-", DateTime.Now);
			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			
			_log.LogText(_logChannel, "-", msgPack, "A05", "NetClientSalesService.ResetTimeout", 
				extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
		}

		public void ExtendCustomerInfoEntryTimeout(int extentionTimeSec)
		{
			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.ExtendCustomerInfoEntryTimeout");

			UITimeoutChangeRequest res = new UITimeoutChangeRequest("-", DateTime.Now, TimeoutChangeMode.MandatoryExtension, extentionTimeSec);
			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

			_log.LogText(_logChannel, "-", msgPack, "A05", "NetClientSalesService.ExtendCustomerInfoEntryTimeout",
				extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
		}

		public void RemoveCustomerInfoEntryTimeoutExtension()
		{
			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.RemoveCustomerInfoEntryTimeoutExtend");

			UITimeoutChangeRequest res = new UITimeoutChangeRequest("-", DateTime.Now, TimeoutChangeMode.RemoveMandatoryTimeout, 0);
			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

			_log.LogText(_logChannel, "-", msgPack, "A05", "NetClientSalesService.RemoveCustomerInfoEntryTimeoutExtend",
				extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
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
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out _) == false)
					Task.Delay(100).Wait();
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
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000009); Adnormal result !!;", "A21",
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

		/////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		/// <summary>
		/// Send to local server then expect one/many response/s.
		/// </summary>
		/// <param name="sendKioskMsg"></param>
		/// <param name="processTag"></param>
		/// <param name="waitDelaySec"></param>
		/// <returns></returns>
		private UIxTResult Request<UIxTResult>(IKioskMsg sendKioskMsg, string processTag, out bool isServerResponded, int waitDelaySec = 60)
		{
			string pId = Guid.NewGuid().ToString();

			isServerResponded = false;
			IKioskMsg kioskMsg = null;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log?.LogText(_logChannel, pId, $@"Start - {processTag}", "A01", "NetClientSalesService.Request");

			NetMessagePack msgPack = new NetMessagePack(sendKioskMsg) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log?.LogText(_logChannel, pId,
				msgPack, "A05", $@"{processTag} => NetClientSalesService.Request", extraMsg: "MsgObj: NetMessagePack");

			if (_netInterface == null)
				return default;

			_netInterface?.SendMsgPack(msgPack);

			UIxKioskDataRequestBase reqBase = sendKioskMsg?.GetMsgData();
			DateTime endTime = DateTime.Now.AddSeconds(waitDelaySec);

			while (endTime.Ticks >= DateTime.Now.Ticks)
			{
				if (Thread.CurrentThread.ThreadState.IsStateInList(
					ThreadState.AbortRequested, ThreadState.StopRequested,
					ThreadState.Aborted, ThreadState.Stopped))
				{
					break;
				}

				else if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out IKioskMsg data) == false)
					Task.Delay(100).Wait();

				else
				{
					kioskMsg = data;
					isServerResponded = true;
					break;
				}
			}

			_log?.LogText(_logChannel, pId, kioskMsg, "A10", "NetClientSalesService.Request", extraMsg: $@"MsgObj: {kioskMsg?.GetType().Name}");

			if (isServerResponded && (kioskMsg?.GetMsgData() is UIxTResult result))
			{
				return result;
			}
			else
			{
				_log?.LogText(_logChannel, pId, $@"Problem; isServerResponded : {isServerResponded}", "B01", "NetClientSalesService.Request");
				return default;
			}
		}


		/// <summary>
		/// Send to local server without expecting response 
		/// </summary>
		/// <param name="sendKioskMsg"></param>
		/// <param name="processTag"></param>
		private void SendToServerOnly(IKioskMsg sendKioskMsg, string processTag)
		{
			Guid lastNetProcessId;

			_log.LogText(_logChannel, "-", $@"Start - {processTag}", "A01", "NetClientSalesService.SendToServerOnly");

			NetMessagePack msgPack = new NetMessagePack(sendKioskMsg) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", $@"{processTag} => NetClientSalesService.SendToServerOnly", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
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
					_recvedNetProcIdTracker.AddNetProcessId(e.ReceivedData.NetProcessId, e.ReceivedData.MsgObject);
					RaiseOnDataReceived(sender, e);
				}
			}
			else if (e.ReceivedData?.Module == AppModule.Unknown)
			{
				string errMsg = $@"Error : {e.ReceivedData.ErrorMessage}; NetProcessId: {e.ReceivedData.NetProcessId}";
				_log.LogText(_logChannel, "-", errMsg, "A02", "NetClientSalesService._netInterface_OnDataReceived", NssIT.Kiosk.AppDecorator.Log.MessageType.Error, netProcessId: e.ReceivedData.NetProcessId);
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
				_log.LogError(_logChannel, "-", new Exception("Unhandled event exception; (EXIT9000200)", ex), "EX01", "NetClientSalesService.RaiseOnDataReceived", netProcessId: e?.ReceivedData?.NetProcessId);
			}
		}
	}
}