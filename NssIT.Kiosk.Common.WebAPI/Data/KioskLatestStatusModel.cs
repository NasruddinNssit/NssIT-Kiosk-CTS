using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data
{
    [Serializable]
    public class KioskLatestStatusModel
    {
        public string MachineId { get; set; }
        public DateTime LastUpdateDateTime { get; set; }
        public DateTime MachineLocalDateTime { get; set; }
        public long MachineLocalDateTimeTicks { get; set; }
        public KioskCheckingCode CheckingCode { get; set; }
        public string CheckingName { get; set; }

        public string CheckingDescription { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string Remark { get; set; }

        public KioskStatusRemarkType RemarkType { get; set; }
    }
}
