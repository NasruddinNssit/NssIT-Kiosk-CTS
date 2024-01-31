using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI
{
	[Serializable]
	public class UICustomerMessage : IKioskMsg
	{
		public Guid? RefNetProcessId { get; private set; }
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIPayment;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;

		public CommInstruction Instruction { get; } = (CommInstruction)UIPaymInstruction.ShowCustomerMessage;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public string CustmerMsg { get; set; } = null;
		//public string ProcessMsg { get; set; } = null;
		//public string ErrorMsg { get; set; } = null;

		public UIVisibility DisplayCustmerMsg { get; set; } = UIVisibility.VisibleNotChanged;
		//public UIVisibility DisplayProcessMsg { get; set; } = UIVisibility.VisibleNotChanged;
		//public UIVisibility DisplayErrorMsg { get; set; } = UIVisibility.VisibleNotChanged;

		public UICustomerMessage(Guid? refNetProcessId, string processId, DateTime timeStamp)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
		}

		public void Dispose() { }
	}
}
