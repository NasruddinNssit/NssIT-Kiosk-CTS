using System;


namespace NssIT.Kiosk.Client.ViewPage.Seat
{
	[Serializable]
	public enum SeatState
	{
		Blank = 0,
		Driver = 1,
		Occupied = 2,
		Selected = 3,
		Available = 5,
		Invalid = 99
	}
}