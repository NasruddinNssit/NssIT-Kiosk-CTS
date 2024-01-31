using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Network
{
	[Serializable]
	public class NetMessagePack : IDisposable
	{
		public Guid NetProcessId { get; private set; } = Guid.Empty;
		public AppModule Module { get; set; } = AppModule.Unknown;
		public CommInstruction Instruction { get; set; } = CommInstruction.Blank;

		public string ErrorMessage { get; set; } = null;

		//public CommunicationMedium MediaCommunucation { get; set; } = CommunicationMedium.TCP;

		// For TCP
		public int OriginalServicePort { get; set; } = -1;
		public int DestinationPort { get; set; } = -1;

		// For SignalR
		//public string OriginalServiceURL { get; set; } = null;
		//public string DestinationURL { get; set; } = null;

		public IKioskMsg MsgObject { get; set; } = null;

		///// xxxxxxxxxxxxxxxxxxx Future Enhancement xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		///// <summary>
		///// Bundle multiple Message Pack in to one group
		///// </summary>
		//public Guid BundlePackId { get; set; } = Guid.Empty;
		///// <summary>
		///// Index of a message pack in a bundle group.
		///// </summary>
		//public int PackIndex { get; set; } = 0;
		//public int TotalMsgPack { get; set; } = 1;
		///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string ModuleDesc
		{
			get
			{
				return Enum.GetName(typeof(AppModule), Module);
			}
		}

		public string InstructionDesc
		{
			get
			{
				return Enum.GetName(typeof(CommInstruction), Instruction);
			}
		}

		/// <summary>
		/// Constructor used only for testing
		/// </summary>
		public NetMessagePack(Guid netProcessId)
		{
			NetProcessId = netProcessId;
		}

		public NetMessagePack(IKioskMsg msgObject)
		{
			MsgObject = msgObject;
			Module = msgObject.Module;
			Instruction = msgObject.Instruction;
			NetProcessId = msgObject.RefNetProcessId ?? Guid.Empty;
			ErrorMessage = msgObject.ErrorMessage;
		}

		public void Dispose()
		{
			MsgObject = null;
		}
	}
}
