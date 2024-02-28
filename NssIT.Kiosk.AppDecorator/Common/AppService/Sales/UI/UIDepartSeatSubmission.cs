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
	public class UIDepartSeatSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.DepartSeatSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public CustSeatDetail[] PassengerSeatDetailList { get; private set; } = null;
		public PickupNDropList PickupAndDropList { get; private set; } = null;
		public decimal DepartInsurance { get; private set; } = 0M;
		public string DepartDate { get; private set; } = null;
		public string DepartBusType { get; private set; } = null;
		public decimal DepartTerminalCharge { get; private set; } = 0M;
		public int DepartTripCode { get; private set; } = 0;
		public decimal DepartAdultPrice { get; private set; } = 0M;
		public string DepartAdultExtra { get; private set; } = null;
		public decimal DepartAdultDisc { get; private set; } = 0M;
		public decimal DepartOnlineQrCharge { get; private set; } = 0M;

		public decimal DepartSkyWayAmount { get; private set; } = 0M;

		public UIDepartSeatSubmission(string processId, DateTime timeStamp, CustSeatDetail[] custSeatDetailList, PickupNDropList pickupAndDropList,
			string departDate, string departBusType,
			decimal departInsurance, decimal departTerminalCharge,
			int departTripCode, decimal departAdultPrice,
			string departAdultExtra, decimal departAdultDisc,
			decimal departOnlineQrCharge,decimal departSkyWayAmount)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			PassengerSeatDetailList = custSeatDetailList;
			PickupAndDropList = pickupAndDropList;
			DepartDate = departDate;
			DepartBusType = departBusType;
			DepartInsurance = departInsurance;
			DepartTerminalCharge = departTerminalCharge;
			DepartTripCode = departTripCode;
			DepartAdultPrice = departAdultPrice;
			DepartAdultExtra = departAdultExtra;
			DepartAdultDisc = departAdultDisc;
			DepartOnlineQrCharge = departOnlineQrCharge;
			DepartSkyWayAmount = departSkyWayAmount;
		}

		public void Dispose()
		{ }
	}
}
