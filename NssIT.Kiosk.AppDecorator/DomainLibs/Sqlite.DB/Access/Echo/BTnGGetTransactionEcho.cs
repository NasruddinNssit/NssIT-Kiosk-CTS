using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.TableEntity;
using NssIT.Kiosk.AppDecorator.Common.Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access.Echo
{
    public class BTnGGetTransactionEcho : ITransSuccessEcho, IDisposable
    {
        public BTnGTransactionEnt SaleTransaction { get; private set; }
        public bool IsRecordFound { get; private set; }
        public BTnGGetTransactionEcho(BTnGTransactionEnt bTnGTransaction, bool isRecordFound)
        {
            if ((isRecordFound) && (bTnGTransaction is null))
                throw new Exception("Invalid Sale Transaction data parameter for the constructor");

            SaleTransaction = bTnGTransaction;
            IsRecordFound = isRecordFound;
        }

        public void Dispose()
        {
            SaleTransaction = null;
        }
    }
}
