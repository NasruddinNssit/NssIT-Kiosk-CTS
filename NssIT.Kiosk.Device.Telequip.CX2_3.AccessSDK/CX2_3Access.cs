using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.Device.Telequip.CX2_3.AccessSDK.Result;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.AccessSDK
{
	public class CX2_3Access : IDisposable 
	{
		private const string LogChannel = "CX2_3Access";
		private const double _minimumAmount = 0.1;
		private const int _maxRefundNoteCount = 5;

		public const int SaleMaxWaitingSec = 210; /* 3.30 minutes */

		private bool _isFailMachineConnection = false;
		private bool _workInProgress = false;
		private bool? _isCX2D3Valid = null; /* null: not ready; false: Shutdown; true: ready */
		
		private Thread _threadWorker = null;
		private ConcurrentQueue<CX2_3.OrgApi.PayCX2_3CommandPack> _payB2BCommandPackList = new ConcurrentQueue<CX2_3.OrgApi.PayCX2_3CommandPack>();

		public CX2_3Access()
		{
			_threadWorker = new Thread(new ThreadStart(AccessProcessThreadWorking));
			_threadWorker.SetApartmentState(ApartmentState.STA);
			_threadWorker.IsBackground = true;
			_threadWorker.Start();
		}

		private bool _disposed = false;
		public void Dispose()
		{
			_disposed = true;
		}

		private DbLog _log = null;
		private DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		public bool CheckMachineIsReady(out bool isMachineQuitProcess, out bool isAppShuttingDown)
		{
			isMachineQuitProcess = false;
			isAppShuttingDown = false;

			if (_disposed)
			{
				isAppShuttingDown = true;
				return false;
			}
			else if (_isCX2D3Valid.HasValue == false)
			{
				return false;
			}
			else if (_isCX2D3Valid == false)
			{
				isMachineQuitProcess = true;
				return false;
			}
			else
				return true;
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

		public void Dispense(string ProcessId, decimal dispenseAmount, string docNumbers = null)
		{
			CurrentProcessId = ProcessId;
			dispenseAmount = (dispenseAmount < 0) ? 0.00M : dispenseAmount;
			docNumbers = (string.IsNullOrWhiteSpace(docNumbers)) ? null : docNumbers.Trim();

			CX2_3.OrgApi.PayCX2_3CommandPack newData = new CX2_3.OrgApi.PayCX2_3CommandPack()
			{
				ProcessId = ProcessId,
				Command = OrgApi.OperationCommand.Dispense,
				DispenseAmount = dispenseAmount,
				DocNumbers = docNumbers,
				MaxWaitingSec = SaleMaxWaitingSec
			};

			if (AddExecutionCommand(newData) == false)
			{
				throw new Exception("System busy.");
			}
		}

		private DispensePossibility _latestDispenseState = new DispensePossibility();
		public bool GetDispensePossibility(string ProcessId, decimal dispenseAmount, out string lowCoinMsg, out string machOutOfSvcMsg)
		{
			lowCoinMsg = null;
			machOutOfSvcMsg = null;

			_latestDispenseState.Reset();

			CurrentProcessId = ProcessId;
			dispenseAmount = (dispenseAmount < 0) ? 0.00M : dispenseAmount;

			CX2_3.OrgApi.PayCX2_3CommandPack newData = new CX2_3.OrgApi.PayCX2_3CommandPack()
			{
				ProcessId = ProcessId,
				Command = OrgApi.OperationCommand.GetDispensePossibility,
				DispenseAmount = dispenseAmount,
				MaxWaitingSec = SaleMaxWaitingSec
			};

			if (AddExecutionCommand(newData) == false)
			{
				throw new Exception("System busy.");
			}

			while ((_latestDispenseState.GotStatus == false) || (_workInProgress))
			{
				Task.Delay(100).Wait();
			}

			lowCoinMsg = _latestDispenseState.LowCoinMsg;
			machOutOfSvcMsg = _latestDispenseState.MachineOutOfSvcMsg;

			return _latestDispenseState.IsDispensePossible;
		}

		private bool AddExecutionCommand(CX2_3.OrgApi.PayCX2_3CommandPack commPack)
		{
			if (commPack == null)
				throw new Exception("Data parameter cannot be NULL at AddPaymentData");

			bool isMachReady = CheckMachineIsReady(out bool isMachineQuitProcess, out bool isAppShuttingDown);

			if (isAppShuttingDown)
				throw new Exception("System is shutting down; Sect-02;");
			else if (isMachineQuitProcess)
				throw new Exception("System is shutting down; Sect-03;");
			else if (isMachReady == false)
				throw new Exception("System busy. Cash machine is not ready.");
			
			lock (_payB2BCommandPackList)
			{
				if (_disposed)
					return false;

				if ((_payB2BCommandPackList.Count == 0) && (_isCX2D3Valid == true) && (_workInProgress == false))
				{
					_lastDispenseStatus = PaymentResultStatus.InitNewState;
					//_lastProcessException = null;
					_workInProgress = true;
					_payB2BCommandPackList.Enqueue(commPack);
					Monitor.PulseAll(_payB2BCommandPackList);
					return true;
				}
				else
					return false;
			}
		}

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 10);
		private CX2_3.OrgApi.PayCX2_3CommandPack GetNextTransactionCommand()
		{
			CX2_3.OrgApi.PayCX2_3CommandPack retData = null;

			lock (_payB2BCommandPackList)
			{
				if (_isCX2D3Valid == true)
				{
					if (_payB2BCommandPackList.Count == 0)
					{
						Monitor.Wait(_payB2BCommandPackList, _MaxWaitPeriod);
					}
					if (_payB2BCommandPackList.TryDequeue(out retData))
					{
						return retData;
					}
				}
				else
					Monitor.Wait(_payB2BCommandPackList, _MaxWaitPeriod);
			}
			return null;
		}

		public void CancelRequest()
		{
			// Not nessaccery because once machine triggered to dispense coin, it cannot be stop.
		}

		#region ----- AccessProcessThreadWorking -----
		private PaymentResultStatus _lastDispenseStatus = PaymentResultStatus.InitNewState;
		//private Exception _lastProcessException = null;
		private DateTime _endTime = DateTime.Now;
		private void AccessProcessThreadWorking()
		{
			CX2_3.OrgApi.PayCX2_3CommandPack commandPack = null;
			
			_isCX2D3Valid = null;
			_isFailMachineConnection = false;

			using (CX2_3.OrgApi.IAxCX2_3 axCx2D3 = new CX2_3.OrgApi.AxCX2_3())
			{
				string currDispenseAmount = "";

				try
				{
					if (axCx2D3.ConnectDevice())
					{
						_isCX2D3Valid = true;
					}
					else
						_isFailMachineConnection = true;

					while ((_isCX2D3Valid == true) && (!_disposed))
					{
						// * Check Machine Status, send Machine MalFunction Exception if Cash low OR Do not have coin.
						// * Always send Message to user about Acceptable of changing/refund Bill status.
					
						try
						{
							commandPack = null;
							commandPack = GetNextTransactionCommand();

							if (commandPack != null)
							{
								if ((commandPack.HasCommand) && (commandPack.ProcessDone == false))
								{
									//_ecr.Stop = false;

									currDispenseAmount = string.Format("{0:#,##0.000000}", commandPack.DispenseAmount);
									CurrentProcessId = commandPack.ProcessId ?? "-----";
									axCx2D3.CurrentProcessId = CurrentProcessId;
									
									// $@"Start Process; Thread Hash Code : {Thread.CurrentThread.GetHashCode()};"
									Log.LogText(LogChannel, CurrentProcessId, commandPack, "B01", "CX2_3Access.AccessProcessThreadWorking",
										extraMsg: $@"Start Process; Managed Thread Id : {Thread.CurrentThread.ManagedThreadId}; Thread Hash Code : {Thread.CurrentThread.GetHashCode()}; MsgObj: PayCX2_3CommandPack; ");

									// ----- Dispense Coin
									if (commandPack.Command == OrgApi.OperationCommand.Dispense)
									{
										if ((_latestDispenseState.GotStatus == false) || (CurrentProcessId.Equals(_latestDispenseState.ProcessId) == false))
										{
											UpdateLatestDispenseState(CurrentProcessId, axCx2D3, commandPack.DispenseAmount);
										}

										if (_latestDispenseState.IsDispensePossible)
										{
											string notEnoughCoinMsg = null;
											int totalDispenseCents = Convert.ToInt32(Math.Floor(commandPack.DispenseAmount * 100M));
											if (axCx2D3.DispenseCoin(totalDispenseCents, 
												_latestDispenseState.IsRunningLowCoin10, _latestDispenseState.IsRunningLowCoin20, _latestDispenseState.IsRunningLowCoin50, out notEnoughCoinMsg))
											{
												Log.LogText(LogChannel, CurrentProcessId, $@"Dispense command have been sent; Total Dispense Cents : {totalDispenseCents}",
												"B02", "CX2_3Access.AccessProcessThreadWorking");

												_lastDispenseStatus = PaymentResultStatus.Success;
											}
											else
											{
												Log.LogText(LogChannel, CurrentProcessId, $@"Fail to send Dispense command; Total Dispense Cents : {totalDispenseCents}",
												"B03", "CX2_3Access.AccessProcessThreadWorking");

												// ** to check what kind of error here ..
												_lastDispenseStatus = PaymentResultStatus.MachineMalfunction;
											}
										}
										else
										{
											string lowCoinStatusMsg = "Low Coin Error : " + _latestDispenseState.LowCoinMsg;
											string machMalfunctionMsg = (_latestDispenseState.MachineOutOfSvcMsg != null) ? $@"; Coin Machine Out of Service : {_latestDispenseState.MachineOutOfSvcMsg}" : "";
											
											Log.LogText(LogChannel, CurrentProcessId, $@"{lowCoinStatusMsg}{machMalfunctionMsg}", 
												"B04", "CX2_3Access.AccessProcessThreadWorking");
										}
									}

									// ----- Update Coin Machine State 
									else if (commandPack.Command == OrgApi.OperationCommand.GetDispensePossibility)
											UpdateLatestDispenseState(CurrentProcessId, axCx2D3, commandPack.DispenseAmount);

									// ----- .. unrecognised operation command
									else
										Log.LogText(LogChannel, CurrentProcessId, $@"Unrecognised Operation Command { Enum.GetName(typeof(OrgApi.OperationCommand), commandPack.Command) }", 
											"B05", "CX2_3Access.AccessProcessThreadWorking");

									commandPack.ProcessDone = true;
								}
							}
						}
						catch (Error.CX2_3MalfunctionException exM)
						{
							Log.LogError(LogChannel, _currProcessId, exM, "Ex01", classNMethodName: "CX2_3Access.AccessProcessThreadWorking");

							if (_lastDispenseStatus == PaymentResultStatus.InitNewState)
							{
								_lastDispenseStatus = PaymentResultStatus.MachineMalfunction;
							}

							throw exM;
						}
						catch (Exception ex2)
						{
							Log.LogError(LogChannel, _currProcessId, ex2, "Ex02", classNMethodName: "CX2_3Access.AccessProcessThreadWorking");

							if (_lastDispenseStatus == PaymentResultStatus.InitNewState)
							{
								_lastDispenseStatus = PaymentResultStatus.Error;
							}
						}
						finally
						{
							_workInProgress = false;
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogFatal(LogChannel, _currProcessId, ex, "Ex05", classNMethodName: "CX2_3Access.AccessProcessThreadWorking");

					if (_lastDispenseStatus == PaymentResultStatus.InitNewState)
					{
						_lastDispenseStatus = PaymentResultStatus.MachineMalfunction;
					}
				}
				finally
				{
					Log.LogText(LogChannel, _currProcessId, "Quit B2B Process", "END_X01", "CX2_3Access.AccessProcessThreadWorking");

					axCx2D3.CloseDevice();
					_workInProgress = false;
					_isCX2D3Valid = false;
				}
			}
		}

		/// <summary>
		/// To get the result, the Dispense(..) function must be executed first.
		/// </summary>
		/// <param name="processId"></param>
		/// <returns></returns>
		public bool GetLastDispenseResult(string processId)
		{
			while (_lastDispenseStatus == PaymentResultStatus.InitNewState)
			{
				Task.Delay(300).Wait();
			}

			return (_lastDispenseStatus == PaymentResultStatus.Success);
		}

		private void UpdateLatestDispenseState(string ProcessId, CX2_3.OrgApi.IAxCX2_3 axCx2D3, decimal totalDispenseAmount)
		{
			_latestDispenseState.Reset();

			string lowCoinMsg = null, machineOutOfSvcMsg = null;
			bool isRunningLowCoin10 = false, isRunningLowCoin20 = false, isRunningLowCoin50 = false;
			int totalDispenseCents = Convert.ToInt32(Math.Floor(totalDispenseAmount * 100M));

			axCx2D3.CheckDispensePossibility(totalDispenseCents,
				out lowCoinMsg, out machineOutOfSvcMsg,
				out isRunningLowCoin10, out isRunningLowCoin20, out isRunningLowCoin50);

			_latestDispenseState.ProcessId = ProcessId;
			_latestDispenseState.LowCoinMsg = lowCoinMsg;
			_latestDispenseState.MachineOutOfSvcMsg = machineOutOfSvcMsg;
			_latestDispenseState.IsRunningLowCoin10 = isRunningLowCoin10;
			_latestDispenseState.IsRunningLowCoin20 = isRunningLowCoin20;
			_latestDispenseState.IsRunningLowCoin50 = isRunningLowCoin50;

			_latestDispenseState.GotStatus = true;

			Log.LogText(LogChannel, CurrentProcessId, _latestDispenseState,	"A100", "CX2_3Access.UpdateLatestDispenseState", 
				extraMsg: "MsgObj : DispensePossibility");
		}

		#endregion
	}
}
