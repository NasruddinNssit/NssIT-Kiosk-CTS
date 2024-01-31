using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
    /// <summary>
    /// abstract Data Group Generics type; This is use to devide Ack data into many group
    /// ClassCode:EXIT01.01
    /// </summary>
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [Serializable]
    public abstract class UIxGnAckBase<TData> : UIxKioskDataAckBase, IUIxGenericData
    {
        // IUIxGenericData
        public Exception Error { get; private set; }
        public bool IsDataReadSuccess { get; private set; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public TData Data { get; private set; }

        /// <summary>
        /// FuncCode:EXIT01.0101
        /// </summary>
        /// <param name="refNetProcessId"></param>
        /// <param name="processId"></param>
        /// <param name="data"></param>
        public UIxGnAckBase(Guid? refNetProcessId, string processId, TData data)
            :base()
        {
            if (data == null)
                throw new Exception($@"Invalid parameter occur in UIxData constructor's parameter; (EXIT01.0101.X01);Type {typeof(TData).Name} is expected");

            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            //-------------------------------------
            Data = data;
            IsDataReadSuccess = true;
            Error = null;
        }

        /// <summary>
        /// FuncCode:EXIT01.0102
        /// </summary>
        /// <param name="refNetProcessId"></param>
        /// <param name="processId"></param>
        /// <param name="err"></param>
        public UIxGnAckBase(Guid? refNetProcessId, string processId, Exception err)
            : base()
        {
            if (err is null)
                throw new Exception($@"Unknown error exception occur in UIxData constructor's parameter; (EXIT01.0102.X01);Type {typeof(TData).Name} is expected");

            BaseRefNetProcessId = refNetProcessId;
            BaseProcessId = processId;
            //-------------------------------------
            Error = err;
            IsDataReadSuccess = false;
            Data = default;
        }        
    }
}