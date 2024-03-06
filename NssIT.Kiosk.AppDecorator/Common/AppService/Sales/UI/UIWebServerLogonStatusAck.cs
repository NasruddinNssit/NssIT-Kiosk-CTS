using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
    [Serializable]
    public class UIWebServerLogonStatusAck : IKioskMsg
    {
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;

		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.WebServerLogonStatusAck;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public bool LogonErrorFound { get; private set; } = false;
		public bool LogonSuccess { get; private set; } = false;
		public bool NetworkTimeout { get; private set; } = true;
		public bool IsValidAuthentication { get; private set; } = false;

		/// <summary>
		/// return NssIT.Kiosk.Common.WebService.KioskWebService.destination_status
		/// </summary>
		public object MessageData { get; private set; } = null;

		public string OperationFlag { get; private set; } = null;
		public string OperationTimeFrom { get; private set; } = null;
		public string OperationTimeTo { get; private set; } = null;
		public string SettlementTime { get; private set; } = null;
		public string SettlementFlag { get; private set; } = null;
		public string NameFlag { get; private set; } = null;
		public string ICFlag { get; private set; } = null;
		public string ContactFlag { get; private set; }
		public UIWebServerLogonStatusAck(Guid? refNetProcessId, string processId,
			DateTime timeStamp, 
			bool isLogonSuccess, 
			bool networkTimeout, 
			bool isValidAuthentication,
			string operationFlag,
			string operationTimeFrom,
			string operationTimeTo,
			string settlementTime,
			string settlementFlag,
			string nameFlag,
			string icFlag,
			string contactFlag,
			bool logonErrorFound = false)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			LogonSuccess = isLogonSuccess;
			NetworkTimeout = networkTimeout;
			IsValidAuthentication = isValidAuthentication;
			LogonErrorFound = logonErrorFound;
			OperationFlag = operationFlag;
			OperationTimeFrom = operationTimeFrom;
			OperationTimeTo = operationTimeTo;
			SettlementTime = settlementTime;
			SettlementFlag = settlementFlag;
			NameFlag = nameFlag;
			ICFlag = icFlag;
			ContactFlag = contactFlag;
		}

		public void Dispose()
		{

		}
	}
}
