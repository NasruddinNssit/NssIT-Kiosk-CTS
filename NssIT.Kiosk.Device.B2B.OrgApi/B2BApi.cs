using CCNET;
using CCNET.BillToBill;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data.Machine;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.OrgApi
{
	public class B2BApi : IDisposable
	{
		private const string LogChannel = "B2B_API";
		private B2BDecorator.Data.BillTable[] _billtable;
		private DbLog _log = null;

		private string C2CStatus = "";

		private int intCashD1Limit = 90;
		private int intCashD2Limit = 90;
		private int intCashD3Limit = 90;

		private string _currProcessId = "-";

		public BillToBill objB2B;

		SynchronizationContext _thisContext = null;

		public B2BApi()
		{
			_log = DbLog.GetDbLog();
			objB2B = new BillToBill();

			if (SynchronizationContext.Current != null)
				_thisContext = SynchronizationContext.Current;
			else
				_thisContext = new SynchronizationContext();
		}

		public Guid? CurrentNetProcessId { get; set; } = null;
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

		private DbLog _____log
		{
			get
			{
				return _log;
			}
		}
				
		private int b2bMaxRetryCount = 3;
		public bool SearchAndConnect()
		{
			_____log.LogText(LogChannel, CurrentProcessId, "Start - SearchAndConnect", "A01", "B2BApi.SearchAndConnect");
			bool isConnected = false;

			int b2bRetryCount = 0;
			do
			{
				_____log.LogText(LogChannel, CurrentProcessId, $@"Reconnecting B2B ......; Retry Count : {b2bRetryCount}", "A02", "B2BApi.InitDevice", AppDecorator.Log.MessageType.Info);

				isConnected = objB2B.SearchAndConnect();
				Task.Delay(1000).Wait();
				b2bRetryCount++;

			} while ((!isConnected) && (b2bRetryCount < b2bMaxRetryCount));
			
			_____log.LogText(LogChannel, CurrentProcessId, "End - SearchAndConnect", "A02", "B2BApi.SearchAndConnect");
			return isConnected;
		}

		BillToBill.Event _billToBillStatusChangedHandle = null;
		BillToBill.AnswerReceivedEvent _pollAnswerReceivedHandle = null;

		public bool OpenConnection()
		{
			_____log.LogText(LogChannel, CurrentProcessId, "Start OpenConnection - B2B Connected and Startup ......", "A01", "B2BApi.OpenConnection");

			if (objB2B?.CCNET?.Port.IsOpen == true)
			{
				CloseDevice();
				Task.Delay(3000).Wait();
			}

			if (objB2B?.CCNET?.Port.IsOpen == false)
			{
				_____log.LogText(LogChannel, CurrentProcessId, "Opening port for B2B ..", "A03", "B2BApi.OpenConnection");
				objB2B.Open(); Task.Delay(20).Wait();

				// ----- Handle B2B Machine Events -----
				if (_billToBillStatusChangedHandle == null)
					_billToBillStatusChangedHandle = new BillToBill.Event(objB2B_OnBillToBillStatusChanged);

				if (_pollAnswerReceivedHandle == null)
					_pollAnswerReceivedHandle = new BillToBill.AnswerReceivedEvent(objB2B_OnPollAnswerReceived);

				objB2B.OnBillToBillStatusChanged -= _billToBillStatusChangedHandle;
				objB2B.OnPollAnswerReceived -= _pollAnswerReceivedHandle;

				objB2B.OnBillToBillStatusChanged += _billToBillStatusChangedHandle;
				objB2B.OnPollAnswerReceived += _pollAnswerReceivedHandle;
				// ----- -----

				_____log.LogText(LogChannel, CurrentProcessId, "B2B Polling ..", "A03B", "B2BApi.OpenConnection");
				objB2B.IsPoll = true; Task.Delay(20).Wait();

				_billtable = new B2BDecorator.Data.BillTable[23];
				Answer answer = objB2B.CCNET.RunCommand(CCNETCommand.GET_BILL_TABLE, true); Task.Delay(20).Wait();
				ExtractBillTable(answer.ReceivedData);

				objB2B.PollInterval = 150; Task.Delay(20).Wait();

				//objB2B.Cassettes[0].SetCassetteType(0); /*Set Note Type to 0*/
				//objB2B.Cassettes[1].SetCassetteType(2); /*Set Note Type to 2*/
				//objB2B.Cassettes[2].SetCassetteType(3); /*Set Note Type to 3*/

				_____log.LogText(LogChannel, CurrentProcessId, $@"B2B LoadLimit Cassette 0.. Limit : {intCashD1Limit}", "A04", "B2BApi.OpenConnection");
				objB2B.Cassettes[0].LoadLimit = intCashD1Limit; Task.Delay(20).Wait();

				_____log.LogText(LogChannel, CurrentProcessId, $@"B2B LoadLimit Cassette 1.. Limit : {intCashD2Limit}", "A05", "B2BApi.OpenConnection");
				objB2B.Cassettes[1].LoadLimit = intCashD2Limit; Task.Delay(20).Wait();

				_____log.LogText(LogChannel, CurrentProcessId, $@"B2B LoadLimit Cassette 2.. Limit : {intCashD3Limit}", "A06", "B2BApi.OpenConnection");
				objB2B.Cassettes[2].LoadLimit = intCashD3Limit; Task.Delay(20).Wait();

				_____log.LogText(LogChannel, CurrentProcessId, "End - OpenConnection; B2B Connected", "A10", "B2BApi.OpenConnection");
			}
			else
			{
				_____log.LogText(LogChannel, CurrentProcessId, "End - OpenConnection; Unable to connect Cash (C1) Machine properly.", "E01"
					, "B2BApi.OpenConnection", AppDecorator.Log.MessageType.Error);
			}

			if (objB2B?.CCNET?.Port.IsOpen == true)
				return true;
			else
				return false;
		}

		public Bill_to_Bill_Status B2BStatus
		{
			get => objB2B.BillToBillStatus;
		}

		public B2BDecorator.Data.BillTable[] BillTableArray
		{
			get => _billtable;
		}

		public CassetteCollectoion B2BCassetteList
		{ get => objB2B.Cassettes; }

		public void SetB2BToNormal(string extraTag, out Exception ex, int maxWaitSec = 60)
		{
			ex = null;
			maxWaitSec = (maxWaitSec < 0) ? 1 : maxWaitSec;

			_____log.LogText(LogChannel, CurrentProcessId, $@"Start - B2BToNormal; maxWaitSec : {maxWaitSec}", $@"A04-{extraTag}", "B2BApi.B2BToNormal");

			int pollCount = 0;
			DateTime startTimer = DateTime.Now;
			DateTime endTimer = startTimer.AddSeconds(maxWaitSec);

			Bill_to_Bill_Status status = objB2B.BillToBillStatus;

			do
			{
				objB2B.ToDisableMode(); Task.Delay(20).Wait();
				status = objB2B.BillToBillStatus;

				if (status != Bill_to_Bill_Status.DISABLED)
				{
					pollCount++;
					objB2B.CCNET.RunCommand(CCNETCommand.Poll);
					Task.Delay(350).Wait();
					status = objB2B.BillToBillStatus;
				}
				
			} while ((status != Bill_to_Bill_Status.DISABLED) && (endTimer.Subtract(DateTime.Now).TotalMilliseconds > 0)) ;

			if (status != Bill_to_Bill_Status.DISABLED)
				ex = new Exception($@"Unable to set B2B to normal (DISABLE) state; B2B Latest status : {Enum.GetName(typeof(Bill_to_Bill_Status), status)}; ");

			_____log.LogText(LogChannel, CurrentProcessId, $@"End - B2BToNormal; Last B2B Status : {Enum.GetName(typeof(Bill_to_Bill_Status), status)}; Sent B2B Poll Count : {pollCount}", $@"A04-{extraTag}", "B2BApi.B2BToNormal");
		}

		public Answer RunCommand(CCNETCommand command, byte[] data, string logMsg = "-", string logCodeLoc = "-")
		{
			_____log.LogText(LogChannel, CurrentProcessId, data
				, extraMsg: "Start Run Command : " + Enum.GetName(typeof(CCNETCommand), command) + $@"Start - {logMsg};" + ";MsgObj : Related Byte List"
				, subBlockTag: $@"A01; {logCodeLoc};", classNMethodName: "B2BApi.RunCommand");

			Answer retAns = objB2B.CCNET.RunCommand(command, data);
			Task.Delay(20).Wait();

			_____log.LogText(LogChannel, CurrentProcessId, $@"End Run Command; End - {logMsg};", $@"A10; {logCodeLoc};", "B2BApi.RunCommand");

			return retAns;
		}

		public void Dispense(CCNET.BillToBill.DispenseParameter[] dParams)
		{
			_____log.LogText(LogChannel, CurrentProcessId, dParams, "A01", "B2BApi.Dispense",
				AppDecorator.Log.MessageType.Info,
				extraMsg: "Start - Dispense Bill/Note; Bill/Note Refund; MsgObj: Showing Dispense Parameters", 
				adminMsg: $@"Start Dispend Bill/Note");

			objB2B.Dispense(dParams);

			_____log.LogText(LogChannel, CurrentProcessId, "End - Dispense Bill/Note; Done - Cash Refund; Please Collect Your Ticket; Proceed to PaymentDone;", "A10", "B2BApi.Dispense",
				adminMsg: $@"End Dispend Bill/Note with Success");

		}

		private void objB2B_OnBillToBillStatusChanged(Object sender, EventArgs e)
		{
			_____log.LogText(LogChannel, CurrentProcessId, $@"B2B Status : {Enum.GetName(typeof(Bill_to_Bill_Status), objB2B.BillToBillStatus)}", "A02", "B2BApi.objB2B_OnBillToBillStatusChanged");
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
								casesColl[cassetteInx].DigitBillType = _billtable[Convert.ToInt32(elementArr[1])].DigitBillType;
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
		
		//public event EventHandler<B2BDecorator.Events.InProgressEventArgs> OnInProgress;
		public event EventHandler<B2BDecorator.Events.MachineEventArgs> OnPollAnswerResponse;
		private long _prLoopCount = 1;
		private long _prErrLoopCount = 1;
		private string _lastPollAnswerReceivedStage = null;
		private void objB2B_OnPollAnswerReceived(object sender, CCNETEventArgs e)
		{
			bool roundChkLog = false;
			if ((_lastPollAnswerReceivedStage == null) || GetPollAnswerReceivedLogRequest())
			{
				roundChkLog = true;
				_____log.LogText(LogChannel, CurrentProcessId, "Start objB2B_OnPollAnswerReceived", "A01", "B2BApi.objB2B_OnPollAnswerReceived");
			}
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

					if (objB2B.BillToBillStatus == Bill_to_Bill_Status.ACCEPTING)
					{
						// ----- ACCEPTING State --> when note is detected and accepting
						currState = $@"Bill_To_Bill_Status : {objB2B.BillToBillStatus.ToString()}; Answer.Message : {C2CStatus}; MyAnswer.Data: {AppDecorator.Common.Common2s.BytesToNumbersStr(MyAnswer.Data)}";

						if ((!_lastPollAnswerReceivedStage.Equals(currState)) || (roundChkLog))
						{
							if (CurrentNetProcessId.HasValue)
							{
								_____log.LogText(LogChannel, CurrentProcessId,
									$@"Bill/Note Detected (Stage 1/3); {currState};", "A02", "B2BApi.objB2B_OnPollAnswerReceived",
									adminMsg: $@"Bill/Note (1/3) - Detected");

								B2BDecorator.Events.MachineEventArgs ev = new B2BDecorator.Events.MachineEventArgs()
								{
									MachData = new B2BApiPaymentFound(),
									Message = $@"Banknote detected; Work in progress.."
								};

								OnPollAnswerResponse?.Invoke(null, ev);
							}
							else
							{
								_____log.LogText(LogChannel, CurrentProcessId,
									$@"Net Process Id not found !!; Bill/Note Detected (Stage 1/3); {currState};", "A02E", "B2BApi.objB2B_OnPollAnswerReceived",
									adminMsg: $@"Bill/Note (1/3 E) - Detected ; ");
							}
							_prLoopCount = 1;
						}
						else
							_prLoopCount++;
					}
					else if (objB2B.BillToBillStatus == Bill_to_Bill_Status.STACKING)
					{
						// ----- STACKING State --> when note is stacking
						currState = $@"Bill_To_Bill_Status : {objB2B.BillToBillStatus.ToString()}; Answer.Message : {C2CStatus}";

						if ((!_lastPollAnswerReceivedStage.Equals(currState)) || (roundChkLog))
						{
							_____log.LogText(LogChannel, CurrentProcessId
							, $@"Bill/Note Stacking (Stage 2/3); {currState};", "A03"
							, "B2BApi.objB2B_OnPollAnswerReceived",
							adminMsg: $@"Bill/Note (2/3) - Stacking ");

							_prLoopCount = 1;
						}
						else
							_prLoopCount++;
					}
					else if (objB2B.BillToBillStatus == Bill_to_Bill_Status.PACKED_STACKED)
					{
						// ----- PACKED_STACKED State --> when confirm note in Cassette
						currState = $@"Bill_To_Bill_Status : {objB2B.BillToBillStatus.ToString()}; Answer.Message : {C2CStatus}";

						if (CurrentNetProcessId.HasValue)
						{
							string currency = B2BDecorator.B2BTools.B2BCountryToCurrencyCode.GetCurrencyString(_billtable[Convert.ToInt32(MyAnswer.Data[1])].CountryCode);
							string billValue = String.Format("{0:00.00}", _billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType);

							_____log.LogText(LogChannel, CurrentProcessId
							, $@"Bill/Note Received (Stage 3/3); Done - {currency} {billValue} stored into cassette; {currState};"
							, "A04", "B2BApi.objB2B_OnPollAnswerReceived",
							adminMsg: $@"Bill/Note (3/3) - {currency} {billValue} Stored to Cassette");

							//CloseCashPayTimer();
							//isBillAccepted(Convert.ToInt32(MyAnswer.Data[1]));

							B2BDecorator.Events.MachineEventArgs ev = new B2BDecorator.Events.MachineEventArgs()
							{
								Message = $@"{B2BDecorator.B2BTools.B2BCountryToCurrencyCode.GetCurrencyString(_billtable[Convert.ToInt32(MyAnswer.Data[1])].CountryCode)} {
									String.Format("{0:00.00}", _billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType)}",
								MachData = new B2BApiNewInsertedNote()
								{
									DigitBillType = _billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType
								}
							};

							OnPollAnswerResponse?.Invoke(null, ev);
						}
						else
						{
							_____log.LogText(LogChannel, CurrentProcessId
							, $@"Net Process Id is not found !!; Bill/Note Received (Stage 3/3); Done - {B2BDecorator.B2BTools.B2BCountryToCurrencyCode.GetCurrencyString(_billtable[Convert.ToInt32(MyAnswer.Data[1])].CountryCode)} {
								String.Format("{0:00.00}", _billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType)} stored into cassette; {currState};", "A04E"
							, "B2BApi.objB2B_OnPollAnswerReceived");
						}
						
					}
					else if ((objB2B.BillToBillStatus == Bill_to_Bill_Status.DISPENSED) || 
								(objB2B.BillToBillStatus == Bill_to_Bill_Status.DISPENSING_accessible_to_customer) || 
								(objB2B.BillToBillStatus == Bill_to_Bill_Status.DISPENSING_from_recycling_cassette_to_dispenser) || 
								(objB2B.BillToBillStatus == Bill_to_Bill_Status.DISPENSING_from_recycling_cassette_to_dispenser)
							)
					{
						currState = $@"Bill_To_Bill_Status : {objB2B.BillToBillStatus.ToString()}; Answer.Message : {C2CStatus}" +
							"; Received Answer Data => " + BytesToNumbers(MyAnswer.Data) + "; Send Data => " + BytesToNumbers(sendData) +
							"; Received Sent Data => " + BytesToNumbers(MyAnswer.SendedData);

						if ((!_lastPollAnswerReceivedStage.Equals(currState)) || (roundChkLog))
						{
							_prLoopCount = 1;

							// ----- Dispenses State 
							//if (logEnabled)
							//_____log.LogText(LogChannel, CurrentProcessId, $@"Bill/Note Dispending; {currState}; RM {String.Format("{0:00.00}", billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType)}", "A04M", "B2BApi.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);

							//_____log.LogText(LogChannel, CurrentProcessId, new object[] { MyAnswer.Data,  }, "A04M", "B2BApi.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Info);

							//_____log.LogText(LogChannel, CurrentProcessId, new object[] { MyAnswer.Data, MyAnswer.Additional_Data, MyAnswer.Message }, "A04M", "B2BApi.objB2B_OnPollAnswerReceived", 
							//	AppDecorator.Log.MessageType.Info,
							//	extraMsg: $@"Bill/Note Dispending; {currState}; RM {String.Format("{0:00.00}", billtable[Convert.ToInt32(MyAnswer.Data[1])].DigitBillType)}");

							_____log.LogText(LogChannel, CurrentProcessId, currState, "A04M", "B2BApi.objB2B_OnPollAnswerReceived");
						}
						else
							_prLoopCount++;
					}
					else if (objB2B.BillToBillStatus == Bill_to_Bill_Status.POWER_UP)
					{
						// ----- POWER_UP State
						_____log.LogText(LogChannel, CurrentProcessId, "Bill_to_Bill_Status is POWER_UP", "A05", "B2BApi.objB2B_OnPollAnswerReceived");
						objB2B.CCNET.RunComandNonAnswer(CCNETCommand.RESET);
						_prLoopCount = 1;
					}
					else
					{
						currState = $@"Bill_To_Bill_Status : {objB2B.BillToBillStatus.ToString()}; Answer.Message : {C2CStatus}";

						if ((!_lastPollAnswerReceivedStage.Equals(currState)) || (roundChkLog))
						{
							if (objB2B.BillToBillStatus == Bill_to_Bill_Status.IDLING)
							{
								_____log.LogText(LogChannel, CurrentProcessId, "Bill_to_Bill_Status = IDLING", "A06", "B2BApi.objB2B_OnPollAnswerReceived");
							}
							else
							{
								_____log.LogText(LogChannel, CurrentProcessId, $@"{currState}; Similar Loop Count : {_prLoopCount}; ", "A07", "B2BApi.objB2B_OnPollAnswerReceived");
							}

							_prLoopCount = 1;
						}
						else
							_prLoopCount++;
					}
				}
				else
				{
					_prLoopCount = 1;

					currState = $@"Error From B2B : {MyAnswer?.Error?.Data?.ToString()} - {MyAnswer?.Error?.Message}";

					if ((!_lastPollAnswerReceivedStage.Equals(currState)) || (roundChkLog))
					{
						_____log.LogText(LogChannel, CurrentProcessId, $@"{currState}; Similar Error Loop Count : {_prErrLoopCount}", "A11"
							, "B2BApi.objB2B_OnPollAnswerReceived", AppDecorator.Log.MessageType.Error);
						_prErrLoopCount = 1;
					}
					else
						_prErrLoopCount++;
				}

				_lastPollAnswerReceivedStage = currState;
			}
			catch (Exception ex)
			{
				string stateMsg = Enum.GetName(typeof(Bill_to_Bill_Status), objB2B.BillToBillStatus);

				_____log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BApi.objB2B_OnPollAnswerReceived"
					, adminMsg: $@"Error with Cash Machine; {ex.Message}; Machine State: {stateMsg}");
			}
		}

		private string BytesToNumbers(byte[] data)
		{
			int inx = 0;
			string retStr = "";
			foreach (byte aByte in data)
				retStr += $@"{inx++}: {((int)aByte).ToString()};";

			return retStr;
		}

		private void ExtractBillTable(byte[] Data)
		{
			_____log.LogText(LogChannel, CurrentProcessId, Data, "A01", "B2BApi.ExtractBillTable", extraMsg: "Start ExtractBillTable; MsgObj : Showing Data Array");

			if (Data != null && Data.Length == 125)
			{
				int p = 0;
				for (int i = 3; i < 117; i++)
				{
					_billtable[p].CountryCode = null;
					_billtable[p].CountryCode += Encoding.ASCII.GetString(Data, i + 1, 3);
					switch (Data[i + 4])
					{
						case 0x00:
							_billtable[p].DigitBillType = Data[i];
							_billtable[p].BillType = p;
							break;
						case 0x01:
							_billtable[p].DigitBillType = Data[i] * 10;
							_billtable[p].BillType = p;
							break;
						case 0x02:
							_billtable[p].DigitBillType = Data[i] * 100;
							_billtable[p].BillType = p;
							break;
						case 0x03:
							_billtable[p].DigitBillType = Data[i] * 1000;
							_billtable[p].BillType = p;
							break;
						case 0x04:
							_billtable[p].DigitBillType = Data[i] * 10000;
							_billtable[p].BillType = p;
							break;
						case 0x05:
							_billtable[p].DigitBillType = Data[i] * 100000;
							_billtable[p].BillType = p;
							break;
						case 0x06:
							_billtable[p].DigitBillType = Data[i] * 1000000;
							_billtable[p].BillType = p;
							break;
						default:
							break;
					}

					i = i + 4;
					p++;
				}

				_____log.LogText(LogChannel, CurrentProcessId, _billtable, "A03", "B2BApi.ExtractBillTable", extraMsg: "MsgObj : Showing BillTable List");
			}
			else
			{
				if (Data != null)
				{
					for (int i = 0; i < 23; i++)
					{
						_billtable[i].CountryCode = null;
						_billtable[i].DigitBillType = 0;
						_billtable[i].BillType = 0;
					}
				}
				_____log.LogText(LogChannel, CurrentProcessId, _billtable, "A05", "B2BApi.ExtractBillTable"
					, AppDecorator.Log.MessageType.Error 
					, extraMsg: "Abnormal Bill-table Parameters.; MsgObj : Showing BillTable List");

				throw new Exception("Abnormal Bill-table Parameters.");
			}

			_____log.LogText(LogChannel, CurrentProcessId, "End - ExtractBillTable", "A10", "B2BApi.ExtractBillTable");
		}

		public void CloseDevice()
		{
			try
			{
				_____log.LogText(LogChannel, CurrentProcessId, "Start CloseDevice", "A01", "B2BApi.CloseDevice");

				if (objB2B?.CCNET?.Port.IsOpen == true)
				{

					_____log.LogText(LogChannel, CurrentProcessId, "Start Close Port", "A04", "B2BApi.CloseDevice");

					try
					{
						objB2B.ToDisableMode();
						Task.Delay(20).Wait();
					}
					catch { }

					objB2B.CCNET.Port.Close();
					Task.Delay(20).Wait();
					_____log.LogText(LogChannel, CurrentProcessId, "End Close Port", "A05", "B2BApi.CloseDevice");

				}
				else if (objB2B?.CCNET?.Port.IsOpen == false)
				{
					_____log.LogText(LogChannel, CurrentProcessId, "Port Already Been Closed", "A06", "B2BApi.CloseDevice");
				}
				else
				{
					_____log.LogText(LogChannel, CurrentProcessId
						, $@"B2B instants Error; {(objB2B == null ? "B2B instant is null." : "B2B got instant.")}; {((objB2B?.CCNET == null) ? "CCNET instant is null." : "CCNET got instant.")}; "
						, "A07", "B2BApi.CloseDevice", AppDecorator.Log.MessageType.Error);
				}
			}
			catch (Exception ex)
			{
				_____log.LogError(LogChannel, CurrentProcessId, ex, "E01", "B2BApi.CloseDevice");
			}

			_____log.LogText(LogChannel, CurrentProcessId, "End CloseDevice", "A20", "B2BApi.CloseDevice");
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