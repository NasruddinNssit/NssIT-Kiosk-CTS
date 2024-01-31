using CCNET;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.AppDecorator.Devices.Payment.ErrorNode;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Events;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Error;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command.CommandSpec;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data.Machine;

using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Common.AppService;

namespace NssIT.Kiosk.Device.B2B.AccessSDK.CommandExec
{
	public class B2BPaymentCommandExecution : IDisposable
	{
		private const string LogChannel = "B2BAccess";
		private const int _maxRefundNoteCount = 15;

		private bool _isRaisedCompleteCallbackEvent = false;
		private bool _cancelRequested = false;
		private DateTime _lastInProgressEventReceivedTime = DateTime.MinValue;

		private decimal _coinRefundAmountOnSuccess = 0.00M;

		public string CurrentProcessId { get; private set; } = "-";
		public Guid? CurrentNetProcessId { get; private set; } = null;

		private B2BRMCashData CurrentB2BCashData { get; set; } = null;
		private OrgApi.B2BApi B2bApi { get; set; }
		private CommBusiness.B2BAccess B2bAccess { get; set; }

		private DbLog _log = null;
		private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

		public B2BPaymentCommandExecution() { }

		public B2BCommandPack PaymentWorkingExecution(CommBusiness.B2BAccess currB2bAccess, B2BRMCashData currB2BCashData, B2BCommandPack commandPack, OrgApi.B2BApi currB2bApi)
		{
			if ((currB2bAccess == null) || (currB2bApi == null))
				return commandPack;

			B2BPaymentCommand command = (B2BPaymentCommand)commandPack.Command;

			PaymentResultStatus payCompStatus = PaymentResultStatus.InitNewState;
			_cancelRequested = false;
			_isRaisedCompleteCallbackEvent = false;
			_lastInProgressEventReceivedTime = DateTime.MinValue;

			CurrentB2BCashData = currB2BCashData;

			try
			{
				B2bAccess = currB2bAccess;
				B2bApi = currB2bApi;

				B2bAccess.OnCommandInterrupted += B2bAccess_OnCommandInterrupted;

				B2bApi.CurrentProcessId = command.ProcessId;
				B2bApi.CurrentNetProcessId = command.NetProcessId;
				B2bApi.OnPollAnswerResponse += B2bApi_OnPollAnswerResponse;

				CurrentProcessId = command.ProcessId;
				CurrentNetProcessId = command.NetProcessId;

				_coinRefundAmountOnSuccess = command.CoinRefundAmountOnSuccess;

				payCompStatus = PaymentResultStatus.InitNewState;

				// $@"Start Process; Thread Hash Code : {Thread.CurrentThread.GetHashCode()};"
				Log.LogText(LogChannel, CurrentProcessId, command, "B01", "B2BCommandPaymentExecution.PayECRProcessThreadWorking",
					extraMsg: $@"Start Process; Managed Thread Id : {Thread.CurrentThread.ManagedThreadId}; Thread Hash Code : {Thread.CurrentThread.GetHashCode()}; MsgObj: B2BCommandPayment; ");

				// ----- Collect Payment 
				CurrentB2BCashData = collectPayment(CurrentNetProcessId, command, out payCompStatus);

				// ----- Refunding 
				Exception refundErr = null;
				refundPayment(CurrentNetProcessId, payCompStatus, out refundErr);
				if (refundErr is UnableRefundException)
				{
					// UnableRefundException only occur when sales has success payment but not able to refund because no enough cash.
					payCompStatus = PaymentResultStatus.FailRefundOnPaymentSuccess;

					refundErr = null;
					refundPayment(CurrentNetProcessId, payCompStatus, out refundErr);
				}
				if (refundErr != null)
					throw refundErr;
				// ----- 

				// xxxxx ----- Send Payment Completed Event ----- xxxxx
				{
					if (payCompStatus != PaymentResultStatus.InitNewState)
					{
						whenCompletedSendEvent(payCompStatus, CurrentNetProcessId, "Completed01", CurrentB2BCashData);
					}
					else
					{
						// -- .. abnormal stage
						throw new B2BMalfunctionException("Unregconised payment status.") { LastCustPaymentInfo = CurrentB2BCashData };
					}
				}

			}
			catch (B2BMalfunctionException exM)
			{
				Log.LogError(LogChannel, CurrentProcessId, exM, "Ex01", classNMethodName: "B2BCommandPaymentExecution.OnPayECRProcess");
				whenCompletedSendEvent(PaymentResultStatus.MachineMalfunction, CurrentNetProcessId, "Completed-Ex01", CurrentB2BCashData, exM);
				throw exM;
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, CurrentProcessId, ex, "Ex02", classNMethodName: "B2BCommandPaymentExecution.OnPayECRProcess");
				whenCompletedSendEvent(PaymentResultStatus.Error, CurrentNetProcessId, "Completed-Ex20", CurrentB2BCashData, ex);
				throw ex;
			}
			finally
			{
				try
				{
					B2bAccess.SetB2BToNormalStage(command?.NetProcessId, command?.ProcessId, B2bApi, B2BModuleAppGroup.Payment, 120, "Ending PaymentWorkingExecution", CurrentB2BCashData);
				}
				catch { }

				try
				{
					B2bAccess.OnCommandInterrupted -= B2bAccess_OnCommandInterrupted;
				}
				catch { }

				try
				{
					B2bApi.OnPollAnswerResponse -= B2bApi_OnPollAnswerResponse;
				}
				catch { }

				CurrentB2BCashData = null;
				CurrentNetProcessId = null;
				CurrentProcessId = "-";
				B2bAccess = null;
				B2bApi.CurrentNetProcessId = null;
				B2bApi.CurrentProcessId = "-";
				B2bApi = null;
				command.ProcessDone = true;
			}

			return commandPack;
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			void whenCompletedSendEvent(PaymentResultStatus completedStatus, Guid? netProcessId, string lineTag, ICustomerPaymentInfo custPayInfo, Exception error = null)
			{
				if (_isRaisedCompleteCallbackEvent)
					return;

				UICashMachineStatus cashStt = null;

				if (completedStatus == PaymentResultStatus.Success)
				{
					try
					{
						cashStt = new UICashMachineStatus(netProcessId, CurrentProcessId, DateTime.Now);

						int[] cassetteArr = CurrentB2BCashData.GetCassettesInfo(B2bApi.B2BCassetteList);

						if (cassetteArr != null)
						{
							if (cassetteArr.Length >= 1)
								cashStt.Cassette1NoteCount = cassetteArr[0];
							else
								cashStt.Cassette1NoteCount = -1;

							if (cassetteArr.Length >= 2)
								cashStt.Cassette2NoteCount = cassetteArr[1];
							else
								cashStt.Cassette2NoteCount = -1;

							if (cassetteArr.Length >= 3)
								cashStt.Cassette3NoteCount = cassetteArr[2];
							else
								cashStt.Cassette3NoteCount = -1;
						}

						Log.LogText(LogChannel, CurrentProcessId, $@"Payment Completed", "B20", "B2BCommandPaymentExecution.whenCompletedSendEvent",
							adminMsg: $@"Payment Completed");

					}
					catch (Exception ex)
					{
						Log.LogError(LogChannel, CurrentProcessId, ex, "EX01", classNMethodName: "B2BCommandPaymentExecution.whenCompletedSendEvent");
					}
				}

				TrxCallBackEventArgs compEv = new TrxCallBackEventArgs(netProcessId, CurrentProcessId, B2BModuleAppGroup.Payment)
				{
					ResultStatus = (DeviceProgressStatus)completedStatus,
					Remark = $@"Customer's Last Payment Info: {custPayInfo?.GetLastCustomerPaymentInfo()}",
					Error = error, 
					KioskMessage = cashStt
				};

				_isRaisedCompleteCallbackEvent = true;
				B2bAccess.RaiseOnCompleted(compEv, lineTag);
			}

			B2BRMCashData collectPayment(Guid? netProcessId, B2BPaymentCommand commData, out PaymentResultStatus payResStatus)
			{
				Log.LogText(LogChannel, CurrentProcessId, $@"Start - CollectPayment", "A01", "B2BCommandPaymentExecution.CollectPayment");

				payResStatus = PaymentResultStatus.InitNewState;

				DateTime endTime = DateTime.Now.AddSeconds(commData.MaxWaitingSec);
				bool paymentTimeout = false;

				CurrentB2BCashData.InitNewPrice(commData.Amount, commData.ProcessId);
				B2bAccess.SetB2BToNormalStage(netProcessId, CurrentProcessId, B2bApi, B2BModuleAppGroup.Payment, 120, "Start Payment", CurrentB2BCashData);

				// ----- Start New Payment
				UINewPayment newPay = new UINewPayment(netProcessId, CurrentProcessId, DateTime.Now) { Price = commData.Amount };
				B2bAccess.OnInProgressSentEvent(new InProgressEventArgs(netProcessId, CurrentProcessId)
				{
					ModuleAppGroup = B2BModuleAppGroup.Payment, 
					KioskMessage = newPay,
					PaymentStatus = DeviceProgressStatus.StartPayment,
				});

				// ----- Get Activate Bill Type 
				UIAcceptableBanknote acceptableBanknote = null;
				byte[] billTypeActivationData = CurrentB2BCashData.GetBillTypeActivationData(netProcessId, CurrentProcessId, 
					commData.Amount, B2bApi.B2BCassetteList, out acceptableBanknote);

				acceptableBanknote.ProcessMsg = "Showing acceptable banknote.";

				// ----- Acknowledge Acceptable Banknote
				B2bAccess.OnInProgressSentEvent(new InProgressEventArgs(netProcessId, CurrentProcessId)
				{
					ModuleAppGroup = B2BModuleAppGroup.Payment,
					KioskMessage = acceptableBanknote,
					PaymentStatus = DeviceProgressStatus.CashAcceptableInfor
				});

				// * outstanding work -> send B2B Malfunction if billTypeActivationData is null (because not enough for bill changed); 
				// * Log current available bill qty for each cassette.

				Log.LogText(LogChannel, CurrentProcessId, $@"Activating Bill Type; Byte List: {AppDecorator.Common.Common2s.BytesToNumbersStr(billTypeActivationData)}"
					, "B02", "B2BCommandPaymentExecution.CollectPayment");

				Answer b2bAnswer = B2bApi.RunCommand(CCNETCommand.ENABLE_BILL_TYPES, billTypeActivationData);

				Log.LogText(LogChannel, CurrentProcessId, $@"B2B Enable Bill Type has Completed."
					, "B03", "B2BCommandPaymentExecution.CollectPayment");

				// ----- Loop to check result when payment in progress 
				while ((!paymentTimeout) && (!_cancelRequested) && (!CurrentB2BCashData.IsPaymentDone()))
				{
					Task.Delay(500).Wait();

					if (endTime.Subtract(DateTime.Now).TotalMilliseconds <= 0)
					{
						paymentTimeout = true;
					}
				}

				Log.LogText(LogChannel, CurrentProcessId, $@"End Collecting Payment", "B10", "B2BCommandPaymentExecution.CollectPayment");

				if (_cancelRequested)
				{
					DateTime latestInProgressEventReceivedTime = _lastInProgressEventReceivedTime;
					int maxWaitSecondForCancel = 6;
					int waitSeconds = 0;


					// note : Because _lastInProgressEventReceivedTime may updated in any time. 
					//        So Wait deley will redo if _lastInProgressEventReceivedTime is not same with latestInProgressEventReceivedTime
					do
					{
						latestInProgressEventReceivedTime = _lastInProgressEventReceivedTime;

						//double totalWaitSec = latestInProgressEventReceivedTime.AddSeconds(maxWaitSecondForCancel).Subtract(DateTime.Now).TotalSeconds;

						//if (totalWaitSec > 0)
						//{
						//	waitSeconds = Convert.ToInt32(totalWaitSec);
						//	Task.Delay(1000 * waitSeconds).Wait();
						//}

						Task.Delay(1000 * maxWaitSecondForCancel).Wait();
						//Task.Delay(1000 * 3).Wait();


					} while (latestInProgressEventReceivedTime != _lastInProgressEventReceivedTime);

				}

				// .. payment ending..
				if ((_cancelRequested) || (paymentTimeout))
				{
					payResStatus = _cancelRequested ? PaymentResultStatus.Cancel : PaymentResultStatus.Timeout;

					if (_cancelRequested)
						Log.LogText(LogChannel, CurrentProcessId, $@"Transaction Cancelled", "B20", "B2BCommandPaymentExecution.collectPayment",
							adminMsg: $@"Transaction Cancelled; Inserted Amount : {CurrentB2BCashData.InsertedAmount:#,###.#0}");

					if (paymentTimeout)
						Log.LogText(LogChannel, CurrentProcessId, $@"Timeout", "B20", "B2BCommandPaymentExecution.collectPayment",
							adminMsg: $@"Timeout; Inserted Amount : {CurrentB2BCashData.InsertedAmount:#,###.#0}");
				}
				else if (CurrentB2BCashData.IsPaymentDone())
				{
					payResStatus = PaymentResultStatus.Success;

					Log.LogText(LogChannel, CurrentProcessId, $@"Inserted Amount : {CurrentB2BCashData.InsertedAmount:#,###.#0}; Bill/Note Insertion Done", "B20", "B2BCommandPaymentExecution.collectPayment",
							adminMsg: $@"Bill/Note Insertion Done; Inserted Amount : {CurrentB2BCashData.InsertedAmount:#,###.#0}");
				}

				B2bAccess.SetB2BToNormalStage(netProcessId, CurrentProcessId, B2bApi, B2BModuleAppGroup.Payment, codeLocTag: "Ending Collect Payment ", currCustInfo: CurrentB2BCashData);

				return CurrentB2BCashData;
			}

			bool refundPayment(Guid? netProcessId, PaymentResultStatus payResStatus, out Exception error)
			{
				error = null;
				bool hasRefund = false;
				RefundType typeOfRefund = RefundType.New;

				// ----- Check For Possible Refunding
				CCNET.BillToBill.DispenseParameter[] refundNoteParams = null;

				if ((payResStatus == PaymentResultStatus.Cancel) || (payResStatus == PaymentResultStatus.Timeout) || (payResStatus == PaymentResultStatus.FailRefundOnPaymentSuccess))
				{
					if (CurrentB2BCashData.InsertedAmount > 0)
					{
						Log.LogText(LogChannel, CurrentProcessId, $@"Refund After {
							Enum.GetName(typeof(PaymentResultStatus), payResStatus)}; Inserted Amount : {CurrentB2BCashData.InsertedAmount };", "A02", "B2BCommandPaymentExecution.RefundCash");

						refundNoteParams = CurrentB2BCashData.GetRefundParameters(B2bApi.B2BCassetteList, RefundType.CancelPayment, out error);

						typeOfRefund = RefundType.CancelPayment;
					}
				}
				else if (payResStatus == PaymentResultStatus.Success)
				{
					if (CurrentB2BCashData.IsRefundRequest())
					{
						Log.LogText(LogChannel, CurrentProcessId, $@"Refund After Payment Success; Refund Amount : {CurrentB2BCashData.RefundAmount };", "A03", "B2BCommandPaymentExecution.RefundCash");

						refundNoteParams = CurrentB2BCashData.GetRefundParameters(B2bApi.B2BCassetteList, RefundType.CompletePayment, out error);

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

				decimal price = CurrentB2BCashData.Price;
				decimal insertedAmt = CurrentB2BCashData.InsertedAmount;
				decimal refundAmt = (typeOfRefund == RefundType.New) ? 0.00M :
					(typeOfRefund == RefundType.CompletePayment) ? CurrentB2BCashData.RefundAmount : CurrentB2BCashData.InsertedAmount;

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

						if (dispenseRefundedBanknote(netProcessId, currRefundList.ToArray(), $@"({refundRoundCount}/{refundTotalRoundCount})", typeOfRefund, price, insertedAmt, refundAmt) == true)
							hasRefund = true;

						currRefundList.Clear();
					}
					// ----- 
				}

				//if (hasRefund)
				//	B2bAccess.SetB2BToNormalStage(B2bApi, codeLocTag: "After Refund", currCustInfo: CurrentB2BCashData);

				return hasRefund;
			}

			bool dispenseRefundedBanknote(Guid? netProcessId, CCNET.BillToBill.DispenseParameter[] refundParm, string roundTag,
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

					if (CurrentB2BCashData.GetCurrencyAmount(rfPar.BillType, out currencyStr, out billValue, out errMsg))
						refundMsg += $@"{currencyStr} {billValue} x {rfPar.BillCount}; ";
					else
						refundMsg += $@"Error when reading (for refund) bill type {rfPar.BillType}; {errMsg ?? "--"}; ";
				}

				if (refundMsg.Length > 0)
				{
					Log.LogText(LogChannel, CurrentProcessId, $@"Will Refund {currencyStr}: {returnAmount:###.#0}; Notes : {refundMsg}", "A02", "B2BCommandPaymentExecution.dispenseRefundedBanknote"
						, adminMsg: $@"Will Refund {currencyStr}: {returnAmount:###.#0}; Notes : {refundMsg}");

					B2bAccess.OnInProgressSentEvent(new InProgressEventArgs(netProcessId, CurrentProcessId)
					{
						ModuleAppGroup = B2BModuleAppGroup.Payment,
						Message = $@"{roundTag} - Refund : {refundMsg}",
						PaymentStatus = DeviceProgressStatus.Refunding, KioskMessage = new UIRefundPayment(netProcessId, CurrentProcessId, DateTime.Now)
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
				B2bApi.Dispense(refundParm);
				refundDone = true;
				Task.Delay(500).Wait();
				//b2bXAccess.SetB2BToNormalStage(machApi, codeLocTag: "After Refund", currCustInfo: cashData);

				return refundDone;
			}
		}

		private void B2bAccess_OnCommandInterrupted(object sender, InterruptCommandEventArgs e)
		{
			if (e.CommandCode == B2BCommandCode.CancelPayment)
			{
				_cancelRequested = true;
				Log.LogText(LogChannel, CurrentProcessId, "Cancel Transaction Request", "A01", "B2BCommandPaymentExecution.CancelRequest", netProcessId: CurrentNetProcessId);
				e.Command.ProcessDone = true;
			}
		}
		
		/// <summary>
		/// Event handle for _ecr.OnInProgress. This will pass the handle to another event handle.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void B2bApi_OnPollAnswerResponse(object sender, B2BDecorator.Events.MachineEventArgs e)
		{
			_lastInProgressEventReceivedTime = DateTime.Now;

			Log.LogText(LogChannel, CurrentProcessId, e, "A01", "B2BCommandPaymentExecution.B2bApi_OnPollAnswerResponse"
				, extraMsg: $@"MsgObj: InProgressEventArgs");

			DeviceProgressStatus pgStatus = DeviceProgressStatus.New;
			IKioskMsg machData = null;

			if (e?.MachData is B2BApiNewInsertedNote newNote)
			{
				if ((newNote.DigitBillType > 0) && (CurrentB2BCashData.InsertedAmount == 0))
					pgStatus = DeviceProgressStatus.FirstPayment;
				else
					pgStatus = DeviceProgressStatus.SubPayment;

				CurrentB2BCashData.Pay(newNote.DigitBillType);

				if ((newNote.DigitBillType > 0) && (CurrentB2BCashData.IsPaymentDone() == true) && (pgStatus != DeviceProgressStatus.FirstPayment))
					pgStatus = DeviceProgressStatus.LastPayment;

				machData = new UIOutstandingPayment(CurrentNetProcessId, CurrentProcessId, DateTime.Now)
				{
					LastBillInsertedAmount = newNote.DigitBillType,
					Price = CurrentB2BCashData.Price,
					PaidAmount = CurrentB2BCashData.InsertedAmount,
					OutstandingAmount = CurrentB2BCashData.GetOutstandingPayment(),
					IsPaymentDone = CurrentB2BCashData.IsPaymentDone(),
					RefundAmount = CurrentB2BCashData.RefundAmount,
					IsRefundRequest = CurrentB2BCashData.IsRefundRequest()
				};
			}
			else if (e?.MachData is B2BApiPaymentFound)
			{
				pgStatus = DeviceProgressStatus.PaymentFound;
			}

			if (pgStatus != DeviceProgressStatus.New)
			{
				B2bAccess.OnInProgressSentEvent(new InProgressEventArgs(CurrentNetProcessId, CurrentProcessId)
				{
					ModuleAppGroup = B2BModuleAppGroup.Payment,
					Error = (e.ErrorMessage == null) ? null : (new Exception(e.ErrorMessage)),
					Message = e.Message,
					PaymentStatus = pgStatus,
					KioskMessage = machData
				});
			}
		}

		public void Dispose()
		{
			CurrentB2BCashData = null;
			B2bApi = null;
			B2bAccess = null;

			_log = null;
		}
	}
}
