using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
    /// <summary>
    /// Normal Application Data Group for Generics type
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [Serializable]
    public class UIxGnAppAck<TData> : UIxGnAckBase<TData>
        where TData: new()
    {
        public UIxGnAppAck(Guid? refNetProcessId, string processId, TData data)
            : base(refNetProcessId, processId, data)
        {
            BaseThisDataType = $@"UIxGnAppAck<{(typeof(TData)).Name}>";
        }

        public UIxGnAppAck(Guid? refNetProcessId, string processId, Exception err)
            : base(refNetProcessId, processId, err)
        {
            BaseThisDataType = $@"UIxGnAppAck<{(typeof(TData)).Name}>";
        }        
    }
}