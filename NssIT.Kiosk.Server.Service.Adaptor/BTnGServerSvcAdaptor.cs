using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.AppService.Network;
using NssIT.Kiosk.Network.PaymentGatewayApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.Service.Adaptor
{
	/// <summary>
	/// ClassCode:EXIT70.01
	/// </summary>
	public class BTnGServerSvcAdaptor : IDisposable
	{
		private string _logChannel = "SalesServerService";

		private Log.DB.DbLog _log = null;

		private INetMediaInterface _netInterface;
		private NetInfoRepository _netInfoRepository;
		private IUIApplicationJob _bTngJopApp;

		/// <summary>
		/// FuncCode:EXIT70.0102
		/// </summary>
		public BTnGServerSvcAdaptor(INetMediaInterface netMediaInterface, NetInfoRepository netInfoRepo)
		{
			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			_netInterface = netMediaInterface;
			_netInfoRepository = netInfoRepo;

			_netInterface.OnDataReceived += _netInterface_OnDataReceived;

			//_bTngJopApp = new BTnGApplication();

			//CYA-TEST -- _bTngJopApp = new BTnGApplication(LibShowMessageWindow.MessageWindow.DefaultMessageWindow);
			//------------------------------------
			_bTngJopApp = new BTnGApplication();

			_bTngJopApp.OnShowResultMessage += _bTngJopApp_OnShowResultMessage;
		}

		/// <summary>
		/// FuncCode:EXIT70.0103
		/// </summary>
		private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if ((e == null)
				|| (e.ReceivedData == null)
				|| (e.ReceivedData.Module != AppModule.UIBTnG)
				|| (_disposed == true)
				)
				return;

			_log.LogText(_logChannel, "-", e, "A01", "BTnGServerSvcAdaptor._netInterface_OnDataReceived",
				extraMsg: "Start - _netInterface_OnDataReceived");

			Guid? netProcessId = null;
			if (e.ReceivedData.MsgObject is INetCommandDirective)
			{
				netProcessId = e.ReceivedData.NetProcessId;
				_netInfoRepository.AddNetProcessInfo((INetCommandDirective)e.ReceivedData.MsgObject, e.ReceivedData.OriginalServicePort);
			}

			_bTngJopApp.SendInternalCommand((e.ReceivedData.MsgObject?.ProcessId) ?? "-", e.ReceivedData.NetProcessId, e.ReceivedData.MsgObject);
		}

		/// <summary>
		/// FuncCode:EXIT70.0104
		/// </summary>
		private void _bTngJopApp_OnShowResultMessage(object sender, UIMessageEventArgs e)
		{
			if (_disposed == true)
				return;

			try
			{
				_log.LogText(_logChannel, "-", e,
					"A01", "BTnGServerSvcAdaptor._bTngJopApp_OnShowResultMessage",
					extraMsg: "Start - _bTngJopApp_OnShowResultMessage; MsgObj: UIMessageEventArgs");

				if (_netInfoRepository.GetNetCommunicationInfo(_logChannel, e, "BTnGServerSvcAdaptor._bTngJopApp_OnShowResultMessage",
							out Guid? refNetProcessId, out int destPort, out bool isResponseRequested) == false)
					throw new Exception("Fail to get Net Communication info.");

				if (isResponseRequested == false)
					return;

				NetMessagePack msgPack;
				IKioskMsg svcMsg = e.KioskMsg;

				if (svcMsg == null)
				{
					msgPack = new NetMessagePack(refNetProcessId.Value) { DestinationPort = destPort };

					if (e.Message != null)
						msgPack.ErrorMessage = e.Message;

					else if (e.MsgType == MessageType.ErrorType)
						msgPack.ErrorMessage = "Result not available (EXIT70.0104.X01)";

					else
						msgPack.ErrorMessage = "Unknown result (EXIT70.0104.X02)";

					_log.LogText(_logChannel, "-", e, "EX10", "BTnGServerSvcAdaptor._bTngJopApp_OnShowResultMessage", AppDecorator.Log.MessageType.Error,
						extraMsg: $@"Error: {msgPack.ErrorMessage}. Net Process Id: {refNetProcessId.Value} MsgObj: UIMessageEventArgs");
				}
				else
					msgPack = new NetMessagePack(svcMsg) { DestinationPort = destPort };

				_netInterface.SendMsgPack(msgPack);
				_netInfoRepository.SetActiveResponseCounter(refNetProcessId.Value, out string errorMsgX);
				_netInfoRepository.RemoveInfoWithOneResponse(refNetProcessId.Value);

				_log.LogText(_logChannel, "-", msgPack, "A100", "BTnGServerSvcAdaptor._bTngJopApp_OnShowResultMessage",
					 extraMsg: $@"Sent Sales Process Result to UI - Done; Net Process Id: {refNetProcessId.Value};MsgObj: NetMessagePack");
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, "-", ex, "EX10", "BTnGServerSvcAdaptor._bTngJopApp_OnShowResultMessage");
			}
		}

		private bool _disposed = false;
		public void Dispose()
		{
			_disposed = true;

			try
			{
				_bTngJopApp?.ShutDown();
				//_bTngJopApp?.Dispose();
			}
			catch { }

			if (_netInterface != null)
			{
				_netInterface.OnDataReceived -= _netInterface_OnDataReceived;
				_netInterface = null;
			}

			if (_bTngJopApp != null)
			{
				_bTngJopApp.OnShowResultMessage -= _bTngJopApp_OnShowResultMessage;
				_bTngJopApp = null;
			}
		}

	}
}