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
	public class UIDepartTripSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.DepartTripSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public DateTime DepartPassengerDate { get; set; } = DateTime.MinValue;
		public string DepartPassengerDepartTime { get; set; } = null;
		public string DepartCurrency { get; set; } = null;
		public string DepartOperatorDesc { get; set; } = null;
		public string DepartOperatorLogoUrl { get; set; } = null;
		public decimal DepartPrice { get; set; } = 0M;
		public string DepartTripId { get; set; } = null;
		public string DepartVehicleTripDate { get; set; } = null;
		public string DepartTripNo { get; set; } = null;
		public short DepartTimePosi { get; set; } = -1;
		public string DepartRouteDetail { get; set; } = null;
		public string DepartEmbed { get; set; } = null;
		public string DepartPassengerActualFromStationCode { get; set; } = null;
		public string DepartPassengerActualToStationCode { get; set; } = null;
		public decimal DepartInsurance { get; set; } = 0M;

		public UIDepartTripSubmission(string processId, DateTime timeStamp,
			DateTime departPassengerDate,
			string departPassengerDepartTime, string departCurrency,
			string departOperatorDesc, string departOperatorLogoUrl,
			decimal departPrice,
			string departTripId, string departVehicleTripDate,
			string departTripNo, short departTimePosi,
			string departRouteDetail, string departEmbed,
			string departPassengerActualFromStationCode, string departPassengerActualToStationCode,
			decimal departInsurance)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			DepartPassengerDate = departPassengerDate;
			DepartPassengerDepartTime = departPassengerDepartTime;
			DepartCurrency = departCurrency;
			DepartOperatorDesc = departOperatorDesc;
			DepartOperatorLogoUrl = departOperatorLogoUrl;
			DepartPrice = departPrice;
			DepartTripId = departTripId;
			DepartVehicleTripDate = departVehicleTripDate;
			DepartTripNo = departTripNo;
			DepartTimePosi = departTimePosi;
			DepartRouteDetail = departRouteDetail;
			DepartEmbed = departEmbed;
			DepartPassengerActualFromStationCode = departPassengerActualFromStationCode;
			DepartPassengerActualToStationCode = departPassengerActualToStationCode;
			DepartInsurance = departInsurance;
		}

		public void Dispose() { }
	}
}
