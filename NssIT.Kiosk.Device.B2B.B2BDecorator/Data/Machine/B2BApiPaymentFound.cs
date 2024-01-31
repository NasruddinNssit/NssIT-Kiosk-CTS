using NssIT.Kiosk.AppDecorator.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Data.Machine
{
	public class B2BApiPaymentFound : IMachineData
	{
		public DateTime TimeStamp { get; private set; }

		public string Message { get; set; } = null;
		public string ErrorMessage { get; set; } = null;

		public B2BApiPaymentFound()
		{
			TimeStamp = DateTime.Now;
		}
	}
}
