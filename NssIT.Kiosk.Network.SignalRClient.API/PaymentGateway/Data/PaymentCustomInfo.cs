using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data
{
    public class PaymentCustomInfo
    {
        /// <summary>
        /// Unique Id used to identify a Machine like Kiosk. This id normally in Guid form. 
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// An Id that used to identify a machine in network, like SignalR Connection Id.
        /// </summary>
        public string MachineNetworkID { get; set; }

        /// <summary>
        /// An Id that used to identify a machine in database. Like Kiosk Id
        /// </summary>
        public string MachineCode { get; set; }

        /// <summary>
        /// Use to record new transaction time, like new sale transaction time.
        /// </summary>
        public DateTime CreationLocalTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Transaction amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Number like Booking No., Transaction No., Invoice No., and etc.
        /// </summary>
        public string DocumentNo { get; set; }

        public string StationCode { get; set; }
    }
}
