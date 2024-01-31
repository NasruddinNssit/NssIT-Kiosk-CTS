using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common
{
	public class Common2s
	{
		public static string BytesToNumbersStr(byte[] data)
		{
			int inx = 0;
			string retStr = "";
			foreach (byte aByte in data)
				retStr += $@"{inx++}: {((int)aByte).ToString()};";

			return retStr;
		}
	}
}
