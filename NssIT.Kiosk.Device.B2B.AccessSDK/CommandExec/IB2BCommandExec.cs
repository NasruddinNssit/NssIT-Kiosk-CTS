using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.AccessSDK.CommandExec
{
	public interface IB2BCommandExec : IDisposable
	{
		B2BCommandPack Execute(B2BCommandPack commPack);
	}
}
