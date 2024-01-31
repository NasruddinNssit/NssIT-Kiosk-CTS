using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Device.PAX.IM20.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Log.DB;
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

namespace serialCommunicationECR2
{
    public partial class frmPayWave : Form
    {
		private const string LogChannel = "PayWave Test Form";
		private DbLog _log = null;

		public delegate void CompletePayECRCallBack(TrxCallBackEventArgs e);
		public delegate void PayWaveProgress(InProgressEventArgs e);

		public CompletePayECRCallBack _completeECRHandle;
		public PayWaveProgress _ECRProgress;

		private string _disableTag = "DISABLED";
		private const int _maxRetryCount = 3;
		private const int _defaultCountDelay = 72;

		//CYA-DEBUG .. private const int _defaultCountDelay = 42;
		//---------------------------------------------------

		public const string TransCanceledTag = "Transaction Canceled";

		private bool _cancelButtonEnabled = false;
		private bool _earlyAborted = false;
		private bool _allowedRetry = false;
		private bool _cancelTrans = false;
		private bool _payWaveResponseFound = false;
		private bool _exitLastProcess = false;
		private int _delayCountDown = _defaultCountDelay;
		private int _tryWaveCount = 1;
		private decimal _amount = 0M;
		private string _refNo = "";
		private string _comPort = "";

		//private int _parentWidth, _parentContainerHeight;

		//private oCommon _oComm;
		private PayECRAccess _payWaveAccs = null;

		public bool IsSaleSuccess { get; private set; }
		public ResponseInfo SaleResult { get; private set; } = null;
		public ResponseInfo SettlementResult { get; private set; } = null;

		private System.Timers.Timer _payTimer = null;

		public DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		public frmPayWave(double amount, string refNo, string comport, int parentWidth = 500, int parentContainerHeight = 500)
		{
			_amount = (decimal)amount;
			_refNo = (refNo ?? "").Trim();
			_comPort = (comport ?? "").Trim();
			//_parentWidth = parentWidth;
			//_parentContainerHeight = parentContainerHeight;

			_payWaveAccs = PayECRAccess.GetPayECRAccess(_comPort, PayECRAccess.SaleMaxWaitingSec); /*, @"C:\eTicketing_Log\ECR_Receipts\", @"C:\eTicketing_Log\ECR_LOG", true, true);*/
			_payWaveAccs.OnCompleteCallback += OnCompletePayECRCallBack;
			_payWaveAccs.OnInProgressCall += OnPayECRInProgressCall;

			_completeECRHandle = new CompletePayECRCallBack(OnECRComplete);
			_ECRProgress = new PayWaveProgress(ECRWorkProg);

			InitializeComponent();
		}

		#region - Events -
		private void frmPayWave_Load(object sender, EventArgs e)
		{
			if (_cancellingLock.CurrentCount == 0)
				_cancellingLock.Release();

			this.SuspendLayout();
			//this.Left = (_parentWidth / 2) - (this.Width / 2);
			//this.Top = _parentContainerHeight - this.Height - 40;
			this.ResumeLayout(false);
			this.PerformLayout();

			_endTransactionFlag = false;
			IsSaleSuccess = false;
			txtError.Hide();
			txtError.Text = "";
			DisableCancelButton();

			lblPrice.Text = "RM " + Math.Round(_amount, 2).ToString();

			txtMacBusy.Text = "Sentuh card anda / Tap your card";
			txtMacBusy.SelectionStart = txtMacBusy.Text.Length;
			lblProcessTechMsg.Text = "";

			lblTimer.Text = $@"({(_delayCountDown - 10).ToString()})";

			EnableCancelButton();

			_payTimer = new System.Timers.Timer();
            _payTimer.Elapsed += _payTimer_Elapsed;
			_payTimer.Interval = 1000;
			_payTimer.Enabled = true;
			_payTimer.Start();

			tmrDelay.Interval = 1000;
			tmrDelay.Enabled = true;
			tmrDelay.Start();

			_earlyAborted = false;
			_payWaveAccs.Pay(_refNo, _amount.ToString(), AccountType.CreditCard, _refNo, null, null);

			panel1.Focus();
		}


        private object _tmrDelayLock = new object();
		//private void tmrDelay_Tick(object sender, EventArgs e)
		private void _payTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			string defaultErrorMsg = "";
			lock (_tmrDelayLock)
			{
				bool endFlag = false;
				if (_payTimer.Enabled)
				{
					try
					{
						_payTimer.Stop();
						_payTimer.Enabled = false;

						if (_delayCountDown >= 1)
						{
							_delayCountDown -= 1;

							string runChr = "";
							int tmCount = _delayCountDown - 10;
							if (tmCount < 0) tmCount = 0;

							if (((_delayCountDown % 2) == 0) && (tmCount == 0))
								runChr = " * ";

							this.Invoke(new Action(() => {
								lblTimer.Text = $@"({tmCount.ToString()}) {runChr}";
							}));
							

							if ((_delayCountDown <= 10) && (_earlyAborted == false) && (_exitLastProcess == false) && (_payWaveResponseFound == false))
							{
								if (_cancelButtonEnabled)
								{
									DisableCancelButton();
								}
								this.Invoke(new Action(() =>
								{
									if (SaleResult == null)
									{
										txtMacBusy.Text = "Timeout; No customer response."; txtMacBusy.SelectionStart = txtMacBusy.Text.Length;
										lblProcessTechMsg.Text = "Timeout; No customer response.";
										defaultErrorMsg = "Timeout; No customer response.";
										_delayCountDown = 20;
									}
								}));

								_earlyAborted = true;
								_payWaveAccs.ForceToCancel();
								_exitLastProcess = true;
							}
							else if ((_delayCountDown == 0) && (_exitLastProcess == false) && (_payWaveResponseFound == false))
							{
								if (_cancelButtonEnabled)
								{
									DisableCancelButton();
								}

								this.Invoke(new Action(() =>
								{
									if (SaleResult == null)
									{
										txtMacBusy.Text = "Timeout; No customer response."; txtMacBusy.SelectionStart = txtMacBusy.Text.Length;
										lblProcessTechMsg.Text = "Timeout; No customer response.";
										defaultErrorMsg = "Timeout; No customer response.";
										_delayCountDown = 20;
									}
								}));
								
								_payWaveAccs.ForceToCancel();
								_exitLastProcess = true;
							}
							else if ((_delayCountDown == 0) && (_exitLastProcess == false) && (_payWaveResponseFound == true))
							{
								this.Invoke(new Action(() =>
								{
									txtMacBusy.Text = "Local Timeout; Unable to get response from card server."; txtMacBusy.SelectionStart = txtMacBusy.Text.Length;
									lblProcessTechMsg.Text = "Local Timeout; Unable to get response from card server.";
									defaultErrorMsg = "Local Timeout; Unable to get response from card server.";
								}));

								_delayCountDown = 20;
								_payWaveAccs.ForceToCancel();
								_exitLastProcess = true;
							}
							else if ((_delayCountDown == 0) && (_payWaveResponseFound == true))
							{
								endFlag = true;
							}

						}
						else if (_exitLastProcess == true)
						{
							endFlag = true;
						}
					}
					catch (Exception ex)
					{
						Log.LogError(LogChannel, _refNo, new Exception("Error when PayWave UI Process. ", ex), "EX01", "frmPayWave._payTimer_Elapsed");

						if (defaultErrorMsg.Length > 0)
							defaultErrorMsg = $@"{defaultErrorMsg} & System Error when PayWave UI Process. {ex.Message}";
						else
							defaultErrorMsg = $@"System Error when PayWave UI Process. {ex.Message}";
					}
					finally
					{
						if (!endFlag)
						{
							_payTimer.Enabled = true;
							_payTimer.Start();
						}
						else if (_allowedRetry)
						{
							RetryPayment();
						}
						else if (!_allowedRetry)
						{
							if (defaultErrorMsg.Length == 0)
								defaultErrorMsg = "Fail to make paywave payment. Quit 01.";

							if (SaleResult == null)
								SaleResult = new ResponseInfo() { ResponseMsg = "SALE", StatusCode = "99", ErrorMsg = (_firstErrorMsg ?? defaultErrorMsg) };

							if (SettlementResult == null)
								SettlementResult = new ResponseInfo() { ResponseMsg = "SETTLEMENT", StatusCode = "99", ErrorMsg = "Fail to make paywave payment. Quit 01." };

							Log.LogText(LogChannel, _refNo, "Ending Paywave (1/2)", "B01", "frmPayWave._payTimer_Elapsed");

							this.Invoke(new Action(() =>
							{
								txtMacBusy.Show();
								txtMacBusy.Text = "Ending Transaction .. "; txtMacBusy.SelectionStart = txtMacBusy.Text.Length; txtMacBusy.Refresh();
								lblProcessTechMsg.Text = "Wait for Ending Transaction.";
								_payWaveAccs.SoftShutDown();
								Log.LogText(LogChannel, _refNo, "Ending Paywave (2/2)", "B01", "frmPayWave._payTimer_Elapsed");

								this.DialogResult = DialogResult.OK;
								this.Hide();
							}));
						}
					}
				}
			}
		}

		private bool _endTransactionFlag = false;
		private string _firstErrorMsg = null;
		private void OnECRComplete(TrxCallBackEventArgs e)
		{
			_endTransactionFlag = true;
			ResponseInfo currResult = e.Result;
			_allowedRetry = false;

			if (currResult != null)
			{
				if (currResult.ResponseMsg == "SALE")
					SaleResult = currResult;
				else if (currResult.ResponseMsg == "SETTLEMENT")
					SettlementResult = currResult;
			}

			this.Invoke(new Action(() =>
			{
				if ((e.IsSuccess)
					&& ((currResult.ResponseMsg == "SALE") || (currResult.ResponseMsg == "SETTLEMENT"))
					)
				{
					Program.ShowMsg("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
					Program.ShowMsg(GetResponseInString(e));

					if (currResult.ResponseMsg == "SALE")
					{
						txtMacBusy.Text = "Sale is successful.";
						txtMacBusy.SelectionStart = txtMacBusy.Text.Length;
						lblProcessTechMsg.Text = "Sale is successful.";
						IsSaleSuccess = true;

						// Below logic only valid if no need immediate SETTLEMENT
						_exitLastProcess = true;
						_delayCountDown = 3;
						//--------------------------------
					}
					else if (currResult.ResponseMsg == "SETTLEMENT")
					{
						lblProcessTechMsg.Text = "Settlement Completed";
						_exitLastProcess = true;
						_delayCountDown = 3;
					}
				
				}
				else
				{
					string errMsg = "";

					if (IsSaleSuccess)
					{
						txtMacBusy.Text = "Sale is successful.";
						txtMacBusy.SelectionStart = txtMacBusy.Text.Length;
						lblProcessTechMsg.Text = "Sale is successful.";
					}
					else
					{
						if ((currResult != null) && ((currResult.StatusCode ?? "").Length > 0))
						{
							if ((_cancelTrans) && (currResult.StatusCode.Equals(ResponseStatusCode.TA)))
								errMsg = TransCanceledTag;

							else if (currResult.StatusCode.Equals(ResponseStatusCode.Z1) || currResult.StatusCode.Equals(ResponseStatusCode.Z3))
							{
								if (_tryWaveCount < _maxRetryCount)
									_allowedRetry = true;

								errMsg = currResult.ErrorMsg + $@". Try Wave Count : {_tryWaveCount.ToString()}";
								currResult.ErrorMsg = errMsg;
							}
							else
								errMsg = currResult.ErrorMsg;
						}
						else if ((e.Error != null) && ((e.Error.Message ?? "").Trim().Length > 0))
							errMsg = e.Error.Message;

						else
							errMsg = "System encounter error at the moment. Please try later.";

						_firstErrorMsg = _firstErrorMsg ?? errMsg;

						if (currResult != null)
							currResult.ErrorMsg = errMsg;

						txtMacBusy.Hide();
						txtError.Text = errMsg;
						txtError.Show();
						DisableCancelButton();
					}

					// Delay 12 seconds for exit.
					// Enable Cancel Button for exit immediately.
					_exitLastProcess = true;

					if (_allowedRetry)
						_delayCountDown = 5;
					else if (errMsg.Equals(TransCanceledTag))
						_delayCountDown = 3;
					else
						_delayCountDown = 10;
				}
			}));

			string GetResponseInString(TrxCallBackEventArgs eV)
            {
				string retStr = "Response not found !!";
				if (eV.Result is ResponseInfo resInf)
                {
					retStr = $@"Response Result
{"\t"}Host No          {"\t"}: {resInf.HostNo ?? ""}
{"\t"}Report Time      {"\t"}: {resInf.ReportTime.ToString("dd MMM yyyy HH:mm:ss")}
{"\t"}Response Message {"\t"}: {resInf.ResponseMsg ?? ""}

{"\t"}Terminal ID      {"\t"}: {resInf.TID ?? ""}
{"\t"}Machine Id       {"\t"}: {resInf.MachineId ?? ""}
{"\t"}Merchant ID      {"\t"}: {resInf.MID ?? ""}
{"\t"}Application Id   {"\t"}: {resInf.AID ?? ""}

{"\t"}Card No                   {"\t"}: {resInf.CardNo ?? ""}
{"\t"}Card Type                 {"\t"}: {resInf.CardType ?? ""}
{"\t"}Card Holder Name          {"\t"}: {resInf.CardholderName ?? ""}
{"\t"}Card Application Label    {"\t"}: {resInf.CardAppLabel ?? ""}

{"\t"}Status Code               {"\t"}: {resInf.StatusCode ?? ""}
{"\t"}Additional Data / Ref.No. {"\t"}: {resInf.AdditionalData ?? ""}
{"\t"}Approval Code             {"\t"}: {resInf.ApprovalCode ?? ""}
{"\t"}RRN                       {"\t"}: {resInf.RRN ?? ""}
{"\t"}Transaction Trace No.     {"\t"}: {resInf.TransactionTrace ?? ""}
{"\t"}Transaction Cryptogram    {"\t"}: {resInf.TC ?? ""}

{"\t"}Transaction Amount     {"\t"}: {resInf.Amount ?? ""}
{"\t"}Currency Amount        {"\t"}: {resInf.CurrencyAmount}

{"\t"}Batch Number           {"\t"}: {resInf.BatchNumber ?? ""}
{"\t"}Batch Amount           {"\t"}: {resInf.BatchAmount ?? ""}
{"\t"}Batch Count            {"\t"}: {resInf.BatchCount ?? ""}
{"\t"}Batch Currency Amount  {"\t"}: {resInf.BatchCurrencyAmount}

{"\t"}Void Amount            {"\t"}: {resInf.VoidAmount ?? ""}
{"\t"}Void Currency Amount   {"\t"}: {resInf.VoidCurrencyAmount}

{"\t"}Ali Pay Transaction Id {"\t"}: {resInf.AlipayTrxID ?? ""}

{"\t"}Customer Id            {"\t"}: {resInf.CustomerID ?? ""}
{"\t"}Document No.           {"\t"}: {resInf.DocumentNumbers ?? ""}
{"\t"}Error Message          {"\t"}: {resInf.ErrorMsg ?? ""}
{"\t"}Expire Date            {"\t"}: {resInf.ExpiryDate ?? ""}

{"\t"}Parner Transaction Id  {"\t"}: {resInf.PartnerTrxID ?? ""}
{"\t"}Tag                    {"\t"}: {resInf.Tag ?? ""}
";
				}
				return retStr;
			}
		}

		private void ECRWorkProg(InProgressEventArgs e)
		{
			if (_payWaveResponseFound == false)
			{
				if (_endTransactionFlag == false)
					_delayCountDown = PayECRAccess.SaleMaxWaitingSec + 30;

				_payWaveResponseFound = true;

				_cancelTrans = false;
				_exitLastProcess = false;
				_earlyAborted = false;
				_allowedRetry = false;
			}

			string msgX = e.Message ?? "..";

			this.Invoke(new Action(() =>
			{
				if (!IsSaleSuccess)
					txtMacBusy.Text = "Processing ..";
				txtMacBusy.SelectionStart = txtMacBusy.Text.Length;

				lblProcessTechMsg.Text = msgX;

			}));

			if (_cancelButtonEnabled)
			{
				DisableCancelButton();
			}
		}

		private void OnCompletePayECRCallBack(object sender, TrxCallBackEventArgs e)
		{
			if (_completeECRHandle != null)
				this.Invoke(_completeECRHandle, e);
		}

		private void OnPayECRInProgressCall(object sender, InProgressEventArgs e)
		{
			if (_ECRProgress != null)
				this.Invoke(_ECRProgress, e);
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			if (IsCancelEnabled)
			{
				DisableCancelButton();
				_cancelTrans = true;
				_payWaveAccs.CancelRequest();
				_delayCountDown = 25;
			}
		}

		private void frmPayWave_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_payWaveAccs != null)
			{
				_payWaveAccs.OnCompleteCallback -= OnCompletePayECRCallBack;
				_payWaveAccs.OnInProgressCall -= OnPayECRInProgressCall;

				//_payWaveAccs.Dispose();
				_payWaveAccs = null;
			}
			_log = null;
		}
		#endregion

		private void RetryPayment()
		{
			//"Cuba lagi. Sentuh card anda / Try again. Tap your card";
			_tryWaveCount += 1;
			_exitLastProcess = false;
			_payWaveResponseFound = false;
			_endTransactionFlag = false;
			SaleResult = null;

			IsSaleSuccess = false;

			DisableCancelButton();

			this.Invoke(new Action(() => {
				txtError.Hide();
				txtError.Text = "";
				
				lblPrice.Text = "RM " + Math.Round(_amount, 2).ToString();

				txtMacBusy.Text = $"Cuba lagi. Sentuh (masukkan) card anda / Try again. Tap (insert) your card. Try:{_tryWaveCount.ToString()} of 3";
				txtMacBusy.Show();
				txtMacBusy.SelectionStart = txtMacBusy.Text.Length;
				lblProcessTechMsg.Text = "";

				_delayCountDown = _defaultCountDelay;
			
				lblTimer.Text = $@"({(_delayCountDown - 10).ToString()})";
			}));
			
			_earlyAborted = false;
			_payWaveAccs.Pay(_refNo, _amount.ToString(), AccountType.CreditCard, _refNo, null, null);
			_allowedRetry = false;

			EnableCancelButton();

			_payTimer.Enabled = true;
			_payTimer.Start();
		}

		private bool IsCancelEnabled
		{
			get
			{
				bool ret = false;
				this.Invoke(new Action(() =>
				{
					if (btnCancel.Tag.Equals(_disableTag))
					{
						ret = false;
					}
					else
						ret = true;
				}));

				return ret;
			}
		}

		private SemaphoreSlim _cancellingLock = new SemaphoreSlim(1);
		private void DisableCancelButton()
		{
			try
			{
				_cancellingLock.WaitAsync().Wait();
				this.Invoke(new Action(() =>
				{
					_cancelButtonEnabled = false;
					btnCancel.BackgroundImage = global::serialCommunicationECR2.Properties.Resources.B1_CancelBW;
					btnCancel.Tag = _disableTag;
					btnCancel.Refresh();
				}));
				System.Windows.Forms.Application.DoEvents();
			}
			finally
			{
				if (_cancellingLock.CurrentCount == 0)
					_cancellingLock.Release();
			}
		}

		private Thread _delayCancelActivationThreadWorker = null;
		private void EnableCancelButton()
		{
			_cancelButtonEnabled = true;
			EndOldCancelActivationThreadWorker();

			_delayCancelActivationThreadWorker = new Thread(new ThreadStart(delayCancelActivationThreadWorking));
			_delayCancelActivationThreadWorker.IsBackground = true;
			_delayCancelActivationThreadWorker.Start();

			return;

			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			void delayCancelActivationThreadWorking()
			{
				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(7);

				while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
				{
					Thread.Sleep(500);

					if (_payWaveAccs is null)
						break;
				}

				if (_payWaveAccs != null)
				{
					try
					{
						_cancellingLock.WaitAsync().Wait();
						if (_cancelButtonEnabled)
						{
							this.Invoke(new Action(() =>
							{
								btnCancel.BackgroundImage = global::serialCommunicationECR2.Properties.Resources.B1_Cancel;
								btnCancel.Tag = "";
								btnCancel.Refresh();
							}));
						}
					}
					catch (Exception ex)
					{
						Log.LogError(LogChannel, "*", ex, "EX01", "frmPayWave.delayCancelActivationThreadWorking");
					}
					finally
					{
						if (_cancellingLock.CurrentCount == 0)
							_cancellingLock.Release();
					}
				}
			}

			void EndOldCancelActivationThreadWorker()
			{
				if (_delayCancelActivationThreadWorker != null)
				{
					if (((_delayCancelActivationThreadWorker.ThreadState & ThreadState.Aborted) != ThreadState.Aborted)
						&& ((_delayCancelActivationThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped))
					{
						try
						{
							_delayCancelActivationThreadWorker.Abort();
							Thread.Sleep(100);
						}
						catch { }
						finally
						{
							_delayCancelActivationThreadWorker = null;
						}
					}
				}
			}
		}

        private void btnCancel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}