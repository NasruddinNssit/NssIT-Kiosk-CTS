using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator
{
    public static class NssITThreadExtension
    {
        /// <summary>
        /// </summary>
        /// <param name="dState"></param>
        /// <param name="expectedOneState">Must refer to only one Thread State</param>
        /// <returns></returns>
        public static bool IsState(this ThreadState dState, ThreadState expectedOneState)
        {
            if (expectedOneState == dState)
                return true;

            // Note : For ThreadState.Running, (expectedOneState & ThreadState.Running) will always return ThreadState.Running
            else if (expectedOneState == ThreadState.Running)
                return false;

            else
                return ((dState & expectedOneState) == expectedOneState);
        }

        /// <summary>
        /// </summary>
        /// <param name="dState"></param>
        /// <param name="expectedOneStateList">Must refer to only one Thread State for each parameter array</param>
        /// <returns></returns>
        public static bool IsStateInList(this ThreadState dState, params ThreadState[] expectedOneStateList)
        {
            if ((expectedOneStateList is null) || (expectedOneStateList.Length == 0))
                return false;

            foreach (ThreadState stt in expectedOneStateList)
            {
                if (dState.IsState(stt) == true)
                    return true;
            }

            return false;
        }
    }
}
