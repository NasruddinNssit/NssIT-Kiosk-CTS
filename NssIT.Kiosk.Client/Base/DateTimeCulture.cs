using NssIT.Kiosk.AppDecorator.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base
{
    public class DateTimeCulture
    {
        private static string culInMalay = "ms-MY";
        private static CultureInfo providerMalay = new CultureInfo(culInMalay);

        private static string culInEnglish = "en-US";
        private static CultureInfo providerEnglish = new CultureInfo(culInEnglish);

        public static string GetCultureString(DateTime dateTime, string format, LanguageCode languageCode)
        {
            if (languageCode == LanguageCode.Malay)
            {
                return $@"{dateTime.ToString(format, providerMalay)}";
            }
            else
            {
                return $@"{dateTime.ToString(format, providerEnglish)}";
            }
        }
    }
}
