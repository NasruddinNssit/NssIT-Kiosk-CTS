using NssIT.Kiosk.Common.WebAPI.Common.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data.Response
{
    [Serializable]
    public class UpdateKioskStatusResult<T> : BaseServiceResult<T>, iWebApiResult
        where T : BaseCommonObj
    {
        public override T Data { get; set; }

        public UpdateKioskStatusResult() { }
    }
}