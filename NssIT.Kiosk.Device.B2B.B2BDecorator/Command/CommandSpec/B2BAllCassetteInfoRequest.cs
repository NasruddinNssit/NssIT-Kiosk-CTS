using System;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Command.CommandSpec
{
	public class B2BAllCassetteInfoRequest : IB2BCommand
	{
		public B2BCommandCode CommandCode => B2BCommandCode.AllCassetteInfoRequest;

		public string ProcessId { get; private set; }
		public string DocNumbers { get; set; }

		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != B2BCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;
				
		public B2BAllCassetteInfoRequest(string processId, Guid? netProcessId)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
		}

		public void Dispose()
		{

		}
	}
}
