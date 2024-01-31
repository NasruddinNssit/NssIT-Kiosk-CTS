using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace serialCommunicationECR2
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
            Application.Run(new frmMain());
        }

        private static LibShowMessageWindow.MessageWindow _msgWnd = new LibShowMessageWindow.MessageWindow();
        public static void ShowMsg(string message)
        {
            _msgWnd.ShowMessage(message);
        }
    }
}
