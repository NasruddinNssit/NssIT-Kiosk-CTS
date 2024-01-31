using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Data
{
	public struct BillTable
	{
		/// <summary>
		/// Bill Type
		/// </summary>
		public int BillType;
		/// <summary>
		/// Digit of bill type
		/// </summary>
		public int DigitBillType;

		/// <summary>
		/// Country code
		/// </summary>
		public string CountryCode;
	}
}
