using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;

namespace NssIT.Kiosk.Common.AppService.Network.TCP
{
	public class LocalTcpService : INetMediaInterface
	{
		private const string LogChannel = "Tcp";

		private const int _maxDataSize = 65_536;
		/// <summary>
		/// Private thread Worker List
		/// </summary>
		private (Thread LocalServiceTcpTWorker, Thread ClientTcpTWorker) _threads = (null, null);

		/// <summary>
		/// Private data Collection
		/// </summary>
		private (ConcurrentQueue<NetMessagePack> ReceivedDataList, ConcurrentQueue<NetMessagePack> SendDataList) _data
			= (ReceivedDataList: new ConcurrentQueue<NetMessagePack>(), SendDataList: new ConcurrentQueue<NetMessagePack>());

		/// <summary>
		/// Private configuration setting
		/// </summary>
		private (int LocalServicePort, TimeSpan MaxDataWaitPeriod) _setting
			= (LocalServicePort: -1, MaxDataWaitPeriod: new TimeSpan(0, 0, 10));

		/// <summary>
		/// Private flags used within class.
		/// </summary>
		private (bool IsShuttingDown, bool Disposed) _flags = (false, false);

		private (int ClearExpiredNetProcessIdListIntervalSec, DateTime NextClearListTime, ConcurrentDictionary<Guid, DateTime> ExpiredNetProcessIdList) _expiredNetProcess
			= (ClearExpiredNetProcessIdListIntervalSec: 60, NextClearListTime: DateTime.Now, ExpiredNetProcessIdList: new ConcurrentDictionary<Guid, DateTime>());

		private (int ClearDeliveredReceivedMessageIntervalSec, DateTime NextClearListTime, ConcurrentDictionary<Guid, DateTime> DeliveredReceivedMessageList) _deliveredReceivedMessage
			= (ClearDeliveredReceivedMessageIntervalSec: 60, NextClearListTime: DateTime.Now, DeliveredReceivedMessageList: new ConcurrentDictionary<Guid, DateTime>());

		private TcpListener _tcpLocalService = null;
		private SynchronizationContext _localServiceTcpContext = null;

		public event EventHandler<DataReceivedEventArgs> OnDataReceived;

		private DbLog _log = null;
		private DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		public int LocalServicePort { get => _setting.LocalServicePort; }

		public LocalTcpService(int localServicePort)
		{
			if (localServicePort <= 0)
				localServicePort = GetAvailablePort();

			_setting.LocalServicePort = localServicePort;

			_threads.LocalServiceTcpTWorker = new Thread(new ThreadStart(ReceivingThreadWorking));
			_threads.LocalServiceTcpTWorker.IsBackground = true;
			_threads.LocalServiceTcpTWorker.Start();

			_threads.ClientTcpTWorker = new Thread(new ThreadStart(SendingThreadWorking));
			_threads.ClientTcpTWorker.IsBackground = true;
			_threads.ClientTcpTWorker.Start();
		}

		// Having this found is to block message from sending out refer to specified message' NetProcessId.
		/// <summary>
		/// Return false when a received message has sent refer to netProcessId.
		/// </summary>
		/// <param name="netProcessId"></param>
		/// <returns></returns>
		public bool SetExpiredNetProcessId(Guid netProcessId)
		{
			bool isSuccess = true;

			// Note : This Thread only allow simple process. For complicated process please use other method of thread processing
			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				if (_flags.Disposed == false)
				{
					try
					{
						lock (_expiredNetProcess.ExpiredNetProcessIdList)
						{
							//Clear ExpiredNetProcessIdList
							if (_expiredNetProcess.NextClearListTime.Ticks < DateTime.Now.Ticks)
							{
								Guid[] clearNetProcessIdArr = (from keyPair in _expiredNetProcess.ExpiredNetProcessIdList
										   where (keyPair.Value.Ticks <= _expiredNetProcess.NextClearListTime.Ticks)
										   select keyPair.Key).ToArray();

								foreach(Guid netProcId in clearNetProcessIdArr)
									_expiredNetProcess.ExpiredNetProcessIdList.TryRemove(netProcId, out DateTime expiredTime);

								_expiredNetProcess.NextClearListTime = DateTime.Now.AddSeconds(_expiredNetProcess.ClearExpiredNetProcessIdListIntervalSec);
							}

							if (_deliveredReceivedMessage.DeliveredReceivedMessageList.TryGetValue(netProcessId, out DateTime deliveredTime) == true)
								isSuccess = false;

							else if (_expiredNetProcess.ExpiredNetProcessIdList.TryGetValue(netProcessId, out DateTime expiredTime2))
							{
								_expiredNetProcess.ExpiredNetProcessIdList.TryRemove(netProcessId, out DateTime expiredTime3);
								_expiredNetProcess.ExpiredNetProcessIdList.TryAdd(netProcessId, DateTime.Now);
								isSuccess = true;
							}
							else if (_expiredNetProcess.ExpiredNetProcessIdList.TryAdd(netProcessId, DateTime.Now))
								isSuccess = true;

						}
					}
					// Used to handle "_logList" is null after disposed
					catch (Exception ex2) { string byPassStr = ex2.Message; }
				}
			})));
			execThread.IsBackground = true;
			execThread.Start();
			execThread.Join();

			return isSuccess;
		}

		private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);
		public static int GetAvailablePort()
		{
			using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				socket.Bind(DefaultLoopbackEndpoint);
				return ((IPEndPoint)socket.LocalEndPoint).Port;
			}
		}

		private async void ReceivingThreadWorking()
		{
			Log.LogText(LogChannel, "-", "Start - ReceivingThreadWorking", "A01", "LocalTcpService.ReceivingThreadWorking");

			NetMessagePack receivedData = null;
			Thread eventDispatchTWorker = null;

			eventDispatchTWorker = new Thread(new ThreadStart(EventDispatchThreadWorking));
			eventDispatchTWorker.IsBackground = true;
			eventDispatchTWorker.Start();

			try
			{
				byte[] previousFullData = null;
				byte[] newFullData = new byte[0];
				byte[] dataBytes = new Byte[256];
				int readSize = 0;

				_localServiceTcpContext = SynchronizationContext.Current ?? (new SynchronizationContext());
				//_localServiceTcpContext = new SynchronizationContext();
				//_localServiceTcpContext.Send(state => {
				//	string teststr = "testing ..";
				//}, null);

				_tcpLocalService = new TcpListener(IPAddress.Parse("127.0.0.1"), _setting.LocalServicePort);
				_tcpLocalService.Start();

				do
				{
					int nextDataInx = 0;

					try
					{
						using (TcpClient tcpClient = _tcpLocalService.AcceptTcpClient())
						{
							tcpClient.ReceiveTimeout = 5 * 1000;
							await Task.Delay(50);

							int dataSize = (tcpClient.Client?.Available).HasValue ? tcpClient.Client.Available + 16 : 2048;
							if (dataSize < 2048)
								dataSize = 2048;

							using (NetworkStream streamClient = tcpClient.GetStream())
							{
								if (!_flags.IsShuttingDown)
								{
									readSize = 0;
									previousFullData = null;
									newFullData = new byte[0];

									do
									{
										dataBytes = new byte[dataSize];

										readSize = await streamClient.ReadAsync(dataBytes, 0, dataBytes.Length);
										if (readSize > 0)
										{
											nextDataInx = newFullData.Length;

											previousFullData = newFullData;
											newFullData = new byte[previousFullData.Length + readSize];

											previousFullData.CopyTo(newFullData, 0);
											System.Buffer.BlockCopy(dataBytes, 0, newFullData, nextDataInx, readSize);
										}

									} while (readSize > 0);

									receivedData = Desserialize(newFullData) as NetMessagePack;
									PushReceivedData(receivedData);
								}
								try { streamClient.Close(); }
								catch { }
							} // ref.to -- using (NetworkStream stream = tcpClient.GetStream())
							try
							{ tcpClient.Close(); }
							catch { }
						} // ref.to -- using (TcpClient tcpClient = _tcpLocalService.AcceptTcpClient())
					}
					catch (Exception ex) when (ex.Message.Contains("A blocking operation was interrupted by a call to WSACancelBlockingCall"))
					{ /* by pass */ }
					catch (Exception ex)
					{
						//string errMsg = ex.Message;
						Log.LogError(LogChannel, "-", ex, "EX01", "LocalTcpService.ReceivingThreadWorking");
					}

				} while (!_flags.IsShuttingDown);
			}
			catch (Exception ex7)
			{
				//string emsg = exx.Message;
				Log.LogError(LogChannel, "-", ex7, "EX11", "LocalTcpService.ReceivingThreadWorking");
			}
			finally
			{
				Log.LogText(LogChannel, "-", "End - ReceivingThreadWorking", "A100", "LocalTcpService.ReceivingThreadWorking");

				try
				{
					_tcpLocalService?.Stop();
				}
				catch (Exception ex)
				{
					string errMsg = ex.Message;
				}

				try
				{
					_tcpLocalService?.Server.Close();
				}
				catch (Exception ex)
				{
					string errMsg = ex.Message;
				}

				_tcpLocalService = null;
			}

			///// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			///// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			object Desserialize(byte[] data)
			{
				using (MemoryStream memStream = new MemoryStream(data))
				{
					BinaryFormatter binForm = new BinaryFormatter();

					memStream.Write(data, 0, data.Length);
					memStream.Seek(0, SeekOrigin.Begin);
					var obj = binForm.Deserialize(memStream);
					return obj;
				}
			}

			void PushReceivedData(NetMessagePack recvData)
			{
				string processId = string.IsNullOrWhiteSpace(recvData.MsgObject?.ProcessId) ? "-" : recvData.MsgObject.ProcessId;
				Guid netProcessId = recvData.NetProcessId;

				Log.LogText(LogChannel, processId, recvData, "A01", "LocalTcpService.PushReceivedData", netProcessId: netProcessId,
					extraMsg: "Start - PushReceivedData; MsgObj: NetMessagePack");

				if (recvData == null)
					throw new Exception("Data parameter cannot be NULL at PushReceivedData");

				lock (_data.ReceivedDataList)
				{
					if (_flags.Disposed || _flags.IsShuttingDown)
						return;

					_data.ReceivedDataList.Enqueue(recvData);
					Monitor.PulseAll(_data.ReceivedDataList);

					Log.LogText(LogChannel, processId, "End - PushReceivedData", "A100", "LocalTcpService.PushReceivedData", netProcessId: netProcessId);

					return;
				}
			}

			void EventDispatchThreadWorking()
			{
				Log.LogText(LogChannel, "-", "Start - EventDispatchThreadWorking", "A01", "LocalTcpService.EventDispatchThreadWorking");

				NetMessagePack recvData = null;
				DataReceivedEventArgs recvDataEvnAgr = null;

				long logDelayCount = 0;
				while (!_flags.IsShuttingDown)
				{
					Guid netProcessId = Guid.Empty;

					try
					{
						recvData = GetNextReceivedData();

						if(recvData != null)
						{
                            if (recvData.Instruction == (CommInstruction)UISalesInst.CheckOutstandingCardSettlementRequest)
                            {
                                var test = "asdasdasd";
                            }

                            if (recvData.Instruction == (CommInstruction)UISalesInst.CheckOutstandingCardSettlementAck)
                            {
                                var teas = "dasdasdasda";
                            }
                        }
						
						if (recvData != null)
						{
							lock (_expiredNetProcess.ExpiredNetProcessIdList)
							{
								string processId = string.IsNullOrWhiteSpace(recvData.MsgObject?.ProcessId) ? "-" : recvData.MsgObject.ProcessId;
								netProcessId = recvData.NetProcessId;

								//Clear DeliveredReceivedMessageList
								if (_deliveredReceivedMessage.NextClearListTime.Ticks < DateTime.Now.Ticks)
								{
									Guid[] clearNetProcessIdArr = (from keyPair in _deliveredReceivedMessage.DeliveredReceivedMessageList
																   where (keyPair.Value.Ticks <= _deliveredReceivedMessage.NextClearListTime.Ticks)
																   select keyPair.Key).ToArray();

									foreach (Guid netProcId in clearNetProcessIdArr)
										_deliveredReceivedMessage.DeliveredReceivedMessageList.TryRemove(netProcId, out DateTime deliveredMessageTime);
									_deliveredReceivedMessage.NextClearListTime = DateTime.Now.AddSeconds(_deliveredReceivedMessage.ClearDeliveredReceivedMessageIntervalSec);
								}

								if (_expiredNetProcess.ExpiredNetProcessIdList.TryGetValue(netProcessId, out DateTime expiredTime) == false)
								{
									recvDataEvnAgr = new DataReceivedEventArgs(recvData);
									try
									{
										if (OnDataReceived == null)
											throw new Exception("OnDataReceived event is not handled");

										Log.LogText(LogChannel, processId, recvData, "A05", "LocalTcpService.EventDispatchThreadWorking", netProcessId: netProcessId,
										extraMsg: "Dispatching .. ; MsgObj: NetMessagePack");

										if (_deliveredReceivedMessage.DeliveredReceivedMessageList.TryGetValue(netProcessId, out DateTime deliveredMessageTime2))
											_deliveredReceivedMessage.DeliveredReceivedMessageList.TryRemove(netProcessId, out DateTime deliveredMessageTime3);

										_deliveredReceivedMessage.DeliveredReceivedMessageList.TryAdd(netProcessId, DateTime.Now);
										OnDataReceived.Invoke(null, recvDataEvnAgr);

										//if ((logDelayCount % 20) == 0)
										Log.LogText(LogChannel, processId, "Event Dispatch Locally", "A10", "LocalTcpService.EventDispatchThreadWorking", netProcessId: netProcessId);

										logDelayCount++;
									}
									catch (Exception ex)
									{
										Log.LogError(LogChannel, "-", ex, "EX21", "LocalTcpService.EventDispatchThreadWorking", netProcessId: netProcessId);
									}
									finally
									{
										recvData = null;
										recvDataEvnAgr = null;
									}
								}
								else
								{
									Log.LogText(LogChannel, processId, recvData, "A15", "LocalTcpService.EventDispatchThreadWorking", netProcessId: netProcessId,
											extraMsg: "NetMessagePack already expired .. ; MsgObj: NetMessagePack");
								}
							}
						}
					}
					catch (Exception ex)
					{
						Log.LogError(LogChannel, "-", ex, "EX22", "LocalTcpService.EventDispatchThreadWorking", netProcessId: netProcessId);
					}
				}
				Log.LogText(LogChannel, "-", "End - EventDispatchThreadWorking", "A100", "LocalTcpService.EventDispatchThreadWorking");
			}

			NetMessagePack GetNextReceivedData()
			{
				NetMessagePack retData = null;

				lock (_data.ReceivedDataList)
				{
					if (!_flags.IsShuttingDown)
					{
						if (_data.ReceivedDataList.Count == 0)
						{
							Monitor.Wait(_data.ReceivedDataList, _setting.MaxDataWaitPeriod);
						}
						if (_data.ReceivedDataList.TryDequeue(out retData))
						{
							return retData;
						}
					}
				}
				return null;
			}

			bool CheckIsNetProcessIdExpired(Guid netProcessId)
			{
				bool isExpired = false;

				if (_flags.Disposed == false)
				{
					try
					{
						if (_flags.Disposed == false)
							if (_expiredNetProcess.ExpiredNetProcessIdList.TryGetValue(netProcessId, out DateTime expiredTime) == true)
								isExpired = true;
					}
					// Used to handle "_logList" is null after disposed
					catch (Exception ex2) { string byPassStr = ex2.Message; }
				}

				return isExpired;
			}
		}

		private void SendEndingPack()
		{
			try
			{
				Guid aNetProcessId = Guid.NewGuid();
				NetMessagePack endPack = new NetMessagePack(aNetProcessId) { DestinationPort = _setting.LocalServicePort };

				byte[] clientData = Serialize(endPack);

				using (TcpClient tcpClient = new TcpClient("127.0.0.1", _setting.LocalServicePort))
				{
					if (tcpClient.Connected == false)
					{
						Log.LogText(LogChannel, "-", "Fail to send ending pack", "END01", "LocalTcpService.SendEndPack",
							AppDecorator.Log.MessageType.Error, netProcessId: aNetProcessId);
					}
					else
					{
						if (clientData.Length > _maxDataSize)
						{
							tcpClient.SendBufferSize = clientData.Length + 16;
						}

						using (NetworkStream streamClient = tcpClient.GetStream())
						{
							streamClient.Write(clientData, 0, clientData.Length);
							streamClient.Flush();

							try { streamClient.Close(); }
							catch { }
						}
						try { tcpClient.Close(); }
						catch { }
					}
				}
			}
			catch (Exception ex) 
			{
				string errMsg = ex.Message;
			}
			

			byte[] Serialize(Object obj)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					BinaryFormatter bf = new BinaryFormatter();
					bf.Serialize(ms, obj);
					return ms.ToArray();
				}
			}
		}

		public void SendMsgPack(NetMessagePack msgPack)
		{
			if ((_flags.Disposed) || (_flags.IsShuttingDown))
				throw new Exception("System is shutting down; Sect-01;");

			if (msgPack.DestinationPort <= 0)
				throw new Exception("Invalid destination port number assignment; ");

			lock (_data.SendDataList)
			{
				msgPack.OriginalServicePort = _setting.LocalServicePort;
				_data.SendDataList.Enqueue(msgPack);
				Monitor.PulseAll(_data.SendDataList);
			}
		}

		private void SendingThreadWorking()
		{
			NetMessagePack sendData = null;

			try
			{
				while (!_flags.IsShuttingDown)
				{
					sendData = GetNextSendData();

					if ((sendData != null) && (sendData.OriginalServicePort > 0))
					{
						string processId = string.IsNullOrWhiteSpace(sendData.MsgObject?.ProcessId) ? "-" : sendData.MsgObject.ProcessId;
						Guid netProcessId = sendData.NetProcessId;

						try
						{
							byte[] clientData = Serialize(sendData);

							using (TcpClient tcpClient = new TcpClient("127.0.0.1", sendData.DestinationPort))
							{
								if (tcpClient.Connected == false)
								{
									Log.LogText(LogChannel, processId, sendData, "EX01", "LocalTcpService.SendingThreadWorking", AppDecorator.Log.MessageType.Error, netProcessId: netProcessId,
										extraMsg: "Fail to connect with destination. MsgObj: NetMessagePack");
								}
								else
								{
									if (clientData.Length > _maxDataSize)
									{
										tcpClient.SendBufferSize = clientData.Length + 16;
									}

									using (NetworkStream streamClient = tcpClient.GetStream())
									{
										Log.LogText(LogChannel, processId, sendData, "A02", "LocalTcpService.SendingThreadWorking", netProcessId: netProcessId,
											extraMsg: "Sending out .. ; MsgObj: NetMessagePack");

										streamClient.Write(clientData, 0, clientData.Length);
										streamClient.Flush();

										try { streamClient.Close(); }
										catch { }

										Log.LogText(LogChannel, processId, sendData, "A20", "LocalTcpService.SendingThreadWorking", netProcessId: netProcessId,
											extraMsg: $@"Date Sent; Client data size:{clientData.Length}; MsgObj: NetMessagePack");
									}
									try { tcpClient.Close(); }
									catch { }
								}
							}
						}
						catch (Exception ex)
						{
							Log.LogError(LogChannel, processId, ex, "EX05", "LocalTcpService.SendingThreadWorking", netProcessId: netProcessId);

							if (sendData != null)
							{
								Log.LogText(LogChannel, processId, sendData, "EX05B", "LocalTcpService.SendingThreadWorking", AppDecorator.Log.MessageType.Error, netProcessId: netProcessId,
											extraMsg: $@"Error Found; {ex.Message}; MsgObj: NetMessagePack");
							}
						}
					} // ref.to -- if (receivedData != null)
					else if ((sendData != null) && (sendData.OriginalServicePort <= 0))
					{
						Log.LogText(LogChannel, "-", sendData, "EX08", "LocalTcpService.SendingThreadWorking", AppDecorator.Log.MessageType.Error,
							extraMsg: "Unable to read Original Service Port. MsgObj: NetMessagePack");
					}
				} // ref.to -- while (!_flags.IsShuttingDown)
			}
			catch (Exception ex2)
			{
				Log.LogError(LogChannel, "-", ex2, "EX11", "LocalTcpService.SendingThreadWorking");
			}
			///// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
			///// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			byte[] Serialize(Object obj)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					BinaryFormatter bf = new BinaryFormatter();
					bf.Serialize(ms, obj);
					return ms.ToArray();
				}
			}

			NetMessagePack GetNextSendData()
			{
				NetMessagePack retData = null;

				lock (_data.SendDataList)
				{
					if (!_flags.IsShuttingDown)
					{
						if (_data.SendDataList.Count == 0)
						{
							Monitor.Wait(_data.SendDataList, _setting.MaxDataWaitPeriod);
						}
						if (_data.SendDataList.TryDequeue(out retData))
						{
							return retData;
						}
					}
				}
				return null;
			}
		}

		public void ShutdownService()
		{
			Log.LogText(LogChannel, "-", "Start - ShutdownService", "A01", "LocalTcpService.ShutdownService");

			_flags.IsShuttingDown = true;
			
			SendEndingPack();

			Task.Delay(500).Wait();

			lock (_data.SendDataList)
			{
				Monitor.PulseAll(_data.SendDataList);
			}

			lock (_data.ReceivedDataList)
			{
				Monitor.PulseAll(_data.ReceivedDataList);
			}

			_localServiceTcpContext?.Send(state =>
			{
				try
				{
					_tcpLocalService?.Stop();
				}
				catch (Exception ex)
				{
					string errMsg = ex.Message;
				}

				try
				{
					_tcpLocalService?.Server.Close();
				}
				catch (Exception ex)
				{
					string errMsg = ex.Message;
				}

				_tcpLocalService = null;
			}, null);
		}

		public void Dispose()
		{
			_flags.Disposed = true;
			ShutdownService();

			if (OnDataReceived != null)
			{
				Delegate[] delgList = OnDataReceived.GetInvocationList();
				foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
				{
					OnDataReceived -= delg;
				}
			}

			_log = null;
		}
	}
}
