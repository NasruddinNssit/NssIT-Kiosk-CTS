using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
	[Serializable]
	public class SeatDeckCollection
	{
		private int _maxRowCount = 0;
		private int _group1ColumnCount = 0;
		private int _group2ColumnCount = 0;

		public Dictionary<string, SeatInfo> SeatList { get; } = new Dictionary<string, SeatInfo>();

		public SeatDeckCollection()
		{ }

		public bool IsValidCollection
		{
			get
			{
				if (((_group1ColumnCount > 0) || (_group2ColumnCount > 0))
					&&
					(_maxRowCount > 0)
					)
				{
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="group1ColumnCount">Count is always start with 1</param>
		/// <param name="group2ColumnCount">Count is always start with 1</param>
		/// <param name="maxRowCount">Count is always start with 1</param>
		public void Init(int group1ColumnCount, int group2ColumnCount, int maxRowCount)
		{
			_group1ColumnCount = group1ColumnCount;
			_group2ColumnCount = group2ColumnCount;
			_maxRowCount = maxRowCount;
			SeatList.Clear();
		}

		public void ReadContainerDimension(out int group1ColumnCount, out int group2ColumnCount, out int maxRowCount)
		{
			group1ColumnCount = _group1ColumnCount;
			group2ColumnCount = _group2ColumnCount;
			maxRowCount = _maxRowCount;
		}

		/// <summary>
		/// </summary>
		/// <param name="columnInx">Index is always start in 0</param>
		/// <param name="rowInx">Index is always start in 0</param>
		/// <param name="actualId">Id that refer to database to identify the seat</param>
		/// <param name="seatDesc">Short Description Tag show to user</param>
		/// <param name="stateOfSeat"></param>
		public void AddSeat(int columnInx, int rowInx, string actualId, string seatDesc, string seatType, SeatState stateOfSeat, decimal price)
		{
			if (columnInx >= (_group1ColumnCount + _group2ColumnCount))
				throw new Exception("Column inx out of range at Seat Collection");
			if (rowInx >= _maxRowCount)
				throw new Exception("Row inx out of range at Seat Collection");

			SeatInfo newSeat = new SeatInfo(columnInx, rowInx, actualId, seatDesc, seatType, stateOfSeat, price);

			if (SeatList.TryGetValue(newSeat.Id, out SeatInfo seat) == false)
			{
				SeatList.Add(newSeat.Id, newSeat);
			}
		}

		public void SelectSeat(string TagId, out bool isValidSelection)
		{
			isValidSelection = false;

			if (SeatList.TryGetValue(TagId, out SeatInfo seat))
			{
				if ((seat.StateOfSeat == SeatState.Available) ||
					(seat.StateOfSeat == SeatState.Selected))
				{
					// Update Seat 
					SeatList.Remove(seat.Id);
					seat.StateOfSeat = SeatState.Selected;
					SeatList.Add(seat.Id, seat);

					isValidSelection = true;
				}
			}
		}

		public SeatState DeselectSeat(string TagId, out bool isSeatFound)
		{
			isSeatFound = false;
			SeatState sState = SeatState.Invalid;

			if (SeatList.TryGetValue(TagId, out SeatInfo seat))
			{
				isSeatFound = true;

				if ((seat.StateOfSeat == SeatState.Available) ||
					(seat.StateOfSeat == SeatState.Selected))
				{
					// Update Seat 
					SeatList.Remove(seat.Id);
					seat.StateOfSeat = SeatState.Available;
					SeatList.Add(seat.Id, seat);

					sState = SeatState.Available;
				}
			}

			return sState;
		}

		///// <summary>
		///// Return a SeatInfo when a trip has selected; Else return null; 
		///// </summary>
		///// <returns></returns>
		//public SeatInfo GetSelectedActualId()
		//{
		//	var idList = (from tt in SeatList
		//				  where tt.Value.StateOfSeat == SeatState.Selected
		//				  select tt.Value).ToArray();

		//	if (idList.Length > 0)
		//		return idList[0];
		//	else
		//		return null;
		//}
	}
}