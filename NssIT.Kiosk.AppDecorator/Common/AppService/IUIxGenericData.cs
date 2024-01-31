using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
    /// <summary>
    /// Normal data grouping Interface used to identify a generic type without having any Generic notation in interface defination
    /// </summary>
    public interface IUIxGenericData
    {
        Exception Error { get; }
        bool IsDataReadSuccess { get; }
    }
}
