using System;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;


namespace NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI
{
	[Serializable]
	public class UICashMachineStatusSummary : IKioskMsg
	{
		public Guid? RefNetProcessId { get; private set; }
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UICashMachine;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;

		public CommInstruction Instruction { get; } = (CommInstruction)AppService.Instruction.UICashMachineInst.MachineStatusSummary;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public bool IsCashMachineReady { get; set; } = false;
		public bool IsLowCoin { get; set; } = false;
		public string ErrorMessage { get; set; } = null;
		public bool IsCoinMachRecoveryInProgressAfterDispense { get; set; } = false;

		public UICashMachineStatusSummary(Guid? refNetProcessId, string processId, DateTime timeStamp)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
		}

		public void Dispose()
		{

		}
	}
}
