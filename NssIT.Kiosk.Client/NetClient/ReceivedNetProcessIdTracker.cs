using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.NetClient
{
    public class ReceivedNetProcessIdTracker
    {
        private object _conCurrLock = new object();

        private (int ClearNetProcessIdListIntervalSec, DateTime NextClearListTime, ConcurrentDictionary<Guid, ResponseData> NetProcessIdList) _receivedNetProcess
            = (ClearNetProcessIdListIntervalSec: 90, NextClearListTime: DateTime.Now, NetProcessIdList: new ConcurrentDictionary<Guid, ResponseData>());

        public ReceivedNetProcessIdTracker() { }

        public void AddNetProcessId(Guid netProcessId, IKioskMsg kioskMsgResponded)
        {
            lock (_conCurrLock)
            {
                // Clear NetProcessIdList -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
                if (_receivedNetProcess.NextClearListTime.Ticks < DateTime.Now.Ticks)
                {
                    DateTime lastHistoryTime = DateTime.Now.AddSeconds(_receivedNetProcess.ClearNetProcessIdListIntervalSec * -1);

                    Guid[] clearNetProcessIdArr = (from keyPair in _receivedNetProcess.NetProcessIdList
                                                   where (keyPair.Value.ReceivedTime.Ticks <= lastHistoryTime.Ticks)
                                                   select keyPair.Key).ToArray();

                    foreach (Guid netProcId in clearNetProcessIdArr)
                    {
                        _receivedNetProcess.NetProcessIdList.TryRemove(netProcId, out ResponseData resp);
                        resp.Dispose();
                    }

                    _receivedNetProcess.NextClearListTime = DateTime.Now.AddSeconds(_receivedNetProcess.ClearNetProcessIdListIntervalSec);
                }
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

                if (_receivedNetProcess.NetProcessIdList.TryGetValue(netProcessId, out _))
                {
                    _receivedNetProcess.NetProcessIdList.TryRemove(netProcessId, out ResponseData resp2);
                    resp2.Dispose();
                }

                try
                {
                    _receivedNetProcess.NetProcessIdList.TryAdd(netProcessId, new ResponseData() { KioskData = kioskMsgResponded, ReceivedTime = DateTime.Now });
                }
                catch { }
            }
        }

        /// <summary>
        /// Return true if NetProcessId has responded.
        /// </summary>
        /// <param name="netProcessId"></param>
        /// <returns></returns>
        public bool CheckReceivedResponded(Guid netProcessId, out IKioskMsg kioskMsg)
        {
            bool retFound = false;
            kioskMsg = null;
            ResponseData rData = null;

            Thread execThread = new Thread(new ThreadStart(new Action(() =>
            {
                lock (_conCurrLock)
                {
                    if (_receivedNetProcess.NetProcessIdList.TryGetValue(netProcessId, out ResponseData responsedData))
                    {
                        retFound = true;
                        rData = responsedData;
                    }
                }
            })));
            execThread.IsBackground = true;
            execThread.Start();
            execThread.Join(5 * 1000);

            if ((retFound) && ((rData?.KioskData) != null))
            {
                kioskMsg = rData.KioskData;
                return true;
            }
            else
                return false;
        }

        class ResponseData : IDisposable
        {
            public DateTime ReceivedTime { get; set; }
            public IKioskMsg KioskData { get; set; }

            public void Dispose()
            {
                KioskData = null;
            }
        }
    }
}
