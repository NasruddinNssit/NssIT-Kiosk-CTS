using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
{
	public partial class Form5 : Form
	{
		public Form5()
		{
			InitializeComponent();
		}

		private Thread _threadWorker = null;
		private void btnTest_Click(object sender, EventArgs e)
		{
			_threadWorker = new Thread(new ThreadStart(ThreadExecution));
			_threadWorker.SetApartmentState(ApartmentState.STA);
			_threadWorker.IsBackground = true;
			_threadWorker.Start();
		}

		private void ThreadExecution()
		{
			(new Form4()).ShowDialog();
		}
	}
}
