using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.AppService.Network
{
	/// <summary>
	/// Class used to store and manage information about a network communication info refer to a specified transaction.
	/// </summary>
	public class NetInfoRepository
	{
		private bool _disposed = false;

		private ConcurrentDictionary<Guid, NetCommunicationInfo> _netInfoList = null;

		private Log.DB.DbLog _log = null;

		private Thread _threadWorker = null;

		public NetInfoRepository()
		{
			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			_netInfoList = new ConcurrentDictionary<Guid, NetCommunicationInfo>();

			_threadWorker = new Thread(new ThreadStart(RepositoryCleanUpThreadWorking));
			_threadWorker.IsBackground = true;
			_threadWorker.Start();
		}

		public void AddNetProcessInfo(INetCommandDirective netCommDirective, int dataSendingPort)
		{
			lock(_netInfoList)
			{
				if (netCommDirective != null)
					_netInfoList.TryAdd(netCommDirective.BaseNetProcessId, new NetCommunicationInfo(netCommDirective.BaseNetProcessId, dataSendingPort, netCommDirective.CommuCommandDirection));
				else
					_netInfoList.TryAdd(netCommDirective.BaseNetProcessId, new NetCommunicationInfo(netCommDirective.BaseNetProcessId, dataSendingPort, CommunicationDirection.SendOnly));
			}
		}

		/// <summary>
		/// Remove NetCommunicationInfo if CommandCommuDirection is SendOneResponseOne and ResponseCount > 0, CommandCommuDirection is SendOnly, or CommandCommuDirection is Unknown
		/// </summary>
		/// <param name="netProcessId"></param>
		public void RemoveInfoWithOneResponse(Guid netProcessId)
		{
			try
			{
				if (_netInfoList.TryGetValue(netProcessId, out NetCommunicationInfo netInfo) == true)
				{
					if (((netInfo.CommandCommuDirection == CommunicationDirection.SendOneResponseOne) && (netInfo.ResponseCount > 0))
						||
						(netInfo.CommandCommuDirection == CommunicationDirection.SendOnly)
						||
						(netInfo.CommandCommuDirection == CommunicationDirection.Unknown)
						)
					{
						lock (_netInfoList)
						{
							_netInfoList.TryRemove(netInfo.NetProcessId, out NetCommunicationInfo removeNetInfo);
						}
					}
				}
			}
			catch { }			
		}

		public bool SetActiveResponseCounter(Guid netProcessId, out string errorMsg)
		{
			bool isDone = false;
			errorMsg = null;

			errorMsg = null;
			lock (_netInfoList)
			{
				try
				{
					if (_netInfoList.TryGetValue(netProcessId, out NetCommunicationInfo netInfo) == false)
					{
						errorMsg = $@"NetProcessId ({netProcessId.ToString("D")}) is not found (EXIT8101).";
					}
					else
					{
						netInfo.ResponseIncrement();
						netInfo.LastActiveDate = DateTime.Now;
						UpdateNetCommInfo(netInfo);
						isDone = true;
					}
				}
				catch (Exception ex)
				{
					errorMsg = ex.Message + "(EXIT8102)";
				}
			}
			return isDone;


			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			/// <summary>
			/// Update latest Net Communication Info
			/// </summary>
			/// <param name="latestNetInfo"></param>
			void UpdateNetCommInfo(NetCommunicationInfo latestNetInfo)
			{
				if (_netInfoList.ContainsKey(latestNetInfo.NetProcessId))
				{
					_netInfoList.TryGetValue(latestNetInfo.NetProcessId, out NetCommunicationInfo previousNetInfo);
					_netInfoList.TryUpdate(latestNetInfo.NetProcessId, latestNetInfo, previousNetInfo);
				}
			}
		}

		/// <summary>
		/// Get Destination TCP Port. Return 0 if the Port not found refer to netProcessId.
		/// </summary>
		/// <param name="netProcessId"></param>
		/// <param name="isResponseAnswerRequired"></param>
		/// <param name="errorMsg"></param>
		/// <returns>Return 0 if the Port not found refer to netProcessId.</returns>
		public int GetDestinationPort(Guid netProcessId, out bool isResponseAnswerRequired, out string errorMsg)
		{
			isResponseAnswerRequired = false;
			errorMsg = null;
			int retPort = 0;
			lock (_netInfoList)
			{
				try
				{
					if (_netInfoList.TryGetValue(netProcessId, out NetCommunicationInfo netInfo) == false)
					{
						errorMsg = $@"NetProcessId ({netProcessId.ToString("D")}) is not found (EXIT8103).";
					}
					if ((netInfo.CommandCommuDirection == CommunicationDirection.SendOnly) || 
						(netInfo.CommandCommuDirection == CommunicationDirection.Unknown))
					{
						/*By Pass*/
					}
					else
					{
						isResponseAnswerRequired = true;
						retPort = netInfo.DestinationPort;
					}
				}
				catch (Exception ex)
				{
					errorMsg = ex.Message + "(EXIT8104)";
				}
			}

			return retPort;
		}

		/// <summary>
		/// Return false when error found;
		/// </summary>
		/// <param name="logChannel"></param>
		/// <param name="e"></param>
		/// <param name="classMethodTag"></param>
		/// <param name="netProcessId"></param>
		/// <param name="destinationPort"></param>
		/// <param name="isResponseRequested"></param>
		/// <returns></returns>
		public bool GetNetCommunicationInfo(string logChannel, UIMessageEventArgs e, string classMethodTag, out Guid? netProcessId, out int destinationPort, out bool isResponseRequested)
		{
			netProcessId = null;
			destinationPort = -1;
			isResponseRequested = false;

			// Get Net Process Id
			if (e.NetProcessId.HasValue == false)
			{
				_log.LogText(logChannel, "-", e, "EX01", classMethodTag, AppDecorator.Log.MessageType.Error,
					extraMsg: "Net Process Id not found; MsgObj: UIMessageEventArgs");

				return false;
			}

			netProcessId = e.NetProcessId.Value;

			// Get Destination Port and Check for Response Requested Flag
			destinationPort = GetDestinationPort(netProcessId.Value, out isResponseRequested, out string errorMsg);

			if (destinationPort <= 0)
			{
				if (errorMsg != null)
					_log.LogText(logChannel, "-", e, "EX02", classMethodTag, AppDecorator.Log.MessageType.Error,
						extraMsg: $@"Error when read net info; {errorMsg}; MsgObj: UIMessageEventArgs");
				else
					_log.LogText(logChannel, "-", e, "EX03", classMethodTag, AppDecorator.Log.MessageType.Error,
						extraMsg: $@"Error when read net info; Destination port({destinationPort}) not found in system; MsgObj: UIMessageEventArgs");

				return false;
			}

			return true;
		}

		private TimeSpan _maxStorePeriod = new TimeSpan(2, 0, 0);
		private void RepositoryCleanUpThreadWorking()
		{
			TimeSpan noWaitPeriod = new TimeSpan(0);

			while (!_disposed)
			{
				try
				{
					NetCommunicationInfo netProcInfo = null;
					Guid netProcId;
					List<Guid> abordKeyList = new List<Guid>();

					lock (_netInfoList)
					{
						foreach (KeyValuePair<Guid, NetCommunicationInfo> valPair in _netInfoList)
						{
							netProcInfo = valPair.Value;
							netProcId = valPair.Key;
							//if (netProcInfo.CommandCommuDirection == CommunicationDirection.SendOnly)
							//{
							//	abordKeyList.Add(netProcId);
							//}
							//else if ((netProcInfo.CommandCommuDirection == CommunicationDirection.SendOneResponseOne) && (netProcInfo.ResponseCount > 0))
							//{
							//	abordKeyList.Add(netProcId);
							//}
							//else

							// Put into the abort list for NetInfo that is having expired time.
							if (netProcInfo.LastActiveDate.Add(_maxStorePeriod).Subtract(DateTime.Now).TotalSeconds <= 0)
							{
								abordKeyList.Add(netProcId);
							}
						}

						if (abordKeyList.Count > 0)
						{
							foreach (Guid abordNetProcId in abordKeyList)
							{
								_netInfoList.TryRemove(abordNetProcId, out NetCommunicationInfo tempRes);
							}
						}
					}

					// Sleep for 2 minutes
					Thread.Sleep(1000 * 60 * 2);
				}
				catch (Exception ex)
				{
					string tmpMsg = ex.Message;
				}
			}
		}

		public void Dispose()
		{
			_disposed = true;

			if (_threadWorker != null)
			{
				try
				{
					if ((_threadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
						_threadWorker.Interrupt();
				}
				catch { }
				_threadWorker = null;
			}
			
		}

		class NetCommunicationInfo : IDisposable 
		{
			public Guid NetProcessId { get; private set; }
			public DateTime LastActiveDate { get; set; }
			public CommunicationDirection CommandCommuDirection { get; private set; }
			public CommunicationMedium MediaComm { get; private set; } = CommunicationMedium.Unknown;

			public int ResponseCount { get; private set; } = 0;
			
			// Used for SignalR
			// public string DestinationURL { get; private set; } = null;

			// Used for TCP
			public int DestinationPort { get; private set; } = 0;

			public NetCommunicationInfo(Guid netProcessId, int dataSendToPort, CommunicationDirection netCommDirection, CommunicationMedium mediaComm = CommunicationMedium.TCP)
			{
				NetProcessId = netProcessId;
				MediaComm = mediaComm;
				LastActiveDate = DateTime.Now;

				if (dataSendToPort > 0)
					DestinationPort = dataSendToPort;

				if (netCommDirection == CommunicationDirection.Unknown)
					CommandCommuDirection = CommunicationDirection.SendOnly;
				else
					CommandCommuDirection = netCommDirection;
			}

			public void ResponseIncrement()
			{
				ResponseCount++;
			}

			public void Dispose()
			{ 	}
		}
	}
}
