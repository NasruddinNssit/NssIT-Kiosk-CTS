using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
	[Serializable]
	public class SeatInfo : IDisposable
	{
		private const string _seatNoPrefix = "#SeatInfo#";
		private const string _ignoreSeatNoPrefix = "#Ignored#";
		/// <summary>
		/// Format #SeatInfo#CCCRRR_<DB seat ID> ; "#SeatInfo#" is prefix; CCC is 3 digits column numbers; RRR is 3 digits row numbers ; A "_" is a space ;
		/// </summary>
		public string Id { get; private set; } = null;
		/// <summary>
		/// Id refer to database
		/// </summary>
		public string ActualId { get; private set; } = null; 
		public string SeatDesc { get; private set; } = null;
		public string SeatType { get; private set; } = null;
		public int ColumnInx { get; private set; } = -1;
		public int RowInx { get; private set; } = -1;
		public decimal Price { get; private set; } = 0.0M;
		public SeatState StateOfSeat { get; set; } = SeatState.Blank;

		/// <summary>
		/// </summary>
		/// <param name="columnInx"></param>
		/// <param name="rowInx"></param>
		/// <param name="actualId">A null or BLANK will indicate an invalid seat id</param>
		/// <param name="stateOfSeat"></param>
		public SeatInfo(int columnInx, int rowInx, string actualId, string seatDesc, string seatType, SeatState stateOfSeat, decimal price = 1.00M)
		{
			ColumnInx = columnInx;
			RowInx = rowInx;
			ActualId = (actualId ?? "").Trim();
			SeatDesc = (seatDesc ?? "").Trim();
			SeatType = (seatType ?? "").Trim();
			StateOfSeat = stateOfSeat;
			Price = price;

			if ((ActualId.Length > 0) && (SeatDesc.Length > 0))
			{
				Id = $@"{_seatNoPrefix}{ColumnInx:000}{RowInx:000}_{ActualId}";
			}
			else
			{
				Id = $@"{_ignoreSeatNoPrefix}{ColumnInx:000}{RowInx:000}_{ActualId}";
				StateOfSeat = SeatState.Blank;
			}
		}

		public static bool IsSeatIdValid(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
				return false;

			if (id.IndexOf(_seatNoPrefix) == 0)
			{
				return true;
			}
			else
				return false;
		}

		public void Dispose()
		{ }
	}
}
