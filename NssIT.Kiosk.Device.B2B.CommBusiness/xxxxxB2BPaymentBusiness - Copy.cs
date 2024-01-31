using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using NssIT.Kiosk;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.Device.B2B;
using NssIT.Kiosk.Device.B2B.OrgApi;
using NssIT.Kiosk.Device.B2B.AccessSDK;
using NssIT.Kiosk.AppDecorator.Devices.Payment.ErrorNode;

namespace NssIT.Kiosk.Device.B2B.CommBusiness
{
	public class xxxxxB2BPaymentBusiness_Copy : IDisposable
	{
		private const string LogChannel = "B2BAccess";
		private const double _minimumAmount = 0.1;
		private const int _maxRefundNoteCount = 15;

		public const int SaleMaxWaitingSec = 390; /* 6.30 minutes */

		private bool _disposed = false;
		private bool? _isB2BValid = null; /* null: not ready; false: Shutdown; true: ready */
		private bool _isRaisedCompleteCallbackEvent = false;
		private bool _cancelRequested = false;

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 10);
		private B2BRMCashData _currB2BCashData = null;

		private Thread _threadWorker = null;
		private Thread _inProgressEventThreadWorker = null;
		private ConcurrentQueue<OrgApi.PayB2BCommandPack> _payB2BCommandPackList = new ConcurrentQueue<OrgApi.PayB2BCommandPack>();
		private ConcurrentQueue<OrgApi.Events.InProgressEventArgs> _InProgressEventObjList = new ConcurrentQueue<OrgApi.Events.InProgressEventArgs>();

		public event EventHandler<OrgApi.Events.TrxCallBackEventArgs> OnCompleted;
		public event EventHandler<OrgApi.Events.InProgressEventArgs> OnInProgress;

		public xxxxxB2BPaymentBusiness_Copy()
		{
			_threadWorker = new Thread(new ThreadStart(B2BProcessThreadWorking));
			_threadWorker.IsBackground = true;
			_threadWorker.Start();

			_inProgressEventThreadWorker = new Thread(new ThreadStart(DispatchInProgressEventThreadWorking));
			_inProgressEventThreadWorker.IsBackground = true;
			_inProgressEventThreadWorker.Start();
		}

		public void Dispose()
		{
			lock (_payB2BCommandPackList)
			{
				_disposed = true;
				_isB2BValid = false;
				try
				{
					if (OnCompleted != null)
					{
						Delegate[] delgList = OnCompleted.GetInvocationList();
						foreach (EventHandler<OrgApi.Events.TrxCallBackEventArgs> delg in delgList)
						{
							OnCompleted -= delg;
						}
					}
					if (OnInProgress != null)
					{
						Delegate[] delgList = OnInProgress.GetInvocationList();
						foreach (EventHandler<OrgApi.Events.InProgressEventArgs> delg in delgList)
						{
							OnInProgress -= delg;
						}
					}
				}
				catch { }

				OnCompleted = null;
				OnInProgress = null;
				Monitor.PulseAll(_InProgressEventObjList);
				Monitor.PulseAll(_payB2BCommandPackList);
			}

			_log = null;
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

				return (_isB2BValid == true) ? true : false;
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

			OrgApi.PayB2BCommandPack newData = new OrgApi.PayB2BCommandPack()
			{
				ProcessId = ProcessId,
				Command = OrgApi.SalesCommand.Sales
				,
				Amount = amount,
				DocNumbers = docNumbers
				,
				MaxWaitingSec = SaleMaxWaitingSec
			};

			if (AddPaymentData(newData) == false)
			{
				if (_disposed)
					throw new Exception("System is shutting down; Sect-02;");
				else if (_isB2BValid.HasValue == false)
					throw new Exception("System busy. Cash machine is not ready.");
				else if (_isB2BValid == false)
					throw new Exception("System is shutting down; Sect-03;");
				else
					throw new Exception("System busy.");
			}

			_cancelRequested = false;
		}

		private bool AddPaymentData(OrgApi.PayB2BCommandPack data)
		{
			if (data == null)
				throw new Exception("Data parameter cannot be NULL at AddPaymentData");

			lock (_payB2BCommandPackList)
			{
				if (_disposed)
					return false;

				if ((_payB2BCommandPackList.Count == 0) && (_isB2BValid == true))
				{
					_payB2BCommandPackList.Enqueue(data);
					Monitor.PulseAll(_payB2BCommandPackList);
					return true;
				}
				else
					return false;
			}
		}

		public void CancelRequest()
		{
			_cancelRequested = true;
			Log.LogText(LogChannel, CurrentProcessId, "Cancel Transaction Request", "A01", "B2BAccess.CancelRequest");
		}

		#region ----- Handle In progress Event -----

		/// <summary>
		/// Event handle for _ecr.OnInProgress. This will pass the handle to another event handle.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void b2bApi_OnInProgress(object sender, OrgApi.Events.InProgressEventArgs e)
		{
			Log.LogText(LogChannel, CurrentProcessId, e, "A01", "B2BAccess.b2bApi_OnInProgress"
				, extraMsg: $@"MsgObj: InProgressEventArgs");

			OrgApi.NewInsertedBill newBillObj = null;

			if (e?.PaymentStatus == PaymentProgressStatus.BillNoteConfirm)
			{
				OrgApi.NewInsertedBill billObj = (OrgApi.NewInsertedBill)e.MsgObj;
				if (billObj != null)
				{
					if ((billObj.DigitBillType > 0) && (_currB2BCashData.InsertedAmount == 0))
						e.PaymentStatus = PaymentProgressStatus.FirstPayment;
					else
						e.PaymentStatus = PaymentProgressStatus.SubPayment;

					_currB2BCashData.Pay(billObj.DigitBillType);

					if ((billObj.DigitBillType > 0) && (_currB2BCashData.IsPaymentDone() == true) && (e.PaymentStatus != PaymentProgressStatus.FirstPayment))
						e.PaymentStatus = PaymentProgressStatus.LastPayment;

					billObj.OutstandingAmount = _currB2BCashData.GetOutstandingPayment();

					newBillObj = new NewInsertedBill()
					{
						DigitBillType = billObj.DigitBillType,
						Price = _currB2BCashData.Price,
						InsertedAmount = _currB2BCashData.InsertedAmount,
						OutstandingAmount = _currB2BCashData.GetOutstandingPayment(),
						IsPaymentDone = _currB2BCashData.IsPaymentDone(),
						RefundAmount = _currB2BCashData.RefundAmount,
						IsRefundRequest = _currB2BCashData.IsRefundRequest()
					};
				}
			}

			if (e != null)
			{
				AddInProgressEvent(new OrgApi.Events.InProgressEventArgs()
				{
					Error = e.Error,
					Message = e.Message,
					PaymentStatus = e.PaymentStatus,
					MsgObj = newBillObj
				});
			}
		}

		private void DispatchInProgressEventThreadWorking()
		{
			Log.LogText(LogChannel, CurrentProcessId, "Start - DispatchInProgressEventThreadWorking", "A01", "B2BAccess.DispatchInProgressEventThreadWorking");

			OrgApi.Events.InProgressEventArgs inProgEv = null;

			while (!_disposed)
			{
				try
				{
					//inProgEv = await GetNextInProgressEvent();
					inProgEv = GetNextInProgressEvent();
					if (inProgEv != null)
					{
						//OnInProgressSendEvent(inProgEv, "A10", "B2BAccess.DispatchInProgressEventThreadWorking");
						if ((inProgEv != null) && (OnInProgress != null))
						{
							OnInProgress.Invoke(null, inProgEv);
							//Log.LogText(LogChannel, CurrentProcessId, inProgEv, "A10", "B2BAccess.DispatchInProgressEventThreadWorking"
							//	, extraMsg: $@"MsgObj: InProgressEventArgs");
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BAccess.DispatchInProgressEventThreadWorking");
				}
			}

			inProgEv = null;
			while (_InProgressEventObjList.Count > 0)
			{
				_InProgressEventObjList.TryDequeue(out inProgEv);

				inProgEv.Dispose();
			}

			Log.LogText(LogChannel, CurrentProcessId, "End - DispatchInProgressEventThreadWorking", "A100", "B2BAccess.DispatchInProgressEventThreadWorking");

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			OrgApi.Events.InProgressEventArgs GetNextInProgressEvent()
			{
				OrgApi.Events.InProgressEventArgs retEvn = null;

				try
				{
					lock (_InProgressEventObjList)
					{
						if (_isB2BValid == true)
						{
							if (_InProgressEventObjList.Count == 0)
							{
								Monitor.Wait(_InProgressEventObjList, _MaxWaitPeriod);
							}
							if (_InProgressEventObjList.TryDequeue(out retEvn))
							{
								return retEvn;
							}
						}
						else
							Monitor.Wait(_InProgressEventObjList, _MaxWaitPeriod);
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BAccess.GetNextInProgressEvent");
				}

				return null;
			}
		}

		private void AddInProgressEvent(OrgApi.Events.InProgressEventArgs inProcEvent)
		{
			(new Thread(new ThreadStart(new Action(() => {
				try
				{
					lock (_InProgressEventObjList)
					{
						if (inProcEvent != null)
						{
							// Clear Outstanding Events If Payment Has Done.
							if ((inProcEvent.PaymentStatus == PaymentProgressStatus.PaymentDone)
								|| (inProcEvent.PaymentStatus == PaymentProgressStatus.PaymentCancelled)
								|| (inProcEvent.PaymentStatus == PaymentProgressStatus.PaymentTimeout))
							{
								OrgApi.Events.InProgressEventArgs inProgEv = null;
								while (_InProgressEventObjList.Count > 0)
								{
									_InProgressEventObjList.TryDequeue(out inProgEv);
									inProgEv.Dispose();
								}
							}

							_InProgressEventObjList.Enqueue(inProcEvent);
							Monitor.PulseAll(_InProgressEventObjList);
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BAccess.AddInProgressEvent");
				}
			})))
			{ IsBackground = true })
			.Start();
		}

		#endregion

		#region ----- B2BProcessThreadWorking -----
		//DateTime _endTime = DateTime.Now;
		private void B2BProcessThreadWorking()
		{
			OrgApi.PayB2BCommandPack data = null;
			PaymentResultStatus payCompStatus = PaymentResultStatus.InitNewState;

			_isB2BValid = null;

			using (OrgApi.B2BApi b2bApi = new OrgApi.B2BApi())
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
							_isRaisedCompleteCallbackEvent = false;
							data = null;
							data = GetNextPayTransactionData();

							if ((data != null) && (OnCompleted == null))
							{
								throw new AccessSDK.Error.B2BMalfunctionException("Unhandle OnCompleteCallback (II) event. Transaction void.");
							}
							else if (data != null)
							{
								if ((data.HasCommand) && (data.ProcessDone == false))
								{
									//_ecr.Stop = false;
									CurrentProcessId = data.ProcessId ?? "-----";
									b2bApi.CurrentProcessId = CurrentProcessId;
									payCompStatus = PaymentResultStatus.InitNewState;

									// $@"Start Process; Thread Hash Code : {Thread.CurrentThread.GetHashCode()};"
									Log.LogText(LogChannel, CurrentProcessId, data, "B01", "B2BAccess.PayECRProcessThreadWorking",
										extraMsg: $@"Start Process; Managed Thread Id : {Thread.CurrentThread.ManagedThreadId}; Thread Hash Code : {Thread.CurrentThread.GetHashCode()}; MsgObj: PayB2BData; ");

									// ----- Collect Payment 
									_currB2BCashData = CollectPayment(b2bApi, data, _currB2BCashData, out payCompStatus);

									// ----- Refunding 
									Exception refundErr = null;
									RefundPayment(b2bApi, _currB2BCashData, payCompStatus, out refundErr);
									if (refundErr is UnableRefundException)
									{
										// UnableRefundException only occur when sales has success payment but not able to refund because no enough cash.
										payCompStatus = PaymentResultStatus.FailRefundOnPaymentSuccess;

										refundErr = null;
										RefundPayment(b2bApi, _currB2BCashData, payCompStatus, out refundErr);
									}
									if (refundErr != null)
										throw refundErr;
									// ----- 

									// xxxxx ----- Send Payment Completed Event ----- xxxxx
									{
										if (payCompStatus != PaymentResultStatus.InitNewState)
										{
											OnCompletedSendEvent(payCompStatus, "Completed01", _currB2BCashData);
										}
										else
										{
											// -- .. abnormal stage
											throw new AccessSDK.Error.B2BMalfunctionException("Unregconised payment status.") { LastCustPaymentInfo = _currB2BCashData };
										}
									}

									data.ProcessDone = true;
								}
							}
						}
						catch (AccessSDK.Error.B2BMalfunctionException exM)
						{
							Log.LogError(LogChannel, _currProcessId, exM, "Ex01", classNMethodName: "B2BAccess.OnPayECRProcess");

							if (_isRaisedCompleteCallbackEvent == false)
							{
								OnCompletedSendEvent(PaymentResultStatus.MachineMalfunction, "Completed-Ex01", _currB2BCashData, exM);
							}

							throw exM;
						}
						catch (Exception ex)
						{
							Log.LogError(LogChannel, _currProcessId, ex, "Ex02", classNMethodName: "B2BAccess.OnPayECRProcess");

							if (_isRaisedCompleteCallbackEvent == false)
							{
								OnCompletedSendEvent(PaymentResultStatus.Error, "Completed-Ex20", _currB2BCashData, ex);
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

					if (_isRaisedCompleteCallbackEvent == false)
					{
						OnCompletedSendEvent(PaymentResultStatus.MachineMalfunction, "Completed99", _currB2BCashData, ex);
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

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			OrgApi.PayB2BCommandPack GetNextPayTransactionData()
			{
				OrgApi.PayB2BCommandPack retData = null;

				lock (_payB2BCommandPackList)
				{
					if (_isB2BValid == true)
					{
						if (_payB2BCommandPackList.Count == 0)
						{
							Monitor.Wait(_payB2BCommandPackList, _MaxWaitPeriod);
						}
						if (_payB2BCommandPackList.TryDequeue(out retData))
						{
							//_workInProgress = true;
							return retData;
						}
					}
					else
						Monitor.Wait(_payB2BCommandPackList, _MaxWaitPeriod);
				}
				return null;
			}

			void OnCompletedSendEvent(PaymentResultStatus completedStatus, string lineTag, ICustomerPaymentInfo custPayInfo, Exception error = null)
			{
				Log.LogText(LogChannel, _currProcessId, "Start - SendCompleteTransactionEvent", $@"A01; Line: {lineTag}", classNMethodName: "B2BAccess.SendCompleteTransactionEvent");

				OrgApi.Events.TrxCallBackEventArgs compEv = new OrgApi.Events.TrxCallBackEventArgs()
				{
					ResultStatus = completedStatus,
					Remark = $@"Customer's Last Payment Info: {custPayInfo?.GetLastCustomerPaymentInfo()}",
					Error = error
				};

				try
				{
					// Make sure all Inprogress Event has been sent.
					if (_InProgressEventObjList.Count > 0)
					{
						DateTime timeOut = DateTime.Now.AddSeconds(5);
						while ((_InProgressEventObjList.Count > 0) && (timeOut.Subtract(DateTime.Now).TotalSeconds > 0))
						{
							Task.Delay(500).Wait();
						}
					}

					_isRaisedCompleteCallbackEvent = true;
					OnCompleted?.Invoke(null, compEv);
					//System.Windows.Forms.Application.DoEvents();
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, _currProcessId, new Exception($@"Unhandle error exception at OnCompleteCallback event;", ex), "E01", classNMethodName: "B2BAccess.SendCompleteTransactionEvent");
				}

				Log.LogText(LogChannel, _currProcessId, "End - SendCompleteTransactionEvent", "A10", classNMethodName: "B2BAccess.SendCompleteTransactionEvent");
			}

			B2BRMCashData CollectPayment(OrgApi.B2BApi b2bApi, OrgApi.PayB2BCommandPack commData, B2BRMCashData b2bCashData, out PaymentResultStatus payResStatus)
			{
				Log.LogText(LogChannel, CurrentProcessId, $@"Start - CollectPayment", "A01", "B2BAccess.CollectPayment");

				payResStatus = PaymentResultStatus.InitNewState;

				DateTime endTime = DateTime.Now.AddSeconds(commData.MaxWaitingSec);
				bool paymentTimeout = false;

				b2bCashData.InitNewPrice(commData.Amount, commData.ProcessId);
				SetB2BToNormalStage(b2bApi, 120, "Start Payment", b2bCashData);

				// ----- Start New Payment
				StartCashPaymentEvtMessage newPay = new StartCashPaymentEvtMessage(CurrentProcessId) { Price = commData.Amount };
				AddInProgressEvent(new OrgApi.Events.InProgressEventArgs()
				{
					MsgObj = newPay,
					PaymentStatus = PaymentProgressStatus.StartPayment,
				});

				// ----- Get Activate Bill Type 
				AcceptableBanknoteEvtMessage acceptableBanknote = null;
				byte[] billTypeActivationData = b2bCashData.GetBillTypeActivationData(commData.Amount, b2bApi.B2BCassetteList, out acceptableBanknote);

				// ----- Acknowledge Acceptable Banknote
				AddInProgressEvent(new OrgApi.Events.InProgressEventArgs()
				{
					MsgObj = acceptableBanknote,
					PaymentStatus = PaymentProgressStatus.CashAcceptableInfor,
				});

				// * outstanding work -> send B2B Malfunction if billTypeActivationData is null (because not enough for bill changed); 
				// * Log current available bill qty for each cassette.

				Log.LogText(LogChannel, CurrentProcessId, $@"Activating Bill Type; Byte List: {AppDecorator.Common.Common2s.BytesToNumbersStr(billTypeActivationData)}"
					, "B02", "B2BAccess.CollectPayment");

				Answer b2bAnswer = b2bApi.RunCommand(CCNETCommand.ENABLE_BILL_TYPES, billTypeActivationData);

				Log.LogText(LogChannel, CurrentProcessId, $@"B2B Enable Bill Type has Completed."
					, "B03", "B2BAccess.CollectPayment");

				// ----- Loop to check result when payment in progress 
				while ((!paymentTimeout) && (!_cancelRequested) && (!b2bCashData.IsPaymentDone()))
				{
					Task.Delay(500).Wait();

					if (endTime.Subtract(DateTime.Now).TotalMilliseconds <= 0)
					{
						paymentTimeout = true;
					}
				}

				Log.LogText(LogChannel, CurrentProcessId, $@"End Collecting Payment", "B10", "B2BAccess.CollectPayment");

				// .. payment ending..
				if ((_cancelRequested) || (paymentTimeout))
					payResStatus = _cancelRequested ? PaymentResultStatus.Cancel : PaymentResultStatus.Timeout;

				else if (b2bCashData.IsPaymentDone())
					payResStatus = PaymentResultStatus.Success;

				SetB2BToNormalStage(b2bApi, codeLocTag: "Ending Collect Payment ", currCustInfo: b2bCashData);

				return b2bCashData;
			}

			bool RefundPayment(OrgApi.B2BApi b2bApi, B2BRMCashData b2bCashData, PaymentResultStatus payResStatus, out Exception error)
			{
				error = null;
				bool hasRefund = false;
				RefundType typeOfRefund = RefundType.New;

				// ----- Check For Possible Refunding
				CCNET.BillToBill.DispenseParameter[] refundNoteParams = null;

				if ((payResStatus == PaymentResultStatus.Cancel) || (payResStatus == PaymentResultStatus.Timeout) || (payResStatus == PaymentResultStatus.FailRefundOnPaymentSuccess))
				{
					if (b2bCashData.InsertedAmount > 0)
					{
						Log.LogText(LogChannel, CurrentProcessId, $@"Refund After {
							Enum.GetName(typeof(PaymentResultStatus), payResStatus)}; Inserted Amount : {b2bCashData.InsertedAmount };", "A02", "B2BAccess.RefundCash");

						refundNoteParams = b2bCashData.GetRefundParameters(b2bApi.B2BCassetteList, RefundType.CancelPayment, out error);

						typeOfRefund = RefundType.CancelPayment;
					}
				}
				else if (payResStatus == PaymentResultStatus.Success)
				{
					if (b2bCashData.IsRefundRequest())
					{
						Log.LogText(LogChannel, CurrentProcessId, $@"Refund After Payment Success; Refund Amount : {b2bCashData.RefundAmount };", "A03", "B2BAccess.RefundCash");

						refundNoteParams = b2bCashData.GetRefundParameters(b2bApi.B2BCassetteList, RefundType.CompletePayment, out error);

						typeOfRefund = RefundType.CompletePayment;
					}
				}

				if (error != null)
					return false;

				// ----- Refund .. if necessary
				if ((refundNoteParams == null) || (refundNoteParams.Length == 0))
					return false;

				List<CCNET.BillToBill.DispenseParameter> currRefundList = new List<CCNET.BillToBill.DispenseParameter>();
				int
					totalRefundNoteCount = 0,
					outstandingRefundNoteCount = 0,
					refundTotalRoundCount = 0,
					refundRoundCount = 0;

				// ----- Duplicate Refund parameter "refundParams" to "locRefundParams". This is to avoid conflict in processing.
				CCNET.BillToBill.DispenseParameter[] locRefundParams;
				List<CCNET.BillToBill.DispenseParameter> refundList = new List<CCNET.BillToBill.DispenseParameter>();

				foreach (CCNET.BillToBill.DispenseParameter par in refundNoteParams)
				{
					totalRefundNoteCount += par.BillCount;
					refundList.Add(new CCNET.BillToBill.DispenseParameter() { BillCount = par.BillCount, BillType = par.BillType });
				}
				locRefundParams = refundList.ToArray();
				// ----- 

				refundTotalRoundCount = (int)Math.Ceiling(((decimal)totalRefundNoteCount) / ((decimal)_maxRefundNoteCount));

				outstandingRefundNoteCount = totalRefundNoteCount;

				decimal price = b2bCashData.Price;
				decimal insertedAmt = b2bCashData.InsertedAmount;
				decimal refundAmt = (typeOfRefund == RefundType.New) ? 0.00M :
					(typeOfRefund == RefundType.CompletePayment) ? b2bCashData.RefundAmount : b2bCashData.InsertedAmount;

				//decimal sellingPrice, decimal paidAmount, decimal returnAmount

				while (outstandingRefundNoteCount > 0)
				{
					int refNoteCount = 0;
					CCNET.BillToBill.DispenseParameter outPar;

					// ----- ----- ----- ----- -----
					// Create a Refund List "currRefundList" refer to the limitation of _maxRefundNoteCount.
					currRefundList.Clear();
					for (int inx = 0; inx < locRefundParams.Length; inx++)
					{
						outPar = locRefundParams[inx];

						if (outPar.BillCount > 0)
						{
							CCNET.BillToBill.DispenseParameter rp = new CCNET.BillToBill.DispenseParameter();

							rp.BillType = outPar.BillType;

							if ((refNoteCount + outPar.BillCount) > _maxRefundNoteCount)
							{
								rp.BillCount = (_maxRefundNoteCount - refNoteCount);
								outPar.BillCount -= rp.BillCount;
							}
							else
							{
								rp.BillCount = outPar.BillCount;
								outPar.BillCount = 0;
							}

							locRefundParams[inx] = outPar;
							refNoteCount += rp.BillCount;

							currRefundList.Add(rp);
							outstandingRefundNoteCount -= rp.BillCount;

							if (refNoteCount >= _maxRefundNoteCount)
								break;
						}
					}
					// ----- 
					// Dispense Note for refunding
					if (currRefundList.Count > 0)
					{
						refundRoundCount += 1;

						if (RefundBanknote(b2bApi, b2bCashData, currRefundList.ToArray(), $@"({refundRoundCount}/{refundTotalRoundCount})", typeOfRefund, price, insertedAmt, refundAmt) == true)
							hasRefund = true;

						currRefundList.Clear();
					}
					// ----- 
				}

				return hasRefund;
			}

			bool RefundBanknote(OrgApi.B2BApi machApi, B2BRMCashData cashData, CCNET.BillToBill.DispenseParameter[] refundParm, string roundTag,
					RefundType typeRefund, decimal sellingPrice, decimal paidAmount, decimal returnAmount)
			{
				bool refundDone = false;

				string refundMsg = "";
				string currencyStr = null;
				string errMsg = null;
				int billValue = 0;

				foreach (CCNET.BillToBill.DispenseParameter rfPar in refundParm)
				{
					currencyStr = null;
					errMsg = null;
					billValue = 0;

					if (cashData.GetCurrencyAmount(rfPar.BillType, out currencyStr, out billValue, out errMsg))
						refundMsg += $@"{currencyStr} {billValue} x {rfPar.BillCount}; ";
					else
						refundMsg += $@"Error when reading (for refund) bill type {rfPar.BillType}; {errMsg ?? "--"}; ";
				}

				if (refundMsg.Length > 0)
				{
					AddInProgressEvent(new OrgApi.Events.InProgressEventArgs()
					{
						Message = $@"{roundTag} - Refund : {refundMsg}",
						PaymentStatus = PaymentProgressStatus.Refunding,
						MsgObj = new RefundPaymentStatusEvtMessage()
						{
							CustmerMsg = $@"{roundTag} - Refund : {refundMsg}",
							ProcessMsg = $@"{roundTag} - Refund : {refundMsg}",
							Price = sellingPrice,
							PaidAmount = paidAmount,
							RefundAmount = returnAmount,
							TypeOfRefund = typeRefund
						}
					});
				}

				// *Need to check Bill Available in all cassete before Refund 
				machApi.Dispense(refundParm);
				refundDone = true;
				Task.Delay(500).Wait();

				SetB2BToNormalStage(machApi, codeLocTag: "After Refund", currCustInfo: cashData);

				return refundDone;
			}
		}

		private void SetB2BToNormalStage(OrgApi.B2BApi b2b, int maxWaitSec = 60, string codeLocTag = "-", ICustomerPaymentInfo currCustInfo = null)
		{
			if (b2b != null)
			{
				Exception exp02 = null;
				b2b.SetB2BToNormal(codeLocTag, out exp02);

				if (exp02 != null)
				{
					AccessSDK.Error.B2BMalfunctionException exMmf = new AccessSDK.Error.B2BMalfunctionException($@"Machine Malfunction at {codeLocTag}.", exp02) { LastCustPaymentInfo = currCustInfo };

					b2bApi_OnInProgress(null, new OrgApi.Events.InProgressEventArgs()
					{
						PaymentStatus = PaymentProgressStatus.MachineMalFunctionError,
						Error = exp02,
						Message = $@"SetToNormalStateError - Refer to .. {codeLocTag}"
					});
					throw exMmf;
				}
			}
		}
		#endregion

	}
}
