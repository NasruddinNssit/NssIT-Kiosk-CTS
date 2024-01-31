using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Config
{
    public enum WebAPISiteCode
    {
        Unknown = 0,
        Live = 1,
        Development = 2,
        Staging = 3,
        Training = 4,
        Local_Host = 100
    }

    public enum VersionPhase
    {
        Development = 0,
        Production = 1
    }
}
