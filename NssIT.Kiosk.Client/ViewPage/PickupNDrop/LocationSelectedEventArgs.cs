using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.PickupNDrop
{
	public class LocationSelectedEventArgs : EventArgs
	{
		public LocationViewRow Station { get; private set; }

		public LocationSelectedEventArgs(LocationViewRow stationViewRow)
		{
			Station = stationViewRow;
		}
	}
}
