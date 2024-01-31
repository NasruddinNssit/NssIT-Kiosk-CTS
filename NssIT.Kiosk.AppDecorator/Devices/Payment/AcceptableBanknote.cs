using NssIT.Kiosk.AppDecorator.Devices.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public class AcceptableBanknote : IInProgressMsgObj
	{
		private Dictionary<string, Banknote> _banknoteList = new Dictionary<string, Banknote>();

		public bool AddBankNote(string currency, int value)
		{
			string keyStr = $@"{currency?.ToUpper()}-{value}";
			Banknote testNote = null;

			if (_banknoteList.TryGetValue(keyStr, out testNote) == false)
			{
				_banknoteList.Add(keyStr, new Banknote() { Currency = currency?.ToUpper(), Value = value });
				return true;
			}
			
			return false;
		}

		public Banknote[] AcceptNoteArray
		{
			get
			{
				Dictionary<string, Banknote>.Enumerator list = _banknoteList.GetEnumerator();
				List<Banknote> newList = new List<Banknote>();

				while(list.MoveNext())
					if ((list.Current.Value) != null)
						newList.Add(list.Current.Value.Duplicate());
				
				return newList.ToArray();
			}
		}

		public void Dispose()
		{
			_banknoteList.Clear();
		}
	}
}
