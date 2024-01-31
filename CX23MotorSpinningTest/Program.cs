using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace CX23MotorSpinningTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            String thisprocessname = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
            {
                MessageBox.Show("Twice access to CX23MotorSpinningTest Application is prohibited", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
                return;
            }
            else if (Process.GetProcesses().Count(p => p.ProcessName?.ToString().Equals("NssIT.Kiosk.Client", StringComparison.InvariantCultureIgnoreCase) == true) > 0)
            {
                MessageBox.Show("Please close the NssIT.Kiosk.Client application", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
                return;
            }
            else if (Process.GetProcesses().Count(p => p.ProcessName?.ToString().Equals("NssIT.Kiosk.Server", StringComparison.InvariantCultureIgnoreCase) == true) > 0)
            {
                MessageBox.Show("Please stop the NssIT.Kiosk.Server service", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
                return;
            }
            else if (Process.GetProcesses().Count(p => p.ProcessName?.ToString().Equals("KIOSK_MNG", StringComparison.InvariantCultureIgnoreCase) == true) > 0)
            {
                MessageBox.Show("Please close the KIOSK_MNG application", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
                return;
            }

            Application.Run(new Form1());
        }
    }
}
