using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WFmRunThreadManTest
{
    public class TestWorkingThread
    {
        public Guid WorkId { get; private set; } = Guid.NewGuid();
        public int WorkingPeriodSec { get; private set; } = 0;

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private DbLog _log = DbLog.GetDbLog();

        public TestWorkingThread(int workingPeriodSec)
        {
            WorkingPeriodSec = (workingPeriodSec >= 0) ? workingPeriodSec : 0;
        }

        public void StartThreadWorking()
        {
            try
            {
                DateTime expiredTime = DateTime.Now.AddSeconds(WorkingPeriodSec);
                _log.LogText("TestWorkingThread", WorkId.ToString(), $@"Start working; {expiredTime: dd HH:mm:ss.fff}", "A01", "TestWorkingThread.StartThreadWorking");

                do
                {
                    Thread.Sleep(5 * 1000);

                    for (int inx = 0; inx < 1_000; inx++)
                    {
                        Task.Delay(1).Wait();
                    }

                } while (expiredTime.Ticks > DateTime.Now.Ticks);
            }
            catch (Exception ex)
            {
                _log.LogError("TestWorkingThread", WorkId.ToString(), ex, "EX01", "TestWorkingThread.StartThreadWorking");
            }
            finally
            {
                _log.LogText("TestWorkingThread", WorkId.ToString(), "End of Working", "A100", "TestWorkingThread.StartThreadWorking");
            }
        }
    }
}