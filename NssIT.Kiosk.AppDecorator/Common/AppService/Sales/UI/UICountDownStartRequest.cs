using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UICountDownStartRequest : IKioskMsg, INetCommandDirective
    {
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.CountDownStartRequest;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		/// <summary>
		/// Period of counting in second
		/// </summary>
		public int PeriodCountSec { get; private set; } = 30;
		public LanguageCode Language { get; private set; } = LanguageCode.English;
	    public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseMany;

		public UICountDownStartRequest(string processId, DateTime timeStamp, int periodCountSec)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			PeriodCountSec = periodCountSec;
		}

		public void Dispose()
		{

		}
	}
}
