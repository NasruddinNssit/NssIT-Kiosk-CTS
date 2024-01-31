using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common
{
    public class NssITThreadTools
    {
        public static void EndThread(Thread threadWorker)
        {
            if (threadWorker is null)
                return;

            if (
                ((threadWorker.ThreadState & ThreadState.Aborted) == ThreadState.Aborted)
                )
            {
                /*By Pass*/
            }
            else
            {
                try
                {
                    threadWorker.Abort();
                    Thread.Sleep(50);
                }
                catch { }
            }
        }

        public static void WaitThreadToFinish(Thread threadWorker, int waitDelaySec)
        {
            if (threadWorker is null)
                return;
            try
            {
                threadWorker.IsBackground = true;
                threadWorker.Start();

                // Wait for answer
                DateTime endWaitTime = DateTime.Now.AddSeconds(waitDelaySec);

                while (endWaitTime.Ticks > DateTime.Now.Ticks)
                {
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();

                    if (((threadWorker.ThreadState & ThreadState.Aborted) == ThreadState.Aborted)
                        ||
                        ((threadWorker.ThreadState & ThreadState.Stopped) == ThreadState.Stopped)
                        )
                    {
                        break;
                    }
                }
            }
            catch { }
        }

        //public static bool IsThreadState(ThreadState expectedOneState, ThreadState stateData)
        //{
        //    if (expectedOneState == stateData)
        //        return true;
        //    else if (expectedOneState == ThreadState.Running)
        //        return false;
        //    else
        //        return ((stateData & expectedOneState) == expectedOneState);
        //}

        public static string ReadStateToString(ThreadState threadStatus)
        {
            string retStr = "";

            if (threadStatus == ThreadState.Running)
                return Enum.GetName(typeof(ThreadState), ThreadState.Running);

            if ((threadStatus & ThreadState.Aborted) == ThreadState.Aborted)
                retStr += Enum.GetName(typeof(ThreadState), ThreadState.Aborted) + "|";

            if ((threadStatus & ThreadState.AbortRequested) == ThreadState.AbortRequested)
                retStr += Enum.GetName(typeof(ThreadState), ThreadState.AbortRequested) + "|";

            if ((threadStatus & ThreadState.Background) == ThreadState.Background)
                retStr += Enum.GetName(typeof(ThreadState), ThreadState.Background) + "|";

            if ((threadStatus & ThreadState.Stopped) == ThreadState.Stopped)
                retStr += Enum.GetName(typeof(ThreadState), ThreadState.Stopped) + "|";

            if ((threadStatus & ThreadState.StopRequested) == ThreadState.StopRequested)
                retStr += Enum.GetName(typeof(ThreadState), ThreadState.StopRequested) + "|";

            if ((threadStatus & ThreadState.Suspended) == ThreadState.Suspended)
                retStr += Enum.GetName(typeof(ThreadState), ThreadState.Suspended) + "|";

            if ((threadStatus & ThreadState.SuspendRequested) == ThreadState.SuspendRequested)
                retStr += Enum.GetName(typeof(ThreadState), ThreadState.SuspendRequested) + "|";

            if ((threadStatus & ThreadState.Unstarted) == ThreadState.Unstarted)
                retStr += Enum.GetName(typeof(ThreadState), ThreadState.Unstarted) + "|";

            if ((threadStatus & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin)
                retStr += Enum.GetName(typeof(ThreadState), ThreadState.WaitSleepJoin) + "|";

            if (retStr.Length > 0)
                retStr = retStr.Substring(0, (retStr.Length - 1));

            return retStr;
        }
    }
}
