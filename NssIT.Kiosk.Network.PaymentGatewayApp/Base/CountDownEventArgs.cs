//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace NssIT.Kiosk.Network.PaymentGatewayApp.Base
//{
//    public class CountDownEventArgs : EventArgs, IDisposable
//    {
//        public string CountDownCode { get; private set; }
//        public int TimeRemainderSec { get; private set; }
//        public CountDownEventArgs(string countDownCode, int timeRemainderSec)
//        {
//            CountDownCode = (string.IsNullOrWhiteSpace(countDownCode)) ? null : countDownCode.Trim();
//            TimeRemainderSec = timeRemainderSec;
//        }

//        public void Dispose()
//        { }
//    }
//}
