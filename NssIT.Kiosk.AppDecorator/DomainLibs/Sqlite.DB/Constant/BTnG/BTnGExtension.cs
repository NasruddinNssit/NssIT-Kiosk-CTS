using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG
{
    public static class BTnGExtension
    {
        public static BTnGHeaderStatus ToBTnGHeaderStatus(this string statusString)
        {
            if (Enum.TryParse<BTnGHeaderStatus>(statusString, true, out BTnGHeaderStatus tStatus) == true)
                return tStatus;
            else
                throw new Exception($@"Unknown BTnG Header Status ({statusString})");
        }

        public static BTnGDetailStatus ToBTnGDetailStatus(this string statusString)
        {
            if (string.IsNullOrWhiteSpace(statusString))
                throw new Exception($@"Invalid BTnG Detail Status with empty value");
            else if (Enum.TryParse<BTnGDetailStatus>(statusString, true, out BTnGDetailStatus tStatus) == true)
                return tStatus;
            else
                return BTnGDetailStatus.unknown_fail_status;
        }

        //public static string ToBTnGText(this BTnGHeaderStatus thisBTnGHeaderStatus)
        //{
        //    return Enum.GetName(typeof(BTnGHeaderStatus), thisBTnGHeaderStatus);
        //}

        //public static string ToBTnGText(this BTnGDetailStatus thisBTnGDetailStatus)
        //{
        //    if (thisBTnGDetailStatus == BTnGDetailStatus.@new)
        //        return "new";
        //    return Enum.GetName(typeof(BTnGDetailStatus), thisBTnGDetailStatus);
        //}
    }
}
