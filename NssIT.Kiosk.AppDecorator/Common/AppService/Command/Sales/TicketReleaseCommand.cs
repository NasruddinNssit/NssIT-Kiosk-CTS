using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class TicketReleaseCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.TicketReleaseRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		///// Parameters

		public string TransactionNo { get; private set; } = null;

		public TicketReleaseCommand(string processId, Guid? netProcessId, string transactionNo)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
			TransactionNo = transactionNo;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}
