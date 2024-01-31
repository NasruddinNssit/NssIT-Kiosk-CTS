using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Security
{
    public interface IBTnGGuardInfo
    {
        string MerchantId { get; }
        string PublicKey { get; }
        string PrivateKey { get; }
        string WebApiUrl { get; }
        string HashSecretKey { get; }
        string TVMKey { get; }
    }
}