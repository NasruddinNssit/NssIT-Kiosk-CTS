using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class PassengerInfo
    {
        public string Name { get; private set; } = null;
        public string Id { get; private set; } = null;
        public string Contact { get; private set; } = null;

        public string SeatDesn { get; private set; }
        public string SeatId { get; private set; }
        public string SeatType { get; private set; }

        public PassengerInfo(string name, string id, string contact, string seatDesn, string seatId, string seatType)
        {
            Name = name?.Trim();
            Id = id?.Trim();
            Contact = contact?.Trim();
            SeatDesn = seatDesn?.Trim();
            SeatId = seatId?.Trim();
            SeatType = seatType?.Trim();
        }
    }
}
