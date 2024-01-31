using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.PaymentGatewayApp
{
	public interface IBTnGJob
	{
		bool IsDone { get; }
		bool? IsSuccess { get; }
		Exception Error { get; }
		DateTime CreationTime { get; }

		void SetSuccess();
		void SetFail(Exception ex);
	}
}
