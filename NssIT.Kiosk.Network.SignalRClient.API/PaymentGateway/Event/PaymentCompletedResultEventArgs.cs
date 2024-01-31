using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Constant;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Event
{
    /// <summary>
    /// ClassCode:EXIT35.02
    /// </summary>
    public class PaymentCompletedResultEventArgs
    {
        public PaymentResult Result { get; private set; }
        public PaymentReceipt Receipt { get; private set; }
        public Exception Error { get; private set; } = null;

        /// <summary>
        /// Success constructor; FuncCode:EXIT35.0201
        /// </summary>
        public PaymentCompletedResultEventArgs(PaymentReceipt receipt)
        {
            if (receipt is null)
                throw new Exception("Invalid payment receipt instance refer to constructor parameter; (EXIT35.0201.X01)");

            else if (receipt.Status is null)
            {
                Result = PaymentResult.Fail;
                Error = new Exception("Error reading transaction's payment status; (EXIT35.0201.X02)");
                Receipt = receipt;
            }

            else if (receipt.Status.Equals(PaymentStatus.Paid, StringComparison.InvariantCultureIgnoreCase))
            {
                Result = PaymentResult.Success;
                Receipt = receipt;
            }

            else
            {
                Result = PaymentResult.Fail;
                Receipt = receipt;
                Error = new Exception($@"Fail eWallet payment. {PaymentStatus.GetStatusDescription(receipt.Status)}; (EXIT35.0201.X03)");
            }
        }

        /// <summary>
        /// Fail constructor; FuncCode:EXIT35.0202
        /// </summary>
        public PaymentCompletedResultEventArgs(Exception error)
        {
            if ((error is null) || (string.IsNullOrWhiteSpace(error.Message)))
            {
                throw new Exception("Invalid error info. Please create valid exception with error message; (EXIT35.0202.X01)");
            }

            Result = PaymentResult.Fail;
            Error = error;
        }

        /// <summary>
        /// Timeout or Cancel status constructor; FuncCode:EXIT35.0203
        /// </summary>
        public PaymentCompletedResultEventArgs(PaymentResult resultStatus)
        {
            if ((resultStatus == PaymentResult.Timeout) || (resultStatus == PaymentResult.Cancel))
            {
                Result = resultStatus;
            }
            else
                throw new Exception("Only Timeout or Cancel status is allowed in this constructor; (EXITEXIT35.0203.X01)");
        }
    }
}
