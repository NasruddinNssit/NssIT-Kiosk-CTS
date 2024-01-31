using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage
{
	public class DateSelectedEventArgs : EventArgs
	{
		public DateTime SelectedDate { get; private set; }

		public DateSelectedEventArgs(DateTime selectedDate)
		{
			SelectedDate = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, 0, 0, 0, 0);
		}
	}
}
