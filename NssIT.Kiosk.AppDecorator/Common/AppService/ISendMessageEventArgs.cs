using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
	interface ISendMessageEventArgs
	{
		Guid? NetProcessId { get; }
		string ProcessId { get; }
		string Message { get; set; }
		IKioskMsg KioskMessage { get; }
	}
}
