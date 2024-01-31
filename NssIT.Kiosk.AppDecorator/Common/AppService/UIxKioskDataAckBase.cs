using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
    /// <summary>
    /// Interface kiosk data sending from server to client
    /// </summary>
    [Serializable]
    public abstract class UIxKioskDataAckBase
    {
        public Guid? BaseRefNetProcessId { get; protected set; }
        public string BaseProcessId { get; protected set; }
        public string BaseThisDataType { get; protected set; }

        public UIxKioskDataAckBase()
        {
            BaseThisDataType = this.GetType().Name;
        }
    }
}