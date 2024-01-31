using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public class StartCashPayment : IInProgressMsgObj
	{
		public string NewProcessId { get; private set; }
		public decimal Price { get; set; }

		public StartCashPayment(string newProcessId) => NewProcessId = newProcessId;

		public void Dispose()
		{  }
	}
}