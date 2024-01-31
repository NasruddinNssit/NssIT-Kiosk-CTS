using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment.ErrorNode
{
	public class UnableRefundException : Exception 
	{
		public UnableRefundException(string errMsg = "Unable to refund exception") : base(errMsg) { }
		public UnableRefundException(string errMsg, Exception ex) : base(errMsg, ex) { }
	}
}
