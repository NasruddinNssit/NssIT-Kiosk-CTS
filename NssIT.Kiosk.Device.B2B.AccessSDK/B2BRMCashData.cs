using CCNET.BillToBill;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;
using NssIT.Kiosk.AppDecorator.Devices.Payment.ErrorNode;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.AccessSDK
{
	[Serializable]
	public class B2BRMCashData : AppDecorator.Devices.Payment.ICustomerPaymentInfo
	{
		private const string LogChannel = "B2BAccess";

		private decimal _price = 0.00M;
		private decimal _insertedAmount = 0.00M;
		private decimal _outstandingPayment = 0.00M;
		private decimal _refundAmount = 0.00M;
		private string _currProcessId = "-";

		private DbLog _log = null;

		// Note	: For RM	- Cassette 1 (index 0, BillType = 0) for RM 1
		//					- Cassette 2 (index 1, BillType = 2) for RM 5
		//					- Cassette 3 (index 2, BillType = 3) for RM 10
		private int[] ArrCashIn = new int[3];   /* Array refer to Cassette that Store current available cash quantity */
		private int[] ArrCashRec = new int[3];  /* Array refer to Cassette that Store cash quantity that is received for only the latest sales */
		private int[] ArrCashRet = new int[3];  /* Array refer to Cassette that Store cash quantity needed to be refunded refer to the latest sales; Used for cancelling sales; */

		public decimal Price { get { return _price; } }
		public decimal InsertedAmount { get { return _insertedAmount; } }
		public decimal RefundAmount { get { return _refundAmount; } }
		public string CurrProcessId { get { return _currProcessId; } }

		private Dictionary<int, B2BDecorator.Data.BillTable> _billTableList = new Dictionary<int, B2BDecorator.Data.BillTable>();
		private B2BDecorator.Data.BillTable[] _billTable;
		public B2BDecorator.Data.BillTable[] BillTableArray
		{
			get => _billTable;
			set
			{
				_billTableList.Clear();

				if (value?.Length > 0)
					foreach (B2BDecorator.Data.BillTable bill in value)
						_billTableList.Add(bill.BillType, bill);

				_billTable = value;
			}
		}

		public string GetLastCustomerPaymentInfo()
		{
			return $@"Price : RM {Price}; {"\r\n"}Inserted Amount : RM {InsertedAmount}; {"\r\n"}Reference Number : {CurrProcessId}; ";
		}

		public B2BRMCashData(DbLog log, string currentProcessId)
		{
			ResetArr();

			_log = log;
			CurrentProcessId = currentProcessId;
		}

		public bool GetCurrencyAmount(int billType, out string currency, out int billValue, out string errorMsg)
		{
			currency = null;
			billValue = 0;
			errorMsg = null;

			if (_billTableList.Count > 0)
			{
				B2BDecorator.Data.BillTable? bill = _billTableList[billType];

				if (bill.HasValue)
				{
					billValue = bill.Value.DigitBillType;
					currency = B2BDecorator.B2BTools.B2BCountryToCurrencyCode.GetCurrencyString(bill.Value.CountryCode);
					return true;
				}
				errorMsg = $@"Unable to find the bill type [{billType}]";
			}
			else
				errorMsg = $@"Bill Table is empty. Currency not found.";

			return false;
		}
				
		private string CurrentProcessId
		{
			get
			{
				return _currProcessId;
			}
			set
			{
				_currProcessId = string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
			}
		}

		public void InitNewPrice(decimal price, string currentProcessId)
		{
			CurrentProcessId = currentProcessId;

			ResetArr();

			_price = price;
			_outstandingPayment = price;
			_insertedAmount = 0.00M;
			_refundAmount = 0.00M;
			
			_log.LogText(LogChannel, CurrentProcessId, this, "A02", "B2BCashData.InitSellingPrice", extraMsg: $@"MsgObj: B2BCashData Instant Properties");
		}

		public void Pay(int amount)
		{
			_log.LogText(LogChannel, CurrentProcessId, $@"Start Pay; Amount : {amount}; Previous OutstandingPayment : {_outstandingPayment}; Previous RefundAmount : {_refundAmount};", "A01", "B2BCashData.Pay", AppDecorator.Log.MessageType.Info);

			_insertedAmount += amount;
			_outstandingPayment -= amount;

			if (_outstandingPayment < 0)
			{
				_refundAmount += (_outstandingPayment * -1);
				_outstandingPayment = 0;
			}

			switch (amount)
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

			_log.LogText(LogChannel, CurrentProcessId, ArrCashRec, "A10", "B2BCashData.Pay", AppDecorator.Log.MessageType.Info
				, extraMsg: $@"End Pay; Latest OutstandingPayment : {_outstandingPayment}; Latest RefundAmount : {_refundAmount}; MsgObj: Latest Cassettes Array refer to sales received quantity");
		}

		public bool IsPaymentDone()
		{
			return ((_outstandingPayment <= 0) && (_price > 0));
		}

		public bool IsRefundRequest()
		{
			return (_refundAmount > 0);
		}

		public decimal GetOutstandingPayment()
		{
			return _outstandingPayment;
		}

		//public int ReadBillNoteAmount(int billType)
		//{
		//	return billtable[billType].DigitBillType;
		//}

		public byte[] GetBillTypeActivationData(Guid? netProcessId, string processId, decimal amount, CassetteCollectoion cassettes, out UIAcceptableBanknote acceptableBanknote)
		{
			acceptableBanknote = new UIAcceptableBanknote(netProcessId, processId, DateTime.Now);

			// XXXXX 
			Func<byte, UIAcceptableBanknote, UIAcceptableBanknote> 
				ListAcceptableBanknote = (code, accpbanknote) => 
			{
				List<AppDecorator.Devices.Payment.Banknote> BnkNoteList = new List<AppDecorator.Devices.Payment.Banknote>();

				string byteStr = Convert.ToString(code, 2).PadLeft(8, '0');

				int arrInx = 0;
				for(int inx=7; inx >= 0; inx--)
				{
					if (int.Parse(byteStr.Substring(inx,1)) == 1)
					{
						B2BDecorator.Data.BillTable bil = _billTable[arrInx];
						string curr = B2BDecorator.B2BTools.B2BCountryToCurrencyCode.GetCurrencyString(bil.CountryCode);

						AppDecorator.Devices.Payment.Banknote bn = new AppDecorator.Devices.Payment.Banknote() { Currency = curr, Value = bil.DigitBillType };
						BnkNoteList.Add(bn);
					}
					arrInx++;
				}

				accpbanknote.NoteArr = BnkNoteList.ToArray();
				BnkNoteList.Clear();

				return accpbanknote;
			};
			// XXXXX 

			Boolean flgRM50 = false;
			Boolean flgRM20 = false;
			Int32 max50InsertionQty = Convert.ToInt32(Math.Ceiling(amount / 50M));
			Int32 max20InsertionQty = Convert.ToInt32(Math.Ceiling(amount / 20M));

			int caseRM1qty = cassettes[0].BillQuantity;
			int caseRM5qty = cassettes[1].BillQuantity;
			int caseRM10qty = cassettes[2].BillQuantity;

			// RM 50 & RM 20 notes will not activated for amount more than RM 250
			if (amount <= 250)
			{
				bool isSmallNoteRefundPossible = true;

				if ((caseRM1qty < 4) && (caseRM5qty > 0) 
					|| (caseRM1qty < 9) && (caseRM5qty == 0)
					)
				{
					// If small note (RM 1 & RM 5) is not enough for refund, system will disable to the RM 50 & RM 20.
					decimal possibleRefund = ((decimal)max50InsertionQty * 50M) - amount;
					int possibleRefundWithNoCoin = (int)Math.Floor(possibleRefund);
					string possibleRefundWithNoCoinStr = possibleRefundWithNoCoin.ToString().Trim();
					int firstNoteRefundDigit = int.Parse(possibleRefundWithNoCoinStr.Substring(possibleRefundWithNoCoinStr.Length - 1, 1));

					if (firstNoteRefundDigit > 0)
					{
						if ((caseRM5qty == 0) && (caseRM1qty < firstNoteRefundDigit))
							isSmallNoteRefundPossible = false;

						else if ((caseRM5qty > 0) && ((firstNoteRefundDigit - 5) > 0) && (caseRM1qty < (firstNoteRefundDigit - 5)))
							isSmallNoteRefundPossible = false;

						else if ((caseRM5qty > 0) && ((firstNoteRefundDigit - 5) < 0) && (caseRM1qty < firstNoteRefundDigit))
							isSmallNoteRefundPossible = false;
					}
				}
				
				if (isSmallNoteRefundPossible)
				{
					/* Make sure RM 10 note has enought qty for refund when Max RM 50s (max50InsertionQty) has inserted. */
					if (caseRM10qty >= (max50InsertionQty * 5))
						flgRM50 = true;

					/* Make sure RM 10 note has enought qty for refund when Max RM 20s (max20InsertionQty) has inserted.  */
					if (caseRM10qty >= (max20InsertionQty * 2))
						flgRM20 = true;
				}
			}

			byte code1 = 0;
			if (flgRM50 && flgRM20)
				code1 = 61;

			else if (flgRM50)
				code1 = 45;

			else if (flgRM20)
				code1 = 29;

			else
				code1 = 13;

			acceptableBanknote = ListAcceptableBanknote(code1, acceptableBanknote);
			return new byte[] { 0, 0, code1, 0, 0, 0 };
		}

		private void FillCasstleBalc(CassetteCollectoion cassettes)
		{
			_log.LogText(LogChannel, CurrentProcessId, "Start - FillCasstleBalc", "A01", "B2BRMCashData.FillCasstleBalc", AppDecorator.Log.MessageType.Info);

			//CloseCashPayTimer();
			//objB2B.ToDisableMode();

			ArrCashIn[0] = 0;
			ArrCashIn[1] = 0;
			ArrCashIn[2] = 0;

			foreach (CCassette cassette in cassettes)
			{
				switch (cassette.BillType)
				{
					// Bill Type for RM 1
					case 0:
						ArrCashIn[0] = cassette.BillQuantity;
						break;

					// Bill Type for RM 5
					case 2:
						ArrCashIn[1] = cassette.BillQuantity;
						break;

					// Bill Type for RM 10
					case 3:
						ArrCashIn[2] = cassette.BillQuantity;
						break;
				}

			}

			_log.LogText(LogChannel, CurrentProcessId, "End - FillCasstleBalc", "A03", "B2BRMCashData.FillCasstleBalc", AppDecorator.Log.MessageType.Info);
		}

		public DispenseParameter[] GetRefundParameters(CassetteCollectoion cassettes, AppDecorator.Devices.Payment.RefundType refundType, out Exception ex)
		{
			ex = null;

			if (refundType == AppDecorator.Devices.Payment.RefundType.CompletePayment)
			{
				FillCasstleBalc(cassettes);
				return GetCompletedPaymentRefundParameters(out ex);
			}
			else if (refundType == AppDecorator.Devices.Payment.RefundType.CancelPayment )
			{
				return GetCancelPaymentRefundParameters();
			}
			else
			{
				ex = new Exception($@"Unable to handle refund type (Type : {(int)refundType})");
			}

			return new DispenseParameter[0];
		}

		private DispenseParameter[] GetCompletedPaymentRefundParameters(out Exception ex)
		{
			ex = null;
			List<DispenseParameter> dispParamList = new List<DispenseParameter>();

			_log.LogText(LogChannel, CurrentProcessId, $@"Start - GetPaymentDoneRefundParameters; Total refund amount : RM {_refundAmount}", "A01", "B2BRMCashData.GetPaymentDoneRefundParameters"
				, AppDecorator.Log.MessageType.Info);

			if (_refundAmount <= 0)
				return new DispenseParameter[0];

			int noteRefundAmount = Convert.ToInt32(Math.Floor(_refundAmount));
			int intReturnQty = 0;
			DispenseParameter? param = null;

			//----- ----- ----- ----- ----- 
			// Refund RM 10 note
			int intTemp = 0;
			if (noteRefundAmount >= 10 && ArrCashIn[2] > 0)
			{
				param = null;
				intTemp = Convert.ToInt32(Math.Floor(noteRefundAmount / 10M));
				if (ArrCashIn[2] < intTemp)
				{
					intTemp = ArrCashIn[2];
				}
				param = new DispenseParameter() { BillCount = intTemp, BillType = 3 };
				dispParamList.Add(param.Value);

				intReturnQty += intTemp;
				noteRefundAmount -= intTemp * 10;
			}
			//----- ----- ----- ----- ----- 
			// Refund RM 5 note
			intTemp = 0;
			if (noteRefundAmount >= 5 && ArrCashIn[1] > 0)
			{
				param = null;
				intTemp = Convert.ToInt32(Math.Floor(noteRefundAmount / 5M));
				if (ArrCashIn[1] < intTemp)
				{
					intTemp = ArrCashIn[1];
				}
				param = new DispenseParameter() { BillCount = intTemp, BillType = 2 };
				dispParamList.Add(param.Value);

				intReturnQty += intTemp;
				noteRefundAmount -= intTemp * 5;
			}
			//----- ----- ----- ----- ----- 
			// Refund RM 1 note
			if ((noteRefundAmount > 0) && (noteRefundAmount <= ArrCashIn[0]))
			{
				param = null;
				intReturnQty += Convert.ToInt32(noteRefundAmount);
				param = new DispenseParameter() { BillCount = noteRefundAmount, BillType = 0 };
				dispParamList.Add(param.Value);
			}
			else if (noteRefundAmount > 0)
			{
				ex = new UnableRefundException("Cash machine does not have enough banknote to refund after payment success.");
				_log.LogText(LogChannel, CurrentProcessId, $@"Cash machine does not have enough banknote to refund; RM 1 Refund Request : {noteRefundAmount}; Current RM 1 Available Qty : {ArrCashIn[0]};", "A05", "B2BRMCashData.GetPaymentDoneRefundParameters", AppDecorator.Log.MessageType.Info);
				return new DispenseParameter[0];
			}

			//----- ----- ----- ----- ----- 
			////// Total note quantity cannot exceeded ..
			////if (intReturnQty > 24)
			////{
			////	_log.LogText(LogChannel, CurrentProcessId, $@"End - CheckNoteRefund; Refund Note quantity ({intReturnQty}) excceeded (Max is 24);", "A10", "B2BRMCashData.GetPaymentDoneRefundParameters", AppDecorator.Log.MessageType.Info);
			////	ex = new Exception("Machine has exceeded maximum refund note quantity.");
			////	return new DispenseParameter[0];
			////}
			//////----- ----- ----- ----- ----- 

			DispenseParameter[] retArr = (from row in dispParamList select row).OrderBy(r => r.BillType).ToArray();

			if (retArr.Length == 0)
				_log.LogText(LogChannel, CurrentProcessId, $@"End - GetPaymentDoneRefundParameters; No note need to be refunded.", "A15", "B2BRMCashData.GetPaymentDoneRefundParameters", AppDecorator.Log.MessageType.Info);
			else
				_log.LogText(LogChannel, CurrentProcessId, retArr, "A16", "B2BRMCashData.GetPaymentDoneRefundParameters", AppDecorator.Log.MessageType.Info
					, extraMsg: $@"End - GetPaymentDoneRefundParameters; MsgObj : Note Refunding Dispense Parameter array;");

			return retArr;
		}

		public int[] GetCassettesInfo(CassetteCollectoion cassettes)
		{
			FillCasstleBalc(cassettes);

			return ArrCashIn;
		}

		private DispenseParameter[] GetCancelPaymentRefundParameters()
		{
			List<DispenseParameter> dispParamList = new List<DispenseParameter>();

			DispenseParameter? param = null;

			for (int i = 0; i < ArrCashRec.Length; i++)
			{
				param = null;

				if (ArrCashRec[i] > 0)
				{
					switch (i)
					{
						case 0:
							param = new DispenseParameter() { BillCount = ArrCashRec[i], BillType = 0 };
							dispParamList.Add(param.Value);
							_log.LogText(LogChannel, CurrentProcessId, param, "A03", "B2BRMCashData.GetCancelPaymentRefundParameters", AppDecorator.Log.MessageType.Info, 
								extraMsg:"RM 1 [ Qty : " + ArrCashRec[i].ToString() + " ]; MsgObj: A refund parameter;");
							break;
						case 1:
							param = new DispenseParameter() { BillCount = ArrCashRec[i], BillType = 2 };
							dispParamList.Add(param.Value);
							_log.LogText(LogChannel, CurrentProcessId, param, "A04", "B2BRMCashData.GetCancelPaymentRefundParameters", AppDecorator.Log.MessageType.Info,
								extraMsg: "RM 5 [ Qty : " + ArrCashRec[i].ToString() + " ];  MsgObj: A refund parameter;");
							break;
						case 2:
							param = new DispenseParameter() { BillCount = ArrCashRec[i], BillType = 3 };
							dispParamList.Add(param.Value);
							_log.LogText(LogChannel, CurrentProcessId, param, "A05", "B2BRMCashData.GetCancelPaymentRefundParameters", AppDecorator.Log.MessageType.Info,
								extraMsg: "RM 10 [ Qty : " + ArrCashRec[i].ToString() + " ];  MsgObj: A refund parameter;");
							break;
					}
				}
			}

			DispenseParameter[] retArr = (from row in dispParamList select row).OrderBy(r => r.BillType).ToArray();

			if (retArr.Length == 0)
				_log.LogText(LogChannel, CurrentProcessId, $@"End - GetPaymentDoneRefundParameters; No note need to be refunded.", "A10", "B2BRMCashData.GetCancelPaymentRefundParameters");
			else
				_log.LogText(LogChannel, CurrentProcessId, retArr, "A16", "B2BRMCashData.GetCancelPaymentRefundParameters",  
					extraMsg: $@"End - GetPaymentDoneRefundParameters; MsgObj : Note Refunding Dispense Parameter array;");

			return retArr;
		}
		
		private void ResetArr()
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

		
	}
}