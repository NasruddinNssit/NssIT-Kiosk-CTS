using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.PaymentGatewayApp.CustomApp
{
    public enum AppInstructionCode
    {
        Unknown = 0,

        //PaymentGateway
        SendPaymentGatewayListResult,

        DoSampleJob = 1_000_000
    }
}