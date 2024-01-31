using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Instruction
{
	public enum UICashMachineInst
	{
		RequestMachineStatusSummary = CommInstruction.UICashMachRequestMachineStatusSummary,
		MachineStatusSummary = CommInstruction.UICashMachineStatusSummary,
		CashMachineStatus = CommInstruction.UICashMachineStatus
	}
}
