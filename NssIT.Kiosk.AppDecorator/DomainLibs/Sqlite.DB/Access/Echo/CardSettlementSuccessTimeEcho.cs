using NssIT.Kiosk.AppDecorator.Common.Access;
using NssIT.Kiosk.AppDecorator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access.Echo
{
    [Serializable]
    public class CardSettlementSuccessTimeEcho : ITransSuccessEcho, IMessageDuplicate, IDisposable
    {
        public DateTime? LastSettlementSuccessTime { get; private set; }

        ///// Note: Default ctor. is mandatory

        public CardSettlementSuccessTimeEcho Init(DateTime? lastsettlementSuccessTime)
        {
            LastSettlementSuccessTime = lastsettlementSuccessTime;
            return this;
        }

        public dynamic Duplicate()
        {
            return new CardSettlementSuccessTimeEcho().Init(this.LastSettlementSuccessTime);
        }

        public void Dispose()
        {
            LastSettlementSuccessTime = null; ;
        }
    }
}
