using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage
{
	public class KeyboardEventArgs : EventArgs
	{
		public string KeyString { get; private set; } = null;
		public KeyCat KyCat { get; private set; } = KeyCat.NormalChar;

		public KeyboardEventArgs(string chr)
		{
			KeyString = chr;
			KyCat = KeyCat.NormalChar;
		}

		public KeyboardEventArgs(KeyCat kyCat)
		{
			KyCat = kyCat;
		}

		/// <summary>
		/// Key Category
		/// </summary>
		public enum KeyCat
		{
			NormalChar = 0,
			Enter = 1,
			Space = 2,
			Backspace = 3
		}
	}
}
