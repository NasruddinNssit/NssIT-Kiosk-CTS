using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class DepartSeatConfirmCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.DepartSeatConfirmRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		public string TripId { get; private set; }
		public string TripDate { get; private set; }
		public string DepartDate { get; private set; }
		public string DepartTime { get; private set; }
		public string BusType { get; private set; }
		public string FromStationCode { get; private set; }
		public string ToStationCode { get; private set; }
		public decimal AdultPrice { get; private set; }
		public string AdultExtra { get; private set; }
		public decimal AdultDisc { get; private set; }
		public decimal TerminalCharge { get; private set; }
		public decimal OnlineQRCharge { get; private set; }
		public decimal Insurance { get; private set; }
		public int TripCode { get; private set; }
		public decimal TotalAmount { get; private set; }
		public string PickLocationCode { get; private set; }
		public string PickLocationDesn { get; private set; }
		public string PickTime { get; private set; }
		public string DropLocationCode { get; private set; }
		public string DropLocationDesn { get; private set; }
		public CustSeatDetail[] PassengerSeatDetail { get; private set; }

		public DepartSeatConfirmCommand(string processId, Guid? netProcessId,
					string tripId,
					string tripDate,
					string departDate,
					string departTime,
					string busType,
					string fromStationCode,
					string toStationCode,
					decimal adultPrice,
					string adultExtra,
					decimal adultDisc,
					decimal terminalCharge,
					decimal onlineQRCharge,
					decimal insurance,
					decimal totalAmount,
					int tripCode,
					string pickLocationCode,
					string pickLocationDesn,
					string pickTime,
					string dropLocationCode,
					string dropLocationDesn,
					CustSeatDetail[] passengerSeatDetail)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
			TripId = tripId;
			TripDate = tripDate;
			DepartDate = departDate;
			DepartTime = departTime;
			BusType = busType;
			FromStationCode = fromStationCode;
			ToStationCode = toStationCode;
			AdultPrice = adultPrice;
			AdultExtra = adultExtra;
			AdultDisc = adultDisc;
			TerminalCharge = terminalCharge;
			OnlineQRCharge = onlineQRCharge;
			Insurance = insurance;
			TotalAmount = totalAmount;
			TripCode = tripCode;
			PickLocationCode = pickLocationCode;
			PickLocationDesn = pickLocationDesn;
			PickTime = pickTime;
			DropLocationCode = dropLocationCode;
			DropLocationDesn = dropLocationDesn;
			PassengerSeatDetail = passengerSeatDetail;
		}

		public void Dispose()
		{
			NetProcessId = null;
			PassengerSeatDetail = null;
		}
	}
}
