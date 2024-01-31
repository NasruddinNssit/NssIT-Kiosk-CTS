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
	public class UIDepartSeatListRequest : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.DepartSeatListRequest;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public DateTime? DepartPassengerDate { get; private set; } = null;
		public string DepartTripId { get; private set; } = null;
		public string DepartTripNo { get; private set; } = null;
		/// <summary>
		/// Passenger Depart Date refer to his/her station; Same as DepartPassengerDate
		/// </summary>
		public string DepartDate { get; private set; } = null;
		public string DepartVehicleTripDate { get; private set; } = null;
		public string DepartFromStationCode { get; private set; } = null;
		public string DepartToStationCode { get; private set; } = null;
		public short DepartTimePosi { get; private set; } = -1;

		public UIDepartSeatListRequest(string processId, DateTime timeStamp, 
			DateTime departPassengerDate, string departTripId, string departTripNo, string departDate, string departVehicleTripDate, 
			string departFromStationCode, string departToStationCode, short departTimePosi)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			DepartPassengerDate = departPassengerDate;
			DepartTripId = departTripId;
			DepartTripNo = departTripNo;
			DepartDate = departDate;
			DepartVehicleTripDate = departVehicleTripDate;

			DepartFromStationCode = departFromStationCode;
			DepartToStationCode = departToStationCode;
			DepartTimePosi = departTimePosi;
		}

		public void Dispose()
		{

		}
	}
}

