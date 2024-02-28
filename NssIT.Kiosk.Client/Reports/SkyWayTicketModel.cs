using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Reports
{
    public class SkyWayTicketModel
    {
        //tickettype="string" skytickn="string" skyggpamnt="decimal" address1="string" address2="string" address3="string" barvalueskyway="string" skywaydatefrom="string" skywaydateto="string" linccout="string" creationdatetime="string"

        public string tickettype {  get; set; }
        public string skytickn { get; set; }
        public decimal skyggpamnt { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string barvalueskyway { get; set; }

        public string skywaydatefrom { get; set; }

        public string skywaydateto { get; set; }
        public string linccout { get; set; }

        public string creationdatetime { get; set; }

        public byte[] QrTicketData { get; set; }    
    }
}
