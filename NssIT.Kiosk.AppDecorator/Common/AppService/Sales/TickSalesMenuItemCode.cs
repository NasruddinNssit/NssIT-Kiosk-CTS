using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
	public enum TickSalesMenuItemCode
	{
		Non = 0,
		FromStation = 1,
		ToStation = 2,

		DepartDate = 3,
		DepartOperator = 4,
		DepartSeat = 5,

		ReturnDate = 6,
		ReturnOperator = 7,
		ReturnSeat = 8,

		Passenger = 9,
		Payment = 10,

		AfterPayment = 11
	}
}