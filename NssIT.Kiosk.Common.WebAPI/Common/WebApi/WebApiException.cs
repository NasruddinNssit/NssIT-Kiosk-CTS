using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Common.WebApi
{

    /// <summary>
    /// Use to handle unexpected result return from Web API.
    /// </summary>
    public class WebApiException : Exception, iWebApiResult, IDisposable
    {
        public bool Status { get; private set; } = false;
        public IList<string> Messages { get; set; } = null;
        public string Code { get; private set; }
        public Exception Error { get; private set; } = null;

        public bool AllowRetry { get; private set; } = false;

        /// <summary>
        /// Use to handle unexpected result return from Web API.
        /// </summary>
        public WebApiException(Exception ex, string ccode = "99", bool allowRetry = false)
        {
            Messages = new List<string>(new string[] { $@"Error(X) : {ex.Message}" });
            Error = new Exception(MessageString(), ex);
            Code = ccode;
            AllowRetry = allowRetry;
        }

        /// <summary>
        /// Use to handle unexpected result return from Web API.
        /// </summary>
        public WebApiException(string errorMsg, string ccode = "99", bool allowRetry = false)
        {
            Messages = new List<string>(new string[] { (string.IsNullOrWhiteSpace(errorMsg) ? "Error accessing web server" : errorMsg.Trim()) });
            Error = new Exception(MessageString());
            Code = ccode;
            AllowRetry = allowRetry;
        }

        /// <summary>
        /// Use to handle unexpected result return from Web API.
        /// </summary>
        public WebApiException(IList<string> msgList, string ccode = "99", bool allowRetry = false)
        {
            if (msgList?.Count > 0)
                Messages = msgList;
            else
                Messages = new List<string>(new string[] {"Error accessing web server"});

            Error = new Exception(MessageString());
            Code = ccode;
            AllowRetry = allowRetry;
        }

        public string MessageString()
        {
            string msgStr = null;
            foreach (string msg in Messages)
                msgStr += $@"{msg}; ";
            return msgStr;
        }

        public void Dispose()
        {
            Messages = null;
            Error = null;
        }
    }
}
