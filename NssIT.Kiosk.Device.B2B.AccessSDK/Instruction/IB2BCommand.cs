using System;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;

namespace NssIT.Kiosk.Device.B2B.AccessSDK.Instruction
{
	public interface IB2BCommand : IDisposable 
	{
		/// <summary>
		/// One command code only serve with only one Parameter type.
		/// </summary>
		B2BCommandCode CommandCode { get; }

		string ProcessId { get; }
		Guid? NetProcessId { get; }

		bool ProcessDone { get; set; }

		bool HasCommand { get; }
	}
}