using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Log.DB.StatusMonitor
{
    public class StatusHub
    {
		private const int _maxDataKeepingTimeMinutes = 5;

		private ConcurrentDictionary<Guid, KioskStatusData> _statusDataList = new ConcurrentDictionary<Guid, KioskStatusData>();
		private object _listLock = new object();
		private DateTime _nextClearListTime = DateTime.Now.AddMinutes(_maxDataKeepingTimeMinutes);

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static StatusHub _statusHub = null;

		public static StatusHub GetStatusHub()
		{
			if (_statusHub == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_statusHub == null)
					{
						_statusHub = new StatusHub();
					}
					return _statusHub;
				}
				finally
				{
					if (_manLock.CurrentCount == 0)
						_manLock.Release();
				}
			}
			else
				return _statusHub;
		}

		private StatusHub()
		{
			//Init();
		}

		public void LogStatus(KioskStatusData statusData)
        {
			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				// Note : Below work flow only used to insert values to "_logList" only. No additional process is allowed.
				//        Doing extra process may cause too many threads hang in the system. Therefore, additional process on following block of code is prohibited.
				if (_disposed == false)
				{
					try
					{
						//------------------------------------------------------------
						// Note : Below procedures may take longest time when locking
						lock (_listLock)
						{
							CleanUpHistory();

							if (statusData.Remark?.Length > 7995)
                            {
								KioskStatusData stt = statusData.Duplicate();
								stt.Remark = statusData.Remark.Substring(0, 7995);

								_statusDataList.TryAdd(Guid.NewGuid(), stt);
							}
							else
                            {
								_statusDataList.TryAdd(Guid.NewGuid(), statusData);
							}
						}
						//------------------------------------------------------------
					}
					// Used to handle "_logList" is null after disposed
					catch (Exception ex2)
					{
						string byPassStr = ex2.Message;
					}
				}
			})));
			execThread.IsBackground = true;
			execThread.Start();

			return;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			void CleanUpHistory()
            {
				if (_nextClearListTime.Ticks < DateTime.Now.Ticks)
				{
					DateTime lastHistoryTime = DateTime.Now.AddMinutes(_maxDataKeepingTimeMinutes * -1);

					Guid[] dataIdArr = (from keyPair in _statusDataList
												   where ((keyPair.Value is null) || (keyPair.Value?.MachineLocalTime.Ticks <= lastHistoryTime.Ticks))
												   select keyPair.Key).ToArray();

					foreach (Guid dataId in dataIdArr)
					{
						_statusDataList.TryRemove(dataId, out KioskStatusData dat);
						dat.Dispose();
					}

					_nextClearListTime = DateTime.Now.AddMinutes(_maxDataKeepingTimeMinutes);
				}
			}
		}

		private TimeSpan _maxWaitPeriod = new TimeSpan(0, 0, 1);
		/// <summary>
		/// Execute with maximum 1 second waiting period.
		/// </summary>
		/// <returns></returns>
		public KioskStatusData GetNextStatusInfo()
		{
			KioskStatusData retLogInfo = null;
			bool logFound = false;

			Thread execThread = new Thread(new ThreadStart(new Action(() =>
			{
				if (_disposed == false)
				{
					try
					{
						lock (_listLock)
						{
							if (_statusDataList.Count > 0)
                            {
								Guid? earliestKey = null;

                                try
                                {
									// Get the earliest data from list
									earliestKey = _statusDataList.OrderBy(p => p.Value.MachineLocalTime).ToArray()[0].Key;
									// -- -- -- -- -- -- 

									if (earliestKey.HasValue)
									{
										logFound = _statusDataList.TryRemove(earliestKey.Value, out retLogInfo);
									}
								}
								catch 
                                { /* By Pass */ }
							}
						}
					}
					// Used to handle "_logList" is null after disposed
					catch (Exception ex) { string byPassStr = ex.Message; }
				}
			})));
			
			execThread.IsBackground = true;
			execThread.Priority = ThreadPriority.AboveNormal;
			execThread.Start();
			execThread.Join();

			if (logFound)
				return retLogInfo;
			else
				return null;
		}

        //public KioskStatusData wNewStatus_IsUIDisplayNormal(KioskCommonStatus status, string remark)
        //{
        //    KioskStatusData stt = new KioskStatusData()
        //    {
        //        CheckingGroup = StatusCheckingGroup.BasicKioskClient,
        //        CheckingCode = KioskCheckingCode.IsUIDisplayNormal,
        //        Remark = (string.IsNullOrWhiteSpace(remark)) ? null : remark.Trim(),
        //        Status = (int)status
        //    };

        //    stt.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();

        //    return stt;
        //}

        public void zNewStatus_IsUIDisplayNormal(KioskCommonStatus status, string remark)
		{
			KioskStatusData stt = new KioskStatusData()
			{
				CheckingGroup = StatusCheckingGroup.BasicKioskClient,
				CheckingCode = KioskCheckingCode.IsUIDisplayNormal,
				Remark = (string.IsNullOrWhiteSpace(remark)) ? null : remark.Trim(),
				Status = (int)status
			};
			stt.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();
			LogStatus(stt);
		}

		public void zNewStatus_IsCreditCardSettlementDone(KioskCommonStatus status, string remark)
		{
			KioskStatusData stt = new KioskStatusData()
			{
				CheckingGroup = StatusCheckingGroup.CreditCard,
				CheckingCode = KioskCheckingCode.IsCreditCardSettlementDone,
				Remark = (string.IsNullOrWhiteSpace(remark)) ? null : remark.Trim(),
				Status = (int)status
			};
			stt.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();
			LogStatus(stt);
		}

		public void zNewStatus_IsCardMachineDataCommNormal(KioskCommonStatus status, string remark)
		{
			KioskStatusData stt = new KioskStatusData()
			{
				CheckingGroup = StatusCheckingGroup.CreditCard,
				CheckingCode = KioskCheckingCode.IsCardMachineDataCommNormal,
				Remark = (string.IsNullOrWhiteSpace(remark)) ? null : remark.Trim(),
				Status = (int)status
			};
			stt.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();
			LogStatus(stt);
		}

		public void zNewStatus_IsPrinterStandBy(KioskCommonStatus status, string remark)
		{
			KioskStatusData stt = new KioskStatusData()
			{
				CheckingGroup = StatusCheckingGroup.Printer,
				CheckingCode = KioskCheckingCode.IsPrinterStandBy,
				Remark = (string.IsNullOrWhiteSpace(remark)) ? null : remark.Trim(),
				Status = (int)status
			};
			stt.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();
			LogStatus(stt);
		}

		private bool _disposed = false;
		public void Dispose()
		{
			if (_disposed == false)
			{
				_disposed = true;

				try
				{
					lock (_listLock)
					{
						_statusDataList.Clear();
					}
				}
				catch { }
			}
		}
	}

}