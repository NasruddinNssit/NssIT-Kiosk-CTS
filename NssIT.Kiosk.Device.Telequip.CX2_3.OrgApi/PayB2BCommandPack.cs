using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi 
{
	public class PayCX2_3CommandPack
	{
		public string ProcessId { get; set; }
		public OperationCommand Command { get; set; } = OperationCommand.New;
		public decimal DispenseAmount { get; set; }
		
		public string DocNumbers { get; set; }
		public bool ProcessDone { get; set; } = false;

		public int MaxWaitingSec { get; set; } = 300;

		public bool HasCommand { get { return (! (Command == OperationCommand.New) ); } }

		public void Reset()
		{
			ProcessId = null;
			Command = OperationCommand.New;
			DispenseAmount = 0.00M;
		}
	}
}
