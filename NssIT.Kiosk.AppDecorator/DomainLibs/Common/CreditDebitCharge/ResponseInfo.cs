using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge
{
    public class ResponseInfo
    {
        public string Tag { get; set; } = "";

        /// <summary>
        ///		DB Column : hsno
        /// </summary>
        public string HostNo { get; set; } = "";

        /// <summary>
        ///		DB Column : mid
        /// </summary>
        public string MID { get; set; } = "";

        /// <summary>
        ///		DB Column : rmsg
        /// </summary>
        public string ResponseMsg { get; set; } = "";

        /// <summary>
        ///		Used as a Refence Number (like qrid). This number refer to the number that send to paywave reader
        ///		DB Column : adat
        /// </summary>
        public string AdditionalData { get; set; } = "";

        /// <summary>
        ///		DB Column : cdno
        /// </summary>
        public string CardNo { get; set; } = "";

        /// <summary>
        ///		DB Column : bcno
        /// </summary>
        public string BatchNumber { get; set; } = "";

        /// <summary>
        ///		DB Column : ttce
        /// </summary>
        public string TransactionTrace { get; set; } = "";

        public DateTime ReportTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 	DB Column : rrn
        /// </summary>
        public string RRN { get; set; } = "";

        /// <summary>
        /// 	DB Column : apvc
        /// </summary>
        public string ApprovalCode { get; set; } = "";

        /// <summary>
        /// 	DB Column : aid
        /// </summary>
        public string AID { get; set; } = "";

        public string Amount { get; set; } = "";

        /// <summary>
        /// 	DB Column : camt
        /// </summary>
        public decimal CurrencyAmount { get; set; } = 0M;

        public string ExpiryDate { get; set; } = "";

        /// <summary>
        /// 	DB Column : stcd
        /// </summary>
        public string StatusCode { get; set; } = "";

        /// <summary>
        /// 	DB Column : tid
        /// </summary>
        public string TID { get; set; } = "";

        /// <summary>
        ///		Transaction Cryptogram
        /// </summary>
        public string TC { get; set; } = "";

        /// <summary>
        /// 	DB Column : cdnm
        /// </summary>
        public string CardholderName { get; set; } = "";

        /// <summary>
        /// 	DB Column : cdty
        /// </summary>
        public string CardType { get; set; } = "";

        /// <summary>
        /// 	DB Column : cdapplbl; Card Application Label
        /// </summary>
        public string CardAppLabel { get; set; } = "";

        /// <02/11/2023 -  Syah>
        public string CardToken { get; set; } = "";
        /// </02/11/2023 -  Syah>


        public string PartnerTrxID { get; set; } = "";
        public string AlipayTrxID { get; set; } = "";
        public string CustomerID { get; set; } = "";
        public string VoidAmount { get; set; } = "";
        public decimal VoidCurrencyAmount { get; set; } = 0M;

        /// <summary>
        /// 	DB Column : btct
        /// </summary>
        public string BatchCount { get; set; } = "";
        public string BatchAmount { get; set; } = "";

        /// <summary>
        /// 	DB Column : bcam
        /// </summary>
        public decimal BatchCurrencyAmount { get; set; } = 0M;

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        // Extra columns for Application purposes.
        /// <summary>
        ///     Extra columns : Used to record multiple document numbers with a separator string like "#/#" or "^".
        /// </summary>
        public string DocumentNumbers { get; set; } = "";

        /// <summary>
        ///		Extra columns : Used to record Extra Message in application
        /// </summary>
        public string MachineId { get; set; } = "";

        /// <summary>
        ///		Extra columns : Used to record Extra Message in application
        /// </summary>
        public string ErrorMsg { get; set; } = "";
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        public string TrimErrorMsg()
        {
            return (ErrorMsg ?? "").Trim();
        }

        public string ToJSonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ResponseInfo Duplicate(ResponseInfo referInfor)
        {
            ResponseInfo dupInfo = new ResponseInfo();

            dupInfo.Tag = referInfor.Tag;
            dupInfo.ResponseMsg = referInfor.ResponseMsg;
            dupInfo.AdditionalData = referInfor.AdditionalData;
            dupInfo.CardNo = referInfor.CardNo;
            dupInfo.ExpiryDate = referInfor.ExpiryDate;
            dupInfo.StatusCode = referInfor.StatusCode;
            dupInfo.ApprovalCode = referInfor.ApprovalCode;
            dupInfo.RRN = referInfor.RRN;
            dupInfo.TransactionTrace = referInfor.TransactionTrace;
            dupInfo.BatchNumber = referInfor.BatchNumber;
            dupInfo.HostNo = referInfor.HostNo;
            dupInfo.TID = referInfor.TID;
            dupInfo.MID = referInfor.MID;
            dupInfo.AID = referInfor.AID;
            dupInfo.TC = referInfor.TC;
            dupInfo.Amount = referInfor.Amount;
            dupInfo.CurrencyAmount = referInfor.CurrencyAmount;
            dupInfo.CardholderName = referInfor.CardholderName;
            dupInfo.CardType = referInfor.CardType;
            dupInfo.PartnerTrxID = referInfor.PartnerTrxID;
            dupInfo.AlipayTrxID = referInfor.AlipayTrxID;
            dupInfo.CustomerID = referInfor.CustomerID;
            dupInfo.VoidAmount = referInfor.VoidAmount;
            dupInfo.VoidCurrencyAmount = referInfor.VoidCurrencyAmount;
            dupInfo.BatchCount = referInfor.BatchCount;
            dupInfo.BatchAmount = referInfor.BatchAmount;
            dupInfo.BatchCurrencyAmount = referInfor.BatchCurrencyAmount;
            dupInfo.ReportTime = referInfor.ReportTime;
            dupInfo.DocumentNumbers = referInfor.DocumentNumbers;
            dupInfo.MachineId = referInfor.MachineId;
            dupInfo.ErrorMsg = referInfor.ErrorMsg;

            return dupInfo;
        }

        public void SetDefaultIfNull()
        {
            this.Tag = this.Tag ?? "";
            this.ResponseMsg = this.ResponseMsg ?? "";
            this.AdditionalData = this.AdditionalData ?? "";
            this.CardNo = this.CardNo ?? "";
            this.ExpiryDate = this.ExpiryDate ?? "";
            this.StatusCode = this.StatusCode ?? "";
            this.ApprovalCode = this.ApprovalCode ?? "";
            this.RRN = this.RRN ?? "";
            this.TransactionTrace = this.TransactionTrace ?? "";
            this.BatchNumber = this.BatchNumber ?? "";
            this.HostNo = this.HostNo ?? "";
            this.TID = this.TID ?? "";
            this.MID = this.MID ?? "";
            this.AID = this.AID ?? "";
            this.TC = this.TC ?? "";
            this.Amount = this.Amount ?? "";
            this.CardholderName = this.CardholderName ?? "";
            this.CardType = this.CardType ?? "";
            this.PartnerTrxID = this.PartnerTrxID ?? "";
            this.AlipayTrxID = this.AlipayTrxID ?? "";
            this.CustomerID = this.CustomerID ?? "";
            this.VoidAmount = this.VoidAmount ?? "";
            this.BatchCount = this.BatchCount ?? "";
            this.BatchAmount = this.BatchAmount ?? "";
            this.DocumentNumbers = this.DocumentNumbers ?? "";
            this.MachineId = this.MachineId ?? "";
            this.ErrorMsg = this.ErrorMsg ?? "";
        }

        public void ResetInfo()
        {
            this.Tag = "";
            this.ResponseMsg = "";
            this.AdditionalData = "";
            this.CardNo = "";
            this.ExpiryDate = "";
            this.StatusCode = "";
            this.ApprovalCode = "";
            this.RRN = "";
            this.TransactionTrace = "";
            this.BatchNumber = "";
            this.HostNo = "";
            this.TID = "";
            this.MID = "";
            this.AID = "";
            this.TC = "";
            this.Amount = "";
            this.CurrencyAmount = 0M;
            this.CardholderName = "";
            this.CardType = "";
            this.PartnerTrxID = "";
            this.AlipayTrxID = "";
            this.CustomerID = "";
            this.VoidAmount = "";
            this.VoidCurrencyAmount = 0M;
            this.BatchCount = "";
            this.BatchAmount = "";
            this.BatchCurrencyAmount = 0M;
            this.ReportTime = DateTime.Now;

            this.DocumentNumbers = "";
            this.MachineId = "";
            this.ErrorMsg = "";
        }
    }
}
