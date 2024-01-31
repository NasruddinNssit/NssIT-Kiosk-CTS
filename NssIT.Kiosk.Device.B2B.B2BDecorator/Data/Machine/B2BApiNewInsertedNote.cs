using NssIT.Kiosk.AppDecorator.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Data.Machine
{
	public class B2BApiNewInsertedNote : IMachineData
	{
		public DateTime TimeStamp { get; private set; }

		public string Message { get; set; } = null;
		public string ErrorMessage { get; set; } = null;

		public int DigitBillType { get; set; } = 0;

		public B2BApiNewInsertedNote()
		{
			TimeStamp = DateTime.Now;
		}
	}
}
