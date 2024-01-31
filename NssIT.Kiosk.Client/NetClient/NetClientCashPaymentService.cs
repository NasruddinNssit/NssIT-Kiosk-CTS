using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.NetClient
{
    /// <summary>
    /// Net Client Service for Cash Sales Payment
    /// </summary>
    public class NetClientCashPaymentService
    {
		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		private UICashMachineStatusSummary _cashMachineStatusSummary = null;

		private string _currProcessId = null;

		public event EventHandler<DataReceivedEventArgs> OnShowForm;
		public event EventHandler<DataReceivedEventArgs> OnShowHideForm;

		public event EventHandler<DataReceivedEventArgs> OnShowCountdownMessage;
		public event EventHandler<DataReceivedEventArgs> OnShowCustomerMessage;
		public event EventHandler<DataReceivedEventArgs> OnShowProcessingMessage;
		public event EventHandler<DataReceivedEventArgs> OnShowOutstandingPayment;
		public event EventHandler<DataReceivedEventArgs> OnShowRefundPayment;
		public event EventHandler<DataReceivedEventArgs> OnShowBanknote;
		public event EventHandler<DataReceivedEventArgs> OnErrorMessage;
		public event EventHandler<DataReceivedEventArgs> OnSetCancelPermission;

		public NetClientCashPaymentService(INetMediaInterface netInterface)
		{
			_netInterface = netInterface;

			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			if (_netInterface != null)
				_netInterface.OnDataReceived += _netInterface_OnDataReceived;
		}

		private int GetServerPort() => App.SysParam.PrmLocalServerPort;

		private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if ((e is null) || (e.ReceivedData is null))
				return;

			if (e.ReceivedData?.Module == AppModule.UICashMachine)
			{
				switch (e.ReceivedData.Instruction)
				{
					case CommInstruction.UICashMachineStatusSummary:
						_cashMachineStatusSummary = (UICashMachineStatusSummary)e.ReceivedData.MsgObject;
						break;
				}
			}
			else if (e.ReceivedData?.Module == AppModule.UIPayment)
			{
				switch (e?.ReceivedData.Instruction)
				{
					case CommInstruction.UIPaymShowCountdownMessage:
						RaisePaymentEvent(OnShowCountdownMessage, e, "OnShowCountdownMessage");
						break;
					case CommInstruction.UIPaymShowCustomerMessage:
						RaisePaymentEvent(OnShowCustomerMessage, e, "OnShowCustomerMessage");
						break;
					case CommInstruction.UIPaymShowProcessingMessage:
						RaisePaymentEvent(OnShowProcessingMessage, e, "OnShowProcessingMessage");
						break;
					case CommInstruction.UIPaymShowOutstandingPayment:
						RaisePaymentEvent(OnShowOutstandingPayment, e, "OnShowOutstandingPayment");
						break;
					case CommInstruction.UIPaymShowRefundPayment:
						RaisePaymentEvent(OnShowRefundPayment, e, "OnShowRefundPayment");
						break;
					case CommInstruction.UIPaymShowBanknote:
						RaisePaymentEvent(OnShowBanknote, e, "OnShowBanknote");
						break;
					case CommInstruction.ShowErrorMessage:
						RaisePaymentEvent(OnErrorMessage, e, "OnErrorMessage");
						break;
					case CommInstruction.UIPaymShowForm:
						RaisePaymentEvent(OnShowForm, e, "OnShowForm");
						break;
					case CommInstruction.UIPaymHideForm:
						RaisePaymentEvent(OnShowHideForm, e, "OnShowHideForm");
						break;
					case CommInstruction.UIPaymSetCancelPermission:
						RaisePaymentEvent(OnSetCancelPermission, e, "OnSetCancelPermission");
						break;
					default:
						break;
				}
			}
		}

		private void RaisePaymentEvent(EventHandler<DataReceivedEventArgs> anEvent, DataReceivedEventArgs dataArg, string tag)
		{
			try
			{
				if (anEvent != null)
				{
					anEvent.Invoke(null, dataArg);
				}
			}
			catch (Exception ex)
			{
				if (dataArg == null)
					_log.LogError(_logChannel, _currProcessId, new Exception($@"Unhandle event({tag}) exception; (EXIT10100001)", ex), "EX01", "NetClientCashPaymentService.RaiseCashPaymentEvent");
				else
					_log.LogText(_logChannel, _currProcessId, dataArg, "EX02", "NetClientCashPaymentService.RaiseCashPaymentEvent", AppDecorator.Log.MessageType.Error,
						extraMsg: $@"Unhandle event({tag}) exception; (EXIT10100001); MsgObj: DataReceivedEventArgs");
			}
		}

		public bool CheckCashMachineIsReady(string processId, 
			out bool isLowCoin, out bool isCoinMachRecoveryInProgressAfterDispense, 
			out string errorMsg, int waitDelaySec = 60)
		{
			isCoinMachRecoveryInProgressAfterDispense = false;
			isLowCoin = false;
			processId = string.IsNullOrWhiteSpace(processId) ? "-" : processId.Trim();
			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - get CheckCashMachineIsReady", "A01", "NetClientCashPaymentService.CheckCashMachineIsReady");
			errorMsg = null;
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
				_log.LogText(_logChannel, processId, $@"Unable to read from Cash Machine", "A20",
					"NetClientCashPaymentService.CheckCashMachineIsReady");
				throw new Exception("Unable to read from Cash Machine ");
			}

			if (_cashMachineStatusSummary.IsCashMachineReady == false)
			{
				errorMsg = _cashMachineStatusSummary.ErrorMessage;

				_log.LogText(_logChannel, processId, $@"End - IsCashMachineReady: {_cashMachineStatusSummary.IsCashMachineReady}; Error: {errorMsg}", "A100",
				"NetClientCashPaymentService.CheckCashMachineIsReady");
			}
			else
			{
				_log.LogText(_logChannel, processId, $@"End - IsCashMachineReady: {_cashMachineStatusSummary.IsCashMachineReady}", "A101",
				"NetClientCashPaymentService.CheckCashMachineIsReady");
			}

			isCoinMachRecoveryInProgressAfterDispense = _cashMachineStatusSummary.IsCoinMachRecoveryInProgressAfterDispense;
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
				_log.LogError(_logChannel, processId, ex, "EX01-StartPayment", "NetClientCashPaymentService.BtnCancelSales_Click");
				throw ex;
			}

			return isSuccess;
		}

		public void CancelTransaction()
		{
			try
			{
				UICancelPayment cancPay = new UICancelPayment(_currProcessId, DateTime.Now);
				NetMessagePack msgPack = new NetMessagePack(cancPay) { DestinationPort = GetServerPort() };

				_netInterface.SendMsgPack(msgPack);
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, _currProcessId, ex, "EX01", "NetClientCashPaymentService.CancelTransaction");
			}
		}

		public void Dispose()
		{
			try
			{
				if (_netInterface != null)
					_netInterface.OnDataReceived -= _netInterface_OnDataReceived;
				_netInterface = null;

				if (OnShowCountdownMessage != null)
				{
					Delegate[] delgList = OnShowCountdownMessage.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnShowCountdownMessage -= delg;
				}

				if (OnShowCustomerMessage != null)
				{
					Delegate[] delgList = OnShowCustomerMessage.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnShowCustomerMessage -= delg;
				}

				if (OnShowProcessingMessage != null)
				{
					Delegate[] delgList = OnShowProcessingMessage.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnShowProcessingMessage -= delg;
				}

				if (OnShowOutstandingPayment != null)
				{
					Delegate[] delgList = OnShowOutstandingPayment.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnShowOutstandingPayment -= delg;
				}

				if (OnShowRefundPayment != null)
				{
					Delegate[] delgList = OnShowRefundPayment.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnShowRefundPayment -= delg;
				}

				if (OnShowBanknote != null)
				{
					Delegate[] delgList = OnShowBanknote.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnShowBanknote -= delg;
				}

				if (OnErrorMessage != null)
				{
					Delegate[] delgList = OnErrorMessage.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnErrorMessage -= delg;
				}

				if (OnShowForm != null)
				{
					Delegate[] delgList = OnShowForm.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnShowForm -= delg;
				}
				
				if (OnShowHideForm != null)
				{
					Delegate[] delgList = OnShowHideForm.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnShowHideForm -= delg;
				}

				if (OnSetCancelPermission != null)
				{
					Delegate[] delgList = OnSetCancelPermission.GetInvocationList();
					foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
						OnSetCancelPermission -= delg;
				}
			}
			catch { }
		}
	}
}
