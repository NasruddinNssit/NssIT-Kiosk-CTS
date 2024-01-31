using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data.PostRequestParam
{
    [Serializable]
    public class KioskStatusUpdateRequest : IPostRequestParam
    {
        public string MachineId { get; set; }
        public bool IsCleanupExistingMachineStatus { get; set; } = false;
        public KioskLatestStatusModel[] LatestStatusList { get; set; }
    }
}