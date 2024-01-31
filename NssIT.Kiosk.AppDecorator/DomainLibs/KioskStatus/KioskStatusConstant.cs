using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus
{
    public enum KioskCommonStatus
    {
        No = 0,
        Yes = 1
    }

    public enum KioskCashStatus
    {
        Out = 0,
        High = 1,
        Low = 2
    }

    public enum KioskCoinStatus
    {
        Out = 0,
        High = 1,
    }

    public enum KioskStatusRemarkType
    {
        String = 0,
        JSon = 1
    }

    public enum StatusCheckingGroup
    {
        /// <summary>
        /// Like UI Screen 
        /// </summary>
        BasicKioskClient = 0,
        Cash = 1,
        CreditCard = 2,
        EWallet = 3,
        Printer = 4
    }

    public enum KioskCheckingCode
    {
        /// <summary>
        /// Related Enum Status is KioskCommonStatus
        /// </summary>
        [Description("Is UI display normal")]
        IsUIDisplayNormal = 1,

        /// <summary>
        /// Related Enum Status is KioskCommonStatus
        /// </summary>
        [Description("Is cash available")]
        IsCashAvailable = 50,

        /// <summary>
        /// Related Enum Status is KioskCommonStatus
        /// </summary>
        [Description("Is cash having error")]
        IsCashError = 51,

        /// <summary>
        /// Related Enum Status is KioskCashStatus
        /// </summary>
        [Description("Cash Cassette1 State")]
        CashCassette1State = 52,

        /// <summary>
        /// Related Enum Status is KioskCashStatus
        /// </summary>
        [Description("Cash Cassette2 State")]
        CashCassette2State = 53,

        /// <summary>
        /// Related Enum Status is KioskCashStatus
        /// </summary>
        [Description("Cash Cassette3 State")]
        CashCassette3State = 54,

        /// <summary>
        /// Related Enum Status is KioskCoinStatus
        /// </summary>
        [Description("Coin Cassette1 State")]
        CoinCassette1State = 55,

        /// <summary>
        /// Related Enum Status is KioskCoinStatus
        /// </summary>
        [Description("Coin Cassette2 State")]
        CoinCassette2State = 56,

        /// <summary>
        /// Related Enum Status is KioskCoinStatus
        /// </summary>
        [Description("Coin Cassette3 State")]
        CoinCassette3State = 57,

        /// <summary>
        /// Related Enum Status is KioskCommonStatus
        /// </summary>
        [Description("Is Printer Stand By")]
        IsPrinterStandBy = 80,

        /// <summary>
        /// Related Enum Status is KioskCommonStatus
        /// </summary>
        [Description("Is Credit Card Settlement Done")]
        IsCreditCardSettlementDone = 100,
    }

    public static class KioskStatusConstant
    {
        private static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

        public static string GetDescription(this KioskCheckingCode checkingCode)
        {
            return GetEnumDescription(checkingCode);
        }

        public static string GetStatusName(this KioskCheckingCode checkingCode, int statusInt)
        {
            try
            {
                if ((checkingCode == KioskCheckingCode.IsUIDisplayNormal)
                    ||
                    (checkingCode == KioskCheckingCode.IsCashAvailable)
                    ||
                    (checkingCode == KioskCheckingCode.IsCashError)
                    ||
                    (checkingCode == KioskCheckingCode.IsPrinterStandBy)
                    ||
                    (checkingCode == KioskCheckingCode.IsCreditCardSettlementDone)
                )
                {
                    KioskCommonStatus stt = (KioskCommonStatus)statusInt;

                    return stt.ToString();
                }
                else if ((checkingCode == KioskCheckingCode.CashCassette1State)
                    ||
                    (checkingCode == KioskCheckingCode.CashCassette2State)
                    ||
                    (checkingCode == KioskCheckingCode.CashCassette3State)
                    )
                {
                    KioskCashStatus stt = (KioskCashStatus)statusInt;

                    return stt.ToString();
                }
                else if ((checkingCode == KioskCheckingCode.CoinCassette1State)
                    ||
                    (checkingCode == KioskCheckingCode.CoinCassette2State)
                    ||
                    (checkingCode == KioskCheckingCode.CoinCassette3State)
                    )
                {
                    KioskCoinStatus stt = (KioskCoinStatus)statusInt;

                    return stt.ToString();
                }
            }
            catch (Exception ex) 
            {
                string errMsg = ex.Message;
            }

            return $@"ERR_{checkingCode.ToString()}_{statusInt}";
        }
    }
}