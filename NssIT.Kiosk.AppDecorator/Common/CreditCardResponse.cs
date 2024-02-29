using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common
{
    [Serializable]
    public class CreditCardResponse
    {
        /// <summary>
        /// Transaction Date
        /// </summary>
        public DateTime trdt { get; set; }
        /// <summary>
        /// Host Number
        /// </summary>
        public string hsno { get; set; }
        /// <summary>
        /// Merchant ID
        /// </summary>
        public string mid { get; set; }
        /// <summary>
        /// Response Message/Code
        /// </summary>
        public string rmsg { get; set; }
        /// <summary>
        /// Card Number
        /// </summary>
        public string cdno { get; set; }
        /// <summary>
        /// Card Holder Name
        /// </summary>
        public string cdnm { get; set; }
        /// <summary>
        /// Card Type
        /// </summary>
        public string cdty { get; set; }
        /// <summary>
        /// Status Code
        /// </summary>
        public string stcd { get; set; }
        /// <summary>
        /// Additinal Data / Ticket Batch Number / Booking Number
        /// </summary>
        public string adat { get; set; }
        /// <summary>
        /// Batch Number that refer to Host Number
        /// </summary>
        public string bcno { get; set; }
        /// <summary>
        /// Transaction Trace Number
        /// </summary>
        public string ttce { get; set; }
        /// <summary>
        /// RRN - Retrieval Reference Number
        /// </summary>
        public string rrn { get; set; }
        /// <summary>
        /// Approval Code
        /// </summary>
        public string apvc { get; set; }
        /// <summary>
        /// EMV Application ID
        /// </summary>
        public string aid { get; set; }
        /// <summary>
        /// Transaction Cryptogram
        /// </summary>
        public string trcy { get; set; }
        /// <summary>
        /// Currency Amount
        /// </summary>
        public decimal? camt { get; set; }
        /// <summary>
        /// Terminal Id
        /// </summary>
        public string tid { get; set; }
        /// <summary>
        /// Batch Count
        /// </summary>
        public string btct { get; set; }
        /// <summary>
        /// Batch Currency Amount
        /// </summary>
        public decimal? bcam { get; set; }
        /// <summary>
        /// Machine Id
        /// </summary>
        public string mcid { get; set; }
        /// <summary>
        /// Error Message
        /// </summary>
        public string erms { get; set; }
        /// <summary>
        /// Settlement Transaction Id
        /// </summary>
        public long stmTrid { get; set; }
        /// <summary>
        /// Settlement Status Code
        /// </summary>
        public string stmStcd { get; set; }

        public string CardToken { get; set; } 
    }
}
