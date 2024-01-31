using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
    public class DepartTripListCommand : IAccessDBCommand, IDisposable
    {
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.DepartTripListRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//string date, string from, string to

		public DateTime PassengerTravelDate { get; private set; } = DateTime.Now;
		public string FromStationCode { get; private set; } = null;
		public string ToStationCode { get; private set; } = null;

		public DepartTripListCommand(string processId, Guid? netProcessId, DateTime passengerTravelDate, string fromStationCode, string toStationCode)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
			PassengerTravelDate = passengerTravelDate;
			FromStationCode = fromStationCode;
			ToStationCode = toStationCode;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}