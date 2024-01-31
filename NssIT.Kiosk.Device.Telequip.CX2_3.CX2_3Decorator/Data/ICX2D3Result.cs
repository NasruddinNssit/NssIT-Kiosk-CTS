using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Data
{
	public interface ICX2D3Result : IDisposable
	{
		void Reset();
	}
}
