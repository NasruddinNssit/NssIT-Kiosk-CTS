using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    public class TimeFilterEventArgs : EventArgs
    {
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public TimeFilterEventArgs(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
