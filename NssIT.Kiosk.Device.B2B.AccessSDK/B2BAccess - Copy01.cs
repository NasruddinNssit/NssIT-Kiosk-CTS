using CCNET;
using CCNET.BillToBill;
using NssIT.Kiosk.Tools.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk;
using System.Collections.Concurrent;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.B2B.OrgApi;

namespace NssIT.Kiosk.B2B.LocalSDK
{
	public class B2BAccess : IDisposable 
	{
		private const string LogChannel = "B2BAccess";
		private const double _minimumAmount = 0.1;

		public const int SaleMaxWaitingSec = 360; /* 5.30 minutes */

		private bool _isTimeout = false;
		private bool? _isB2BValid = null; /* null: not ready; false: Shutdown; true: ready */
		bool _cancelRequested = false;

		private B2BRMCashData _currB2BCashData = null;

		private Thread _threadWorker = null;
		private ConcurrentQueue<B2B.OrgApi.PayB2BCommandData> _payB2BDataList = new ConcurrentQueue<B2B.OrgApi.PayB2BCommandData>();

		public event EventHandler<B2B.OrgApi.Events.TrxCallBackEventArgs> OnCompleteCallback;
		public event EventHandler<B2B.OrgApi.Events.InProgressEventArgs> OnInProgressCallback;

		public B2BAccess()
		{
			_threadWorker = new Thread(new ThreadStart(B2BProcessThreadWorking));
			_threadWorker.IsBackground = true;
			_threadWorker.Start();
						
			//OnCompleteCallback = null;
			//OnInProgressCall = null;
		}

		private bool _disposed = false;
		public void Dispose()
		{
			lock (_payB2BDataList)
			{
				_disposed = true;
				_isB2BValid = false;
				try
				{
					if (OnCompleteCallback != null)
					{
						Delegate[] delgList = OnCompleteCallback.GetInvocationList();
						foreach (EventHandler<B2B.OrgApi.Events.TrxCallBackEventArgs> delg in delgList)
						{
							OnCompleteCallback -= delg;
						}
					}
					if (OnInProgressCallback != null)
					{
						Delegate[] delgList = OnInProgressCallback.GetInvocationList();
						foreach (EventHandler<B2B.OrgApi.Events.InProgressEventArgs> delg in delgList)
						{
							OnInProgressCallback -= delg;
						}
					}
				}
				catch { }

				OnCompleteCallback = null;
				OnInProgressCallback = null;
				Monitor.PulseAll(_payB2BDataList);
			}
		}

		private DbLog _log = null;
		private DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		public bool IsCashMachineReady
		{
			get
			{
				if (_disposed)
					throw new Exception("Payment Device is shutting down;");

				else if (_isB2BValid == false)
					throw new Exception("Payment Device is shutting down; Sect-01");

				return (_isB2BValid == true) ? true: false;
			}
		}

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

		public void Pay(string ProcessId, decimal amount, string docNumbers = null)
		{
			CurrentProcessId = ProcessId;
			amount = (amount < 0) ? 0.00M : amount;
			docNumbers = (string.IsNullOrWhiteSpace(docNumbers)) ? null : docNumbers.Trim();

			B2B.OrgApi.PayB2BCommandData newData = new B2B.OrgApi.PayB2BCommandData() { ProcessId = ProcessId, Command = B2B.OrgApi.SalesCommand.Sales
				, Amount = amount, DocNumbers = docNumbers
				, MaxWaitingSec = SaleMaxWaitingSec};

			if (AddPaymentData(newData) == false)
			{
				if (!_disposed)
					throw new Exception("System is shutting down; Sect-02;");
				else if (!_isB2BValid.HasValue == false)
					throw new Exception("System busy. Cash machine is not ready.");
				else if (!_isB2BValid == false)
					throw new Exception("System is shutting down; Sect-03;");
				else
					throw new Exception("System busy.");
			}

			_cancelRequested = false;
			_isTimeout = false;
		}

		private bool AddPaymentData(B2B.OrgApi.PayB2BCommandData data)
		{
			if (data == null)
				throw new Exception("Data parameter cannot be NULL at AddPaymentData");

			lock (_payB2BDataList)
			{
				if (_disposed)
					return false;

				if ((_payB2BDataList.Count == 0) && (_isB2BValid == true))
				{
					_payB2BDataList.Enqueue(data);
					Monitor.PulseAll(_payB2BDataList);
					return true;
				}
				else
					return false;
			}
		}
				
		private static SemaphoreSlim _inProgressEventHandleLock = new SemaphoreSlim(1);
		ConcurrentQueue<B2B.OrgApi.Events.InProgressEventArgs> _InProgressEventObjList = new ConcurrentQueue<B2B.OrgApi.Events.InProgressEventArgs>();
		/// <summary>
		/// Event handle for _ecr.OnInProgress. This will pass the handle to another event handle.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void b2bApi_OnInProgress(object sender, B2B.OrgApi.Events.InProgressEventArgs e)
		{
			Log.LogText(LogChannel, CurrentProcessId, e, "A01", "B2BAccess.b2bApi_OnInProgress"
				, extraMsg: $@"MsgObj: InProgressEventArgs");

			B2B.OrgApi.NewIncomingBill newBillObj = null;

			if (e?.PaymentStatus == PaymentProgressStatus.BillNoteConfirm)
			{
				B2B.OrgApi.NewIncomingBill billObj = (B2B.OrgApi.NewIncomingBill)e.MsgObj;
				if (billObj != null)
				{
					if ((billObj.DigitBillType > 0) && (_currB2BCashData.InsertedAmount == 0))
					{
						e.PaymentStatus = PaymentProgressStatus.FirstPayment;
					}
					else
					{
						e.PaymentStatus = PaymentProgressStatus.SubPayment;
					}

					_currB2BCashData.Pay(billObj.DigitBillType);

					if ((billObj.DigitBillType > 0) && (_currB2BCashData.IsPaymentDone() == true) && (e.PaymentStatus != PaymentProgressStatus.FirstPayment))
					{
						e.PaymentStatus = PaymentProgressStatus.LastPayment;
					}

					billObj.OutstandingAmount = _currB2BCashData.GetOutstandingPayment();

					newBillObj = new NewIncomingBill() { DigitBillType = billObj.DigitBillType, OutstandingAmount = billObj.OutstandingAmount };
				}
			}

			try
			{
				_inProgressEventHandleLock.WaitAsync().Wait();

				if (e != null)
				{
					// Clear Outstanding Events If Payment Has Done.
					if ((e.PaymentStatus == PaymentProgressStatus.PaymentDone)
						|| (e.PaymentStatus == PaymentProgressStatus.PaymentCancelled)
						|| (e.PaymentStatus == PaymentProgressStatus.PaymentTimeout))
					{
						B2B.OrgApi.Events.InProgressEventArgs inProgEv = null;
						while (_InProgressEventObjList.Count > 0)
						{
							_InProgressEventObjList.TryDequeue(out inProgEv);
						}
					}

					_InProgressEventObjList.Enqueue(new B2B.OrgApi.Events.InProgressEventArgs() {
						Error = e.Error,
						Message = e.Message,
						PaymentStatus = e.PaymentStatus,
						MsgObj = newBillObj
					});
				}
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BAccess.PayWaveProgressHandle");
			}
			finally
			{
				_inProgressEventHandleLock.Release();
			}
		}

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 10);
		private B2B.OrgApi.PayB2BCommandData GetNextPayTransactionData()
		{
			B2B.OrgApi.PayB2BCommandData retData = null;

			lock (_payB2BDataList)
			{
				if (_isB2BValid == true)
				{
					if (_payB2BDataList.Count == 0)
					{
						Monitor.Wait(_payB2BDataList, _MaxWaitPeriod);
					}
					if (_payB2BDataList.TryDequeue(out retData))
					{
						//_workInProgress = true;
						return retData;
					}
				}
				else
					Monitor.Wait(_payB2BDataList, _MaxWaitPeriod);
			}
			return null;
		}

		
		public void CancelRequest()
		{
			_cancelRequested = true;
		}

		#region ----- B2BProcessThreadWorking -----
		DateTime _endTime = DateTime.Now;
		private void B2BProcessThreadWorking()
		{
			bool onCompleteCallbackEventFlag = false;
			B2B.OrgApi.PayB2BCommandData data = null;
			PaymentResultStatus payCompStatus = PaymentResultStatus.InitNewState;

			_isB2BValid = null;

			using (B2B.OrgApi.B2BApi b2bApi = new OrgApi.B2BApi())
			{
				b2bApi.OnInProgress += b2bApi_OnInProgress;

				try
				{
					if (b2bApi.SearchAndConnect() == true)
					{
						if (b2bApi.OpenConnection())
						{
							_isB2BValid = true;
							_currB2BCashData = new B2BRMCashData(Log, CurrentProcessId);
							_currB2BCashData.InitNewPrice(0, "--");
							_currB2BCashData.BillTableArray = b2bApi.BillTableArray;
						}
					}

					while ((_isB2BValid == true) && (!_disposed))
					{
						// * Check Machine Status, send Machine MalFunction Exception if Cash low OR Do not have coin.
						// * Always send Message to user about Acceptable of changing/refund Bill status.

						try
						{
						
							onCompleteCallbackEventFlag = false;
							data = null;
							data = GetNextPayTransactionData();

							if ((data != null) && (OnCompleteCallback == null))
							{
								throw new Error.B2BMalfunctionException("Unhandle OnCompleteCallback (II) event. Transaction void.");
							}
							else if (data != null)
							{
								if ((data.HasCommand) && (data.ProcessDone == false))
								{
									//_ecr.Stop = false;
									CurrentProcessId = data.ProcessId ?? "-----";
									b2bApi.CurrentProcessId = CurrentProcessId;
									
									// $@"Start Process; Thread Hash Code : {Thread.CurrentThread.GetHashCode()};"
									Log.LogText(LogChannel, CurrentProcessId, data, "B01", "B2BAccess.PayECRProcessThreadWorking",
										extraMsg: $@"Start Process; Managed Thread Id : {Thread.CurrentThread.ManagedThreadId}; Thread Hash Code : {Thread.CurrentThread.GetHashCode()}; MsgObj: PayB2BData; ");

									// xxxxx ----- Collect Payment ----- xxxxx
									{
										_endTime = DateTime.Now.AddSeconds(data.MaxWaitingSec);

										_currB2BCashData.InitNewPrice(data.Amount, data.ProcessId);

										SetB2BToNormalStage(b2bApi, 120, "Set Normal On Stage-01", _currB2BCashData);

										// ----- Activate Bill Type 
										byte[] billTypeActivationData = _currB2BCashData.GetBillTypeActivationData(data.Amount, b2bApi.B2BCassetteList);

										// * outstanding work -> send B2B Malfunction if billTypeActivationData is null (because not enough for bill changed); 
										// * Log current available bill qty for each cassette.

										Log.LogText(LogChannel, CurrentProcessId, $@"Activating Bill Type; Byte List: {AppDecorator.Tools.Common2s.BytesToNumbersStr(billTypeActivationData)}"
											, "B02", "B2BAccess.OnPayECRProcess");

										Answer b2bAnswer = b2bApi.RunCommand(CCNETCommand.ENABLE_BILL_TYPES, billTypeActivationData);

										Log.LogText(LogChannel, CurrentProcessId, $@"B2B Enable Bill Type has Completed."
											, "B03", "B2BAccess.OnPayECRProcess");

										// ----- Loop to check result when payment in progress 
										while ((!_isTimeout) && (!_cancelRequested) && (!_currB2BCashData.IsPaymentDone()))
										{
											if (DispatchEvents() == false)
												Task.Delay(500).Wait();

											if (_endTime.Subtract(DateTime.Now).TotalMilliseconds <= 0)
											{
												_isTimeout = true;
											}
										}
									}

									payCompStatus = PaymentResultStatus.InitNewState;

									// xxxxx ----- Refunding ----- xxxxx
									{
										SetB2BToNormalStage(b2bApi, codeLocTag: "Set Normal On Refunding-Stage-02", currCustInfo: _currB2BCashData);

										// ----- Check For Possible Refunding
										CCNET.BillToBill.DispenseParameter[] refundParams = null;

										if ((_cancelRequested) || (_isTimeout))
										{
											payCompStatus = _cancelRequested ? PaymentResultStatus.Cancel : PaymentResultStatus.Timeout;

											if (_currB2BCashData.InsertedAmount > 0)
											{
												Log.LogText(LogChannel, CurrentProcessId, $@"Refund After Payment Completed; Inserted Amount : {_currB2BCashData.InsertedAmount };", "B20", "B2BAccess.PayECRProcessThreadWorking");

												Exception ex = null;
												refundParams = _currB2BCashData.GetRefundParameters(b2bApi.B2BCassetteList, RefundType.CancelPayment, out ex);

												if (ex != null)
													throw new Error.B2BMalfunctionException("Refund Error when transaction cancelled.", ex) { LastCustPaymentInfo = _currB2BCashData };
											}
										}
										else if (_currB2BCashData.IsPaymentDone())
										{
											payCompStatus = PaymentResultStatus.Success;

											if (_currB2BCashData.IsRefundRequest())
											{
												Log.LogText(LogChannel, CurrentProcessId, $@"Refund After Payment Completed; Refund Amount : {_currB2BCashData.RefundAmount };", "B20", "B2BAccess.PayECRProcessThreadWorking");

												Exception ex = null;
												refundParams = _currB2BCashData.GetRefundParameters(b2bApi.B2BCassetteList, RefundType.CompletePayment, out ex);

												if (ex != null)
													throw new Error.B2BMalfunctionException("Refund Error when payment transaction has completed.", ex) { LastCustPaymentInfo = _currB2BCashData };
											}
										}

										// ----- Refund .. if necessary
										if (refundParams?.Length > 0)
										{
											string refundMsg = "";
											string currencyStr = null;
											string errMsg = null;
											int billValue = 0;											

											foreach (CCNET.BillToBill.DispenseParameter rfPar in refundParams)
											{
												currencyStr = null;
												errMsg = null;
												billValue = 0;

												if (_currB2BCashData.GetCurrencyAmount(rfPar.BillType, out currencyStr, out billValue, out errMsg))
													refundMsg += $@"{currencyStr} {billValue} x {rfPar.BillCount}; ";
												else
													refundMsg += $@"Error when reading (for refund) bill type {rfPar.BillType}; {errMsg??"--"}; ";
											}

											if (refundMsg.Length > 0)
											{
												SendInProgressCallbackEvent(new B2B.OrgApi.Events.InProgressEventArgs()
												{
													Message = $@"Refund : {refundMsg}",
													PaymentStatus = PaymentProgressStatus.Refunding
												}, "C01", "B2BAccess.B2BProcessThreadWorking");
											}

											// *Need to check Bill Available in all cassete before Refund 
											b2bApi.Dispense(refundParams);
										}
									}

									SetB2BToNormalStage(b2bApi, codeLocTag: "Set Cash Machine to Normal On Complete Transaction; Stage-03", currCustInfo: null);

									// xxxxx ----- Send Payment Completed Event ----- xxxxx
									{
										if (payCompStatus != PaymentResultStatus.InitNewState)
										{
											SendCompleteTransactionEvent(payCompStatus, "Completed01", _currB2BCashData);
											onCompleteCallbackEventFlag = true;
										}
										else
										{
											// -- .. abnormal stage
											throw new Error.B2BMalfunctionException("Unregconised payment status.") { LastCustPaymentInfo = _currB2BCashData };
										}
									}

									data.ProcessDone = true;
								}
							}
							else
							{
								DispatchEvents();
							}
						}
						catch (Error.B2BMalfunctionException exM)
						{
							Log.LogError(LogChannel, _currProcessId, exM, "Ex01", classNMethodName: "B2BAccess.OnPayECRProcess");

							if (onCompleteCallbackEventFlag == false)
							{
								Log.LogError(LogChannel, _currProcessId, exM, "Ex01", classNMethodName: "B2BAccess.OnPayECRProcess");
								SendCompleteTransactionEvent(PaymentResultStatus.MachineMalfunction, "Completed19", _currB2BCashData, exM);
								onCompleteCallbackEventFlag = true;
							}

							throw exM;
						}
						catch (Exception ex)
						{
							Log.LogError(LogChannel, _currProcessId, ex, "Ex02", classNMethodName: "B2BAccess.OnPayECRProcess");

							if (onCompleteCallbackEventFlag == false)
							{
								Log.LogText(LogChannel, _currProcessId, "Do OnCompleteCallback", "Ex10", "B2BAccess.OnPayECRProcess");
								SendCompleteTransactionEvent(PaymentResultStatus.Error, "Completed20", _currB2BCashData, ex);
								onCompleteCallbackEventFlag = true;
							}
						}
						finally
						{
							//_workInProgress = false;
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogFatal(LogChannel, _currProcessId, ex, "Ex05", classNMethodName: "B2BAccess.OnPayECRProcess");

					if (onCompleteCallbackEventFlag == false)
					{
						SendCompleteTransactionEvent(PaymentResultStatus.Error, "Completed99", _currB2BCashData, ex);
						onCompleteCallbackEventFlag = true;
					}
				}
				finally
				{
					Log.LogText(LogChannel, _currProcessId, "Quit B2B Process", "END_X01", "B2BAccess.OnPayECRProcess");

					b2bApi.CloseDevice();
					b2bApi.OnInProgress -= b2bApi_OnInProgress;
					_isB2BValid = false;
				}
			}
		}

		private void RefundCash(B2B.OrgApi.B2BApi b2bApi, PaymentResultStatus payCompStatus)
		{
			SetB2BToNormalStage(b2bApi, codeLocTag: "Set Normal On Refunding-Stage-02", currCustInfo: _currB2BCashData);

			// ----- Check For Possible Refunding
			CCNET.BillToBill.DispenseParameter[] refundParams = null;

			if ((payCompStatus == PaymentResultStatus.Cancel) || (payCompStatus == PaymentResultStatus.Timeout))
			{
				if (_currB2BCashData.InsertedAmount > 0)
				{
					Log.LogText(LogChannel, CurrentProcessId, $@"Refund After Payment Completed; Inserted Amount : {_currB2BCashData.InsertedAmount };", "B20", "B2BAccess.PayECRProcessThreadWorking");

					Exception ex = null;
					refundParams = _currB2BCashData.GetRefundParameters(b2bApi.B2BCassetteList, RefundType.CancelPayment, out ex);

					if (ex != null)
						throw new Error.B2BMalfunctionException("Refund Error when transaction cancelled.", ex) { LastCustPaymentInfo = _currB2BCashData };
				}
			}
			else if (payCompStatus == PaymentResultStatus.Success)
			{
				if (_currB2BCashData.IsRefundRequest())
				{
					Log.LogText(LogChannel, CurrentProcessId, $@"Refund After Payment Completed; Refund Amount : {_currB2BCashData.RefundAmount };", "B20", "B2BAccess.PayECRProcessThreadWorking");

					Exception ex = null;
					refundParams = _currB2BCashData.GetRefundParameters(b2bApi.B2BCassetteList, RefundType.CompletePayment, out ex);

					if (ex != null)
						throw new Error.B2BMalfunctionException("Refund Error when payment transaction has completed.", ex) { LastCustPaymentInfo = _currB2BCashData };
				}
			}

			// ----- Refund .. if necessary
			if (refundParams?.Length > 0)
			{
				string refundMsg = "";
				string currencyStr = null;
				string errMsg = null;
				int billValue = 0;

				foreach (CCNET.BillToBill.DispenseParameter rfPar in refundParams)
				{
					currencyStr = null;
					errMsg = null;
					billValue = 0;

					if (_currB2BCashData.GetCurrencyAmount(rfPar.BillType, out currencyStr, out billValue, out errMsg))
						refundMsg += $@"{currencyStr} {billValue} x {rfPar.BillCount}; ";
					else
						refundMsg += $@"Error when reading (for refund) bill type {rfPar.BillType}; {errMsg ?? "--"}; ";
				}

				if (refundMsg.Length > 0)
				{
					SendInProgressCallbackEvent(new B2B.OrgApi.Events.InProgressEventArgs()
					{
						Message = $@"Refund : {refundMsg}",
						PaymentStatus = PaymentProgressStatus.Refunding
					}, "C01", "B2BAccess.B2BProcessThreadWorking");
				}

				// *Need to check Bill Available in all cassete before Refund 
				b2bApi.Dispense(refundParams);
			}
		}

		private void SetB2BToNormalStage(OrgApi.B2BApi b2b, int maxWaitSec = 60, string codeLocTag = "-", ICustomerPaymentInfo currCustInfo = null)
		{
			if (b2b != null)
			{
				Exception exp02 = null;
				b2b.SetB2BToNormal(out exp02);

				if (exp02 != null)
				{
					Error.B2BMalfunctionException exMmf = new Error.B2BMalfunctionException($@"Machine Malfunction at {codeLocTag}.", exp02) { LastCustPaymentInfo = currCustInfo };

					b2bApi_OnInProgress(null, new B2B.OrgApi.Events.InProgressEventArgs()
					{
						PaymentStatus = PaymentProgressStatus.MachineMalFunctionError,
						Error = exp02,
						Message = $@"SetToNormalStateError - Refer to .. {codeLocTag}"
					});
					throw exMmf;
				}
			}
		}

		private void SendCompleteTransactionEvent(PaymentResultStatus completedStatus, string lineTag, ICustomerPaymentInfo custPayInfo, Exception error = null)
		{
			Log.LogText(LogChannel, _currProcessId, "Start - SendCompleteTransactionEvent", $@"A01; Line: {lineTag}", classNMethodName: "B2BAccess.SendCompleteTransactionEvent");

			B2B.OrgApi.Events.TrxCallBackEventArgs compEv = new B2B.OrgApi.Events.TrxCallBackEventArgs()
			{
				ResultStatus = completedStatus,
				Remark = $@"Customer's Last Payment Info: {custPayInfo?.GetLastCustomerPaymentInfo()}",
				Error = error
			};

			try
			{
				DispatchEvents();
				System.Windows.Forms.Application.DoEvents();

				OnCompleteCallback?.Invoke(null, compEv);
				System.Windows.Forms.Application.DoEvents();
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, _currProcessId, new Exception($@"Unhandle error exception at OnCompleteCallback event;", ex), "E01", classNMethodName: "B2BAccess.SendCompleteTransactionEvent");
			}

			Log.LogText(LogChannel, _currProcessId, "End - SendCompleteTransactionEvent", "A10", classNMethodName: "B2BAccess.SendCompleteTransactionEvent");
		}

		private bool DispatchEvents()
		{
			B2B.OrgApi.Events.InProgressEventArgs inProgEv = null;
			bool hasEvent = false;

			int evCount = _InProgressEventObjList.Count;

			// Grab all event instants into local memory (list).
			System.Collections.Generic.Queue<B2B.OrgApi.Events.InProgressEventArgs> list = new System.Collections.Generic.Queue<B2B.OrgApi.Events.InProgressEventArgs>();
			try
			{
				_inProgressEventHandleLock.WaitAsync().Wait();
				for (int evInx = 0; evInx < evCount; evInx++)
				{
					inProgEv = null;
					if (_InProgressEventObjList.TryDequeue(out inProgEv))
					{
						if (OnInProgressCallback != null)
						{
							list.Enqueue(inProgEv);
						}
					}
				}
			}
			catch
			{ }
			finally
			{
				_inProgressEventHandleLock.Release();
			}
			// --

			// Send event instants to Outter layer (or to UI); With this method, system will not disturb B2B API operation.
			while (list.Count > 0)
			{
				inProgEv = list.Dequeue();
				if (inProgEv != null)
				{
					hasEvent = true;
					SendInProgressCallbackEvent(inProgEv, "A10", "B2BAccess.DispatchEvents");
				}
			}
			return hasEvent;
			// --
		}

		private void SendInProgressCallbackEvent(B2B.OrgApi.Events.InProgressEventArgs inProgEv, string lineTag = "-", string clsMetName = "-")
		{
			if (inProgEv == null)
				return;

			try
			{
				if (inProgEv != null)
				{

					OnInProgressCallback.Invoke(null, inProgEv);
					System.Windows.Forms.Application.DoEvents();
				}
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, new Exception("Unhandle OnInProgressCall event.", ex), $@"E01-{lineTag}", clsMetName);
			}
		}

		#endregion






	}
}