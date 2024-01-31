using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
	/// <summary>
	/// UIReq UI Request
	/// </summary>
	/// <typeparam name="MData"></typeparam>
	[Serializable]
	public class UIReq<MData> : IKioskMsg, INetCommandDirective, IGnReq
		where MData : UIxKioskDataRequestBase
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public string ErrorMessage { get; set; } = null;
		public MData MsgData { get; private set; } = default;
		public dynamic GetMsgData() => MsgData;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseMany;

		//Obsolete start on 7th May 2021
		public CommInstruction Instruction { get; } = CommInstruction.ReferToGenericsUIObj;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		//------------------------------------------------------------------------------------------------

		public UIReq(string processId, AppModule module, DateTime timeStamp, MData messageData)
		{
			BaseNetProcessId = messageData.BaseNetProcessId;
			CommuCommandDirection = messageData.CommuCommandDirection;

			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			MsgData = messageData;
			Module = module;
			TimeStamp = timeStamp;
		}

		private UIReq() { /*To disable constructor for public and protected type*/ }

		public void Dispose()
		{

		}
	}
}
