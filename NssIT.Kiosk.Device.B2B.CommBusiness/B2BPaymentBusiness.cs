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
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command.CommandSpec;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Events;
using NssIT.Kiosk.Device.Telequip.CX2_3.AccessSDK;
using NssIT.Kiosk.AppDecorator.Common.AppService.Payment.UI;

namespace NssIT.Kiosk.Device.B2B.CommBusiness
{
	public class B2BPaymentBusiness : IDisposable
	{
		private const string LogChannel = "B2BApplication(B)";
		private const double _minimumAmount = 0.1;

		public const int SaleMaxWaitingSec = 390; /* 6.30 minutes */

		private bool _disposed = false;
		private bool _isPreviousPaymentDone = true;
		private Guid? _currentNetProcessId = null;
		private decimal _currentAmount = 0.00M;
		private decimal _currCoinRefundAmount = 0.00M;
		private string _currentdocNumbers = null;

		private B2BAccess _b2bAccess = null;

		public event EventHandler<TrxCallBackEventArgs> OnCompleted;
		public event EventHandler<InProgressEventArgs> OnInProgress;

		private CX2D3Access _cx2d3 = null;

		public B2BPaymentBusiness()
		{
			_cx2d3 = new CX2D3Access();

			_b2bAccess = B2BAccess.GetB2BAccess();

			_b2bAccess.OnInProgress += _b2bAccess_OnInProgress;
			_b2bAccess.OnCompleted += _b2bAccess_OnCompleted;
		}

		private DbLog _log = null;
		private DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
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

		public bool ReadIsPaymentDeviceReady(out bool isLowCoin, out bool isRecoveryInProgressAfterDispenseCoin, out string errorMessage)
		{
			isLowCoin = false;
			isRecoveryInProgressAfterDispenseCoin = false;
			errorMessage = null;

			if (_disposed)
				throw new Exception("Application already shutdown. (EXIT8367)");

			// Check Machines Ready Status
			bool isBanknoteMachineReady = _b2bAccess.IsCashMachineReady;

			if (isBanknoteMachineReady)
			{
				bool isCoinMachReady = _cx2d3.CheckMachineIsReady(out bool isMachineQuitProcess, out bool isAppShuttingDown, out isLowCoin, 
					out bool isAccessSDKBusy, out isRecoveryInProgressAfterDispenseCoin, 
					out errorMessage);

				if (!isCoinMachReady)
				{
					if (isMachineQuitProcess)
						throw new Exception("Coin Machine already quit process.");
					else if (isAppShuttingDown)
						throw new Exception("Coin Machine already shutdown.");
					else
						return false;
				}
			}

			return isBanknoteMachineReady;
		}

		private void _b2bAccess_OnCompleted(object sender, TrxCallBackEventArgs e)
		{
			if (e.ModuleAppGroup != B2BDecorator.Command.B2BModuleAppGroup.Payment)
				return;

			if (_disposed)
			{
				Log.LogText(LogChannel, _currProcessId, "B2BPaymentBusiness has shutdowned.", $@"A01", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnCompleted");
				return;
			}

			Log.LogText(LogChannel, _currProcessId, e, $@"A02", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnCompleted", 
				extraMsg: "Start - _b2bAccess_OnCompleted; MsgObj: TrxCallBackEventArgs");

			if (e.IsSuccess)
			{
				try
				{
					if (_currCoinRefundAmount != 0.00M)
					{
						if (e.KioskMessage is UICashMachineStatus cashStt)
						{
							cashStt.RefundCoinAmount = Convert.ToInt32(Math.Floor(_currCoinRefundAmount * 100M)) ;
						}

						Log.LogText(LogChannel, _currProcessId, $@"Start to send coin dispense command for {_currCoinRefundAmount}.", $@"A03", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnCompleted");
						_cx2d3.Dispense(CurrentProcessId, _currCoinRefundAmount);
						Log.LogText(LogChannel, _currProcessId, $@"End sending of coin dispense command.", $@"A04", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnCompleted");
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, _currProcessId, new Exception($@"Error when dispense coin for amount of {_currCoinRefundAmount};", ex), "E01", 
						classNMethodName: "B2BPaymentBusiness._b2bAccess_OnCompleted");
				}
				finally
				{
					_currCoinRefundAmount = 0.00M;
				}
			}
			
			try
			{
				_isPreviousPaymentDone = true;
				OnCompleted?.Invoke(null, e);
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, _currProcessId, new Exception($@"Unhandle error exception at _b2bAccess_OnCompleted event;", ex), "E02", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnCompleted");
			}

			Log.LogText(LogChannel, _currProcessId, "End - _b2bAccess_OnCompleted", "A10", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnCompleted");
		}

		private void _b2bAccess_OnInProgress(object sender, InProgressEventArgs e)
		{
			if (e.ModuleAppGroup != B2BDecorator.Command.B2BModuleAppGroup.Payment)
				return;

			if (_disposed)
			{
				Log.LogText(LogChannel, _currProcessId, "B2BPaymentBusiness has shutdowned.", $@"A01", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnInProgress");
				return;
			}

			Log.LogText(LogChannel, _currProcessId, e, $@"A2", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnInProgress",
				extraMsg: "Start - _b2bAccess_OnInProgress; MsgObj: TrxCallBackEventArgs");

			try
			{
				OnInProgress?.Invoke(null, e);
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, _currProcessId, new Exception($@"Unhandle error exception at _b2bAccess_OnInProgress event;", ex), "E01", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnInProgress");
			}

			Log.LogText(LogChannel, _currProcessId, "End - _b2bAccess_OnInProgress", "A10", classNMethodName: "B2BPaymentBusiness._b2bAccess_OnInProgress");
		}

		public bool Pay(string processId, Guid? netProcessId, decimal amount, out string errorMsg, string docNumbers = null)
		{
			errorMsg = null;

			if (_disposed)
				throw new Exception("System is shutting down (EXIT8365);");

			if (!_isPreviousPaymentDone)
				throw new Exception("Existing payment still in progress (EXIT8366); New payment is not allowed.");

			// * ..(outstanding) check Banknote and coin machine for refund capability.

			decimal coinRefundAmount = 0.00M;

			amount = (amount < 0) ? 0.00M : amount;
			docNumbers = (string.IsNullOrWhiteSpace(docNumbers)) ? null : docNumbers.Trim();

			// Find Possible of Coin Refund
			//----------------------------------
			{
				decimal testVal = 1.00M;
				string amountPaymentStr = $@"{amount:###0.00}";
				string coinPaymentStr = amountPaymentStr.Substring(amountPaymentStr.Length - 2, 2);
				string lastCoinPaymentChar = amountPaymentStr.Substring(amountPaymentStr.Length - 1, 1);

				if ((int.Parse(lastCoinPaymentChar) % 10) != 0)
				{
					throw new Exception($@"Payment amount should not have single digit value in cent (First Cent Digit: {lastCoinPaymentChar}). Machine not able to refund with related coin type.");
				}

				decimal coinPayment = decimal.Parse($@"0.{coinPaymentStr}");

				if (coinPayment != 0)
				{
					coinRefundAmount = testVal - coinPayment;
				}
			}

			// Check Coin Machine
			//------------------------
			if (coinRefundAmount != 0)
			{
				//bool chkCashMach = IsPaymentDeviceReady;

				// Check Coin Refund Possibility
				if (_cx2d3.GetDispensePossibility(processId, coinRefundAmount, out string lowCoinMsg, out string machOutOfSvc) == false)
				{
					throw new Exception($@"Low Coin : {lowCoinMsg}; Mach Out Of Service : {machOutOfSvc}");
				}
				else
				{
					Log.LogText(LogChannel, processId, $@"Should refund coin on transaction success : RM {coinRefundAmount:#.#0}", "A01", "B2BPaymentBusiness.Pay",
						adminMsg: $@"Should refund coin on transaction success : RM {coinRefundAmount:#.#0}");
				}
			}
			//----------------------------------------------------------------------------------------------------

			B2BPaymentCommand newData = new B2BPaymentCommand(processId, netProcessId, amount, coinRefundAmount, docNumbers, SaleMaxWaitingSec);

			bool isAddPaymentSuccess = _b2bAccess.AddCommand(new B2BCommandPack(newData), out errorMsg);
			
			if (isAddPaymentSuccess)
			{
				CurrentProcessId = processId;
				_currentNetProcessId = netProcessId;
				_currentAmount = amount;
				_currentdocNumbers = docNumbers;
				_currCoinRefundAmount = coinRefundAmount;
				_isPreviousPaymentDone = false;
			}
			
			return isAddPaymentSuccess;
		}

		public void CancelRequest(string ProcessId, Guid? netProcessId)
		{
			_b2bAccess.RequestInterrupt(new B2BPaymentCancelCommand(ProcessId, netProcessId));
			Log.LogText(LogChannel, CurrentProcessId, "Cancel Transaction Request", "A01", "B2BAccess.CancelRequest");
		}

		public void Dispose()
		{
			_disposed = true;

			if (_cx2d3 != null)
			{
				try
				{
					_cx2d3.Dispose();
				}
				catch (Exception ex)
				{
					string tt1 = ex.Message;
				}

				_cx2d3 = null;
			}

			if (_b2bAccess != null)
			{
				try
				{
					_b2bAccess.Dispose();
				}
				catch { }
			}

			try
			{
				if (OnCompleted != null)
				{
					Delegate[] delgList = OnCompleted.GetInvocationList();
					foreach (EventHandler<TrxCallBackEventArgs> delg in delgList)
					{
						OnCompleted -= delg;
					}
				}
				if (OnInProgress != null)
				{
					Delegate[] delgList = OnInProgress.GetInvocationList();
					foreach (EventHandler<InProgressEventArgs> delg in delgList)
					{
						OnInProgress -= delg;
					}
				}
			}
			catch { }

			_b2bAccess = null;
			OnCompleted = null;
			OnInProgress = null;

			_log = null;
		}

	}
}
