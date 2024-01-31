using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class DepartSeatListCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.DepartSeatListRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//string date, string from, string to

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

		public DepartSeatListCommand(string processId, Guid? netProcessId,
			DateTime departPassengerDate, string departTripId, string departTripNo, string departDate, string departVehicleTripDate,
			string departFromStationCode, string departToStationCode, short departTimePosi)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

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
			NetProcessId = null;
		}
	}
}
