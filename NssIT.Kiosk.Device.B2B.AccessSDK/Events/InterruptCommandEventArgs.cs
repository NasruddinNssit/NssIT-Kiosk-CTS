using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.Device.B2B.AccessSDK.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.AccessSDK.Events
{
	public class InterruptCommandEventArgs : EventArgs, IMachineCommandInterrupt
	{
		public Guid? NetProcessId { get; private set; } = null;
		public string ProcessID { get; private set; } = "-";
		public B2BCommandCode CommandCode { get; private set; } = B2BCommandCode.UnKnown;
		public IB2BCommand Command { get; private set; } = null;

		public InterruptCommandEventArgs(IB2BCommand command)
		{
			if (command != null)
			{
				NetProcessId = command.NetProcessId;
				ProcessID = command.ProcessId;
				CommandCode = command.CommandCode;
			}
			Command = command;
		}

		public void Dispose()
		{
			Command = null;
		}
	}
}
