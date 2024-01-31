using System;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI
{
	[Serializable]
	public class UIRequestCashMachingStatusSummary : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; }
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UICashMachine;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;

		public CommInstruction Instruction { get; } = (CommInstruction)AppService.Instruction.UICashMachineInst.RequestMachineStatusSummary;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOnly;

		public UIRequestCashMachingStatusSummary(string processId, DateTime timeStamp)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
		}

		public void Dispose()
		{
			
		}
	}
}
