using NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Command;
using NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi.LowCoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
{
	public interface IAxCX2_3 : IDisposable 
	{
		string CurrentProcessId { get; set;  }

		LowCoinManager LowCoinMan { get; }

		bool IsRecoveryInProgressAfterDispense { get; }

		bool ConnectDevice();
		
		void CloseDevice();

		void RebootRecoverSequence(bool spinMotor = true);

		bool CheckDispensePossibility(out string lowCoinMsg, out string machOutOfSvcMsg,
			out bool isRunningLowCoin10, out bool isRunningLowCoin20, out bool isRunningLowCoin50);

		string GetDispenseCode(int totalDispenseCents, out string notEnoughCoinMessage);

		bool DispenseCoin(int totalDispenseCents, out string notEnoughCoinMessage);

		bool CheckLowCoinStatus(out bool isRunningLowCoin10, out bool isRunningLowCoin20, out bool isRunningLowCoin50);

		bool SpinMachineMotor(int spinMotorCount, string tag = "");

		bool SpinDispenseByBin(CX2D3SpinIndex binIndex);
	}
}
