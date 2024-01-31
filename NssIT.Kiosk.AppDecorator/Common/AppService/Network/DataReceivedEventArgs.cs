using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Network
{
	public class DataReceivedEventArgs : EventArgs, IDisposable
	{
		public DataReceivedEventArgs(NetMessagePack receivedData)
		{
			ReceivedData = receivedData;
		}

		public NetMessagePack ReceivedData { get; private set; }

		public void Dispose()
		{
			ReceivedData = null;
		}
	}
}
