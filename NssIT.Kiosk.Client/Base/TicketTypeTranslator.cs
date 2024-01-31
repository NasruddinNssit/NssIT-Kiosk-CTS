using NssIT.Kiosk.AppDecorator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base
{
    public class TicketTypeTranslator
    {
        public static string GetTypeDescription(LanguageCode language, string code)
        {
            code = string.IsNullOrWhiteSpace(code) ? "?" : code.ToUpper().Trim();
            if (language == LanguageCode.Malay)
            {
                if (code.Equals("A", StringComparison.InvariantCultureIgnoreCase))
                    return "Dewasa";

                else if (code.Equals("C", StringComparison.InvariantCultureIgnoreCase))
                    return "Kanak-kanak";

                else if (code.Equals("O", StringComparison.InvariantCultureIgnoreCase))
                    return "D";

                else if (code.Equals("S", StringComparison.InvariantCultureIgnoreCase))
                    return "Warga Emas";

                else if (code.Equals("D", StringComparison.InvariantCultureIgnoreCase))
                    return "Pemandu";

                else if (code.Equals("F", StringComparison.InvariantCultureIgnoreCase))
                    return "Kakitangan";

                else if (code.Equals("T", StringComparison.InvariantCultureIgnoreCase))
                    return "Lain-lain ";
            }
            else
            {
                if (code.Equals("A", StringComparison.InvariantCultureIgnoreCase))
                    return "Adult";

                else if (code.Equals("C", StringComparison.InvariantCultureIgnoreCase))
                    return "Child";

                else if (code.Equals("O", StringComparison.InvariantCultureIgnoreCase))
                    return "D";

                else if (code.Equals("S", StringComparison.InvariantCultureIgnoreCase))
                    return "Senior";

                else if (code.Equals("D", StringComparison.InvariantCultureIgnoreCase))
                    return "Driver";

                else if (code.Equals("F", StringComparison.InvariantCultureIgnoreCase))
                    return "Staff";

                else if (code.Equals("T", StringComparison.InvariantCultureIgnoreCase))
                    return "Others";
            }

            return "Error";
        }
    }
}
