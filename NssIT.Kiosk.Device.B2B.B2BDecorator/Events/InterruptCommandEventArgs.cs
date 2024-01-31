using System;

using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Events
{
	/// <summary>
	/// Used to interrupt a B2B process. Like .. payment process.
	/// </summary>
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
