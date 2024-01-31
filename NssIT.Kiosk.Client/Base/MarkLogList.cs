using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Log.DB.MarkingLog.TriggerNTimeInterval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base
{
    public class MarkLogList
    {
        private static MarkLogList LogList = new MarkLogList();

        private TriggerTimeIntervalMarkingLog _cardMarkingLog = null;

        private MarkLogList() { }

        public static MarkLogList GetLogList() => LogList;

        public MarkLogList ActivateCardMarkingLog()
        {
            _cardMarkingLog = MarkingLogMaster.ClientCardMarkingLog;
            return this;
        }

        public void QuitMarkingLog()
        {
            try
            {
                MarkingLogMaster.QuitAllLog();
            }
            catch { }
        }

        public TriggerTimeIntervalMarkingLog CreditCard
        {
            get
            {
                if (_cardMarkingLog != null)
                    return _cardMarkingLog;

                else
                    throw new Exception("Error. Credit Card Marking Log has not been activated.");
            }
        }
    }
}
