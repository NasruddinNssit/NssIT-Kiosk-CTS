using NssIT.Kiosk.Log.DB.MarkingLog;
using NssIT.Kiosk.Log.DB.MarkingLog.TriggerNTimeInterval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Log.DB
{
    public class MarkingLogMaster
    {
        private static Lazy<TriggerTimeIntervalMarkingLog> _clientCardMarkingLogHandle = new Lazy<TriggerTimeIntervalMarkingLog>(() => new TriggerTimeIntervalMarkingLog("ClientCardMarkingLog", 20, 1));
        private static TriggerTimeIntervalMarkingLog _clientCardMarkingLog = null;

        public static TriggerTimeIntervalMarkingLog ClientCardMarkingLog
        {
            get
            {
                return _clientCardMarkingLog ?? (_clientCardMarkingLog = _clientCardMarkingLogHandle.Value);
            }
        }

        public static void QuitAllLog()
        {
            bool delayRequested = false;

            if (_clientCardMarkingLog != null)
            {
                delayRequested = true;
                _clientCardMarkingLog.SendOutstanding();
            }

            if (delayRequested)
            {
                Thread.Sleep(350);
                BaseMarkingLog.GetDbLog().Dispose();
                Thread.Sleep(350);
            }
        }
    }
}
