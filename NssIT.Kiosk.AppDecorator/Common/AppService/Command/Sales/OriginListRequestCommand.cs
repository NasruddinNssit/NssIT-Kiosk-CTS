using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	/// <summary>
	/// Sales Get Destination List Command
	/// </summary>
	public class OriginListRequestCommand : IAccessDBCommand, IDisposable 
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.OriginListRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;
		
		public OriginListRequestCommand(string processId, Guid? netProcessId)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}
