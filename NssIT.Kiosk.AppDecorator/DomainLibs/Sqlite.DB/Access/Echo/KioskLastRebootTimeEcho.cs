using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access.Echo
{
    [Serializable]
    public class KioskLastRebootTimeEcho : ITransSuccessEcho, IMessageDuplicate, IDisposable 
    {
        public DateTime? LastRebootTime { get; private set; }

        ///// Note: Default ctor. is mandatory 

        public KioskLastRebootTimeEcho Init(DateTime? lastRebootTime)
        {
            LastRebootTime = lastRebootTime;
            return this;
        }

        public dynamic Duplicate()
        {
            return new KioskLastRebootTimeEcho().Init(this.LastRebootTime);
        }

        public void Dispose()
        {
            LastRebootTime = null; ;
        }
    }
}
