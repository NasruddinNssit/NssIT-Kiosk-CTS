using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
	public enum ResultStatus
	{
		New = -1,               /* Mean this status is new and not yet assigned with any status */
		UnknownState = 0,
		Success = 1,
		Fail = 2,
		NetworkTimeout = 3,
		ErrorFound = 4
	}
}
