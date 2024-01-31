using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Menu
{
	//public enum TickSalesMenuItemCode
	//{
	//	FromStation = 0,
	//	ToStation = 1,

	//	DepartDate = 2,
	//	DepartOperator = 3,
	//	DepartSeat = 4,

	//	ReturnDate = 5,
	//	ReturnOperator = 6,
	//	ReturnSeat = 7,

	//	Passenger = 8,
	//	Payment = 9
	//}
	public class MenuEditTemporarySwitch
    {
		//IMenuExec
		private Dictionary<TickSalesMenuItemCode, bool> _menuItemAvailabilityHistory = new Dictionary<TickSalesMenuItemCode, bool>();

		private bool _menuEditAlreadyDisabled = false;
		public void DisableMenuEditButton(IMenuExec menuPage)
		{
			if (_menuEditAlreadyDisabled == false)
			{
				_menuEditAlreadyDisabled = true;

				RecordAllMenuItemAvailability(menuPage);

				menuPage.IsEditAllowedDepartDate = false;
				menuPage.IsEditAllowedDepartOperator = false;
				menuPage.IsEditAllowedDepartSeat = false;
				menuPage.IsEditAllowedPassenger = false;
				menuPage.IsEditAllowedReturnDate = false;
				menuPage.IsEditAllowedReturnOperator = false;
				menuPage.IsEditAllowedReturnSeat = false;
				menuPage.IsEditAllowedToStation = false;
				menuPage.IsEditAllowedFromStation = false;
			}
		}

		private void RecordAllMenuItemAvailability(IMenuExec menuPage)
		{
			_menuItemAvailabilityHistory.Clear();

			_menuItemAvailabilityHistory.Add(TickSalesMenuItemCode.DepartDate, menuPage.IsEditAllowedDepartDate);
			_menuItemAvailabilityHistory.Add(TickSalesMenuItemCode.DepartOperator, menuPage.IsEditAllowedDepartOperator);
			_menuItemAvailabilityHistory.Add(TickSalesMenuItemCode.DepartSeat, menuPage.IsEditAllowedDepartSeat);
			_menuItemAvailabilityHistory.Add(TickSalesMenuItemCode.Passenger, menuPage.IsEditAllowedPassenger);

			_menuItemAvailabilityHistory.Add(TickSalesMenuItemCode.ReturnDate, menuPage.IsEditAllowedReturnDate);
			_menuItemAvailabilityHistory.Add(TickSalesMenuItemCode.ReturnOperator, menuPage.IsEditAllowedReturnOperator);
			_menuItemAvailabilityHistory.Add(TickSalesMenuItemCode.ReturnSeat, menuPage.IsEditAllowedReturnSeat);
			_menuItemAvailabilityHistory.Add(TickSalesMenuItemCode.ToStation, menuPage.IsEditAllowedToStation);
			_menuItemAvailabilityHistory.Add(TickSalesMenuItemCode.FromStation, menuPage.IsEditAllowedFromStation);
		}

		public void RecoverMenuEditButton(IMenuExec menuPage)
		{
			if (_menuEditAlreadyDisabled)
			{
				_menuEditAlreadyDisabled = false;

				if (_menuItemAvailabilityHistory.TryGetValue(TickSalesMenuItemCode.FromStation, out bool val4))
					menuPage.IsEditAllowedFromStation = val4;

				if (_menuItemAvailabilityHistory.TryGetValue(TickSalesMenuItemCode.ToStation, out bool val9))
					menuPage.IsEditAllowedToStation = val9;

				if (_menuItemAvailabilityHistory.TryGetValue(TickSalesMenuItemCode.DepartDate, out bool val1))
					menuPage.IsEditAllowedDepartDate = val1;

				if (_menuItemAvailabilityHistory.TryGetValue(TickSalesMenuItemCode.DepartOperator, out bool val2))
					menuPage.IsEditAllowedDepartOperator = val2;

				if (_menuItemAvailabilityHistory.TryGetValue(TickSalesMenuItemCode.DepartSeat, out bool val3))
					menuPage.IsEditAllowedDepartSeat = val3;

				if (_menuItemAvailabilityHistory.TryGetValue(TickSalesMenuItemCode.Passenger, out bool val5))
					menuPage.IsEditAllowedPassenger = val5;

				if (_menuItemAvailabilityHistory.TryGetValue(TickSalesMenuItemCode.ReturnDate, out bool val6))
					menuPage.IsEditAllowedReturnDate = val6;

				if (_menuItemAvailabilityHistory.TryGetValue(TickSalesMenuItemCode.ReturnOperator, out bool val7))
					menuPage.IsEditAllowedReturnOperator = val7;

				if (_menuItemAvailabilityHistory.TryGetValue(TickSalesMenuItemCode.ReturnSeat, out bool val8))
					menuPage.IsEditAllowedReturnSeat = val8;
			}
		}
	}
	
}
