using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Log.DB
{
	public class LogDBManager
	{
		private const string _logDBFolderName = @"LogDB";
		private const string _logDBName = @"NssITKioskLog";
		private const string _masterLogDBFileName = @"NssITKioskLogMaster.db";

		public string _executionFilePath;
		public string _executionFolderPath;

		public LogDBManager()
		{
			_executionFilePath = Assembly.GetExecutingAssembly().Location;

			FileInfo fInf = new FileInfo(_executionFilePath);
			_executionFolderPath = fInf.DirectoryName;

			LogDBFolderPath = $@"{_executionFolderPath}\{_logDBFolderName}";
		}


		public string LogDBFolderPath { get; private set; }

		public string CurrentDBFilePath
		{
			get
			{
				// Check Expired Time
				DateTime expTime = CheckExpiredTime();
				// -- -- -- -- -- -- -- -- -- -- -- --

				string fileFolder = $@"{LogDBFolderPath}\{_logDBPostFixTime.ToString("yyyy")}";
				string filePath = $@"{fileFolder}\{_logDBName}{_logDBPostFixTime.ToString("yyyyMMdd")}.db";

				FileInfo fInf = new FileInfo(filePath);
				if (fInf.Exists == false)
				{
					// Create New Log DB File .
					try
					{
						FileInfo masInf = new FileInfo($@"{LogDBFolderPath}\{_masterLogDBFileName}");

						if (masInf.Exists == false)
						{
							if (ErrorMsg is null) ErrorMsg = "Unable to allocate Log DB Master File";
							filePath = null;
						}
						else
						{
							if (Directory.Exists(fileFolder) == false)
							{
								Directory.CreateDirectory(fileFolder);
							}

							File.Copy(masInf.FullName, filePath);

							FileInfo fInf2 = new FileInfo(filePath);
							if (fInf2.Exists == false)
							{
								if (ErrorMsg is null) ErrorMsg = $@"Unable to Copy Log DB File to Destination ({filePath})";
								filePath = null;
							}
						}
					}
					catch (Exception ex)
					{
						filePath = null;
						if (ErrorMsg is null) ErrorMsg = $@"Error at (CurrentDBFilePath) : ({ex.Message})";
					}
				}

				return filePath;
			}
		}

		public string ErrorMsg { get; private set; } = null;

		private DateTime? _expiredTime = null;
		private DateTime CheckExpiredTime()
		{
			if ((_expiredTime.HasValue == false) || (_expiredTime.Value.Subtract(DateTime.Now).TotalMilliseconds < 0))
			{
				DateTime postFixTime = RefreshLogDBPostFixTime();
				int expYear = 0;
				int expMonth = 0;
				int expDay = 0;

				if (postFixTime.Day > 20)
				{
					DateTime nTime = postFixTime.AddMonths(1);
					expYear = nTime.Year;
					expMonth = nTime.Month;
					expDay = 1;
				}
				else if (postFixTime.Day > 10)
				{
					expYear = postFixTime.Year;
					expMonth = postFixTime.Month;
					expDay = 21;
				}
				else
				{
					expYear = postFixTime.Year;
					expMonth = postFixTime.Month;
					expDay = 11;
				}

				_expiredTime = new DateTime(expYear, expMonth, expDay, 0, 0, 0, 0);
			}
			return _expiredTime.Value;
		}

		private DateTime _logDBPostFixTime;
		private DateTime RefreshLogDBPostFixTime()
		{
			//10 Days for one Log DB file.

			DateTime currTime = DateTime.Now;

			int expYear = 0;
			int expMonth = 0;
			int expDay = 0;

			if (currTime.Day > 20)
			{
				expYear = currTime.Year;
				expMonth = currTime.Month;
				expDay = 21;
			}
			else if (currTime.Day > 10)
			{
				expYear = currTime.Year;
				expMonth = currTime.Month;
				expDay = 11;
			}
			else
			{
				expYear = currTime.Year;
				expMonth = currTime.Month;
				expDay = 1;
			}

			_logDBPostFixTime = new DateTime(expYear, expMonth, expDay, 0, 0, 0, 0);

			return _logDBPostFixTime;
		}

	}
}
