using NssIT.Kiosk;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.Device.B2B.CommBusiness;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Error;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Events;
using NssIT.Kiosk.Log.DB;
using System;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.Device.B2B.AccessSDK;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command.CommandSpec;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;

namespace NssIT.Kiosk.Device.B2B.B2BApp
{
	/// <summary>
	/// Used to read data from machine. And write data to machine.
	/// </summary>
	public class B2BJobApplication : IUIApplicationJob, IDisposable
	{
		private const string LogChannel = "B2BReadWriteApplication";

		private bool _disposed = false;
		private B2BAccess _b2bAccess = null;

		public event EventHandler<UIMessageEventArgs> OnShowResultMessage;

		private DbLog _log = null;
		private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

		private string _currProcessId = "-";
		public string CurrentProcessId
		{
			get => _currProcessId;
			private set => _currProcessId = string.IsNullOrEmpty(value) ? "-" : value.Trim();
		}

		public bool IsDeviceReady
		{
			get
			{
				if (_disposed)
					throw new Exception("Application already shutdown. (EXIT8011)");
				return _b2bAccess.IsCashMachineReady;
			}
		}

		public B2BJobApplication()
		{
			_b2bAccess = B2BAccess.GetB2BAccess();
			//_b2bAccess.OnInProgress += _b2bAccess_OnInProgress;
			_b2bAccess.OnCompleted += _b2bAccess_OnCompleted;
		}

		private void _b2bAccess_OnCompleted(object sender, TrxCallBackEventArgs e)
		{
			if (e.ModuleAppGroup != B2BDecorator.Command.B2BModuleAppGroup.MachineReadWrite)
				return;

			if (_disposed)
			{
				Log.LogText(LogChannel, _currProcessId, $@"B2BJobApplication has shutdowned. Net Process Id: {e?.NetProcessId};", 
					$@"A01", classNMethodName: "B2BJobApplication._b2bAccess_OnCompleted");
				return;
			}

			Log.LogText(LogChannel, _currProcessId, e, $@"A02", classNMethodName: "B2BJobApplication._b2bAccess_OnCompleted",
				extraMsg: $@"Start - _b2bAccess_OnCompleted; Net Process Id:{e?.NetProcessId}; MsgObj: TrxCallBackEventArgs");

			// e.EventMessageObj must has valid for returning a result.
			if (e.KioskMessage != null)
			{
				MessageType msgTyp = string.IsNullOrWhiteSpace(e.KioskMessage.ErrorMessage) ? MessageType.NormalType : MessageType.ErrorType;
				RaiseOnShowResultMessage(e.NetProcessId, e.KioskMessage, msgTyp);
			}
			else
				RaiseOnShowResultMessage(e.NetProcessId, null, MessageType.ErrorType);

			Log.LogText(LogChannel, _currProcessId, $@"End - _b2bAccess_OnCompleted; Net Process Id:{e?.NetProcessId}", "A10", classNMethodName: "B2BJobApplication._b2bAccess_OnCompleted");
		}

		private SemaphoreSlim _asyncSendLock = new SemaphoreSlim(1);
		public async Task<bool> SendInternalCommand(string processId, Guid? netProcessId, IKioskMsg svcMsg)
		{
			try
			{
				await _asyncSendLock.WaitAsync();

				if (svcMsg.Instruction == (CommInstruction)UIB2BInstruction.AllCassetteInfoRequest)
					GetAllCassetteInfo(processId, netProcessId);
				else
				{
					Log.LogText(LogChannel, _currProcessId,
						svcMsg, 
						$@"A02", 
						classNMethodName: "B2BJobApplication.SendInternalCommand",
						extraMsg: $@"Unregconized Instruction Code; Net Process Id: {netProcessId}; MsgObj: IUISvcMsg");
				}

				Log.LogText(LogChannel, _currProcessId, $@"Done - SendInternalCommand; Net Process Id:{netProcessId}", $@"A03", classNMethodName: "B2BJobApplication.SendInternalCommand");
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "B2BJobApplication.SendInternalCommand");
			}
			finally
			{
				_asyncSendLock.Release();
			}

			return true;

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			void GetAllCassetteInfo(string procId, Guid? netProcId)
			{
				try
				{
					if (_disposed)
						throw new Exception("System is shutting down (EXIT8015);");

					B2BAllCassetteInfoRequest command = new B2BAllCassetteInfoRequest(procId, netProcId);

					Log.LogText(LogChannel, _currProcessId, $@"Start - GetAllCassetteInfo; Net Process Id:{netProcId}", $@"A02", classNMethodName: "B2BJobApplication.GetAllCassetteInfo");

					bool addCommandResult = _b2bAccess.AddCommand(new B2BCommandPack(command), out string errorMsg);

					if (addCommandResult == false)
					{
						if (string.IsNullOrWhiteSpace(errorMsg) == false)
							throw new Exception(errorMsg);
						else
							throw new Exception("Unknown error (EXIT8010).");
					}
				}
				catch (Exception ex)
				{
					RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command. " + ex.Message);
				}
			}
		}

		private static SemaphoreSlim _showResultMessageLock = new SemaphoreSlim(1);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="netProcessId"></param>
		/// <param name="kioskMsg">Must has a valid object in order to return a result to UI.</param>
		/// <param name="msgType"></param>
		/// <param name="message">Normally this is 2nd error message. 1st error message is in returnObj.ErrorMessage</param>
		private void RaiseOnShowResultMessage(Guid? netProcessId, IKioskMsg kioskMsg, MessageType? msgType = null, string message = null)
		{
			try
			{
				_showResultMessageLock.WaitAsync().Wait();

				if (kioskMsg != null)
				{
					MessageType msgTy = (msgType.HasValue == false) ? MessageType.NormalType : msgType.Value;

					if ((string.IsNullOrWhiteSpace(kioskMsg.ErrorMessage)) && (msgTy == MessageType.NormalType || msgTy == MessageType.UnknownType))
						OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.NormalType });
					else
						OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.ErrorType });
				}
				else
				{
					message = (string.IsNullOrWhiteSpace(message) == true) ? "Result not available. (EXIT8013)" : message;
					OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = null, MsgType = MessageType.ErrorType });
				}
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, new Exception($@"Unhandle event error OnShowCustomerMessage. Net Process Id: {netProcessId}", ex), "E01", "B2BJobApplication.ShowCustomerMessage");
			}
			finally
			{
				_showResultMessageLock.Release();
			}
		}

		public bool ShutDown()
		{
			_disposed = true;

			if (OnShowResultMessage != null)
			{
				Delegate[] delgList = OnShowResultMessage.GetInvocationList();
				foreach (EventHandler<UIMessageEventArgs> delg in delgList)
				{
					OnShowResultMessage -= delg;
				}
			}
			
			return true;
		}

		public void Dispose()
		{
			_disposed = true;

			if (OnShowResultMessage != null)
			{
				Delegate[] delgList = OnShowResultMessage.GetInvocationList();
				foreach (EventHandler<UIMessageEventArgs> delg in delgList)
				{
					OnShowResultMessage -= delg;
				}
			}

			_log = null;
			_b2bAccess = null;
		}
	}
}
