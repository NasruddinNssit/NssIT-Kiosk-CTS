using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public interface IMachineCommandInterrupt : IDisposable 
	{
		Guid? NetProcessId { get; }
		string ProcessID { get; }
	}
}
