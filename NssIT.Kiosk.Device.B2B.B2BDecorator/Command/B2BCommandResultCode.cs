using System;
using NssIT.Kiosk.AppDecorator.Devices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.B2B.B2BDecorator.Command
{
	public enum B2BCommandResultCode
	{
		Unknown = DeviceProgressStatus.UnknownState,
		Success = DeviceProgressStatus.Success,
		Fail = DeviceProgressStatus.Fail
	}
}
