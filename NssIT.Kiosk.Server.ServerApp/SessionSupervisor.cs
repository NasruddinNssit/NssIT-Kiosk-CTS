using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp
{
    /// <summary>
    /// Used to monitor NetProcessId
    /// </summary>
    public class SessionSupervisor
    {
        private List<Guid> _sessionNetProcessIDList = new List<Guid>();
        private object _lock = new object();

        public void AddNetProcessId(Guid netProcessId)
        {
            if (netProcessId.Equals(Guid.Empty))
                return;

            lock(_lock)
            {
                _sessionNetProcessIDList.Add(netProcessId);
            }
        }

        public void CleanNetProcessId()
        {
            lock(_lock)
            {
                _sessionNetProcessIDList.Clear();
            }
        }

        public bool FindNetProcessId(Guid netProcessId)
        {
            if (netProcessId.Equals(Guid.Empty))
                return false;

            lock(_lock)
            {
                if (_sessionNetProcessIDList.Exists(id => id.Equals(netProcessId)))
                    return true;
                return false;
            }
        }
    }
}
