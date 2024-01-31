using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
	public class StateChangedEventArgs : EventArgs
	{
		public string State { get; private set; }

		public StateChangedEventArgs(string state)
		{
			State = state;
		}
	}
}
