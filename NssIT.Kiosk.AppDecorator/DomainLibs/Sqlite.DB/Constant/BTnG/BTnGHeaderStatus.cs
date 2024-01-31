using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG
{
    public enum BTnGHeaderStatus
    {
        /// <summary>
        /// Status Type : In Progress
        /// </summary>
        NEW = 0,

        /// <summary>
        /// Status Type : Final
        /// </summary>
        SUCCESS = 1,

        /// <summary>
        /// Status Type : Final
        /// </summary>
        FAIL = 2,

        /// <summary>
        /// Status Type : Final
        /// </summary>
        CANCEL = 3,

        /// <summary>
        /// Status Type : In Progress
        /// </summary>
        CANCEL_REQUEST = 4,

        /// <summary>
        /// Status Type : In Progress
        /// </summary>
        PENDING = 5

        /// <summary>
        /// Used to log error exception in WebAPI; Status Type : -;
        /// Note : "Do not remove even this state is remarked"
        /// </summary>
        //ERROR_LOG = 6
    }


}