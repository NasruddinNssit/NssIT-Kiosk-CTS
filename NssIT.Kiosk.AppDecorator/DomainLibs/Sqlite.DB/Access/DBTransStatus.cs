using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access
{
    public class DBTransStatus : IDisposable
    {
        public bool IsTransDone { get; private set; } = false;
        public bool IsSuccess { get; private set; } = false;
        public bool IsErrorFound { get; private set; } = false;
        public Exception Error { get; private set; } = null;
        public DateTime TransCreatedTime { get; } = DateTime.Now;
        public DBTransStatus()
        {
            IsTransDone = false;
            IsSuccess = false;
            IsErrorFound = false;
            Error = null;
        }

        public void SetTransSuccess()
        {
            IsTransDone = true;
            IsSuccess = true;
            IsErrorFound = false;
            Error = null;
        }

        public void SetTransFail(Exception ex)
        {
            IsTransDone = true;
            IsSuccess = false;
            IsErrorFound = true;
            Error = ex;
        }

        public void Dispose()
        {
            Error = null;
        }
    }
}
