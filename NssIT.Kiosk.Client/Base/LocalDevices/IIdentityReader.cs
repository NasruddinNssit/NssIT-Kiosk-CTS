using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base.LocalDevices
{
    public interface IIdentityReader
    {
        PassengerIdentity ReadIC(int waitDelaySec = 10);
    }
}
