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
	public class UISalesCardSettlementSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.CardSettlementSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		//public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string HostNo { get; set; }
		public string BatchNumber { get; set; }
		public string BatchCount { get; set; }
		public decimal BatchCurrencyAmount { get; set; }
		public string StatusCode { get; set; }
		public string MachineId { get; set; }
		public string ErrorMessage { get; set; }

		public UISalesCardSettlementSubmission(string processId, DateTime timeStamp,
			string hostNo, string batchNumber, string batchCount, decimal batchCurrencyAmount, 
			string statusCode, string machineId, string errorMessage)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			HostNo = hostNo;
			BatchNumber = batchNumber;
			BatchCount = batchCount;
			BatchCurrencyAmount = batchCurrencyAmount;
			StatusCode = statusCode;
			MachineId = machineId;
			ErrorMessage = errorMessage;
		}

		public void Dispose()
		{

		}
	}
}