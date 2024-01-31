using CCNET;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Machine.UI;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Events;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.AccessSDK.CommandExec
{
	public class B2BGetCassetteInfoExecution : IB2BCommandExec
	{
		private const string LogChannel = "B2BAccess";

		private bool _isRaisedCompleteCallbackEvent = false;
		private DateTime _lastInProgressEventReceivedTime = DateTime.MinValue;

		public string CurrentProcessId { get; private set; } = "-";
		public Guid? CurrentNetProcessId { get; private set; } = null;

		private B2BRMCashData CurrentB2BCashData { get; set; } = null;
		private OrgApi.B2BApi B2bApi { get; set; }
		private CommBusiness.B2BAccess B2bAccess { get; set; }

		private DbLog _log = null;
		private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

		public B2BGetCassetteInfoExecution(CommBusiness.B2BAccess currB2bAccess, B2BRMCashData currB2BCashData, OrgApi.B2BApi currB2bApi)
		{
			B2bAccess = currB2bAccess;
			CurrentB2BCashData = currB2BCashData;
			B2bApi = currB2bApi;
		}

		public B2BCommandPack Execute(B2BCommandPack commandPack)
		{
			try
			{
				_isRaisedCompleteCallbackEvent = false;

				CurrentNetProcessId = commandPack.NetProcessId;
				CurrentProcessId = commandPack.ProcessId;

				Log.LogText(LogChannel, CurrentProcessId,
					commandPack, 
					"A01", "B2BGetCassetteInfoExecution.Execute", netProcessId: CurrentNetProcessId,
					extraMsg: $@"Start - Execute; Managed Thread Id : {Thread.CurrentThread.ManagedThreadId}; Thread Hash Code : {Thread.CurrentThread.GetHashCode()} ");

				B2BCassetteInfoCollection casesInfoColl = B2bApi.GetCassetteQtyStatus();
				UIB2BAllCassetteInfo uiMsg = new UIB2BAllCassetteInfo(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { CassetteInfoCollection = casesInfoColl };

				whenCompletedSendEvent(B2BCommandResultCode.Success, commandPack.NetProcessId, "B2BGetCassetteInfoExecution.Execute.A02", uiMsg);

				Log.LogText(LogChannel, CurrentProcessId, "End - Execute", "A10", "B2BGetCassetteInfoExecution.Execute", netProcessId: CurrentNetProcessId,
									extraMsg: "End - Execute; MsgObj: B2BCommandPack");
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, ex, "Ex01", classNMethodName: "B2BGetCassetteInfoExecution.Execute", netProcessId: CurrentNetProcessId);

				UIB2BAllCassetteInfo uiMsg = new UIB2BAllCassetteInfo(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { ErrorMessage = ex.Message };

				whenCompletedSendEvent(B2BCommandResultCode.Fail, commandPack.NetProcessId, "B2BGetCassetteInfoExecution.Execute.EX01", uiMsg, ex);
			}

			return commandPack;

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			void whenCompletedSendEvent(B2BCommandResultCode completedStatus, Guid? netProcessId, string lineTag, UIB2BAllCassetteInfo uiAllCassetteInfo,
				Exception error = null)
			{
				if (_isRaisedCompleteCallbackEvent == true)
					return;

				TrxCallBackEventArgs compEv = new TrxCallBackEventArgs(netProcessId, CurrentProcessId, B2BModuleAppGroup.MachineReadWrite)
				{
					ResultStatus = (DeviceProgressStatus)completedStatus,
					Error = error, KioskMessage = uiAllCassetteInfo
				};

				_isRaisedCompleteCallbackEvent = true;

				Log.LogText(LogChannel, CurrentProcessId,
					compEv,
					"A01", "B2BGetCassetteInfoExecution.whenCompletedSendEvent", netProcessId: netProcessId,
					extraMsg: $@"Start - Execute; MsgObj: TrxCallBackEventArgs");

				B2bAccess.RaiseOnCompleted(compEv, lineTag);

				Log.LogText(LogChannel, CurrentProcessId, "End - Execute", "A10", "B2BGetCassetteInfoExecution.whenCompletedSendEvent", netProcessId: netProcessId);
			}
		}

		public void Dispose()
		{
			CurrentB2BCashData = null;
			B2bApi = null;
			B2bAccess = null;
			_log = null;
		}

	}
}
