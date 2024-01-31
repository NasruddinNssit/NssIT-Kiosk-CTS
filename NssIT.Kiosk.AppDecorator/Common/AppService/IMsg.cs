using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
	public interface IMsg : IDisposable
	{
		/// <summary>
		/// Combination of Module & Instruction to form a message mapping for a specifix Message Object Type.
		/// </summary>
		AppModule Module { get; }
		string ModuleDesc { get; }

		/// <summary>
		/// Combination of Module & Instruction to form a message mapping for a specifix Message Object Type.
		/// </summary>
		CommInstruction Instruction { get; }
		string InstructionDesc { get; }

		string ProcessId { get; }
		DateTime TimeStamp { get; }
	}
}
