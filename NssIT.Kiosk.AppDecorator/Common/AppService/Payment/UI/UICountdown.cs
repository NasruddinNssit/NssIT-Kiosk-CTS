using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI
{
	[Serializable]
	public class UICountdown : IKioskMsg
	{
		public Guid? RefNetProcessId { get; private set; }
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIPayment;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;

		public CommInstruction Instruction { get; } = (CommInstruction)AppService.Instruction.UIPaymInstruction.ShowCountdownMessage;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public string CountdownMsg { get; set; } = null;

		public UICountdown(Guid? refNetProcessId, string processId, DateTime timeStamp)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
