using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator.Devices.Payment;

namespace NssIT.Kiosk.Device.B2B.AccessSDK.Error
{
	public class B2BMalfunctionException : Exception, IDisposable
	{
		public ICustomerPaymentInfo LastCustPaymentInfo { get; set; } = null;
		public B2BMalfunctionException(string errMsg = "B2B Malfunction Exception") : base(errMsg) { }
		public B2BMalfunctionException(string errMsg, Exception ex) : base(errMsg, ex) { }
	
		public void Dispose()
		{
			LastCustPaymentInfo = null;
		}
	}
}
