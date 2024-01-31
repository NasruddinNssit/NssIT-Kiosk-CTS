using System;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Common.AppService.Instruction;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Data
{
	public class B2BCassetteInfoEvtMessage : IKioskMsg
	{
		public Guid? RefNetProcessId { get; set; }
		public string ProcessId { get; set; }
		public DateTime TimeStamp { get; private set; }

		public AppModule Module { get; } = AppModule.UIPayment;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }

		public CommInstruction Instruction { get; } = (CommInstruction)UIB2BInstruction.AllCassetteInfo;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		public B2BCassetteInfoCollection CassetteInfoCollection { get; set; } = null;

		public B2BCassetteInfoEvtMessage()
		{
			TimeStamp = DateTime.Now;
		}

		public void Dispose()
		{
			
		}
	}
}