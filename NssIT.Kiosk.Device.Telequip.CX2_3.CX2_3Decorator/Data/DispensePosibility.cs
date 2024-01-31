namespace NssIT.Kiosk.Device.Telequip.CX2_3.CX2D3Decorator.Data
{
	public class DispensePossibility : ICX2D3Result
	{
		public string ProcessId { get; set; } = null;

		public bool GotStatus { get; set; } = false;

		/// <summary>
		/// Indicate coin machine is slowly running out of coin. Like 20 cents is running low.
		/// </summary>
		public string LowCoinMsg { get; set; } = null;
		public string MachineOutOfSvcMsg { get; set; } = null;

		///// <summary>
		///// Lack of a specified coin amount for dispense. Like Dispense 80 cents but lack of 20 cents
		///// </summary>
		//public string LackOfCoinMsg { get; set; } = null;

		public bool IsRunningLowCoin10 { get; set; } = false;
		public bool IsRunningLowCoin20 { get; set; } = false;
		public bool IsRunningLowCoin50 { get; set; } = false;

		//public string DispenseCode { get; set; } = null;

		public bool IsDispensePossible
		{
			get
			{
				return (MachineOutOfSvcMsg == null);
			}
		}

		//public string DispenseCodeDesc
		//{
		//	get
		//	{
		//		if (DispenseCode == null)
		//			return null;
		//		else if (DispenseCode.Length != 8)
		//			return $@"Invalid DispenseCode length ({DispenseCode.Length})";
		//		else
		//		{
		//			int codeLng = DispenseCode.Length;

		//			return $@"10 cents : {DispenseCode.Substring(0, 1)}pcs, 20 cents : {DispenseCode.Substring(1, 1)}pcs, 50 cents : {DispenseCode.Substring(2, 1)}pcs";
		//		}
		//	}
		//}

		public void Reset()
		{
			ProcessId = null;
			GotStatus = false;
			LowCoinMsg = null;
			//LackOfCoinMsg = null;
			//DispenseCode = null;
			MachineOutOfSvcMsg = null;
			IsRunningLowCoin10 = false;
			IsRunningLowCoin20 = false;
			IsRunningLowCoin50 = false;
		}

		public void Dispose()
		{ LowCoinMsg = null;
			MachineOutOfSvcMsg = null; }
	}
}