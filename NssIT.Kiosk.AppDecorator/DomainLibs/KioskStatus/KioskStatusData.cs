using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus
{
    [Serializable]
    public class KioskStatusData : IDisposable 
    {
        public StatusCheckingGroup CheckingGroup { get; set; }
        public DateTime MachineLocalTime { get; set; }
        public KioskCheckingCode CheckingCode { get; set; }
        public int Status { get; set; }
        public string Remark { get; set; }
        public IStatusRemark RemarkObj { get; set; }
        public IStatusLocalExtension StatusExtension { get; set; }

        public void Dispose()
        {
            RemarkObj = null;
            StatusExtension = null;
        }

        public KioskStatusData Duplicate()
        {
            return new KioskStatusData()
            {
                CheckingGroup = this.CheckingGroup, 
                MachineLocalTime = this.MachineLocalTime, 
                CheckingCode = this.CheckingCode, 
                Remark = this.Remark, 
                RemarkObj = this.RemarkObj?.Duplicate(), 
                Status = this.Status,
                StatusExtension = this.StatusExtension?.Duplicate()
            };
        }
    }

    public class KioskStatusDataUniqueTime
    {
        private static DateTime _lastUniqueTime = DateTime.MinValue;
        private static object _lock = new object();

        public static DateTime GetNewMachineLocalTime()
        {
            DateTime retTime = DateTime.Now;

            Thread tWork = new Thread(new ThreadStart(new Action(() => 
            {
                lock(_lock)
                {
                    retTime = DateTime.Now;
                    if (_lastUniqueTime.Ticks == retTime.Ticks)
                    {
                        Thread.Sleep(new TimeSpan(1));
                        retTime = DateTime.Now;
                    }
                    _lastUniqueTime = retTime;
                }
            })));
            tWork.IsBackground = true;
            tWork.Priority = ThreadPriority.AboveNormal;
            tWork.Start();
            tWork.Join();

            return retTime;
        }
    }
}