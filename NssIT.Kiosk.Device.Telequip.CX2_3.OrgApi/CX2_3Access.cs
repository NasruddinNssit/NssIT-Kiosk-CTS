using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Telequip.CX2_3.OrgApi
{
	public class CX2_3Access
	{
		private const string LogChannel = "CX2_3Access";
		private const double _minimumAmount = 0.1;
		private const int _maxRefundNoteCount = 5;

		public const int SaleMaxWaitingSec = 360; /* 5.30 minutes */

		private bool? _isB2BValid = null; /* null: not ready; false: Shutdown; true: ready */
		private bool _isRaisedCompleteCallbackEvent = false;
		private bool _cancelRequested = false;

		private Thread _threadWorker = null;

		public CX2_3Access()
		{
			_threadWorker = new Thread(new ThreadStart(B2BProcessThreadWorking));
			_threadWorker.SetApartmentState(ApartmentState.STA);
			_threadWorker.IsBackground = true;
			_threadWorker.Start();
		}

		private bool _disposed = false;
		public void Dispose()
		{
			_disposed = true;
		}

		public bool IsCashMachineReady
		{
			get
			{
				if (_disposed)
					throw new Exception("Payment Device is shutting down;");

				else if (_isB2BValid == false)
					throw new Exception("Payment Device is shutting down; Sect-01");

				return (_isB2BValid == true) ? true : false;
			}
		}

		#region ----- B2BProcessThreadWorking -----
		DateTime _endTime = DateTime.Now;
		private void B2BProcessThreadWorking()
		{

		}

		#endregion
	}
}
