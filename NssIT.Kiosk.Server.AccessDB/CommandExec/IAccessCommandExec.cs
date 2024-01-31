using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
	public interface IAccessCommandExec
	{
		AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack);
	}
}
