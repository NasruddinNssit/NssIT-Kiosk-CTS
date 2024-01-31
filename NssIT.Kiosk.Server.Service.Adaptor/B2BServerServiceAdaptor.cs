using System;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Machine.UI;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;
using NssIT.Kiosk.AppDecorator.Devices;

namespace NssIT.Kiosk.Server.Service.Adaptor
{
	public class B2BServerServiceAdaptor : IDisposable
	{
		private string _logChannel = "BanknoteMachServerService";

		private Log.DB.DbLog _log = null;

		private INetMediaInterface _netInterface;
		private NetInfoRepository _netInfoRepository;
		private IUIBanknoteReadWrite _bankNoteReadWriteApp;

		public B2BServerServiceAdaptor(INetMediaInterface netMediaInterface, IUIBanknoteReadWrite bankNoteReadWriteApp, NetInfoRepository netInfoRepo)
		{
			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			_netInterface = netMediaInterface;
			_netInfoRepository = netInfoRepo;

			_netInterface.OnDataReceived += _netInterface_OnDataReceived;

			_bankNoteReadWriteApp = bankNoteReadWriteApp;
			_bankNoteReadWriteApp.OnShowResultMessage += _bankNoteReadWriteApp_OnShowResultMessage;
		}

		private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if ((e == null)
				|| (e.ReceivedData == null)
				|| (e.ReceivedData.Module != AppModule.UIB2B)
				)
				return;

			_log.LogText(_logChannel, "-", e, "A01", "BanknoteMachineServerSvcAdaptor._netInterface_OnDataReceived",
				extraMsg: "Start - _netInterface_OnDataReceived");

			Guid? netProcessId = null;
			if (e.ReceivedData.MsgObject is INetCommandDirective)
			{
				netProcessId = e.ReceivedData.NetProcessId;
				_netInfoRepository.AddNetProcessInfo((INetCommandDirective)e.ReceivedData.MsgObject, e.ReceivedData.OriginalServicePort);
			}

			_bankNoteReadWriteApp.SendInternalCommand((e.ReceivedData.MsgObject?.ProcessId) ?? "-", e.ReceivedData.NetProcessId, e.ReceivedData.MsgObject);
		}

		private void _bankNoteReadWriteApp_OnShowResultMessage(object sender, UIMessageEventArgs e)
		{
			try
			{
				_log.LogText(_logChannel, "-", e,
					"A01", "BanknoteMachineServerSvcAdaptor._bankNoteReadWriteApp_OnShowResultMessage",
					extraMsg: "Start - _bankNoteReadWriteApp_OnShowResultMessage; MsgObj: UIMessageEventArgs");

				if (_netInfoRepository.GetNetCommunicationInfo(_logChannel, e, "BanknoteMachineServerSvcAdaptor._bankNoteReadWriteApp_OnShowResultMessage",
							out Guid? refNetProcessId, out int destPort, out bool isResponseRequested) == false)
					throw new Exception("Fail to get Net Communication info.");

				if (isResponseRequested == false)
					return;

				NetMessagePack msgPack;
				IUISvcMsg svcMsg = GetResultSvcMessage(e.MessageObj, refNetProcessId, e.MsgType.Value);

				if (svcMsg == null)
				{
					msgPack = new NetMessagePack(refNetProcessId.Value) { DestinationPort = destPort };

					if (e.Message != null)
						msgPack.ErrorMessage = e.Message;

					else if (e.MsgType == MessageType.ErrorType)
						msgPack.ErrorMessage = "Result not available (EXIT7102)";

					else 
						msgPack.ErrorMessage = "Unknown result (EXIT7104)";

					_log.LogText(_logChannel, "-", e, "EX10", "BanknoteMachineServerSvcAdaptor._bankNoteReadWriteApp_OnShowResultMessage", AppDecorator.Log.MessageType.Error,
						extraMsg: $@"Error: {msgPack.ErrorMessage}. Net Process Id: {refNetProcessId.Value} MsgObj: UIMessageEventArgs");
				}
				else
					msgPack = new NetMessagePack(svcMsg) { DestinationPort = destPort };

				_netInterface.SendMsgPack(msgPack);
				_netInfoRepository.SetActiveResponseCounter(refNetProcessId.Value, out string errorMsgX);
				_netInfoRepository.RemoveInfoWithOneResponse(refNetProcessId.Value);

				_log.LogText(_logChannel, "-", msgPack, "A100", "BanknoteMachineServerSvcAdaptor._bankNoteReadWriteApp_OnShowResultMessage",
					 extraMsg: $@"Sent Show Paymant Page Instruction to UI - Done; Net Process Id: {refNetProcessId.Value};MsgObj: NetMessagePack");
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, "-", ex, "EX10", "BanknoteMachineServerSvcAdaptor._bankNoteReadWriteApp_OnShowResultMessage");
			}

			// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			IUISvcMsg GetResultSvcMessage(IEventMessageObj evtMsgObj, Guid? refNetProcId, MessageType? msgType)
			{
				if (evtMsgObj == null)
					return null;

				IUISvcMsg retMsg = null;

				if ((evtMsgObj.MessageCode == (CommInstruction)UIB2BInstruction.AllCassetteInfoRequest) && (evtMsgObj is B2BCassetteInfoEvtMessage caseInfoEvtMsg))
				{
					if (msgType.Value == MessageType.NormalType)
						retMsg = new UIB2BAllCassetteInfo(refNetProcId, "-", DateTime.Now) { CassetteInfoCollection = caseInfoEvtMsg.CassetteInfoCollection };
					else if (msgType.Value == MessageType.ErrorType)
						retMsg = new UIB2BAllCassetteInfo(refNetProcId, "-", DateTime.Now) { ErrorMessage = caseInfoEvtMsg.ErrorMessage ?? "Unknown Error (EXIT7103)" };
					else
						retMsg = new UIB2BAllCassetteInfo(refNetProcId, "-", DateTime.Now) { ErrorMessage = "Unknown Error (EXIT7101)" };
				}
				return retMsg;
			}
		}

		public void Dispose()
		{
			if (_netInterface != null)
			{
				_netInterface.OnDataReceived -= _netInterface_OnDataReceived;
				_netInterface = null;
			}

			if (_bankNoteReadWriteApp != null)
			{
				_bankNoteReadWriteApp.OnShowResultMessage -= _bankNoteReadWriteApp_OnShowResultMessage;
				_bankNoteReadWriteApp = null;
			}
		}
	}
}