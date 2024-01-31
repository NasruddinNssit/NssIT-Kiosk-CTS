using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Instruction
{
	public enum UIVisibility
	{
		VisibleNotChanged = CommInstruction.NotChanged,
		VisibleEnabled = CommInstruction.VisibleEnabled,
		VisibleDisabled = CommInstruction.VisibleDisabled
	}
}