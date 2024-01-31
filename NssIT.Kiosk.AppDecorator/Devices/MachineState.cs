using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices
{
	public enum MachineState
	{
		[Description("New")]
		New = -1,               /* Mean this status is new and not yet assigned with any status */

		[Description("NotFound")]
		NotFound = 0,			/* Device is not found */

		[Description("Ready")] /* Ready to used; In used */
		Ready = 1,

		[Description("Initiation")]
		Initiation = 20,

		[Description("MachineMalFunctionError")]
		MachineMalFunction = 999000,
	}
}
