using System;
using System.Threading;
using System.Threading.Tasks;

using NssIT.Kiosk;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;

using NssIT.Kiosk.Device.B2B.CommBusiness;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Error;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Events;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common;

namespace NssIT.Kiosk.Device.B2B.B2BApp
{
	/// <summary>
	/// Class Level : Manage, Control, and Navigate
	/// Class Responsibilities	: Manage payment 
	///							: Control countdown of payment, and quit a payment transaction when timeout.
	///							: Check latest status of cash machines (Bill and Coin).
	/// </summary>
	public class B2BPaymentApplication : IUIPayment
	{
		private const string LogChannel = "B2BApplication";
		private const string TransCancelledTag = "Transaction Cancelled";

		private UICashMachineStatus _cashMachineStatus = null;

		private B2BPaymentBusiness _b2bPayBusin = null;

		private xPaymentInfo _payData = new xPaymentInfo();
		private xCoundDownInfo _countDown = new xCoundDownInfo();
		private xCashMachineInfo _cashMachine = new xCashMachineInfo();

		public B2BPaymentApplication()
		{
			_b2bPayBusin = new B2BPaymentBusiness();
			_b2bPayBusin.OnCompleted += _b2bPayBusin_OnCompleted;
			_b2bPayBusin.OnInProgress += _b2bPayBusin_OnInProgress;

			_countDown.CoundDownThreadWorker = new Thread(new ThreadStart(CountDownThreadExec));
			_countDown.CoundDownThreadWorker.IsBackground = true;
			_countDown.CoundDownThreadWorker.Start();
		}

		private DbLog _log = null;
		private DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		public void Dispose()
		{
			if (OnShowProcessingMessage != null)
			{
				Delegate[] delgList = OnShowProcessingMessage.GetInvocationList();
				foreach (EventHandler<UIMessageEventArgs> delg in delgList)
				{
					OnShowProcessingMessage -= delg;
				}
			}

			if (_b2bPayBusin != null)
			{
				try
				{
					_b2bPayBusin.Dispose();
				}
				catch (Exception ex)
				{
					string tt1 = ex.Message;
				}

				_b2bPayBusin = null;
			}

			if (_countDown.CoundDownThreadWorker != null)
			{
				try
				{
					_countDown.CoundDownThreadWorker.Interrupt();
				}
				catch { }

				_countDown.CoundDownThreadWorker = null;
			}
		}

		#region ----- Interface -> IUIPayment -----

		public bool IsSaleSuccess { get; private set; }

		public bool IsPaymentCompleted
		{
			get => (_payData?.TransCompletedResult != null);
		}

		public Guid? CurrentNetProcessId { get; private set; } = null;

		private string _currProcessId = "-";
		public string CurrentProcessId
		{
			get
			{
				return _currProcessId;
			}
			private set
			{
				_currProcessId = string.IsNullOrEmpty(value) ? "-" : value.Trim();
			}
		}

		public bool ReadIsPaymentDeviceReady(out bool isLowCoin, out bool isRecoveryInProgressAfterDispenseCoin, out string errorMessage)
		{
			isRecoveryInProgressAfterDispenseCoin = false;
			isLowCoin = false;
			errorMessage = null;

			if (_b2bPayBusin == null)
				throw new Exception("Application already shutdown (EXIT8321).");

			if (_cashMachine.IsMalfunction)
				throw new Exception("Cash machine Malfunction (EXIT8324).");

			bool retIsReady = _b2bPayBusin.ReadIsPaymentDeviceReady(out isLowCoin, out isRecoveryInProgressAfterDispenseCoin, out errorMessage);

			return retIsReady;
		}

		public event EventHandler<UIMessageEventArgs> OnShowProcessingMessage;

		private SemaphoreSlim _asyncPaymentLock = new SemaphoreSlim(1);
		public async Task<bool> MakePayment(string processId, Guid? netProcessId, decimal amount)
		{
			decimal paymentAmount = (Math.Floor((amount * 100.00M))) / 100M;

			bool isSuccess = false;
			try
			{
				await _asyncPaymentLock.WaitAsync();

				if (_cashMachine.IsMalfunction)
					throw new B2BMalfunctionException();

				if (!_payData.IsProcActivated)
				{
					if (amount <= 0)
						throw new Exception("Payment amount must more then 0.00");

					OnShowProcessingMessageSendEvent(netProcessId, null, 
						new UIError(netProcessId, processId, DateTime.Now) { DisplayErrorMsg = UIVisibility.VisibleDisabled, ErrorMessage = "" });

					OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
						new UICustomerMessage (CurrentNetProcessId, processId, DateTime.Now)
						{
							CustmerMsg = "Please Pay : " + "RM " + Math.Round(amount, 2).ToString(),
							DisplayCustmerMsg = UIVisibility.VisibleEnabled
						});

					OnShowProcessingMessageSendEvent(netProcessId, "..start");

					OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
						new UICountdown(CurrentNetProcessId, processId, DateTime.Now) { CountdownMsg = $@"{xCoundDownInfo.DefaultCountDelay}" });

					_payData.IsAllowToCancel = true;
					OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
						new UISetCancelPermission(CurrentNetProcessId, processId, DateTime.Now) { IsCancelEnabled = UIAvailability.Enabled });

					Log.LogText(LogChannel, processId, $@"Start Make Payment; Amount : {amount:#,###.00}",
									"A01", "B2BApplication.MakePayment", adminMsg: $@"---------- ---------- Start Make Payment; Ticket Price Amount : {amount:#,###.00} ---------- ----------");

					isSuccess = _b2bPayBusin.Pay(processId, netProcessId, amount, out string errorMsg, processId);

					if (isSuccess)
					{
						Log.LogText(LogChannel, processId, $@"Start Payment Transaction",
									"A01", "B2BApplication.MakePayment", adminMsg: $@"Start Payment Transaction");

						_cashMachineStatus = null;
						IsSaleSuccess = false;
						CurrentProcessId = processId;
						CurrentNetProcessId = netProcessId;

						_payData.ResetToStartPayment();
						_countDown.CancelFlag = false;
						_countDown.DelayCount = xCoundDownInfo.DefaultCountDelay;
						_countDown.LoopEnabled = true;

						_cashMachine.IsResponseFound = false;
					}
					else
					{
						Log.LogText(LogChannel, processId, $@"Unable to start Payment Transaction",
									"A01", "B2BApplication.MakePayment", adminMsg: $@"Unable to start Payment Transaction");

						OnShowProcessingMessageSendEvent(netProcessId, null, 
							new UIError(netProcessId, processId, DateTime.Now) { DisplayErrorMsg = UIVisibility.VisibleEnabled, ErrorMessage = "Unable to start payment.." });

						errorMsg = string.IsNullOrWhiteSpace(errorMsg) ? "Unknown error (EXIT8322)." : errorMsg;
						throw new Exception(errorMsg);
					}
				}
				else
				{
					Log.LogText(LogChannel, processId, $@"System busy; Please make payment later (EXIT8323).",
									"A01", "B2BApplication.MakePayment", adminMsg: $@"System busy; Please make payment later (EXIT8323).");

					throw new Exception("System busy; Please make payment later (EXIT8323).");
				}
			}
			finally
			{
				_asyncPaymentLock.Release();
			}

			return isSuccess;
		}

		public void CancelTransaction()
		{
			if (_payData.IsAllowToCancel)
			{
				_payData.IsAllowToCancel = false;
				OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
					new UISetCancelPermission(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { IsCancelEnabled = UIAvailability.Disabled });

				_payData.IsTransCancelled = true;
				_b2bPayBusin.CancelRequest(CurrentProcessId, CurrentNetProcessId);
				_countDown.DelayCount = 20;
				_countDown.CancelFlag = true;
			}
		}

		private long _lastCallCount = 0;
		private void OnShowProcessingMessageSendEvent(Guid? netProcessId, string processMessage, IKioskMsg MsgObj = null, bool? transactionHasCompleted = null)
		{
			try
			{
				if (transactionHasCompleted == true)
				{
					_payData.IsTransCompleted = true;
					_payData.IsRefundCompleted = true;
				}

				if (MsgObj is null)
				{
					MsgObj = (new UIProcessingMessage(netProcessId, CurrentProcessId, DateTime.Now) { ProcessMsg = processMessage });
				}
				else if (MsgObj is UINewPayment paymData)
				{
					paymData.CustmerMsg = $@"Please Pay RM {paymData.Price:#,##0.00}";
					paymData.InitCountDown = 360;
					paymData.ProcessMsg = $@"Start Payment; Price RM {paymData.Price:#,##0.00}";
					MsgObj = paymData;
				}
				else if (MsgObj is UICountdown countD)
				{
					if ((_payData.IsTransCompleted) && (_payData.IsProcActivated))
					{
						_lastCallCount++;
						countD.CountdownMsg = ((_lastCallCount % 2) == 0) ? "*" : "**";
					}
					MsgObj = countD;
				}
				
				OnShowProcessingMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) {Message = processMessage, KioskMsg = MsgObj });
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, new Exception("Unhandle event error OnShowProcessingMessage.", ex), "A01", "B2BApplication.ShowProcessingMessage");
			}
		}

		public bool ShutDown()
		{
			try
			{
				_b2bPayBusin.Dispose();
			}
			catch { }

			return true;
		}

		#endregion

		#region ----- CountDownThread -----

		private void CountDownThreadExec()
		{
			while (_b2bPayBusin != null)
			{
				CountDownLoop();

				try
				{
					Task.Delay(1000).Wait();
				}
				catch (ThreadInterruptedException)
				{ /*by pass*/ }
				catch (Exception ex)
				{
					string errMsg = ex.Message;
				}
			}
		}

		private string _lastRunChr = "";
		private void CountDownLoop()
		{
			if (_countDown.LoopEnabled == false)
				return;

			string defaultErrorMsg = "";
			lock (_countDown.TimerDelayLock)
			{
				bool endFlag = false;
				try
				{
					if ((_countDown.DelayCount >= 1) || ((_payData.IsRefundCompleted == false) && (!_payData.IsTransCompleted)) )
					{
						if ((_payData.IsRefundCompleted == false) && (!_payData.IsTransCompleted))
						{
							/*By pass; Wait until transaction fully completed.*/
							OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
								new UICountdown(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { CountdownMsg = $@"Refund" });
						}
						else
						{
							_countDown.DelayCount = (_countDown.DelayCount > 0) ? (_countDown.DelayCount - 1) : 0;

							if (_countDown.DelayCount <= 0)
							{
								Log.LogText(LogChannel, CurrentProcessId, $@"Count down deley : {_countDown.DelayCount}", 
									"A01", "B2BApplication.CountDownLoop");
							}

							string runChr = "";
							int tmCount = _countDown.DelayCount - 10;
							if (tmCount < 0) tmCount = 0;

							if ((((_countDown.DelayCount % 2) == 0) && (tmCount == 0)) 
								|| (_countDown.CancelFlag == true))
							{
								runChr = " * ";
								if (_lastRunChr.Trim().Equals(runChr.Trim()))
									runChr = " ** ";
								_lastRunChr = runChr;

								if (_countDown.CancelFlag == true)
									tmCount = 0;
							}
							
							OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
								new UICountdown(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { CountdownMsg = $@"{tmCount.ToString()}{runChr}" });

							// When customer do nothing with kiosk when payment and having 10 seconds to timeout..
							if ((!_payData.IsPaymentFound) &&
								(_countDown.DelayCount <= 10) && (_payData.IsEarlyAborted == false) &&
								(_payData.IsRequestQuitProcess == false) && (_cashMachine.IsResponseFound == false))
							{
								_countDown.CancelFlag = true;
								_b2bPayBusin.CancelRequest(CurrentProcessId, CurrentNetProcessId);
																
								if (_payData.IsAllowToCancel)
								{
									_payData.IsAllowToCancel = false;

									OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
										new UISetCancelPermission(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { IsCancelEnabled = UIAvailability.Disabled });
								}

								if (_payData.TransCompletedResult == null)
								{
									OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
										new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { CustmerMsg = "Timeout; Customer not responding." });

									OnShowProcessingMessageSendEvent(CurrentNetProcessId, "Timeout; Customer not responding.");

									defaultErrorMsg = "Timeout; Customer not responding.";
									_countDown.DelayCount = 20;
								}

								_payData.IsEarlyAborted = true;
								_payData.IsRequestQuitProcess = true;
							}

							// When customer do nothing with kiosk when payment and already reached timeout..
							else if ((!_payData.IsPaymentFound) && (_countDown.DelayCount == 0) && (_payData.IsRequestQuitProcess == false) &&
								(_cashMachine.IsResponseFound == false))
							{
								if (_payData.TransCompletedResult == null)
								{
									if (_payData.IsAllowToCancel)
									{
										_payData.IsAllowToCancel = false;

										OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
											new UISetCancelPermission(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { IsCancelEnabled = UIAvailability.Disabled });
									}
									OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
										new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { CustmerMsg = "Timeout; Customer not responding." });

									OnShowProcessingMessageSendEvent(CurrentNetProcessId, "Timeout; Customer not responding.");

									defaultErrorMsg = "Timeout; Customer not responding.";
									_countDown.DelayCount = 20;
								}

								_countDown.CancelFlag = true;
								_b2bPayBusin.CancelRequest(CurrentProcessId, CurrentNetProcessId);
								_payData.IsRequestQuitProcess = true;
							}

							// When customer has did something with kiosk (IsResponseFound is true) but already reached timeout..
							else if ((!_payData.IsPaymentFound) && (_countDown.DelayCount == 0) && (_payData.IsRequestQuitProcess == false) &&
								(_cashMachine.IsResponseFound == true))
							{
								OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
										new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { CustmerMsg = "Customer Timeout." });

								OnShowProcessingMessageSendEvent(CurrentNetProcessId, "Customer Timeout.");
								defaultErrorMsg = "Customer Timeout.";

								_countDown.DelayCount = 20;
								_countDown.CancelFlag = true;
								_b2bPayBusin.CancelRequest(CurrentProcessId, CurrentNetProcessId);
								_payData.IsRequestQuitProcess = true;
							}

							// When customer do something with kiosk (IsResponseFound is true), transaction has completed OR no payment found, and already timeout..
							else if ((_countDown.DelayCount == 0) && (_cashMachine.IsResponseFound == true) && (_payData.IsTransCompleted || ((!_payData.IsPaymentFound))))
							{
								endFlag = true;
								Log.LogText(LogChannel, CurrentProcessId, $@"End of Count down loop; IsTransCompleted: {_payData.IsTransCompleted.ToString()}; IsPaymentFound: {_payData.IsPaymentFound.ToString()}",
									"A20", "B2BApplication.CountDownLoop");
							}
						}
					}
					// Already timeout, payment has completed OR payment not found, and ready to quit process.
					else if ((_payData.IsRequestQuitProcess == true) && (_payData.IsTransCompleted || (!_payData.IsPaymentFound)))
					{
						endFlag = true;
						Log.LogText(LogChannel, CurrentProcessId, $@"End of Count down loop; IsTransCompleted: {_payData.IsTransCompleted.ToString()}; IsPaymentFound: {_payData.IsPaymentFound.ToString()}",
									"A30", "B2BApplication.CountDownLoop");
					}
				}
				catch (ThreadInterruptedException ex)
				{
					// By Pass
					string chkStr = ex.Message;
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, CurrentProcessId, new Exception("Error when PayWave UI Process", ex), 
						"E01", "B2BPaymentApplication.CountDownLoop");

					if (defaultErrorMsg.Length > 0)
						defaultErrorMsg = $@"{defaultErrorMsg} & System Error when PayWave UI Process. {ex.Message}";
					else
						defaultErrorMsg = $@"System Error when PayWave UI Process. {ex.Message}";
				}
				finally
				{
					if (endFlag)
					{
						_countDown.LoopEnabled = false;
						_payData.IsProcActivated = false;

						if (_payData.TransCompletedResult == null)
						{
							_cashMachine.IsMalfunction = true;

							OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
								new UIError(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { DisplayErrorMsg = UIVisibility.VisibleEnabled, ErrorMessage = "System Malfunction suspected." });
							OnShowProcessingMessageSendEvent(CurrentNetProcessId, "System Malfunction suspected");

							Log.LogText(LogChannel, CurrentProcessId, $@"End Cash Machine Transaction; System Malfunction suspected; NetProcessID.:{CurrentNetProcessId?.ToString("D")}", "A75", "B2BApplication.CountDownLoop");
						}
						else
						{
							OnShowProcessingMessageSendEvent(CurrentNetProcessId, "End Transaction.", transactionHasCompleted: true);

							Log.LogText(LogChannel, CurrentProcessId, $@"End Cash Machine Transaction; NetProcessID.:{CurrentNetProcessId?.ToString("D")}", "A76", "B2BApplication.CountDownLoop");

							//Task.Delay(5000).Wait();
							Task.Delay(1000).Wait();

							PaymentResult payStt = PaymentResult.Fail;

							if (_payData.TransCompletedResult is null)
								payStt = PaymentResult.Fail;
							else if (_payData.TransCompletedResult.IsSuccess)
								payStt = PaymentResult.Success;
							else if (_payData.TransCompletedResult.ResultStatus == (DeviceProgressStatus)PaymentResultStatus.Timeout)
								payStt = PaymentResult.Timeout;
							else if (_payData.TransCompletedResult.ResultStatus == (DeviceProgressStatus)PaymentResultStatus.Cancel)
								payStt = PaymentResult.Cancel;

							UIHideForm hideForm = null;

							if ((payStt == PaymentResult.Success) && (_cashMachineStatus != null))
							{
								hideForm = new UIHideForm(CurrentNetProcessId, CurrentProcessId, DateTime.Now, payStt, 
											_cashMachineStatus.Cassette1NoteCount, 
											_cashMachineStatus.Cassette2NoteCount, 
											_cashMachineStatus.Cassette3NoteCount, 
											_cashMachineStatus.RefundCoinAmount);
							}
							else
							{
								hideForm = new UIHideForm(CurrentNetProcessId, CurrentProcessId, DateTime.Now, payStt, -1, -1, -1, -1);
							}

							OnShowProcessingMessageSendEvent(CurrentNetProcessId, null, hideForm);
							//OnShowProcessingMessageSendEvent(CurrentNetProcessId, null, new UIHideForm(CurrentNetProcessId, CurrentProcessId, DateTime.Now));
						}

						Log.LogText(LogChannel, CurrentProcessId, $@"---------- --------- End of Payment ---------- ----------",
									"A30", "B2BApplication.CountDownLoop", adminMsg: $@"---------- --------- End of Payment ---------- ----------");
					}
				}
			}
		}

		#endregion

		#region ----- B2BAccess EventsHandles & Related Methods -----

		private void _b2bPayBusin_OnCompleted(object sender, TrxCallBackEventArgs evn)
		{
			if (evn.ModuleAppGroup != B2BDecorator.Command.B2BModuleAppGroup.Payment)
				return;

			try
			{
				Do_B2bAccess_OnCompleted(evn);
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BApplication._b2bAccess_OnCompleteCallback");
			}

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			void Do_B2bAccess_OnCompleted(TrxCallBackEventArgs e)
			{
				if (e.ModuleAppGroup != B2BDecorator.Command.B2BModuleAppGroup.Payment)
					return;

				if (_payData.IsAllowToCancel)
				{
					_payData.IsAllowToCancel = false;
					OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
						new UISetCancelPermission(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { IsCancelEnabled = UIAvailability.Disabled });
				}

				Log.LogText(LogChannel, CurrentProcessId, _payData.TransCompletedResult, "A01", "B2BApplication.Do_B2bAccess_OnCompleted",
					extraMsg: "MsgObj: Parameter TrxCallBackEventArgs;");

				ITrxCallBackEventArgs currResult = e.Duplicate();

				if (e.IsSuccess)
				{
					if (e.KioskMessage is UICashMachineStatus cashStt)
					{
						_cashMachineStatus = cashStt;
					}

					OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
						new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now)
						{
							CustmerMsg = "Sale is successful.",
							DisplayCustmerMsg = UIVisibility.VisibleEnabled
						}, transactionHasCompleted: true);

					OnShowProcessingMessageSendEvent(CurrentNetProcessId, "Sale is successful.");

					_payData.TransCompletedResult = currResult;
					_payData.IsRequestQuitProcess = true;

					if (_payData.IsRefundCompleted == false)
						_payData.IsRefundCompleted = true;
										
					_countDown.DelayCount = 0;

					IsSaleSuccess = true;
				}
				else
				{
					string errMsg = "";

					if (IsSaleSuccess)
					{
						OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
							new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now)
							{
								CustmerMsg = "Sale is successful.",
								DisplayCustmerMsg = UIVisibility.VisibleEnabled
							}, transactionHasCompleted: true);

						OnShowProcessingMessageSendEvent(CurrentNetProcessId, "Sale is successful.");

						if (_payData.IsRefundCompleted == false)
							_payData.IsRefundCompleted = true;

						_countDown.DelayCount = 0;
					}
					else if (currResult.ResultStatus == (DeviceProgressStatus)PaymentResultStatus.FailRefundOnPaymentSuccess)
					{
						//_isRefundCompleted = true;
						_payData.TransCompletedResult = currResult;

						OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
							new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now)
							{
								CustmerMsg = "Cash machine has no enough cash to refund. Sales Transaction is Cancelled.",
								DisplayCustmerMsg = UIVisibility.VisibleEnabled
							}, transactionHasCompleted: true);

						OnShowProcessingMessageSendEvent(CurrentNetProcessId, $@"Cash machine has no enough cash to refund. Sales Transaction is Cancelled.{"\r\n"}{e?.Error?.Message}");

						Task.Delay(5000).Wait();
						//Task.Delay(500).Wait();

						_countDown.DelayCount = 5;
					}
					else
					{
						if (currResult != null)
						{
							if ((_payData.IsTransCancelled) && (currResult.ResultStatus == (DeviceProgressStatus)PaymentResultStatus.Cancel))
								errMsg = TransCancelledTag;

							else if ((currResult.ResultStatus == (DeviceProgressStatus)PaymentResultStatus.Error) ||
								(currResult.ResultStatus == (DeviceProgressStatus)PaymentResultStatus.Fail) ||
								(currResult.ResultStatus == (DeviceProgressStatus)PaymentResultStatus.Unknown))
							{
								_payData.IsErrorFound = true;
								errMsg = e.Error.Message;
							}
							else if (currResult.ResultStatus == (DeviceProgressStatus)PaymentResultStatus.MachineMalfunction)
							{
								_payData.IsErrorFound = true;
								_cashMachine.IsMalfunction = true;

								if (e?.Error?.Message.Trim().Length > 0)
									errMsg = $@"System encounter Cash Machine Malfunction; {e.Error.Message}; ";
								else
									errMsg = "System encounter Cash Machine Malfunction (Unknown error).";
							}

						}
						else if ((e?.Error?.Message ?? "").Trim().Length > 0)
							errMsg = e.Error.Message;

						else
							errMsg = "System encounter error at the moment. Please try later.";

						_payData.FirstErrorMsg = _payData.FirstErrorMsg ?? errMsg;

						if (currResult != null)
							_payData.TransCompletedResult = currResult;

						OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
							new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now)
							{
								CustmerMsg = "",
								DisplayCustmerMsg = UIVisibility.VisibleDisabled 
							}, transactionHasCompleted: true);

						OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
								new UIError(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { DisplayErrorMsg = UIVisibility.VisibleEnabled, ErrorMessage = errMsg });

						if(_payData.IsTransCancelled)
							_countDown.DelayCount = 0;
						else
							_countDown.DelayCount = 3;
					}
				}

				_cashMachine.IsResponseFound = true;
				_payData.IsRequestQuitProcess = true;
			}
		}

		private void _b2bPayBusin_OnInProgress(object sender, InProgressEventArgs evn)
		{
			if (evn.ModuleAppGroup != B2BDecorator.Command.B2BModuleAppGroup.Payment)
				return; 

			try
			{
				Do_B2bAccess_OnInProgress(evn);
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BApplication._b2bAccess_OnInProgressCallback");
			}

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			void Do_B2bAccess_OnInProgress(InProgressEventArgs e)
			{
				Log.LogText(LogChannel, CurrentProcessId, e, "A01", "B2BApplication.Do_B2bAccess_OnInProgress",
					extraMsg: "MsgObj: Parameter InProgressEventArgs;");

				if (((e.KioskMessage is UINewPayment) || (e.KioskMessage is UIAcceptableBanknote)) == false)
					_cashMachine.IsResponseFound = true;

				// Change Count Down delay once a payment has detected.
				if ((e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.PaymentFound) && (!_payData.IsPaymentFound))
				{
					_countDown.DelayCount = (B2BPaymentBusiness.SaleMaxWaitingSec - 30) < 60 ? 60 : (B2BPaymentBusiness.SaleMaxWaitingSec - 30);
					_payData.IsPaymentFound = true;

					OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
						new UICountdown(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { CountdownMsg = _countDown.DelayCount.ToString() });
				}

				if (!IsSaleSuccess)
				{
					if ((!_payData.IsStopTransWhenErrFound) && (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.Error))
					{
						_payData.IsErrorFound = true;

						OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
							new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now)
							{
								CustmerMsg = "",
								DisplayCustmerMsg = UIVisibility.VisibleDisabled
							});

						OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
								new UIError(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { DisplayErrorMsg = UIVisibility.VisibleEnabled, ErrorMessage = e?.Error?.Message });

					}
					else if (
					  (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.MachineInternalTimeout) ||
					  (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.MachineMalFunctionError) ||
					  (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.PaymentCancelled)
					  )
					{
						_payData.IsErrorFound = true;

						OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
							new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now)
							{
								CustmerMsg = "",
								DisplayCustmerMsg = UIVisibility.VisibleDisabled
							});

						if (!_payData.IsStopTransWhenErrFound)
						{
							if (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.MachineMalFunctionError)
							{
								if (_cashMachine.IsMalfunction == false)
								{
									_countDown.DelayCount = 120;
								}
								_cashMachine.IsMalfunction = true;
								_payData.IsStopTransWhenErrFound = true;
								_payData.IsRequestQuitProcess = true;

								_countDown.CancelFlag = true;
								_b2bPayBusin.CancelRequest(CurrentProcessId, CurrentNetProcessId);

								OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
									new UIError(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { DisplayErrorMsg = UIVisibility.VisibleEnabled, ErrorMessage = "Machine MalFunction Error" });

								OnShowProcessingMessageSendEvent(CurrentNetProcessId, $@"Machine MalFunction Error; {e?.Error?.Message}");
							}
							else if (!_payData.IsStopTransWhenErrFound == false)
							{
								_payData.IsStopTransWhenErrFound = true;

								if (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.MachineInternalTimeout)
								{
									_payData.IsRequestQuitProcess = true;

									_countDown.DelayCount = 120;
									_countDown.CancelFlag = true;
									_b2bPayBusin.CancelRequest(CurrentProcessId, CurrentNetProcessId);
									
									OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
										new UIError(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { DisplayErrorMsg = UIVisibility.VisibleEnabled, ErrorMessage = "Cash Machine Internal Timeout" });

									OnShowProcessingMessageSendEvent(CurrentNetProcessId, $@"Cash Machine Internal Timeout; {e?.Error?.Message}");
								}
								else if (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.PaymentCancelled)
								{
									_payData.IsRequestQuitProcess = true;
									_countDown.DelayCount = 120;

									OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
										new UIError(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { DisplayErrorMsg = UIVisibility.VisibleEnabled, ErrorMessage = "Transaction Cancelled" });

									OnShowProcessingMessageSendEvent(CurrentNetProcessId, $@"Transaction Cancelled; {e?.Error?.Message}");
								}
								else
								{
									string errMsg = e?.Error?.Message;
									errMsg = string.IsNullOrWhiteSpace((errMsg ?? "")) ? "Unknown Error; Sect-A1" : errMsg.Trim();

									OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
										new UIError(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { DisplayErrorMsg = UIVisibility.VisibleEnabled, ErrorMessage = errMsg });

									OnShowProcessingMessageSendEvent(CurrentNetProcessId, $@"{errMsg}; " + Enum.GetName(typeof(AppDecorator.Devices.DeviceProgressStatus), e.PaymentStatus));
								}
							}
						}
					}
					else
					{
						if (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.StartPayment)
						{
							OnShowProcessingMessageSendEvent(CurrentNetProcessId, null, e.KioskMessage);
						}
						else if (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.CashAcceptableInfor)
						{
							OnShowProcessingMessageSendEvent(CurrentNetProcessId, e.Message, e.KioskMessage);
						}
						else if (e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.Refunding)
						{
							if (_payData.IsAllowToCancel)
							{
								_payData.IsAllowToCancel = false;
								OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
									new UISetCancelPermission(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { IsCancelEnabled = UIAvailability.Disabled });
							}
							_payData.IsRefundCompleted = false;

							if ((!_payData.IsStopTransWhenErrFound) && ((e?.Message) != null))
								OnShowProcessingMessageSendEvent(CurrentNetProcessId, e.Message, e.KioskMessage);
						}
						else if ((e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.FirstPayment) ||
							(e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.SubPayment) ||
							(e.PaymentStatus == AppDecorator.Devices.DeviceProgressStatus.LastPayment))
						{
							UIOutstandingPayment outPay = (UIOutstandingPayment)e.KioskMessage;

							if (outPay.IsPaymentDone)
							{
								if (_payData.IsAllowToCancel)
								{
									_payData.IsAllowToCancel = false;
									OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
										new UISetCancelPermission(CurrentNetProcessId, CurrentProcessId, DateTime.Now) { IsCancelEnabled = UIAvailability.Disabled });
								}

								if (outPay.IsRefundRequest)
								{
									outPay.CustmerMsg = $@"Payment done; Remember to collect all your refund (RM {outPay.RefundAmount:#,##0.00});";
									outPay.ProcessMsg = outPay.CustmerMsg;
								}
								else
								{
									outPay.CustmerMsg = $@"Payment completed.";
									outPay.ProcessMsg = outPay.CustmerMsg;
								}
							}
							else
							{
								outPay.CustmerMsg = $@"Outstanding Amount RM {outPay.OutstandingAmount:#,##0.00};";
								outPay.ProcessMsg = outPay.CustmerMsg;
							}

							OnShowProcessingMessageSendEvent(CurrentNetProcessId, null, outPay);
						}
						else
						{
							if (!_payData.IsStopTransWhenErrFound)
								OnShowProcessingMessageSendEvent(CurrentNetProcessId, null,
									new UICustomerMessage(CurrentNetProcessId, CurrentProcessId, DateTime.Now)
									{
										CustmerMsg = "Processing ..",
										DisplayCustmerMsg = UIVisibility.VisibleEnabled 
									});
						}

						OnShowProcessingMessageSendEvent(CurrentNetProcessId, e.Message ?? "..");
					}
				}
				else
					OnShowProcessingMessageSendEvent(CurrentNetProcessId, e.Message ?? "..");
			}
		}

		#endregion

		#region ----- Local Data Class (x) -----

		class xCoundDownInfo : IDisposable
		{
			public const int DefaultCountDelay = 132;

			public Thread CoundDownThreadWorker { get; set; } = null;
			public object TimerDelayLock { get; } = new object();
			public bool LoopEnabled { get; set; } = false;
			public int DelayCount { get; set; } = DefaultCountDelay;
			public bool CancelFlag { get; set; } = false;

			public xCoundDownInfo()
			{ }

			public void Dispose()
			{
				CoundDownThreadWorker = null;
			}
		}

		class xPaymentInfo : IDisposable
		{
			public string FirstErrorMsg { get; set; } = null;
			public ITrxCallBackEventArgs TransCompletedResult { get; set; } = null;

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXX Control Flags XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			// (1) Payment Switching Control Flag
			public bool IsAllowToCancel { get; set; } = true;
			// (2) Before Payment
			public bool IsProcActivated { get; set; } = false;        /* "Is Process Activated" - To Activate a payment transaction, and to stop the transaction*/
																	  // (3) Payment progressing..
			public bool IsPaymentFound { get; set; } = false;
			public bool IsErrorFound { get; set; } = false;
			// (4) Ending Payment
			public bool? IsRefundCompleted { get; set; } = null;
			public bool IsStopTransWhenErrFound { get; set; } = false;
			public bool IsEarlyAborted { get; set; } = false;
			public bool IsRequestQuitProcess { get; set; } = false;    /* Request to stop a payment transaction refer to a state like timeout or after payment has completed */
			public bool IsTransCancelled { get; set; } = false;
			// (5) End & Quit Payment
			public bool IsTransCompleted { get; set; } = false;
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			public xPaymentInfo() { }

			public void ResetToStartPayment()
			{
				IsProcActivated = true;

				FirstErrorMsg = null;
				TransCompletedResult = null;

				IsAllowToCancel = true;
				IsPaymentFound = false;
				IsErrorFound = false;
				IsRefundCompleted = null;
				IsStopTransWhenErrFound = false;
				IsEarlyAborted = false;
				IsRequestQuitProcess = false;
				IsTransCancelled = false;
				IsTransCompleted = false;
			}

			public void Dispose() { }
		}

		class xCashMachineInfo : IDisposable
		{
			public bool IsResponseFound { get; set; } = false;
			public bool IsMalfunction { get; set; } = false;

			public xCashMachineInfo() { }
			public void Dispose() { }
		}

		#endregion

	}
}