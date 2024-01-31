using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
    /// <summary>
    /// BTnG Data Group for Generics type; Used to handle BTnG data type
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [Serializable]
    public class UIxGnBTnGAck<TData> : UIxGnAckBase<TData>
        where TData : new()
    {
        public UIxGnBTnGAck(Guid? refNetProcessId, string processId, TData data)
            : base(refNetProcessId, processId, data)
        {
            BaseThisDataType = $@"UIxGnBTnGAck<{(typeof(TData)).Name}>";
        }

        public UIxGnBTnGAck(Guid? refNetProcessId, string processId, Exception err)
            : base(refNetProcessId, processId, err)
        {
            BaseThisDataType = $@"UIxGnBTnGAck<{(typeof(TData)).Name}>";
        }
    }
}