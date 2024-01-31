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
	public class UIDestinationSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.DestinationSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public string DestinationCode { get; private set; } = null;
		public string DestinationName { get; private set; } = null;

		public UIDestinationSubmission(string processId, DateTime timeStamp, string destinationCode, string destinationName)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			DestinationCode = destinationCode;
			DestinationName = destinationName;
		}

		public void Dispose()
		{ }
	}
}
