using NssIT.Kiosk.AppDecorator.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Events
{
	public class MachineEventArgs : IMachineEventArgs
	{
		public DateTime TimeStamp { get; set; }
		public string Message { get; set; } = null;
		public IMachineData MachData { get; set; } = null;
		public string ErrorMessage { get; set; } = null;

		public MachineEventArgs()
		{
			TimeStamp = DateTime.Now;
		}

		public MachineEventArgs(DateTime timeStamp)
		{
			TimeStamp = timeStamp;
		}
	}
}
