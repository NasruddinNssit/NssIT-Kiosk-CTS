using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Data
{
	[Serializable]
	public class B2BCassetteInfoCollection
	{
		private readonly B2BCassetteInfo[] _cassetteInfo = null;

		public B2BCassetteInfoCollection()
		{
			_cassetteInfo = new B2BCassetteInfo[GetNumberOfCassette()];
		}

		public int GetNumberOfCassette()
		{
			return 3;
		}

		public B2BCassetteInfo this[int cassetteIndex]
		{
			get { return (_cassetteInfo[cassetteIndex] ?? (_cassetteInfo[cassetteIndex] = new B2BCassetteInfo())); }
		}
	}
}
