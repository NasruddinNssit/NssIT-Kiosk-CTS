using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using System;

namespace NssIT.Kiosk.Server.AccessDB.AxCommand
{
    public interface IAxCommand<TResult> : IAx
        where TResult : UIxKioskDataAckBase
    {
        string ProcessId { get; }
        Guid? NetProcessId { get;  }
        AppCallBackDelg CallBackEvent { get; }
    }
}