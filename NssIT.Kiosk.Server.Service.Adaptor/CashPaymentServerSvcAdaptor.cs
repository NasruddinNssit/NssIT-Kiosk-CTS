using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NssIT.Kiosk;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;


namespace NssIT.Kiosk.Server.Service.Adaptor
{
	public class CashPaymentServerSvcAdaptor : IDisposable 
	{
		private string _currProcessId = "-";
		private string _logChannel = "PaymentServerService";
		private string _processId = null;

		private NssIT.Kiosk.Log.DB.DbLog _log = null;

		private INetMediaInterface _netInterface;
		private NetInfoRepository _netInfoRepository;
		private IUIPayment _cashPaymentApp;

		public CashPaymentServerSvcAdaptor(INetMediaInterface netMediaInterface, IUIPayment cashPaymentApp, NetInfoRepository netInfoRepo)
		{
			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			_netInterface = netMediaInterface;
			_netInfoRepository = netInfoRepo;

			_netInterface.OnDataReceived += _netInterface_OnDataReceived;

			_cashPaymentApp = cashPaymentApp;
			_cashPaymentApp.OnShowProcessingMessage += _cashPaymentApp_OnShowProcessingMessage;
		}

		private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (_disposed == true)
				return;

			// ..will be removed according to Module
			if ((e?.ReceivedData?.Instruction == CommInstruction.UICashMachRequestMachineStatusSummary))
			{
				//By Pass
			}
			else if ((e == null)
				|| (e.ReceivedData == null)
				|| (e.ReceivedData.Module != AppModule.UIPayment)
				)
				return;

			_log.LogText(_logChannel, _processId, e, "A01", "CashPaymentServerSvcAdaptor._netInterface_OnDataReceived",
				extraMsg:"Start - _netInterface_OnDataReceived");

			Guid? netProcessId = null;
			if (e.ReceivedData.MsgObject is INetCommandDirective)
			{
				netProcessId = e.ReceivedData.NetProcessId;
				_netInfoRepository.AddNetProcessInfo((INetCommandDirective)e.ReceivedData.MsgObject, e.ReceivedData.OriginalServicePort);
			}

			// Make New Payment
			if ((e.ReceivedData.Instruction == CommInstruction.UIPaymCreateNewPayment) && (e.ReceivedData.MsgObject is UIMakeNewPayment makeNewPay))
				MakeNewPayment(makeNewPay);

			// Cancel Existing Payment/Transaction
			else if (e.ReceivedData.Instruction == CommInstruction.UIPaymCancelPayment)
				CancelPayment();

			// Get Cash Machine Status Summary
			else if ((e.ReceivedData.Instruction == CommInstruction.UICashMachRequestMachineStatusSummary) && (netProcessId.HasValue))
				GetCashMachineStatusSummary(e.ReceivedData.MsgObject.ProcessId, netProcessId.Value, e.ReceivedData.OriginalServicePort);

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			void MakeNewPayment(UIMakeNewPayment newPay)
			{
				if (_disposed == true)
					return;

				string prevProsId = CurrentProcessId;
				try
				{
					CurrentProcessId = newPay.ProcessId;
					_cashPaymentApp.MakePayment(newPay.ProcessId, newPay.BaseNetProcessId, newPay.Price);

					_log.LogText(_logChannel, CurrentProcessId, $@"Make new payment; Price: {newPay.Price}", "A05", "CashPaymentServerSvcAdaptor._netInterface_OnDataReceived");
				}
				catch (Exception ex)
				{
					CurrentProcessId = prevProsId;
					_log.LogError(_logChannel, newPay.ProcessId, ex, "E01-Make New Payment", "CashPaymentServerSvcAdaptor._netInterface_OnDataReceived",
						adminMsg: $@"Fail to start payment");
				}
			}
			//XXXXX
			void CancelPayment()
			{
				if (_disposed == true)
					return;

				try
				{
					_cashPaymentApp.CancelTransaction();

					_log.LogText(_logChannel, CurrentProcessId, $@"Cancel Payment", "A10", "CashPaymentServerSvcAdaptor._netInterface_OnDataReceived"
						, adminMsg: $@"Cancel Payment");
				}
				catch (Exception ex)
				{
					_log.LogError(_logChannel, CurrentProcessId, ex, "E02-Cancel Existing Payment/Transaction", "CashPaymentServerSvcAdaptor._netInterface_OnDataReceived"
						, adminMsg: $@"Error encountered when Cancel Payment");
				}
			}
			//XXXXX
			void GetCashMachineStatusSummary(string processId, Guid netProcId, int destinationPort)
			{
				if (_disposed == true)
					return;

				UICashMachineStatusSummary res = null;
				processId = processId ?? "-";

				try
				{
					try
					{

						bool isCashMachineReady = _cashPaymentApp.ReadIsPaymentDeviceReady(out bool isLowCoin, out bool isRecoveryInProgressAfterDispenseCoin, out string errorMessage);

						res = new UICashMachineStatusSummary(netProcId, processId, DateTime.Now)
						{
							IsCashMachineReady = isCashMachineReady, 
							IsCoinMachRecoveryInProgressAfterDispense = isRecoveryInProgressAfterDispenseCoin,
							IsLowCoin = isLowCoin,
							ErrorMessage = errorMessage
						};
					}
					catch (Exception ex)
					{
						res = new UICashMachineStatusSummary(netProcId, processId, DateTime.Now)
						{
							IsCashMachineReady = false,
							ErrorMessage = string.IsNullOrWhiteSpace(ex.Message) ? "Unknown Error" : ex.Message
						};
					}

					NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = destinationPort };

					_netInterface.SendMsgPack(msgPack);
					_netInfoRepository.SetActiveResponseCounter(netProcId, out string errorMsg);
					_netInfoRepository.RemoveInfoWithOneResponse(netProcId);
					_log.LogText(_logChannel, processId, "Sent Cash Machine Status Summary to UI - Done", "A100", "CashPaymentServerSvcAdaptor.GetCashMachineStatusSummary");
				}
				catch (Exception ex)
				{
					_log.LogError(_logChannel, CurrentProcessId, ex, "EX10", "CashPaymentServerSvcAdaptor.GetCashMachineStatusSummary");
				}
			}
		}

		public string CurrentProcessId
		{
			get
			{
				return _currProcessId;
			}
			private set
			{
				_currProcessId = string.IsNullOrEmpty(value) ? "-" : value.Trim();
			}
		}

		/// <summary>
		/// Return false when error found;
		/// </summary>
		/// <param name="e"></param>
		/// <param name="classMethodTag"></param>
		/// <param name="netProcessId"></param>
		/// <param name="destinationPort"></param>
		/// <param name="isResponseRequested"></param>
		/// <returns></returns>
		private bool GetNetCommunicationInfo(UIMessageEventArgs e,  string classMethodTag, out Guid? netProcessId, out int destinationPort, out bool isResponseRequested)
		{
			netProcessId = null;
			destinationPort = -1;
			isResponseRequested = false;

			if (_disposed == true)
				return false;

			// Get Net Process Id
			if (e.NetProcessId.HasValue == false)
			{
				_log.LogText(_logChannel, CurrentProcessId, e, "EX01", classMethodTag, AppDecorator.Log.MessageType.Error,
					extraMsg: "Net Process Id not found; MsgObj: UIMessageEventArgs");

				return false;
			}

			netProcessId = e.NetProcessId.Value;

			// Get Destination Port and Check for Response Requested Flag
			destinationPort = _netInfoRepository.GetDestinationPort(netProcessId.Value, out isResponseRequested, out string errorMsg);

			if (destinationPort <= 0)
			{
				if (errorMsg != null)
					_log.LogText(_logChannel, CurrentProcessId, e, "EX02", classMethodTag, AppDecorator.Log.MessageType.Error,
						extraMsg: $@"Error when read net info; {errorMsg}; MsgObj: UIMessageEventArgs");
				else
					_log.LogText(_logChannel, CurrentProcessId, e, "EX03", classMethodTag, AppDecorator.Log.MessageType.Error,
						extraMsg: $@"Error when read net info; Destination port({destinationPort}) not found in system; MsgObj: UIMessageEventArgs");

				return false;
			}

			return true;
		}

		private void _cashPaymentApp_OnShowProcessingMessage(object sender, UIMessageEventArgs e)
		{
			if (_disposed == true)
				return;

			try
			{
				_log.LogText(_logChannel, CurrentProcessId, e,
					"A01", "CashPaymentServerSvcAdaptor._cashPaymentApp_OnShowProcessingMessage",
					extraMsg: "Start - _cashPaymentApp_OnShowProcessingMessage; MsgObj: UIMessageEventArgs");

				if (GetNetCommunicationInfo(e, "CashPaymentServerSvcAdaptor._cashPaymentApp_OnShowProcessingMessage",
							out Guid? refNetProcessId, out int destPort, out bool isResponseRequested) == false)
					throw new Exception("Fail to get Net Communication info.");

				if (isResponseRequested == false)
					return;

				IKioskMsg iMsg = null;

				if (e.KioskMsg != null)
					iMsg = e.KioskMsg;
				else
					iMsg = (new UIProcessingMessage(refNetProcessId, CurrentProcessId, DateTime.Now) { ProcessMsg = e.Message }) ;

				NetMessagePack msgPack = new NetMessagePack(iMsg) { DestinationPort = destPort };

				_netInterface.SendMsgPack(msgPack);
				_netInfoRepository.SetActiveResponseCounter(refNetProcessId.Value, out string errorMsgX);

				_log.LogText(_logChannel, CurrentProcessId, msgPack, "A100", "CashPaymentServerSvcAdaptor._cashPaymentApp_OnShowProcessingMessage",
					 extraMsg: "Sent Processing Message to UI - Done; MsgObj: NetMessagePack");
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, CurrentProcessId, ex, "EX10", "CashPaymentServerSvcAdaptor._cashPaymentApp_OnShowProcessingMessage");
			}
		}

		private bool _disposed = false;
		public void Dispose()
		{
			_disposed = true;

			if (_netInterface != null)
			{
				_netInterface.OnDataReceived -= _netInterface_OnDataReceived;
				_netInterface = null;
			}

			if (_cashPaymentApp != null)
			{
				_cashPaymentApp.OnShowProcessingMessage -= _cashPaymentApp_OnShowProcessingMessage;
				_cashPaymentApp = null;
			}
		}
	}
}
