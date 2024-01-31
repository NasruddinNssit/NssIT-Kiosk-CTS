using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common
{
	public class WaitForInterrupt
	{
		private Thread _threadWorker = null;
		private SemaphoreSlim _asyncLock = new SemaphoreSlim(1);
		private bool _raiseInterruptFlag = false;

		public WaitForInterrupt()
		{ }

		public void Wait(TimeSpan aPeriod)
		{
			NewThreadWorker(aPeriod);
			try
			{
				_threadWorker.Join();
			}
			catch { }
		}

		public void InterruptAll()
		{
			try
			{
				_raiseInterruptFlag = true;

				if (_threadWorker != null)
					_threadWorker.Interrupt();
			}
			catch { }
		}

		private void NewThreadWorker(TimeSpan aPeriod)
		{
			if ((_threadWorker != null) && (_threadWorker.ThreadState == ThreadState.Running))
				return;

			try
			{
				_asyncLock.WaitAsync().Wait();

				if ((_threadWorker != null) && (_threadWorker.ThreadState == ThreadState.Running))
					return;

				if (_threadWorker != null)
				{
					if (_threadWorker.ThreadState.IsState(ThreadState.Stopped))
					{ /*By Pass*/ }
					else 
					{
						try
						{
							_threadWorker.Abort();
						}
						catch { }

						Thread.Sleep(30);
					}

					_threadWorker = null;
				}

				_threadWorker = new Thread(ThreadWorking);
				_threadWorker.IsBackground = true;
				_raiseInterruptFlag = false;
				_threadWorker.Start(aPeriod);
			}
			catch { }
			finally
			{
				_asyncLock.Release();
			}
		}

		private void ThreadWorking(object aPeriod)
		{
			TimeSpan xPeriod = (TimeSpan)aPeriod;
			DateTime timeOut = DateTime.Now.Add(xPeriod);

			try
			{
				while (timeOut.Subtract(DateTime.Now).TotalMilliseconds > 0)
				{
					if (_raiseInterruptFlag)
						break;

					if (timeOut.Subtract(DateTime.Now).TotalSeconds >= 10)
						Thread.Sleep(10000);

					else if (xPeriod.TotalSeconds >= 1)
						Thread.Sleep(1);

					else
						Thread.Sleep(20);
				}
			}
			catch { }
		}
	}
}
