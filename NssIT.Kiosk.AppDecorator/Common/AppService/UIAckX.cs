using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
	[Serializable]
	public class UIAckX<MData> : IKioskMsg, IGnAckX
		where MData : UIxKioskDataAckBase
	{
		public Guid? RefNetProcessId { get; private set; }
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; private set; } = AppModule.Unknown;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public string ErrorMessage { get; set; } = null;
		public MData MsgData { get; private set; } = default;
		public dynamic GetMsgData() => MsgData;

		//Obsolete start on 7th May 2021
		public CommInstruction Instruction { get; } = CommInstruction.ReferToGenericsUIObj;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		//------------------------------------------------------------------------------------------------

		public UIAckX(Guid? refNetProcessId, string processId, AppModule module, DateTime timeStamp, MData messageData)
		{
			//messageData?.Base_UpdateProcessInfo(processId, refNetProcessId);

			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			MsgData = messageData;
			Module = module;
			TimeStamp = timeStamp;
		}

		private UIAckX() { /*To disable constructor for public and protected type*/ }

		public void Dispose()
		{

		}
	}
}
