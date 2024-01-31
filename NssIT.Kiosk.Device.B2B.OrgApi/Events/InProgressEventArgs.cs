using NssIT.Kiosk.AppDecorator.Devices.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.OrgApi.Events
{
	public class InProgressEventArgs : EventArgs, IInProgressEventArgs 
	{
		public InProgressEventArgs() { }
		/// <summary>
		/// When PaymentStatus = PaymentFound, and 
		/// </summary>
		public string Message { get; set; } = "Work in progress";
		public IInProgressMsgObj MsgObj { get; set; } = null;
		public Exception Error { get; set; } = null;
		public PaymentProgressStatus PaymentStatus { get; set; } = PaymentProgressStatus.New;

		public string PaymentStatusDesc
		{
			get
			{
				return Enum.GetName(typeof(PaymentProgressStatus), this.PaymentStatus);
			}
		}

		public void Dispose()
		{
			Error = null;
		}
	}
}
