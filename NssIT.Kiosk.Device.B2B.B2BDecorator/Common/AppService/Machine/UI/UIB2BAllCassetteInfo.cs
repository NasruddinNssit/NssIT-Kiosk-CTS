using System;
using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Machine.UI
{
	[Serializable]
	public class UIB2BAllCassetteInfo : IKioskMsg
	{
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIB2B;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;

		public CommInstruction Instruction { get; } = (CommInstruction)UIB2BInstruction.AllCassetteInfo;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public B2BCassetteInfoCollection CassetteInfoCollection { get; set; } = null;

		public UIB2BAllCassetteInfo(Guid? refNetProcessId, string processId, DateTime timeStamp)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
		}

		public void Dispose()
		{ }
	}
}
