using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Data
{
	[Serializable]
	public class B2BCassetteInfo
	{
		public int CassetteNo { get; set; } = -1;
		public int BillType { get; set; } = -1;
		public int DigitBillType { get; set; } = -1;
		public int BillQty { get; set; } = -1;
		public bool IsCassettePresence { get; set; } = false;
		public bool IsCassetteFull { get; set; } = false;
		public bool IsEscrowEnable { get; set; } = false;
	}
}
