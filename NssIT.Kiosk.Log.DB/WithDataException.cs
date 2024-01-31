using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Log.DB
{
    public class WithDataException : Exception
    {
        public string ExceptionType => "WithDataException";
        public object DataX { get; private set; }
        public string DataXType { get; private set; }
        public WithDataException(string errorMsg, Exception exx, object data)
            : base(errorMsg, exx)
        {
            DataX = data;
            DataXType = data?.GetType()?.ToString();
        }
    }
}
