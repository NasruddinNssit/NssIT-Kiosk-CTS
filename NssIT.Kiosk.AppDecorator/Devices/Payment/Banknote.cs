using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	[Serializable]
	public class Banknote
	{
		public string Currency { get; set; }
		public int Value { get; set; }

		public Banknote Duplicate()
		{
			return new Banknote() { Currency = this.Currency, Value = this.Value };
		}
	}
}