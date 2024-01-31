using NssIT.Kiosk.AppDecorator.Devices.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Error
{
	public class CX2_3MalfunctionException : Exception, IDisposable
	{
		public ICustomerPaymentInfo LastCustPaymentInfo { get; set; } = null;
		public CX2_3MalfunctionException(string errMsg = "CX2_3 Malfunction Exception") : base(errMsg) { }
		public CX2_3MalfunctionException(string errMsg, Exception ex) : base(errMsg, ex) { }

		public void Dispose()
		{
			LastCustPaymentInfo = null;
		}
	}
}
