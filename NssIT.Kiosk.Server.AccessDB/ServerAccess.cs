using NssIT.Kiosk.AppDecorator.Common.AppService.Command;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Server.AccessDB.CommandExec;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB
{
	public class ServerAccess : IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private bool _disposed = false;
		private bool _serverShutDown = false;

		private bool? _isWebServiceSiteDetected = null; /* null: not yet detect; false: site not found; true: site detected */

        private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 10);

        private Thread _commandExecutionThreadWorker = null;
        private Thread _sendMessageThreadWorker = null;

        private ConcurrentQueue<SendMessageEventArgs> _sendMessageEventObjList = new ConcurrentQueue<SendMessageEventArgs>();

        private AccessCommandRepository _commandRepository = new AccessCommandRepository();

        // Below Events Should be implemented by CommonBusiness OR Application layer 
        public event EventHandler<SendMessageEventArgs> OnSendMessage;

		public const int RetryIntervalSec = 3;

		public const int NetworkTimeout = 9999999;
		public const int InvalidAuthentication = 9999998;
		public const int WebAccessError = 9999997;
		public const int NoDetailFound = 9999996;

		private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private ServiceSoapClient _soap = null;
        public ServiceSoapClient Soap
        {
            get
            {
                if (_soap == null)
                {
                    BasicHttpBinding binding = new BasicHttpBinding();
					binding.MaxReceivedMessageSize = 2097150;
					EndpointAddress address = new EndpointAddress(AppDecorator.Config.Setting.GetSetting().WebServiceURL);
                    _soap = new ServiceSoapClient(binding, address);
                }
                return _soap;
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

		private ServerAccess()
        {
            _commandExecutionThreadWorker = new Thread(new ThreadStart(CommandExecutionThreadWorking));
            _commandExecutionThreadWorker.IsBackground = true;
            _commandExecutionThreadWorker.Start();

            _sendMessageThreadWorker = new Thread(new ThreadStart(SendMessageThreadWorking));
            _sendMessageThreadWorker.IsBackground = true;
            _sendMessageThreadWorker.Start();
        }

        private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
        private static ServerAccess _access = null;

        public static ServerAccess GetAccessServer()
        {
            if (_access != null)
                return _access;

            try
            {
                _manLock.WaitAsync().Wait();

                if (_access == null)
                {
                    _access = new ServerAccess();
                }
            }
            finally
            {
                _manLock.Release();
            }

            return _access;
        }

		public void QueryServerStatus(out bool isServerDisposed, out bool isServerShutdown, out bool isBusyDetectingWebService, out bool isWebServiceDetected)
		{
			isServerDisposed = _disposed;
			isServerShutdown = _serverShutDown;

			isBusyDetectingWebService = (_isWebServiceSiteDetected.HasValue == false) ? true : false;

			if (isBusyDetectingWebService)
				isWebServiceDetected = false;
			else
				isWebServiceDetected = _isWebServiceSiteDetected.Value;
		}

		public bool AddCommand(AccessCommandPack commPack, out string errorMsg)
        {
            errorMsg = null;
            if (commPack == null)
            {
                errorMsg = "Invalid command specification (EXIT21274).";
                return false;
            }

            try
            {
                if (_disposed)
                    throw new Exception("System is shutting down; (EXIT21201);");
                else if (_isWebServiceSiteDetected.HasValue == false)
                    throw new Exception("System busy. Connecting web server in progress. (EXIT21202);");
                else if (_isWebServiceSiteDetected.Value == false)
                    throw new Exception("Unable to connect with web server at the moment. (EXIT21203);");

                if (commPack.NetProcessId.HasValue == false)
                    Log.LogText(LogChannel, commPack?.ProcessId ?? "-", "Net Process Id not found.", "EXIT21204", "ServerAccess.AddCommand", AppDecorator.Log.MessageType.Error);

                return _commandRepository.EnQueueNewCommandPack(commPack, out errorMsg);

            }
            catch (Exception ex)
            {
                errorMsg = $@"Unable to add job to service (EXIT21205); {ex.Message}";
                Log.LogError(LogChannel, commPack?.ProcessId ?? "-", new Exception("Unable to add job to cash machine;", ex), "EX01", "ServerAccess.AddCommand");
            }
            return false;
        }


		#region ----- CommandExecutionThreadWorking -----

		private void CommandExecutionThreadWorking()
		{
			AccessCommandPack commandPack = null;
			_isWebServiceSiteDetected = null;
			_serverShutDown = false;

			try
			{
				_isWebServiceSiteDetected = false;
				_isWebServiceSiteDetected = Security.GetTimeStamp(out string timeStampStr1);

				IAccessCommandExec jobWorker = null;
				while (!_disposed)
				{
					// * Check Machine Status, send Machine MalFunction Exception if Cash low OR Do not have coin.
					// * Always send Message to user about Acceptable of changing/refund Bill status.
					try
					{
						commandPack = null;
						commandPack = _commandRepository.DeQueueCommandPack();

						if (_isWebServiceSiteDetected == false)
						{
							_isWebServiceSiteDetected = false;
							_isWebServiceSiteDetected = Security.GetTimeStamp(out string timeStampStr2);
						}

						if (commandPack != null)
						{
							if (OnSendMessage == null)
							{
								Log.LogText(LogChannel, commandPack.ProcessId, "System unable to send result message; Event (OnSendMessage) not being asigned (EXIT21303)", "A04", "ServerAccess.CommandExecutionThreadWorking",
								AppDecorator.Log.MessageType.Error);
							}
							else
							{
								if ((commandPack.HasCommand) && (commandPack.ProcessDone == false))
								{
									CurrentProcessId = commandPack.ProcessId;
									CurrentNetProcessId = commandPack.NetProcessId;

									Log.LogText(LogChannel, CurrentProcessId, commandPack, "A06", "ServerAccess.CommandExecutionThreadWorking",
										netProcessId: CurrentNetProcessId,
										extraMsg: "Start - CommandExecution; MsgObj: AccessCommandPack");

									jobWorker = GetCommandExec(commandPack.CommandCode);

									if (jobWorker != null)
										commandPack = jobWorker.Execute(this, commandPack);
									else
										Log.LogText(LogChannel, CurrentProcessId, commandPack, "A08", "ServerAccess.CommandExecutionThreadWorking",
											netProcessId: CurrentNetProcessId, extraMsg: "No IAccessCommandExec is found; MsgObj: AccessCommandPack");

									Log.LogText(LogChannel, CurrentProcessId, commandPack, "A10", "ServerAccess.CommandExecutionThreadWorking",
										netProcessId: CurrentNetProcessId, extraMsg: "End - CommandExecution; MsgObj: AccessCommandPack");
								}
							}
							//}
						} 						
					}
					catch (Exception ex)
					{
						Log.LogError(LogChannel, _currProcessId, ex, "Ex02", classNMethodName: "ServerAccess.CommandExecutionThreadWorking");
					}
					finally
					{
						if (commandPack != null)
						{
							commandPack.Command.ProcessDone = true;
							_commandRepository.UpdateCommandPack(commandPack);

							CurrentProcessId = "=";
							CurrentNetProcessId = null;
						}

						if (jobWorker is IDisposable disposable)
						{
							disposable.Dispose();
							jobWorker = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				_serverShutDown = true;
				Log.LogFatal(LogChannel, _currProcessId, ex, "Ex05", classNMethodName: "ServerAccess.CommandExecutionThreadWorking");
			}
			finally
			{
				_serverShutDown = true;

				Log.LogText(LogChannel, _currProcessId, "Quit Server Access Process", "END_X01", "ServerAccess.CommandExecutionThreadWorking");

				_isWebServiceSiteDetected = false;
			}
			
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			// below event handle used only to handle machine status without command.

			IAccessCommandExec GetCommandExec(AccessDBCommandCode commandCode)
			{
				IAccessCommandExec theRunner = null;

				if (commandCode == AccessDBCommandCode.OriginListRequest)
					return new GetOriginExecution();
				else if(commandCode == AccessDBCommandCode.CheckOutstandingCardSettlementRequest)
                    return new CheckOutstandingSettlementExecution();
                else if (commandCode == AccessDBCommandCode.DestinationListRequest)
					return new GetDestinationExecution();
				else if (commandCode == AccessDBCommandCode.WebServerLogonRequest)
					return new WebServerLogonExecution();
				else if (commandCode == AccessDBCommandCode.DepartTripListRequest)
					return new GetDepartTripExecution();
				else if (commandCode == AccessDBCommandCode.DepartSeatListRequest)
					return new GetDepartSeatListExecution();
				else if (commandCode == AccessDBCommandCode.DepartSeatConfirmRequest)
					return new DepartSeatConfirmExecution();
				else if (commandCode == AccessDBCommandCode.DepartCustInfoUpdateELSEReleaseSeatRequest)
					return new DepartCustInfoUpdateELSEReleaseSeatExecution();
				else if (commandCode == AccessDBCommandCode.CompleteTransactionElseReleaseSeatRequest)
					return new CompleteTransactionElseReleaseSeatExecution();
				else if (commandCode == AccessDBCommandCode.TicketReleaseRequest)
					return new ReleaseSeatExecution();

				return theRunner;
			} 
		}

		#endregion

		#region ----- Handle In progress Event -----

		private void SendMessageThreadWorking()
		{
			Log.LogText(LogChannel, CurrentProcessId, "Start - SendMessageThreadWorking", "A01", "ServerAccess.SendMessageThreadWorking");

			SendMessageEventArgs inProgEv = null;

			while (!_disposed)
			{
				try
				{
					inProgEv = GetNextMessageEvent();
					if (inProgEv != null)
					{
						if (OnSendMessage != null)
						{
							try
							{
								string processId = string.IsNullOrWhiteSpace(inProgEv.ProcessId) ? "-" : inProgEv.ProcessId;

								Log.LogText(LogChannel, processId, inProgEv, "A01", "ServerAccess.SendMessageThreadWorking", netProcessId: inProgEv.NetProcessId,
									extraMsg: "Start - DispatchInProgressEventThreadWorking ; MsgObj: InProgressEventArgs");

								//UITicketReleaseResult
								if (inProgEv.KioskMessage is UISeatReleaseResult)
								{
									Log.LogText(LogChannel, processId, "End - SendMessageThreadWorking - Message UITicketReleaseResult Ignored Sent;",
										"A10", "ServerAccess.SendMessageThreadWorking", netProcessId: inProgEv.NetProcessId);
								}
								else
								{
									OnSendMessage.Invoke(null, inProgEv);
									Log.LogText(LogChannel, processId, "End - SendMessageThreadWorking",
										"A10", "ServerAccess.SendMessageThreadWorking", netProcessId: inProgEv.NetProcessId);
								}
							}
							catch (Exception ex)
							{
								Log.LogError(LogChannel, CurrentProcessId, new Exception("Unhandled OnInProgress event calling.", ex), "E01", "ServerAccess.SendMessageThreadWorking");
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, CurrentProcessId, ex, "E02", "ServerAccess.DispatchInProgressEventThreadWorking");
				}
			}

			// Ending send of InProgressEvent refer to _InProgressEventObjList.
			inProgEv = null;
			while (_sendMessageEventObjList.TryDequeue(out SendMessageEventArgs inProgEv2))
			{ inProgEv2.Dispose(); }

			Log.LogText(LogChannel, CurrentProcessId, "End - DispatchInProgressEventThreadWorking", "A100", "ServerAccess.SendMessageThreadWorking");

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			SendMessageEventArgs GetNextMessageEvent()
			{
				SendMessageEventArgs retEvn = null;

				try
				{
					lock (_sendMessageEventObjList)
					{
						if (_disposed == false)
						{
							if (_sendMessageEventObjList.Count == 0)
							{
								Monitor.Wait(_sendMessageEventObjList, _MaxWaitPeriod);
							}
							if (_sendMessageEventObjList.TryDequeue(out retEvn))
							{
								return retEvn;
							}
						}
						else
							Monitor.Wait(_sendMessageEventObjList, _MaxWaitPeriod);
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, CurrentProcessId, ex, "E01", "ServerAccess.GetNextMessageEvent");
				}

				return null;
			}
		}

		public void RaiseOnSendMessage(SendMessageEventArgs sndMsgEvent, string lineTag)
		{
			(new Thread(new ThreadStart(new Action(() => {
				try
				{
					lock (_sendMessageEventObjList)
					{
						if (sndMsgEvent != null)
						{
							_sendMessageEventObjList.Enqueue(sndMsgEvent);
							Monitor.PulseAll(_sendMessageEventObjList);
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, CurrentProcessId, ex, "E01", $@"ServerAccess.RaiseOnSendMessage; {lineTag}");
				}
			})))
			{ IsBackground = true })
			.Start();
		}

		#endregion

		public void Dispose()
        {
            lock (_sendMessageEventObjList)
            {
                _disposed = true;
                _isWebServiceSiteDetected = null;

                try
                {
                    if (OnSendMessage != null)
                    {
                        Delegate[] delgList = OnSendMessage.GetInvocationList();
                        foreach (EventHandler<SendMessageEventArgs> delg in delgList)
                        {
                            OnSendMessage -= delg;
                        }
                    }
                }
                catch { }

                while (_sendMessageEventObjList.TryDequeue(out SendMessageEventArgs evt))
                { evt.Dispose(); }

                OnSendMessage = null;

                _commandRepository.Dispose();
                Task.Delay(100).Wait();

                Monitor.PulseAll(_sendMessageEventObjList);
            }
            _log = null;
        }
    }
}
