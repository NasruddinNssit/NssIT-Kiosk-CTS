using System;
using NssIT.Kiosk.AppDecorator.Common.Access;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Sqlite.DB.AccessDB.Base
{
    public interface IDBAxExecution<out Echo>
        where Echo : ITransSuccessEcho
    {
        Guid CommandId { get; }
        Echo SuccessEcho { get; }
        DBTransStatus ResultStatus { get; }
        void DBExecute(DatabaseAx dbAx);
    }
}
