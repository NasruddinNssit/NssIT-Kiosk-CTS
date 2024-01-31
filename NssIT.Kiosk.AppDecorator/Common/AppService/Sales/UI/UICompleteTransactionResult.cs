using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UICompleteTransactionResult : IKioskMsg, IUserSession
	{
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.CompleteTransactionResult;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		/// <summary>
		/// return NssIT.Kiosk.Common.WebService.KioskWebService.transcomplete_status
		/// </summary>
		public object MessageData { get; private set; } = null;
		public ProcessResult ProcessState { get; private set; } = ProcessResult.Fail;
		public UserSession Session { get; private set; } = null;

		public UICompleteTransactionResult(Guid? refNetProcessId, string processId, DateTime timeStamp, object messageData, ProcessResult processState)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			MessageData = messageData;
			ProcessState = processState;
		}

		public void UpdateSession(UserSession session) => Session = session;

		public void Dispose()
		{
			MessageData = null;
		}
	}
}
