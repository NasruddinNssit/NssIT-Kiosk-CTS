using CCNET;
using CCNET.BillToBill;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using NssIT.Kiosk;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.AppDecorator.Devices;
using NssIT.Kiosk.AppDecorator.Devices.Payment;
using NssIT.Kiosk.Device.B2B;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Error;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Command;
using NssIT.Kiosk.Device.B2B.OrgApi;
using NssIT.Kiosk.Device.B2B.AccessSDK;
using NssIT.Kiosk.Device.B2B.AccessSDK.CommandExec;

namespace NssIT.Kiosk.Device.B2B.CommBusiness
{
	public class B2BAccess : IDisposable 
	{
		private const string LogChannel = "B2BAccess";

		private bool _disposed = false;
		private bool? _isB2BValid = null; /* null: not ready; false: Shutdown; true: ready */
		private bool? _isMachineDetected = null; /* null: not start detecting; false: not detected; true: detected */

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 10);
		private B2BRMCashData _currB2BCashData = null;

		private Thread _b2bCommandExecutionThreadWorker = null;
		private Thread _inProgressEventThreadWorker = null;
		
		private ConcurrentQueue<B2BDecorator.Events.InProgressEventArgs> _InProgressEventObjList = new ConcurrentQueue<B2BDecorator.Events.InProgressEventArgs>();

		private B2BPaymentCommandExecution _paymentCommExecution = new B2BPaymentCommandExecution();
		private B2BCommandRepository _commandRepository = new B2BCommandRepository();

		// Below Events Should be implemented by CommonBusiness OR Application layer 
		public event EventHandler<B2BDecorator.Events.TrxCallBackEventArgs> OnCompleted;
		public event EventHandler<B2BDecorator.Events.InProgressEventArgs> OnInProgress;

		// Below Events Should be implemented by a B2B Command Class (like .. B2BCommandPaymentExecution) 
		public event EventHandler<B2BDecorator.Events.InterruptCommandEventArgs> OnCommandInterrupted;

		private DbLog _log = null;
		private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

		public bool IsCashMachineReady
		{
			get
			{
				if (_disposed)
					throw new Exception("Payment Device is shutting down (EXIT8271);");

				else if (_isMachineDetected == false)
					throw new Exception("Banknote machine is not found (EXIT8273);");

				else if (_isB2BValid == false)
					throw new Exception("Payment Device is shutting down (EXIT8272);");

				return (_isB2BValid == true) ? true : false;
			}
		}

		private Guid? CurrentNetProcessId { get; set; } = null;
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
				
		private B2BAccess()
		{
			_b2bCommandExecutionThreadWorker = new Thread(new ThreadStart(B2BCommandExecutionThreadWorking));
			_b2bCommandExecutionThreadWorker.IsBackground = true;
			_b2bCommandExecutionThreadWorker.Start();

			_inProgressEventThreadWorker = new Thread(new ThreadStart(DispatchInProgressEventThreadWorking));
			_inProgressEventThreadWorker.IsBackground = true;
			_inProgressEventThreadWorker.Start();
		}

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static B2BAccess _b2bAccess = null;
		public static B2BAccess GetB2BAccess()
		{
			if (_b2bAccess != null)
				return _b2bAccess;

			try
			{
				_manLock.WaitAsync().Wait();

				if (_b2bAccess == null)
				{
					_b2bAccess = new B2BAccess();
				}
			}
			finally
			{
				_manLock.Release();
			}

			return _b2bAccess;
		}

		public void Dispose()
		{
			lock (_InProgressEventObjList)
			{
				_disposed = true;
				_isB2BValid = false;
				_isMachineDetected = null;

				try
				{
					if (OnCompleted != null)
					{
						Delegate[] delgList = OnCompleted.GetInvocationList();
						foreach (EventHandler<B2B.B2BDecorator.Events.TrxCallBackEventArgs> delg in delgList)
						{
							OnCompleted -= delg;
						}
					}
					if (OnInProgress != null)
					{
						Delegate[] delgList = OnInProgress.GetInvocationList();
						foreach (EventHandler<B2BDecorator.Events.InProgressEventArgs> delg in delgList)
						{
							OnInProgress -= delg;
						}
					}
					if (OnCommandInterrupted != null)
					{
						Delegate[] delgList = OnCommandInterrupted.GetInvocationList();
						foreach (EventHandler<B2BDecorator.Events.InterruptCommandEventArgs> delg in delgList)
						{
							OnCommandInterrupted -= delg;
						}
					}
				}
				catch { }

				while (_InProgressEventObjList.TryDequeue(out B2BDecorator.Events.InProgressEventArgs evt))
				{ evt.Dispose(); }

				OnCompleted = null;
				OnInProgress = null;

				_commandRepository.Dispose();
				Task.Delay(100).Wait();
				_paymentCommExecution.Dispose();
				
				Monitor.PulseAll(_InProgressEventObjList);
			}
			_log = null;
		}

		public bool AddCommand(B2BCommandPack commPack, out string errorMsg)
		{
			errorMsg = null;
			if (commPack == null)
			{
				errorMsg = "Invalid command specification (EXIT8274).";
				return false;
			}

			try
			{
				if (_disposed)
					throw new Exception("System is shutting down; Sect-02;");
				else if (_isB2BValid.HasValue == false)
					throw new Exception("System busy. Cash machine is not ready. Sect-03;");
				else if (_isMachineDetected == false)
					throw new Exception("Banknote machine is not found; Sect-04;");
				else if (_isB2BValid == false)
					throw new Exception("System is shutting down; Sect-05;");	
				
				if (commPack.NetProcessId.HasValue == false)
					Log.LogText(LogChannel, commPack?.ProcessId ?? "-", "Net Process Id not found.", "EX02", "B2BAccess.AddCommand", AppDecorator.Log.MessageType.Error);
				
				return _commandRepository.EnQueueNewCommandPack(commPack, out errorMsg);

			}
			catch (Exception ex)
			{
				errorMsg = $@"Unable to add job to cash machine (EXIT2576); {ex.Message}";
				Log.LogError(LogChannel, commPack?.ProcessId ?? "-", new Exception(errorMsg, ex), "EX01", "B2BAccess.AddCommand");
			}
			return false;
		}
	
		public void RequestInterrupt(IB2BCommand interruptCommand)
		{
			try
			{
				// Below event normally send to a B2B Command object.
				OnCommandInterrupted?.Invoke(null, new B2B.B2BDecorator.Events.InterruptCommandEventArgs(interruptCommand));
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, ((interruptCommand == null) ? "-" : (interruptCommand.ProcessId ?? "-")), 
					new Exception("Unhandled exception when calling OnCommandInterrupted event.", ex), "EX01", "B2BAccess.RequestInterrupt");
			}
		}

		///// <summary>
		///// This allow cash machine to go back to stand by mode to accept a new task.
		///// </summary>
		//public void FreeCashMachine()
		//{
		//	_commandRepository.ResetBusyStatus();
		//}
				
		#region ----- B2BProcessThreadWorking -----

		private void B2BCommandExecutionThreadWorking()
		{
			B2BCommandPack commandPack = null;
			_isB2BValid = null;

			using (B2BApi b2bApi = new OrgApi.B2BApi())
			{
				b2bApi.CurrentNetProcessId = null;
				b2bApi.CurrentProcessId = "B2BAcc";
				b2bApi.OnPollAnswerResponse += B2bApi_OnPollAnswerResponse;

				try
				{
					if (b2bApi.SearchAndConnect() == true)
					{
						if (b2bApi.OpenConnection())
						{
							_isB2BValid = true;
							_isMachineDetected = true;
							_currB2BCashData = new B2BRMCashData(Log, CurrentProcessId);
							_currB2BCashData.InitNewPrice(0, "--");
							_currB2BCashData.BillTableArray = b2bApi.BillTableArray;
						}
						else
							_isMachineDetected = false;
					}
					else
						_isMachineDetected = false;

					while ((_isB2BValid == true) && (!_disposed))
					{
						// * Check Machine Status, send Machine MalFunction Exception if Cash low OR Do not have coin.
						// * Always send Message to user about Acceptable of changing/refund Bill status.
						try
						{
							commandPack = null;
							commandPack = _commandRepository.DeQueueCommandPack();
							if (commandPack != null)
								b2bApi.OnPollAnswerResponse -= B2bApi_OnPollAnswerResponse;

							if ((commandPack != null) && (OnCompleted == null))
							{
								throw new B2BMalfunctionException("Unhandle OnCompleteCallback (II) event. Transaction void.");
							}
							else if (commandPack != null)
							{
								if ((commandPack.HasCommand) && (commandPack.ProcessDone == false))
								{
									CurrentProcessId = commandPack.ProcessId;
									CurrentNetProcessId = commandPack.NetProcessId;

									b2bApi.CurrentNetProcessId = commandPack.NetProcessId;
									b2bApi.CurrentProcessId = CurrentProcessId;

									Log.LogText(LogChannel, b2bApi.CurrentProcessId, commandPack, "A01", "B2BAccess.B2BCommandExecutionThreadWorking", 
										netProcessId: b2bApi.CurrentNetProcessId,
										extraMsg: "Start - B2BCommandExecution; MsgObj: B2BCommandPack");

									if (commandPack.CommandCode == B2BCommandCode.StartMakePayment)
									{
										//_paymentCommExecution.PaymentWorkingExecution(this, _currB2BCashData, (B2BPaymentCommand)data.Command, b2bApi);
										commandPack = _paymentCommExecution.PaymentWorkingExecution(this, _currB2BCashData, commandPack, b2bApi);
									}
									else
									{
										IB2BCommandExec jobWorker = GetCommandExec(commandPack.CommandCode, b2bApi);
										commandPack = jobWorker.Execute(commandPack);
									}

									Log.LogText(LogChannel, b2bApi.CurrentProcessId, "End - B2BCommandExecution; MsgObj: B2BCommandPack", "A10", "B2BAccess.B2BCommandExecutionThreadWorking",
										netProcessId: b2bApi.CurrentNetProcessId);
								}
							}
						}
						catch (B2BMalfunctionException exM)
						{
							Log.LogError(LogChannel, _currProcessId, exM, "Ex01", classNMethodName: "B2BAccess.OnPayECRProcess");
						}
						catch (Exception ex)
						{
							Log.LogError(LogChannel, _currProcessId, ex, "Ex02", classNMethodName: "B2BAccess.OnPayECRProcess");
						}
						finally
						{
							if (commandPack != null)
							{
								commandPack.Command.ProcessDone = true;
								_commandRepository.UpdateCommandPack(commandPack);

								b2bApi.OnPollAnswerResponse += B2bApi_OnPollAnswerResponse;
								b2bApi.CurrentNetProcessId = null;
								b2bApi.CurrentProcessId = "B2BAcc";

								CurrentProcessId = "=";
								CurrentNetProcessId = null;

								_commandRepository.ResetBusyState();
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogFatal(LogChannel, _currProcessId, ex, "Ex05", classNMethodName: "B2BAccess.OnPayECRProcess");
				}
				finally
				{
					Log.LogText(LogChannel, _currProcessId, "Quit B2B Process", "END_X01", "B2BAccess.OnPayECRProcess");

					b2bApi.CloseDevice();
					b2bApi.OnPollAnswerResponse -= B2bApi_OnPollAnswerResponse;
					_isB2BValid = false;
					_isMachineDetected = null;
				}
			}

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			// below event handle used only to handle machine status without command.
			void B2bApi_OnPollAnswerResponse(object sender, B2BDecorator.Events.MachineEventArgs e)
			{
				Log.LogText(LogChannel, _currProcessId, e, "A01", "B2BAccess.B2bApi_OnPollAnswerResponse", extraMsg: "MsgObj: MachineEventArgs");
			}

			IB2BCommandExec GetCommandExec(B2BCommandCode commandCode, B2BApi b2bApi)
			{
				IB2BCommandExec theRunner = null;

				if(commandCode == B2BCommandCode.AllCassetteInfoRequest)
					return new B2BGetCassetteInfoExecution(this, _currB2BCashData, b2bApi);
				
				return theRunner;
			}


		}

		public bool RaiseOnCompleted(B2BDecorator.Events.TrxCallBackEventArgs completedEvent, string lineTag)
		{
			bool retVal = false;

			Log.LogText(LogChannel, _currProcessId, completedEvent, $@"A01; Line: {lineTag}", classNMethodName: "B2BAccess.OnCompletedSendEvent",
				extraMsg: "Start - OnCompletedSendEvent; MsgObj: TrxCallBackEventArgs", netProcessId: completedEvent.NetProcessId);

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

				OnCompleted?.Invoke(null, completedEvent);
				//System.Windows.Forms.Application.DoEvents();
				retVal = true;
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, _currProcessId, new Exception($@"Unhandle error exception at OnCompleteCallback event;", ex), "E01", classNMethodName: "B2BAccess.OnCompletedSendEvent");
			}

			Log.LogText(LogChannel, _currProcessId, "End - OnCompletedSendEvent", "A10", classNMethodName: "B2BAccess.OnCompletedSendEvent");

			return retVal;
		}

		public void SetB2BToNormalStage(Guid? netProcessId, string processId,
			OrgApi.B2BApi b2b, B2BModuleAppGroup appGroup, int maxWaitSec = 60, string codeLocTag = "-", ICustomerPaymentInfo currCustInfo = null)
		{
			if (b2b != null)
			{
				Exception exp02 = null;
				b2b.SetB2BToNormal(codeLocTag, out exp02);

				if (exp02 != null)
				{
					B2BMalfunctionException exMmf = new B2BMalfunctionException($@"Machine Malfunction at {codeLocTag}.", exp02) { LastCustPaymentInfo = currCustInfo };

					OnInProgressSentEvent(new B2BDecorator.Events.InProgressEventArgs(netProcessId, processId)
					{
						ModuleAppGroup = appGroup,
						Error = exp02,
						Message = $@"SetToNormalStateError - Refer to .. {codeLocTag}",
						PaymentStatus = DeviceProgressStatus.MachineMalFunctionError
					});

					throw exMmf;
				}
			}
		}
		#endregion

		#region ----- Handle In progress Event -----

		private void DispatchInProgressEventThreadWorking()
		{
			Log.LogText(LogChannel, CurrentProcessId, "Start - DispatchInProgressEventThreadWorking", "A01", "B2BAccess.DispatchInProgressEventThreadWorking");

			B2BDecorator.Events.InProgressEventArgs inProgEv = null;

			while (!_disposed)
			{
				try
				{
					inProgEv = GetNextInProgressEvent();
					if (inProgEv != null)
					{
						//OnInProgressSendEvent(inProgEv, "A10", "B2BAccess.DispatchInProgressEventThreadWorking");
						if ((inProgEv != null) && (OnInProgress != null))
						{
							try
							{
								string processId = string.IsNullOrWhiteSpace(inProgEv.ProcessId) ? "-" : inProgEv.ProcessId;

								Log.LogText(LogChannel, processId, inProgEv, "A01", "B2BAccess.DispatchInProgressEventThreadWorking", netProcessId: inProgEv.NetProcessId,
									extraMsg: "Start - DispatchInProgressEventThreadWorking ; MsgObj: InProgressEventArgs");

								OnInProgress.Invoke(null, inProgEv);

								Log.LogText(LogChannel, processId, "End - DispatchInProgressEventThreadWorking", 
									"A10", "B2BAccess.DispatchInProgressEventThreadWorking", netProcessId: inProgEv.NetProcessId);
							}
							catch (Exception ex)
							{
								Log.LogError(LogChannel, CurrentProcessId, new Exception("Unhandled OnInProgress event calling.", ex), "E01", "B2BAccess.DispatchInProgressEventThreadWorking");
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, CurrentProcessId, ex, "E02", "B2BAccess.DispatchInProgressEventThreadWorking");
				}
			}

			// Ending send of InProgressEvent refer to _InProgressEventObjList.
			inProgEv = null;
			while (_InProgressEventObjList.TryDequeue(out B2BDecorator.Events.InProgressEventArgs inProgEv2))
			{ inProgEv2.Dispose(); }

			Log.LogText(LogChannel, CurrentProcessId, "End - DispatchInProgressEventThreadWorking", "A100", "B2BAccess.DispatchInProgressEventThreadWorking");

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			B2BDecorator.Events.InProgressEventArgs GetNextInProgressEvent()
			{
				B2BDecorator.Events.InProgressEventArgs retEvn = null;

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

		public void OnInProgressSentEvent(B2BDecorator.Events.InProgressEventArgs inProcEvent)
		{
			(new Thread(new ThreadStart(new Action(() => {
				try
				{
					lock (_InProgressEventObjList)
					{
						if (inProcEvent != null)
						{
							// Clear Outstanding Events If Payment Has Done.
							if ((inProcEvent.PaymentStatus == DeviceProgressStatus.PaymentDone)
								|| (inProcEvent.PaymentStatus == DeviceProgressStatus.PaymentCancelled)
								|| (inProcEvent.PaymentStatus == DeviceProgressStatus.PaymentTimeout))
							{
								while (_InProgressEventObjList.TryDequeue(out B2BDecorator.Events.InProgressEventArgs inProgEv))
								{ inProgEv.Dispose(); }
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

	}
}