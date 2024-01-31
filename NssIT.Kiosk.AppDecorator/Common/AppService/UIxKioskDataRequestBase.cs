using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
    /// <summary>
    /// Interface (abstract class) kiosk data sending from client to server
    /// </summary>
    [Serializable]
    public abstract class UIxKioskDataRequestBase : INetCommandDirective
    {
        public abstract CommunicationDirection CommuCommandDirection { get; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public Guid? BaseRefNetProcessId => BaseNetProcessId;
        public string BaseProcessId { get; private set; }
        public Guid BaseNetProcessId { get; private set; } = Guid.NewGuid();
        public string BaseThisDataType { get; protected set; }

        public UIxKioskDataRequestBase(string processId)
        {
            BaseProcessId = processId;
            BaseThisDataType = this.GetType().Name;
        }
    }
}