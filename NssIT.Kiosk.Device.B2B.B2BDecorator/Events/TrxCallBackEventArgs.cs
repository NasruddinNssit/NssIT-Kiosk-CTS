using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;
using System;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Events
{
	public class TrxCallBackEventArgs : EventArgs, ITrxCallBackEventArgs
	{
		public Guid? NetProcessId { get; private set; }
		public string ProcessId { get; private set; }

		public string Remark { get; set; } = null;
		public Exception Error { get; set; } = null;
		public DeviceProgressStatus ResultStatus { get; set; } = (DeviceProgressStatus)PaymentResultStatus.InitNewState;

		public IKioskMsg KioskMessage { get; set; } = null;

		public B2BModuleAppGroup ModuleAppGroup { get; set; } = B2BModuleAppGroup.Unknown ;

		public bool IsSuccess
		{
			get
			{
				if ((ResultStatus == (DeviceProgressStatus)PaymentResultStatus.Success))
					return true;
				else if ((ResultStatus == DeviceProgressStatus.Success))
					return true;
				else
					return false;
			}
		}
		public string ErrorMsg { get { return Error?.Message; } }

		public TrxCallBackEventArgs(Guid? netProcessId, string processId, B2BModuleAppGroup moduleAppGroup)
		{
			ModuleAppGroup = moduleAppGroup;
			NetProcessId = netProcessId;
			ProcessId = processId;
		}

		public void ResetInfo()
		{
			ResultStatus = (DeviceProgressStatus)PaymentResultStatus.InitNewState;
			Error = null;
		}

		public void SetEmptyTo(bool setNull = true)
		{ }

		public void AddErrorMessage(string errorMsg)
		{
			if (string.IsNullOrWhiteSpace(errorMsg) == false)
			{
				if (Error == null)
					Error = new Exception(errorMsg);
				else
					Error = new Exception($@"{errorMsg}; {Error.Message};", Error);
			}
		}

		public ITrxCallBackEventArgs Duplicate()
		{
			TrxCallBackEventArgs ev = new TrxCallBackEventArgs(this.NetProcessId, this.ProcessId, this.ModuleAppGroup)
				{ KioskMessage = this.KioskMessage, Error = this.Error, ResultStatus = this.ResultStatus, Remark = this.Remark };

			return ev;
		}

		public void Dispose()
		{
			Error = null;
			KioskMessage = null;
		}
	}
}
