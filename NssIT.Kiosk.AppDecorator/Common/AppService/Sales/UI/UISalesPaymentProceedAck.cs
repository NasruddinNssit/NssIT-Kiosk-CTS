using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UISalesPaymentProceedAck : IKioskMsg, IUserSession
	{
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;

		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.SalesPaymentProceedAck;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		
		public string ErrorMessage { get; set; } = null;

		//////// return NssIT.Kiosk.AppDecorator.Common.AppService.Sales.SalesPaymentData
		/// <summary>
		/// Null
		/// </summary>
		public object MessageData { get; private set; } = null;

		public UserSession Session { get; private set; } = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="refNetProcessId"></param>
		/// <param name="processId"></param>
		/// <param name="timeStamp"></param>
		/// <param name="messageData">Refer to NssIT.Kiosk.AppDecorator.Common.AppService.Sales.SalesPaymentData</param>
		public UISalesPaymentProceedAck(Guid? refNetProcessId, string processId, DateTime timeStamp)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			//if (messageData is SalesPaymentData)
			//{
			//	MessageData = messageData;
			//}
			//else
			//	throw new Exception("Invalid Sales Payment Data Object.");
		}

		public void UpdateSession(UserSession session) => Session = session;

		public void Dispose()
		{ }
	}
}
