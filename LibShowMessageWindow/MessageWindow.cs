using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibShowMessageWindow
{
    public class MessageWindow : IDisposable 
    {
		WFmMessageWindow _msgWindow = null;

		private static SemaphoreSlim _lock = new SemaphoreSlim(1);
		private static MessageWindow _msgWin = null;

		public MessageWindow()
		{
			Thread tWorker = new Thread(new ThreadStart(new Action(() => {
				_msgWindow = new WFmMessageWindow();
				_msgWindow.ShowDialog();

				//_msgWindow.Show();
				//while(true)
				//	Thread.Sleep(300);

			})));
			tWorker.IsBackground = true;
			tWorker.SetApartmentState(ApartmentState.STA);
			tWorker.Start();
		}

		public static MessageWindow DefaultMessageWindow
        {
            get
            {
				if (_msgWin is null)
                {
					try
					{
						_lock.WaitAsync().Wait();
						if (_msgWin is null)
						{
							_msgWin = new MessageWindow();
						}
					}
					catch { }
                    finally
                    {
						_lock.Release();
					}
				}
				return _msgWin;
			}
        }

		public void ShowMessage(string msg, DateTime? time = null)
		{
			time = (!time.HasValue) ? DateTime.Now : time;
			msg = msg ?? "";

			_msgWindow?.ShowMessage(msg, time);
		}

		public void Dispose()
		{
			if (_msgWindow != null)
			{
				_msgWindow.Invoke(new Action(() => {
					lock (_msgWindow)
					{
						try { _msgWindow.Close(); } catch { }
						try { _msgWindow.Dispose(); } catch { }
						_msgWindow = null;
					}
				}));
			}
		}
	}
}
