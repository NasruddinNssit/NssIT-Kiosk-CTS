using System;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Instruction;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Command
{
	public enum B2BCommandCode
	{
		UnKnown = CommInstruction.Blank,
		StartMakePayment = CommInstruction.UICashMachRequestStartMakePayment,
		CancelPayment = CommInstruction.UICashMachRequestCancelPayment,

		AllCassetteInfoRequest = UIB2BInstruction.AllCassetteInfoRequest
	}
}
