using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace LibShowMessageWindow
{
	partial class WFmMessageWindow : Form 
	{
		private ConcurrentQueue<MessagePack> _msgList = new ConcurrentQueue<MessagePack>();

		private bool _loadWithMinimizedWindow = true;

		private int _maxMsgLen = 30000;
		private int _trimMsgLen = 30000;

		public WFmMessageWindow(bool loadWithMinimizedWindow = true)
		{
			InitializeComponent();

			_loadWithMinimizedWindow = loadWithMinimizedWindow;
			Thread ReadMsgThreadWorker = new Thread(new ThreadStart(ReadMsgThreadWorking));
			ReadMsgThreadWorker.IsBackground = true;
			ReadMsgThreadWorker.Start();
		}

		private void ChangeMaxMsgSize(int maxSize)
        {
			_maxMsgLen = maxSize;
			_trimMsgLen = Convert.ToInt32(Math.Round((Convert.ToDecimal(_maxMsgLen) * 0.7M), 0));
			grpMsgSize.Text = $@"Max Message Size. - Trim Msg Size is {_trimMsgLen}";
		}

		private void WFmMessageWindow_Load(object sender, EventArgs e)
		{
			this.TopMost = true;

			ChangeMaxMsgSize(int.Parse(rbtMaxMsgSize8.Text));

			if (_loadWithMinimizedWindow)
				this.WindowState = FormWindowState.Minimized;
		}

		private bool _closing = false;
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			_closing = true;
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void ReadMsgThreadWorking()
		{
			MessagePack inforMsg = null;
			while (_closing == false)
			{
				try
				{
					inforMsg = GetNextMsg();

					if (inforMsg != null)
					{
						this.Invoke(new Action(() => {

							if (chkShowLatestMessageOnTop.Checked)
							{
								if (inforMsg.Message?.Trim().Length > 0)
									txtMsg.Text = $@"{inforMsg.TTime.ToString("HH:mm:ss.fffffff")} -:{inforMsg.Message}{"\r\n"}{txtMsg.Text}";
								else
									txtMsg.Text = $@"{"\r\n"}{txtMsg.Text}";

								if (txtMsg.Text.Length > _maxMsgLen)
									txtMsg.Text = txtMsg.Text.Substring(0, _trimMsgLen);
								
								if (txtMsg.Text.Length > 5)
								{
									txtMsg.SelectionStart = 1;
									txtMsg.SelectionLength = 1;
								}
							}
							else
							{
								if (inforMsg.Message?.Trim().Length > 0 )
									txtMsg.AppendText($@"{inforMsg.TTime.ToString("HH:mm:ss.fffffff")} -:{inforMsg.Message}{"\r\n"}");
								else
									txtMsg.AppendText($@"{"\r\n"}");

								if (txtMsg.Text.Length > _maxMsgLen)
									txtMsg.Text = txtMsg.Text.Substring(_trimMsgLen);
								
								if (txtMsg.Text.Length > 5)
								{
									txtMsg.SelectionStart = txtMsg.Text.Length - 2;
									txtMsg.SelectionLength = 1;
								}
							}

							txtMsg.ScrollToCaret();
						}));
					}
				}
				catch (Exception ex)
				{
					string byPassMsg = ex.Message;
				}
			}
		}

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 1);
		private MessagePack GetNextMsg()
		{
			MessagePack retMsg = null;
			bool logFound = false;

			if (_closing == false)
			{
				try
				{
					lock (_msgList)
					{
						if (_msgList.Count == 0)
						{
							Monitor.Wait(_msgList, _MaxWaitPeriod);
						}
						logFound = _msgList.TryDequeue(out retMsg);
					}
				}
				// Used to handle "_logList" is null after disposed
				catch (Exception ex) 
				{ string byPassStr = ex.Message; }
			}

			if (logFound)
				return retMsg;
			else
				return null;
		}

		public void ShowMessage(string msg, DateTime? time = null)
		{
			time = (!time.HasValue) ? DateTime.Now : time;
			msg = msg ?? "";

			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				// Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
				//        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
				if (_closing == false)
				{
					try
					{
						lock (_msgList)
						{
							_msgList.Enqueue(new MessagePack(msg, time.Value));
							Monitor.PulseAll(_msgList);
						}
					}
					// Used to handle "_msgList" is null after disposed
					catch (Exception ex2) 
					{ string byPassStr = ex2.Message; }
				}
			})));
			execThread.IsBackground = true;
			execThread.Start();
		}

		private void btnCopyToClipboard_Click(object sender, EventArgs e)
		{
			try
			{
				Clipboard.SetText(txtMsg.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void btnClearMessage_Click(object sender, EventArgs e)
		{
			txtMsg.Text = "";
		}

		private void WFmMessageWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.Invoke(new Action(() => {
				_closing = true;
				txtMsg.Text = "";
			}));			
		}

		private const int CP_NOCLOSE_BUTTON = 0x200;
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams myCp = base.CreateParams;
				myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
				return myCp;
			}
		}

		private void chkTopMost_CheckedChanged(object sender, EventArgs e)
		{
			if (chkTopMost.Checked)
            {
				this.TopMost = true;
			}
			else
            {
				this.TopMost = false;
			}
			System.Windows.Forms.Application.DoEvents();
		}

		class MessagePack : IDisposable
		{
			public string Message { get;  private set;}
			public DateTime TTime { get; private set; }

			public MessagePack (string msg, DateTime time)
			{
				Message = msg;
				TTime = time;
			}

			public void Dispose()
			{
				Message = null;
			}
		}

        private void FontChanged_Event(object sender, EventArgs e)
        {
			if (this.rbtMicrosoftSansSerif.Checked)
            {
				this.txtMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				System.Windows.Forms.Application.DoEvents();
			}
			else if (this.rbtCourierNew.Checked)
            {
				this.txtMsg.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				System.Windows.Forms.Application.DoEvents();
			}
        }

        private void rbtMaxMsgSize_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
				RadioButton btn = (RadioButton)sender;
				ChangeMaxMsgSize(int.Parse(btn.Text));
			}
			catch(Exception ex)
            {
				ShowMessage(ex.ToString());
			}			
		}
    }
}