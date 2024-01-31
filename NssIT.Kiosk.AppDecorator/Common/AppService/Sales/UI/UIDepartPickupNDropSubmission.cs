using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UIDepartPickupNDropSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.DepartPickupNDropSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public string DepartPick { get; private set; } = null;
		public string DepartPickDesn { get; private set; } = null;
		public string DepartPickTime { get; private set; } = null;
		public string DepartDrop { get; private set; } = null;
		public string DepartDropDesn { get; private set; } = null;

		public UIDepartPickupNDropSubmission(string processId, DateTime timeStamp, 
			string departPick,
			string departPickDesn, string departPickTime,
			string departDrop, string departDropDesn)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			DepartPick = departPick;
			DepartPickDesn = departPickDesn;
			DepartPickTime = departPickTime;
			DepartDrop = departDrop;
			DepartDropDesn = departDropDesn;
		}

		public void Dispose()
		{ }
	}
}
