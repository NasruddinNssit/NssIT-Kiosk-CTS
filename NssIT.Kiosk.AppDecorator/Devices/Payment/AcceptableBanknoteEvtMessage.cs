using System;
using System.Collections.Generic;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Devices;

namespace NssIT.Kiosk.AppDecorator.Devices.Payment
{
	public class AcceptableBanknoteEvtMessage : IKioskMsg
	{
		public Guid? RefNetProcessId { get; set; }
		public string ProcessId { get; set; }
		public DateTime TimeStamp { get; private set; }

		public AppModule Module { get; } = AppModule.UIPayment;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }

		public CommInstruction Instruction { get; } = CommInstruction.Blank;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		private Dictionary<string, Banknote> _banknoteList = new Dictionary<string, Banknote>();

		public bool AddBankNote(string currency, int value)
		{
			TimeStamp = DateTime.Now;

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
