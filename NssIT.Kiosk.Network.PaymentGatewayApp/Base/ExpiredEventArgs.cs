//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace NssIT.Kiosk.Network.PaymentGatewayApp.Base
//{
//    public class ExpiredEventArgs : EventArgs, IDisposable
//    {
//        public string CountDownCode { get; private set; }
//        public DateTime ExpiredTime { get; private set; }

//        public ExpiredEventArgs(string countDownCode, DateTime expiredTime)
//        {
//            CountDownCode = (string.IsNullOrWhiteSpace(countDownCode)) ? null : countDownCode.Trim();
//            ExpiredTime = expiredTime;
//        }

//        public void Dispose()
//        { }
//    }
//}
