using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices
{
	public interface IMachineEventArgs
	{
		DateTime TimeStamp { get; set; }

		string Message { get; set; }
		IMachineData MachData { get; set; }
		string ErrorMessage { get; set; }
	}
}
