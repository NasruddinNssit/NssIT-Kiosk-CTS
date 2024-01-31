using CCNET;
using CCNET.BillToBill;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;

namespace NssIT.Kiosk.Device.B2B.AccessSDK
{
    public class B2BTest : IDisposable 
    {
		private const string LogChannel = "B2BTest";

		public BillToBill objB2B;
		public B2BDecorator.Data.BillTable[] billtable;

		private DbLog _log = null;

		private string C2CStatus = "";
		private int[] ArrCashIn = new int[3];
		private int[] ArrCashRec = new int[3];
		private int[] ArrCashRet = new int[3];

		private int intCashD1Limit = 90;
		private int intCashD2Limit = 90;
		private int intCashD3Limit = 90;

		//public struct BillTable
		//{
		//	/// <summary>
		//	/// Bill Type
		//	/// </summary>
		//	public int BillType;
		//	/// <summary>
		//	/// Digit of bill type
		//	/// </summary>
		//	public int DigitBillType;

		//	/// <summary>
		//	/// Country code
		//	/// </summary>
		//	public string CountryCode;
		//}

		public B2BTest()
		{
			_log = DbLog.GetDbLog();
			objB2B = new BillToBill();
		}

		private DbLog _____log
		{
			get
			{
				return _log;
			}
		}

		protected Answer answer;
		public Answer Answer
		{
			get { return answer; }
			set { answer = value; }
		}

		private string _currProcessId = "-";
		public string CurrentProcessId
		{
			get
			{
				return _currProcessId;
			}
			set
			{
				_currProcessId = string.IsNullOrEmpty(value) ? "-" : value.Trim();
			}
		}

		private int b2bMaxRetryCount = 50000000;
		public async Task InitDevice()
		{
			int intCount = 0;
			_prLoopCount = 1;
			_lastPollAnswerReceivedStage = null;

			_____log.LogText(LogChannel, CurrentProcessId, "Start Init-B2B Device", "A01", "B2BTest.InitDevice", AppDecorator.Log.MessageType.Info);

			try
			{
				int b2bRetryCount = 0;

				CloseDevice();

				while (!objB2B.SearchAndConnect() && b2bRetryCount < b2bMaxRetryCount)
				{
					_____log.LogText(LogChannel, CurrentProcessId, $@"Reconnecting B2B ......; Retry Count : {b2bRetryCount}", "A02", "B2BTest.InitDevice", AppDecorator.Log.MessageType.Info);
					intCount += 1;
					Task.Delay(1000).Wait();
					b2bRetryCount++;
				}

				if (intCount < b2bMaxRetryCount)
				{
					_____log.LogText(LogChannel, CurrentProcessId, "B2B Connected and Startup ......", "A03", "B2BTest.InitDevice", AppDecorator.Log.MessageType.Info);

					objB2B.Open(); Task.Delay(20).Wait();
					objB2B.OnBillToBillStatusChanged += new BillToBill.Event(objB2B_OnBillToBillStatusChanged);
					objB2B.OnPollAnswerReceived += new BillToBill.AnswerReceivedEvent(objB2B_OnPollAnswerReceived);

					objB2B.CCNET.OnDataSended += new CCNET.Iccnet.DataSendedEvent(xxT);

					objB2B.IsPoll = true; Task.Delay(20).Wait();

					billtable = new B2BDecorator.Data.BillTable[23];
					Answer = objB2B.CCNET.RunCommand(CCNETCommand.GET_BILL_TABLE, true); Task.Delay(20).Wait();

					ExtractBillTable(Answer.ReceivedData);

					objB2B.PollInterval = 150; Task.Delay(20).Wait();

					//objB2B.Cassettes[0].SetCassetteType(0); /*Set Note Type to 0*/
					//objB2B.Cassettes[1].SetCassetteType(2); /*Set Note Type to 2*/
					//objB2B.Cassettes[2].SetCassetteType(3); /*Set Note Type to 3*/

					objB2B.Cassettes[0].LoadLimit = intCashD1Limit; Task.Delay(20).Wait();
					objB2B.Cassettes[1].LoadLimit = intCashD2Limit; Task.Delay(20).Wait();
					objB2B.Cassettes[2].LoadLimit = intCashD3Limit; Task.Delay(20).Wait();

					_____log.LogText(LogChannel, CurrentProcessId, "B2B Connected", "A04", "B2BTest.InitDevice", AppDecorator.Log.MessageType.Info);
				}
				else
				{
					_____log.LogText(LogChannel, CurrentProcessId, "Failed to Connect B2B", "A05", "B2BTest.InitDevice", AppDecorator.Log.MessageType.Info);
				}
			}
			catch (Exception ex)
			{
				_____log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BTest.InitDevice");
			}

			await Task.Delay(10);
		}

		private void xxT(object sender, CCNETSendDataEventArgs fe)
		{
			_____log.LogText(LogChannel, CurrentProcessId,
							new string[] { "xxT-->fe.SentData =>",  BytesToNumbers(fe.SentData) },
							"A04M", "B2BTest.xxT");
		}

		private int intTimer = 68;
		private decimal outstandingBalc = 0.00M;
		private decimal sellingPrice = 0.00M;
		public void StartReceive(decimal amount)
		{
			sellingPrice = amount;

			_____log.LogText(LogChannel, CurrentProcessId, $@"Processing ... Please wait; Amount : RM {String.Format("{0:00.00}", amount)}", "A01", "B2BTest.StartReceive", AppDecorator.Log.MessageType.Info);

			Boolean flgRM50;
			Boolean flgRM20;
			Int32 int50 = 0;
			Int32 int20 = 0;
			int50 = Convert.ToInt32(sellingPrice / 50);
			int20 = Convert.ToInt32(sellingPrice / 20);

			int intRM10Qty = objB2B.Cassettes[2].BillQuantity;

			if (sellingPrice - (int50 * 50) > 0)
			{
				int50 += 1;
			}

			if (sellingPrice - (int20 * 20) > 0)
			{
				int20 += 1;
			}

			flgRM50 = false;
			if (intRM10Qty >= (int50 * 5))
			{
				flgRM50 = true;
			}
			if ((int50 * 5) > 20)
			{
				flgRM50 = false;
			}

			flgRM20 = false;
			if (intRM10Qty >= (int20 * 2))
			{
				flgRM20 = true;
			}

			if ((int20 * 2) > 20)
			{
				flgRM20 = false;
			}

			_____log.LogText(LogChannel, CurrentProcessId, "Please Pay : RM " + string.Format("{0,0:N2}", sellingPrice), "A01", "B2BTest.StartReceive", AppDecorator.Log.MessageType.Info);

			ResetArr();
			outstandingBalc = sellingPrice;

			_____log.LogText(LogChannel, CurrentProcessId, "Ticket Price : RM " + outstandingBalc.ToString(), "A02", "B2BTest.StartReceive", AppDecorator.Log.MessageType.Info);

			intTimer = 68;

			_____log.LogText(LogChannel, CurrentProcessId, "showCashPaymentInfor(dblPrice, flgRM20, flgRM50)", "A03", "B2BTest.StartReceive", AppDecorator.Log.MessageType.Info);

			if (flgRM50 && flgRM20)
			{
				objB2B.CCNET.RunCommand(CCNETCommand.ENABLE_BILL_TYPES, new byte[] { 0, 0, 61, 0, 0, 0 });
			}
			else if (flgRM50)
			{
				objB2B.CCNET.RunCommand(CCNETCommand.ENABLE_BILL_TYPES, new byte[] { 0, 0, 45, 0, 0, 0 });
			}
			else if (flgRM20)
			{
				objB2B.CCNET.RunCommand(CCNETCommand.ENABLE_BILL_TYPES, new byte[] { 0, 0, 29, 0, 0, 0 });
			}
			else
			{
				objB2B.CCNET.RunCommand(CCNETCommand.ENABLE_BILL_TYPES, new byte[] { 0, 0, 13, 0, 0, 0 });
			}
		}

		public void EndReceivingCash()
		{
			if (objB2B != null)
			{
				_____log.LogText(LogChannel, CurrentProcessId, "B2B ToDisableMode in progress", "A01", "B2BTest.EndReceivingCash", AppDecorator.Log.MessageType.Info);
				objB2B.ToDisableMode();
				_____log.LogText(LogChannel, CurrentProcessId, "Done - B2B ToDisableMode", "A02", "B2BTest.EndReceivingCash", AppDecorator.Log.MessageType.Info);
			}
		}

		public void ResetArr()
		{
			ArrCashRec[0] = 0;
			ArrCashRec[1] = 0;
			ArrCashRec[2] = 0;

			ArrCashRet[0] = 0;
			ArrCashRet[1] = 0;
			ArrCashRet[2] = 0;

			ArrCashIn[0] = 0;
			ArrCashIn[1] = 0;
			ArrCashIn[2] = 0;
		}

		public void CancelReceive()
		{
			if (C2CStatus == "IDLING")
			{
				_____log.LogText(LogChannel, CurrentProcessId, "Start - Cancel Transaction", "A01", "B2BTest.CancelReceive", AppDecorator.Log.MessageType.Info);

				objB2B.ToDisableMode();

				CloseCashPayTimer();
				CashInsertedRefund();

				_____log.LogText(LogChannel, CurrentProcessId, "Unlock Ticket", "A02", "B2BTest.CancelReceive", AppDecorator.Log.MessageType.Info);
			}
			else
			{
				_____log.LogText(LogChannel, CurrentProcessId, "_frmCashPayment.DisplayMacBusy(true);", "A03", "B2BTest.CancelReceive", AppDecorator.Log.MessageType.Info);
			}
		}

		public void CloseDevice()
		{
			try
			{
				_____log.LogText(LogChannel, CurrentProcessId, "Start CloseDevice", "A01", "B2BTest.CloseDevice", AppDecorator.Log.MessageType.Info);

				if (objB2B?.CCNET?.Port.IsOpen == true)
				{
					
					_____log.LogText(LogChannel, CurrentProcessId, "Start Close Port", "A04", "B2BTest.CloseDevice", AppDecorator.Log.MessageType.Info);
					objB2B.CCNET.Port.Close(); Task.Delay(20).Wait();
					_____log.LogText(LogChannel, CurrentProcessId, "End Close Port", "A05", "B2BTest.CloseDevice", AppDecorator.Log.MessageType.Info);
						
				}
				else if (objB2B?.CCNET?.Port.IsOpen == false)
				{
					_____log.LogText(LogChannel, CurrentProcessId, "Port Already Been Closed", "A06", "B2BTest.CloseDevice", AppDecorator.Log.MessageType.Info);
				}
			}
			catch (Exception ex)
			{
				_____log.LogError(LogChannel, CurrentProcessId, ex, "A07", "B2BTest.CloseDevice");
			}

			_____log.LogText(LogChannel, CurrentProcessId, "End CloseDevice", "A08", "B2BTest.CloseDevice", AppDecorator.Log.MessageType.Info);
		}

		public void objB2B_OnBillToBillStatusChanged(Object sender, EventArgs e)
		{
			//ResetCasstleInfo();
			_____log.LogText(LogChannel, CurrentProcessId, $@"B2B Status : {objB2B.BillToBillStatus}", "A02", "B2BTest.objB2B_OnBillToBillStatusChanged", AppDecorator.Log.MessageType.Info);
		}

		private DateTime? _nextLogTime = null;
		private int _pollAnswerReceivedLogDelayMinutes = 5;
		private bool GetPollAnswerReceivedLogRequest()
		{
			if ((_nextLogTime.HasValue == false) || (_nextLogTime.Value.Subtract(DateTime.Now).TotalSeconds <= 0))
			{
				_nextLogTime = DateTime.Now.AddMinutes(_pollAnswerReceivedLogDelayMinutes);
				return true;
			}
			return false;
		}

		private long _prLoopCount = 1;
		private long _prErrLoopCount = 1;
		private string _lastPollAnswerReceivedStage = null;
		public void objB2B_OnPollAnswerReceived(object sender, CCNETEventArgs e)
		{
			bool logEnabled = (_lastPollAnswerReceivedStage == null) || GetPollAnswerReceivedLogRequest();
			if (logEnabled) _____log.LogText(LogChannel, CurrentProcessId, "Start objB2B_OnPollAnswerReceived", "A01", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);
			_lastPollAnswerReceivedStage = _lastPollAnswerReceivedStage ?? "-";
			
			try
			{
				Answer MyAnswer = e.ReceivedAnswer;
				byte[] sendData = e.SentData;
				string currState = "";

				if (MyAnswer.Error == null)
				{
					_prErrLoopCount = 1;
					C2CStatus = (MyAnswer.Message ?? "--").Trim();

					currState = $@"Bill_To_Bill_Status : {objB2B.BillToBillStatus.ToString()}; Answer.Message : {C2CStatus}";

					if (!_lastPollAnswerReceivedStage.Equals(currState))
						logEnabled = true;
					else
						_prLoopCount++;

					if (objB2B.BillToBillStatus == Bill_to_Bill_Status.ACCEPTING)
					{
						// ----- ACCEPTING State --> when note is detected and accepting
						if (logEnabled) _____log.LogText(LogChannel, CurrentProcessId, $@"Bill/Note Detected (Stage 1/3); Accepting RM {String.Format("{0:00.00}", billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType)}; {currState};", "A02", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);
					}
					else if (objB2B.BillToBillStatus == Bill_to_Bill_Status.STACKING)
					{
						// ----- STACKING State --> when note is stacking
						if (logEnabled) _____log.LogText(LogChannel, CurrentProcessId, $@"Bill/Note Stacking (Stage 2/3); Staging RM {String.Format("{0:00.00}", billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType)}; {currState};", "A03", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);
					}
					else if (objB2B.BillToBillStatus == Bill_to_Bill_Status.PACKED_STACKED)
					{
						// ----- PACKED_STACKED State --> when confirm note in Cassette
						_____log.LogText(LogChannel, CurrentProcessId, $@"Bill/Note Received (Stage 3/3); Done - RM {String.Format("{0:00.00}", billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType)} stored into cassette; {currState}", "A04", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);
						CloseCashPayTimer();
						isBillAccepted(Convert.ToInt32(MyAnswer.Data[1]));
					}
					else if ((objB2B.BillToBillStatus == Bill_to_Bill_Status.DISPENSED)
						|| (objB2B.BillToBillStatus == Bill_to_Bill_Status.DISPENSING_accessible_to_customer)
						|| (objB2B.BillToBillStatus == Bill_to_Bill_Status.DISPENSING_from_recycling_cassette_to_dispenser)
						|| (objB2B.BillToBillStatus == Bill_to_Bill_Status.DISPENSING_from_recycling_cassette_to_dispenser)
						)
					{
						// ----- Dispenses State 
						//if (logEnabled)
						//_____log.LogText(LogChannel, CurrentProcessId, $@"Bill/Note Dispending; {currState}; RM {String.Format("{0:00.00}", billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType)}", "A04M", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);

						//_____log.LogText(LogChannel, CurrentProcessId, new object[] { MyAnswer.Data,  }, "A04M", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);

						//_____log.LogText(LogChannel, CurrentProcessId, new object[] { MyAnswer.Data, MyAnswer.Additional_Data, MyAnswer.Message }, "A04M", "B2BTest.objB2B_OnPollAnswerReceived", 
						//	AppDecorator.Log.MessageType.Info,
						//	extraMsg: $@"Bill/Note Dispending; {currState}; RM {String.Format("{0:00.00}", billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType)}");

						_____log.LogText(LogChannel, CurrentProcessId, 
							new string[] { "Received Answer Data => ", BytesToNumbers(MyAnswer.Data), "Send Data => ",  BytesToNumbers(sendData),
								"Received Sent Data => ", BytesToNumbers(MyAnswer.SendedData)  }, 
							"A04M", "B2BTest.objB2B_OnPollAnswerReceived",
							AppDecorator.Log.MessageType.Info,
							extraMsg: $@"Bill/Note Dispending; {currState};");

					}


					else if (objB2B.BillToBillStatus == Bill_to_Bill_Status.IDLING)
					{
						// ----- IDLING State --> when receiving note
						if (logEnabled) _____log.LogText(LogChannel, CurrentProcessId, "Bill_to_Bill_Status = IDLING", "A05", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);
					}
					else if (objB2B.BillToBillStatus == Bill_to_Bill_Status.POWER_UP)
					{
						// ----- POWER_UP State
						_____log.LogText(LogChannel, CurrentProcessId, "Bill_to_Bill_Status is POWER_UP", "A06", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);
						objB2B.CCNET.RunComandNonAnswer(CCNETCommand.RESET);
					}
					else if (logEnabled)
					{
						_____log.LogText(LogChannel, CurrentProcessId, $@"{currState}; Similar Loop Count : {_prLoopCount}; ", "A07", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);
						_prLoopCount = 1;
					}
				}
				else
				{
					_prLoopCount = 1;

					currState = $@"Error From B2B : {MyAnswer?.Error?.Data?.ToString()} - {MyAnswer?.Error?.Message}";

					if (!_lastPollAnswerReceivedStage.Equals(currState))
					{
						_____log.LogText(LogChannel, CurrentProcessId, $@"{currState}; Similar Error Loop Count : {_prErrLoopCount}", "A11", "B2BTest.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Error);
						_prErrLoopCount = 1;
					}
					else
						_prErrLoopCount++;

				}

				_lastPollAnswerReceivedStage = currState;
			}
			catch (Exception ex)
			{
				_____log.LogError(LogChannel, CurrentProcessId, ex, "A21", "B2BTest.objB2B_OnPollAnswerReceived");
			}
		}

		string BytesToNumbers(byte[] data)
		{
			int inx = 0;
			string retStr = "";
			foreach (byte aByte in data)
				retStr += $@"{inx++}: {((int)aByte).ToString()};";

			return retStr;
		}

		bool isBillAccepted(int intBillType)
		{
			_____log.LogText(LogChannel, CurrentProcessId, $@"Start - isBillAccepted", "A01", "B2BTest.isBillAccepted", AppDecorator.Log.MessageType.Info);

			//---ESCROW RETURN STACKED IN PASSIVE MODE
			//-----------------------------------ESCROW MODE----------------------------------------------------------
			//if (receivedData != null && sendedData[3] == 0x33 && receivedData[3] == 0x80)
			//{
			//    BalanceChanged(billtable[((int)receivedData[4])].DigitBillType);
			//    return true;

			//}
			//--------------------------------------------------------------------------------------------------------
			//if (receivedData != null && sendedData[3] == 0x33 && receivedData[3] == 0x81)
			//{
			//    BalanceChanged(billtable[((int)receivedData[4])].DigitBillType);
			//    return true;
			//}
			//--------------------------------------------------------------------------------------------------------
			//if (receivedData != null && sendedData[3] == 0x33 && receivedData[3] == 0x82)
			//{
			BalanceChanged(billtable[intBillType].DigitBillType);

			_____log.LogText(LogChannel, CurrentProcessId, "End - isBillAccepted", "A02", "B2BTest.isBillAccepted", AppDecorator.Log.MessageType.Info);

			return true;
			//}
			//return false;
		}

		public void BalanceChanged(int intAmountReceived)
		{
			_____log.LogText(LogChannel, CurrentProcessId, "Start - BalanceChanged", "A01", "B2BTest.BalanceChanged", AppDecorator.Log.MessageType.Info);

			switch (intAmountReceived)
			{
				case 1:
					ArrCashRec[0] += 1;
					break;
				case 5:
					ArrCashRec[1] += 1;
					break;
				case 10:
					ArrCashRec[2] += 1;
					break;
				case 20:
					ArrCashRec[2] += 2;
					break;
				case 50:
					ArrCashRec[2] += 5;
					break;
			}

			_____log.LogText(LogChannel, CurrentProcessId, ArrCashRec, "A02", "B2BTest.BalanceChanged", AppDecorator.Log.MessageType.Info
				, extraMsg: $@"Previous Balance : {outstandingBalc.ToString()}; MsgObjDesc: Showing Cash Rec. Array (Bill/Note Received in Cassettes)");

			outstandingBalc = Math.Round(outstandingBalc, 2);
			decimal dblAmountReceived = (decimal)Math.Round(Convert.ToDouble(intAmountReceived), 2);
			outstandingBalc -= dblAmountReceived;
			_____log.LogText(LogChannel, CurrentProcessId, $@"Balance should return : RM {outstandingBalc.ToString()}", "A03", "B2BTest.BalanceChanged", AppDecorator.Log.MessageType.Info);
			if (outstandingBalc == 0)
			{
				//-- Payment Done without any cash refund
				//------------------------------------------
				_____log.LogText(LogChannel, CurrentProcessId, "Sales wihout refund; Please Collect Your Ticket; ", "A04", "B2BTest.BalanceChanged", AppDecorator.Log.MessageType.Info);

				objB2B.ToDisableMode();
				Task.Delay(20).Wait();
				objB2B.ToDisableMode();

				Thread myThread = new Thread(new ThreadStart(ThreadPaymentDone));
				myThread.Start();
				_____log.LogText(LogChannel, CurrentProcessId, "ThreadPaymentDone Process Created", "A07", "B2BTest.BalanceChanged", AppDecorator.Log.MessageType.Info);
			}
			else if (outstandingBalc < 0)
			{
				//-- Payment Done with cash refund
				//--------------------------------------
				_____log.LogText(LogChannel, CurrentProcessId, $@"Please Collect Your Balance : RM {string.Format("{0,0:N2}", (-1 * outstandingBalc))}", "A08", "B2BTest.BalanceChanged", AppDecorator.Log.MessageType.Info);

				objB2B.ToDisableMode();
				Task.Delay(20).Wait();

				FillCasstleBalc();
				
				objB2B.ToDisableMode();

				Thread myThread = new Thread(new ThreadStart(ThreadCashRefundStart));
				myThread.Start();

				_____log.LogText(LogChannel, CurrentProcessId, "ThreadCashRefundStart Process Created", "A10", "B2BTest.BalanceChanged", AppDecorator.Log.MessageType.Info);
			}
			else
			{
				//-- Payment not done .. proceed to payment process
				//----------------------------------------------------
				_____log.LogText(LogChannel, CurrentProcessId, "Please Pay : RM " + string.Format("{0,0:N2}", outstandingBalc), "A11", "B2BTest.BalanceChanged", AppDecorator.Log.MessageType.Info);
			}
		}

		void ThreadPaymentDone()
		{
			_____log.LogText(LogChannel, CurrentProcessId, "Start - ThreadPaymentDone", "A01", "B2BTest.ThreadPaymentDone", AppDecorator.Log.MessageType.Info);

			PaymentDone();

			_____log.LogText(LogChannel, CurrentProcessId, "End - ThreadPaymentDone", "A02", "B2BTest.ThreadPaymentDone", AppDecorator.Log.MessageType.Info);
		}

		private void PaymentDone()
		{
			//oComm.Writelog("tmrPrintTicket", "Start");

			//CYA - tmrPrintTicket.Stop();
			//CYA - tmrPrintTicket.Interval = 5000;
			//CYA - tmrPrintTicket.Start();

			//oComm.Writelog("Print Ticket", "Called");

			//oComm.Writelog("Print Ticket", "Done");
			//SetMainLabel(0, "Printing Ticket");

			_____log.LogText(LogChannel, CurrentProcessId, "PaymentDone", "A01", "B2BTest.PaymentDone", AppDecorator.Log.MessageType.Info);
		}

		private void FillCasstleBalc()
		{
			_____log.LogText(LogChannel, CurrentProcessId, "Start - FillCasstleBalc", "A01", "B2BTest.FillCasstleBalc", AppDecorator.Log.MessageType.Info);

			CloseCashPayTimer();
						
			objB2B.ToDisableMode();

			int intloop = -1;
			foreach (CCassette cassette in objB2B.Cassettes)
			{
				intloop += 1;
				ArrCashIn[intloop] = cassette.BillQuantity;
			}

			_____log.LogText(LogChannel, CurrentProcessId, "End - FillCasstleBalc", "A03", "B2BTest.FillCasstleBalc", AppDecorator.Log.MessageType.Info);
		}

		private void CloseCashPayTimer()
		{
			_____log.LogText(LogChannel, CurrentProcessId, "Start - CloseCashPayTimer", "A01", "B2BTest.CloseCashPayTimer", AppDecorator.Log.MessageType.Info);
			//this.Invoke(new Action(() => {
			//	intTimer = 0;
			//	flgCashPayTimer = false;
			//	tmrCashPay.Stop();
			//	_frmCashPayment.setTimer("");
			//}));
			_____log.LogText(LogChannel, CurrentProcessId, "End - CloseCashPayTimer", "A01", "B2BTest.CloseCashPayTimer", AppDecorator.Log.MessageType.Info);
		}

		void ThreadCashRefundStart()
		{
			_____log.LogText(LogChannel, CurrentProcessId, "Start - CashRefund", "A01", "B2BTest.ThreadCashRefundStart", AppDecorator.Log.MessageType.Info);
			CashRefund();
			_____log.LogText(LogChannel, CurrentProcessId, "End - CashRefund", "A02", "B2BTest.ThreadCashRefundStart", AppDecorator.Log.MessageType.Info);
		}

		public void CashRefund()
		{
			_____log.LogText(LogChannel, CurrentProcessId, "Enter", "A01", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);

			bool flg = true;
			bool flgCoin = true;
			decimal dblTempNoteBalc = 0;
			decimal dblTempCoinBalc = 0;
			//System.Threading.Thread.Sleep(500);
			try
			{
				outstandingBalc = -1 * outstandingBalc;
				dblTempNoteBalc = Convert.ToInt32(Math.Floor(outstandingBalc));
				dblTempCoinBalc = (outstandingBalc - dblTempNoteBalc) * 100;

				flg = CheckNoteRefund(dblTempNoteBalc, flg);
				
				if (flg && dblTempCoinBalc > 0)
				{
					//CYA-DEBUG flgCoin = CheckCoinRefund(dblTempCoinBalc, flg);
					_____log.LogText(LogChannel, CurrentProcessId, $@"CheckCoinRefund; Coin Refund : {dblTempCoinBalc.ToString()}; Bill/Note Need To Refund : {(flg ? "Yes": "No")}; Coin Need To Refund : {(flgCoin ? "Yes": "No")};", "A04", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
				}

				if (flg && flgCoin)
				{
					if (dblTempCoinBalc > 0)
					{
						_____log.LogText(LogChannel, CurrentProcessId, $@"Start Dispense Coins, AxTQ01001.DispenseByColumn; Balance Coin : {dblTempCoinBalc.ToString()}", "A05", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
						//CYA-DEBUG flgCoin = AxTQ01001.DispenseByColumn(GetDispenseString(Convert.ToInt32(dblTempCoinBalc)));
						_____log.LogText(LogChannel, CurrentProcessId, $@"End Dispensen Coins, AxTQ01001.DispenseByColumn; ", "A05A", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
					}
					if (flgCoin)
					{
						_____log.LogText(LogChannel, CurrentProcessId, $@"Start - Coin Refund Process; Proceed to UpdateKIOSKCoin; Coin Dispense : RM {(dblTempCoinBalc / 100).ToString()}", "A06", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
						//CYA-DEBUG UpdateKIOSKCoin(Convert.ToInt32(dblTempCoinBalc));

						if (dblTempNoteBalc > 0)
						{
							_____log.LogText(LogChannel, CurrentProcessId, @"Proceed to refund Bill/Note", "A07", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);

							int i = 0;
							int intTemp = 0;
							for (i = 0; i < ArrCashRet.Length; i++)
							{
								if (ArrCashRet[i] > 0)
								{
									intTemp += 1;
								}
							}

							CCNET.BillToBill.DispenseParameter[] Dpara;
							Dpara = new CCNET.BillToBill.DispenseParameter[intTemp];
							intTemp = -1;

							_____log.LogText(LogChannel, CurrentProcessId, "Checking the Dispense Bill/Note Parameter .....", "A07A", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);

							for (i = 0; i < ArrCashRet.Length; i++)
							{
								if (ArrCashRet[i] > 0)
								{
									switch (i)
									{
										case 0:
											intTemp += 1;
											Dpara[intTemp].BillCount = ArrCashRet[i];
											Dpara[intTemp].BillType = 0;
											_____log.LogText(LogChannel, CurrentProcessId, $@"Cash Should Refund RM 1 Note for Qty : {ArrCashRet[i].ToString()}", "A07B", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
											break;
										case 1:
											intTemp += 1;
											Dpara[intTemp].BillCount = ArrCashRet[i];
											Dpara[intTemp].BillType = 2;
											_____log.LogText(LogChannel, CurrentProcessId, $@"Cash Should Refund RM 5 Note for Qty : {ArrCashRet[i].ToString()}", "A07C", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
											break;
										case 2:
											intTemp += 1;
											Dpara[intTemp].BillCount = ArrCashRet[i];
											Dpara[intTemp].BillType = 3;
											_____log.LogText(LogChannel, CurrentProcessId, $@"Cash Should Refund RM 10 Note for Qty : {ArrCashRet[i].ToString()}", "A07D", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
											break;
									}
								}
							}

							_____log.LogText(LogChannel, CurrentProcessId, Dpara, "A08", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info, 
												extraMsg: "Start - Dispense Bill/Note; Bill/Note Refund; Showing Dispense Parameters");
							objB2B.Dispense(Dpara);
							_____log.LogText(LogChannel, CurrentProcessId, "End - Dispense Bill/Note; Done - Cash Refund; Please Collect Your Ticket; Proceed to PaymentDone;", "A09", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);

							PaymentDone();
						}
						else
						{
							PaymentDone();
						}

						_____log.LogText(LogChannel, CurrentProcessId, $@"End - Coin Refund Process; ", "A11A", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
					}
					else
					{
						// When error encountered .. 
						CashInsertedRefund();
					}
				}
				else
				{
					// When error encountered .. 
					CashInsertedRefund();
					
					if (!flg)
					{
						_____log.LogText(LogChannel, CurrentProcessId, $@"Note Machine - Dispense Error; Not Enough Cash(Bill/Note) To Refund; Cash(Bill/Note) To Refund : {dblTempNoteBalc};", "A15", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
					}
					else
					{
						_____log.LogText(LogChannel, CurrentProcessId, $@"Coin Machine - Dispense Error 2; Not Enough Coin To Refund; Coin To Refund : {dblTempCoinBalc};", "A15", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
					}
				}
			}
			catch (Exception ex)
			{
				_____log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BTest.CashRefund", currentTime:DateTime.Now);
			}

			_____log.LogText(LogChannel, CurrentProcessId, $@"End - CashRefund; ", "A20", "B2BTest.CashRefund", AppDecorator.Log.MessageType.Info);
		}

		public Boolean CheckNoteRefund(decimal dblTempNoteBalc, Boolean flg)
		{
			_____log.LogText(LogChannel, CurrentProcessId, $@"Start - CheckNoteRefund; Note-Balc : RM {dblTempNoteBalc.ToString()}", "A01", "B2BTest.CheckNoteRefund", AppDecorator.Log.MessageType.Info);

			int intReturnQty = 0;
			int intTemp = 0;
			if (dblTempNoteBalc >= 10 && ArrCashIn[2] > 0)
			{
				intTemp = Convert.ToInt32(Math.Floor(dblTempNoteBalc / 10));
				if (ArrCashIn[2] < intTemp)
				{
					intTemp = ArrCashIn[2];
				}
				ArrCashRet[2] = intTemp;
				intReturnQty += intTemp;
				dblTempNoteBalc -= intTemp * 10;
			}
			intTemp = 0;
			if (dblTempNoteBalc >= 5 && ArrCashIn[1] > 0)
			{
				intTemp = Convert.ToInt32(Math.Floor(dblTempNoteBalc / 5));
				if (ArrCashIn[1] < intTemp)
				{
					intTemp = ArrCashIn[1];
				}
				ArrCashRet[1] = intTemp;
				intReturnQty += intTemp;
				dblTempNoteBalc -= intTemp * 5;
			}

			if (dblTempNoteBalc > 0 && ArrCashIn[0] > 0)
			{
				if (Convert.ToInt32(dblTempNoteBalc) <= ArrCashIn[0])
				{
					ArrCashRet[0] = Convert.ToInt32(dblTempNoteBalc);
					intReturnQty += Convert.ToInt32(dblTempNoteBalc);
				}
				else
				{
					flg = false;
				}
			}
			else
			{
				if (dblTempNoteBalc > 0) flg = false;
			}
			if (intReturnQty > 24)
			{
				_____log.LogText(LogChannel, CurrentProcessId, $@"End - CheckNoteRefund; Return flg : False; Return-Qty > 24; intReturnQty : {intReturnQty}; ", "A19", "B2BTest.CheckNoteRefund", AppDecorator.Log.MessageType.Info);
				return false;
			}

			_____log.LogText(LogChannel, CurrentProcessId, $@"End - CheckNoteRefund; Return flg : {flg.ToString()}", "A20", "B2BTest.CheckNoteRefund", AppDecorator.Log.MessageType.Info);
			return flg;
		}

		public void CashInsertedRefund()
		{
			_____log.LogText(LogChannel, CurrentProcessId, "Start - CashInsertedRefund", "A01", "B2BTest.CashInsertedRefund", AppDecorator.Log.MessageType.Info);

			int intTemp = 0;
			objB2B.ToDisableMode();

			int i = 0;
			for (i = 0; i < ArrCashRec.Length; i++)
			{
				if (ArrCashRec[i] > 0)
				{
					intTemp += 1;
				}
			}
			CCNET.BillToBill.DispenseParameter[] Dpara = new CCNET.BillToBill.DispenseParameter[intTemp];

			_____log.LogText(LogChannel, CurrentProcessId, ArrCashRec, "A02", "B2BTest.CashInsertedRefund", AppDecorator.Log.MessageType.Info
				, extraMsg: "MsgObjDesc : Showing Cash Rec. Array");

			intTemp = -1;
			for (i = 0; i < ArrCashRec.Length; i++)
			{
				if (ArrCashRec[i] > 0)
				{
					switch (i)
					{
						case 0:
							intTemp += 1;
							Dpara[intTemp].BillCount = ArrCashRec[i];
							Dpara[intTemp].BillType = 0;

							_____log.LogText(LogChannel, CurrentProcessId, "RM 1 [ Qty : " + ArrCashRec[i].ToString() + " ]", "A03", "B2BTest.CashInsertedRefund", AppDecorator.Log.MessageType.Info);
							break;
						case 1:
							intTemp += 1;
							Dpara[intTemp].BillCount = ArrCashRec[i];
							Dpara[intTemp].BillType = 2;
							_____log.LogText(LogChannel, CurrentProcessId, "RM 5 [ Qty : " + ArrCashRec[i].ToString() + " ]", "A04", "B2BTest.CashInsertedRefund", AppDecorator.Log.MessageType.Info);
							break;
						case 2:
							intTemp += 1;
							Dpara[intTemp].BillCount = ArrCashRec[i];
							Dpara[intTemp].BillType = 3;
							_____log.LogText(LogChannel, CurrentProcessId, "RM 10 [ Qty : " + ArrCashRec[i].ToString() + " ]", "A05", "B2BTest.CashInsertedRefund", AppDecorator.Log.MessageType.Info);
							break;
					}

					_____log.LogText(LogChannel, CurrentProcessId, "Done - Cancel Refund", "A06", "B2BTest.CashInsertedRefund", AppDecorator.Log.MessageType.Info);
				}
			}

			_____log.LogText(LogChannel, CurrentProcessId, Dpara, "A07", "B2BTest.CashInsertedRefund", AppDecorator.Log.MessageType.Info
				, extraMsg: "Start Dispense Bill/Note; MsgObjDesc : Showing Dispense Params Array");
			objB2B.Dispense(Dpara);
			_____log.LogText(LogChannel, CurrentProcessId, "End - CashInsertedRefund; End Dispense Bill/Note;", "A08", "B2BTest.CashInsertedRefund", AppDecorator.Log.MessageType.Info);
		}

		public void ExtractBillTable(byte[] Data)
		{
			_____log.LogText(LogChannel, CurrentProcessId, Data, "A01", "B2BTest.ExtractBillTable", AppDecorator.Log.MessageType.Info
				, extraMsg: "Start ExtractBillTable; MsgObjDesc : Showing Data Array");

			if (Data != null && Data.Length == 125)
			{
				int p = 0;
				for (int i = 3; i < 117; i++)
				{
					billtable[p].CountryCode = null;
					billtable[p].CountryCode += Encoding.ASCII.GetString(Data, i + 1, 3);
					switch (Data[i + 4])
					{
						case 0x00:
							billtable[p].DigitBillType = Data[i];
							billtable[p].BillType = p;
							break;
						case 0x01:
							billtable[p].DigitBillType = Data[i] * 10;
							billtable[p].BillType = p;
							break;
						case 0x02:
							billtable[p].DigitBillType = Data[i] * 100;
							billtable[p].BillType = p;
							break;
						case 0x03:
							billtable[p].DigitBillType = Data[i] * 1000;
							billtable[p].BillType = p;
							break;
						case 0x04:
							billtable[p].DigitBillType = Data[i] * 10000;
							billtable[p].BillType = p;
							break;
						case 0x05:
							billtable[p].DigitBillType = Data[i] * 100000;
							billtable[p].BillType = p;
							break;
						case 0x06:
							billtable[p].DigitBillType = Data[i] * 1000000;
							billtable[p].BillType = p;
							break;
						default:
							break;
					}

					i = i + 4;
					p++;
				}

				_____log.LogText(LogChannel, CurrentProcessId, billtable, "A03", "B2BTest.ExtractBillTable", AppDecorator.Log.MessageType.Info
					, extraMsg: "MsgObjDesc : Showing BillTable Array");
			}
			else
			{
				if (Data != null)
				{
					for (int i = 0; i < 23; i++)
					{
						billtable[i].CountryCode = null;
						billtable[i].DigitBillType = 0;
						billtable[i].BillType = 0;
					}
				}
				_____log.LogText(LogChannel, CurrentProcessId, billtable, "A05", "B2BTest.ExtractBillTable", AppDecorator.Log.MessageType.Info
					, extraMsg: "Abnormal Data Parameter; MsgObjDesc : Showing BillTable Array");
			}

			_____log.LogText(LogChannel, CurrentProcessId, "End - ExtractBillTable", "A10", "B2BTest.ExtractBillTable", AppDecorator.Log.MessageType.Info);
		}

		public B2BCassetteInfoCollection GetCassetteQtyStatus()
		{
			B2BCassetteInfoCollection casesColl = new B2BCassetteInfoCollection();

			Answer ans = objB2B.CCNET.RunCommand(CCNETCommand.RECYCLING_CASSETTE_STATUS);

			if (ans.SendedData[3] == 0x3B && ans.Message.ToLower() != "illegal comand" && ans.ReceivedData != null)
			{
				string[] columnArr = null;
				int numberOfCassette = casesColl.GetNumberOfCassette();

				// Note : rowArr.Length must same as numberOfCassette
				string[] rowArr = ans.Message.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

				// Read Bill Type and Qty for Cassette00
				for (int cassetteInx = 0; cassetteInx < numberOfCassette; cassetteInx++)
				{
					string rowStr = rowArr[cassetteInx];

					columnArr = rowStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
					for (int dInx = 0; dInx < columnArr.Length; dInx++)
					{
						string[] elementArr = columnArr[dInx].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

						switch (dInx)
						{
							// Read Cassette Index
							case 0:
								casesColl[cassetteInx].CassetteNo = Convert.ToInt32(elementArr[1]);
								break;

							// Read Cassette Bill Type
							case 1:
								casesColl[cassetteInx].BillType = Convert.ToInt32(elementArr[1]);
								break;

							// Read Cassette Current Quantity
							case 2:
								casesColl[cassetteInx].BillQty = Convert.ToInt32(elementArr[1]);
								break;

							// Read Cassette OccasionOfPresence flag
							case 3:
								casesColl[cassetteInx].IsCassettePresence = Convert.ToBoolean(elementArr[1].Trim().ToLower());
								break;

							// Read "Cassette Is Full flag"
							case 4:
								casesColl[cassetteInx].IsCassetteFull = Convert.ToBoolean(elementArr[1].Trim().ToLower());
								break;

							// Read "Cassette Is used as escrow flag"
							case 5:
								casesColl[cassetteInx].IsEscrowEnable = Convert.ToBoolean(elementArr[1].Trim().ToLower());
								break;
						}
					}
				}
			}

			return casesColl;
		}

		public void Dispose()
		{
			try
			{
				CloseDevice();
				try { objB2B?.Dispose(); } catch { }

				objB2B = null;
			}
			catch { }
		}
	}
}
