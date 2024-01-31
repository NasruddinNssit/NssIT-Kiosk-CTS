using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG
{
    public enum BTnGKioskVoidTransactionState
    {
        Error = BTnGDetailStatus.m_machine_error,
        Timeout = BTnGDetailStatus.m_machine_timeout,
        CancelRefundRequest = BTnGDetailStatus.m_machine_cancel_refund_request,
    }
}
