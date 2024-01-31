using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage
{
	public class TextMessageEventArgs : EventArgs
	{
		public string Message { get; set; }
		public DateTime Time { get; set; }
	}
}
