using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices
{
	public interface IMachineData
	{
		DateTime TimeStamp { get; }

		string Message { get; set; }
		string ErrorMessage { get; set; }
	}
}
