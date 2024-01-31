using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class EndOfMyKadScanEventArgs : EventArgs, IDisposable
    {
        private PassengerIdentity _pssgId = null;

        public EndOfMyKadScanEventArgs(PassengerIdentity passengerIdentity)
        {
            _pssgId = passengerIdentity;
        }

        public PassengerIdentity Identity
        {
            get => _pssgId;
        }

        public void Dispose()
        {
            _pssgId = null;
        }


    }
}
