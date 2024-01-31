using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Data 
{
	/// <summary>
	/// B2B Execution Result
	/// </summary>
	public interface IB2BResultData : IDisposable
	{
		Guid RefExecutionId { get; }
	}
}
