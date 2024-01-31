using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.WebAPI.Data
{
	/// <summary>
	/// Class for service to return result.
	/// </summary>
	[Serializable]
	public abstract class BaseServiceResult<T>
	{
		public bool Status { get; set; }
		public IList<string> Messages { get; set; }
		public string Code { get; set; }
		public virtual T Data { get; set; }

		public string MessageString()
		{
			string msgStr = null;
			foreach (string msg in Messages)
				msgStr += $@"{msg}; ";
			return msgStr;
		}

		public void Dispose()
		{
			Data = default;
			Messages = null;
		}
	}
}
