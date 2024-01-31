using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.Reports
{
    public class PDFTools
    {
        private static string _logChannel = "Payment";

        //For whatever reason, sometimes adobe likes to be a stage 5 clinger.
        //So here we kill it with fire.
        public static bool KillAdobe(string name)
        {
            try
            {
                foreach (Process clsProcess in Process.GetProcesses().Where(
                         clsProcess => clsProcess.ProcessName.StartsWith(name)))
                {
                    clsProcess.Kill();
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}