using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Command;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi 
{
	public class PayCX2_3CommandPack
	{
		public string ProcessId { get; set; }
		public CX2D3CommandCode Command { get; set; } = CX2D3CommandCode.New;
		public decimal DispenseAmount { get; set; }
		
		public string DocNumbers { get; set; }
		public bool ProcessDone { get; set; } = false;

		public int MaxWaitingSec { get; set; } = 300;

		public bool HasCommand { get { return (! (Command == CX2D3CommandCode.New) ); } }

		public void Reset()
		{
			ProcessId = null;
			Command = CX2D3CommandCode.New;
			DispenseAmount = 0.00M;
		}
	}
}
