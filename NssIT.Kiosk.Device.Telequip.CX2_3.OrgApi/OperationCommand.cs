using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
{
	public enum OperationCommand
	{
		New = 0,
		Initiate = 1,
		Dispense = 2,
		GetDispensePossibility = 3,

		CloseMachine = 1000
	}
}
