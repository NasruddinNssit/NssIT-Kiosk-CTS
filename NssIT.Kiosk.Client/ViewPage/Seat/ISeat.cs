using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.ViewPage.Trip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
	public interface ISeat
	{
		void InitDepartSeatData(UIDepartSeatListAck uiDepartSeatList, UserSession session);



		/// <summary>
		/// </summary>
		/// <param name="travalMode"></param>
		/// <param name="seatDeckArray">Array[0] away refer to lower deck for bus.</param>
		/// <param name="maximumSeatPerTrip"></param>
		/// <param name="defaultSeatPrice"></param>
		/// <param name="currency"></param>
		/// <param name="insuranceRequested"></param>
		/// <param name="insurancePrice">This price is separated from ticket price</param>

		//void InitSeatData(TripMode travalMode, SeatDeckCollection[] seatDeckArray, int maximumSeatPerTrip, string currency, bool insuranceRequested, 
		//	decimal insurance, string passengerDate,
		//	string busType, decimal terminalCharge,
		//	int tripCode, decimal adultPrice,
		//	string adultExtra, decimal adultDisc,
		//	decimal onlineQrCharge);

		TripMode TravalMode { get; }
		SeatInfo[] SelectedSeatList { get; }

		string Currency { get; }
		//bool InsuranceRequested { get; }
		decimal InsurancePrice { get; }


	}
}
