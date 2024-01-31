using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command
{
	public interface IAccessDBCommand
	{
		AccessDBCommandCode CommandCode { get; }

		string ProcessId { get; }
		Guid? NetProcessId { get; }

		bool ProcessDone { get; set; }

		bool HasCommand { get; }
	}
}
