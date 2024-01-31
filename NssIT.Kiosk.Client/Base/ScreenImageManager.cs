using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NssIT.Kiosk.Client.Base
{
	public class ScreenImageManager
	{
		private const string LogChannel = "ImageManager";

		private const string _logDBFolderName = @"ImagesLog";
		private const string _imgType = "png";
		private const int _maxFileStoringDay = 7;

		private string _fileNamePrefix = "";

		public string _executionFilePath;
		public string _executionFolderPath;

		private int _lastDeleteHistoryDate = 0;

		private DbLog Log { get; set; }

		public ScreenImageManager(string fileNamePrefix)
		{
			Log = DbLog.GetDbLog();

			if (string.IsNullOrWhiteSpace(fileNamePrefix) == false)
			{
				_fileNamePrefix = fileNamePrefix.Trim();
			}

			_executionFilePath = Assembly.GetExecutingAssembly().Location;

			FileInfo fInf = new FileInfo(_executionFilePath);
			_executionFolderPath = fInf.DirectoryName;

			ImageFolderPath = $@"{_executionFolderPath}\{_logDBFolderName}";
		}

		private string _imageFolderPath = "";

		public string ImageFolderPath
		{
			get => _imageFolderPath;
			private set
			{
				_imageFolderPath = value;

				try
				{
					if (Directory.Exists(_imageFolderPath) == false)
					{
						Directory.CreateDirectory(_imageFolderPath);
					}
				}
				catch (Exception ex)
				{
					_imageFolderPath = null;
					Log.LogError(LogChannel, "-", ex, "EX01", "ScreenImageManager.ImageFolderPath");
				}
			}
		}

		public void CaptureScreenImage(string postFixName)
		{
			try
			{
				Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
				Graphics graph = Graphics.FromImage(bitmap as Image);
				graph.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
				bitmap.Save(GetNewImageFilePath(postFixName), System.Drawing.Imaging.ImageFormat.Png);
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, "-", ex, "EX01", "ScreenImageManager.CaptureScreenImage");
			}
		}

		private string GetNewImageFilePath(string postFixName)
		{
			postFixName = (string.IsNullOrWhiteSpace(postFixName) == false) ? postFixName.Trim() : "";

			string fileFolder = ImageFolderPath;
			string filePath = $@"{fileFolder}\{_fileNamePrefix}_{DateTime.Now.ToString("yyyyMMddHHmmss")}_{postFixName}.{_imgType}";

			FileInfo fInf = new FileInfo(filePath);
			if (fInf.Exists == false)
			{
				// Create New Log DB File .
				try
				{
					if (Directory.Exists(fileFolder) == false)
					{
						Directory.CreateDirectory(fileFolder);
					}
				}
				catch (Exception ex)
				{
					filePath = null;
					Log.LogError(LogChannel, "-", ex, "EX01", "ScreenImageManager.GetNewImageFilePath");
				}
			}
			return filePath;
		}

		private Thread _deleteHistoryThreadWorker = null;
		public void DeleteHistoryFile()
		{
			int latestDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

			if (_lastDeleteHistoryDate >= latestDate)
				return;

			if (_deleteHistoryThreadWorker != null)
			{
				if (_deleteHistoryThreadWorker.ThreadState.IsState(ThreadState.Stopped) == false)
				{
					try
					{
						_deleteHistoryThreadWorker.Abort();
						Task.Delay(500).Wait();
					}
					catch { }
				}
			}

			_deleteHistoryThreadWorker = new Thread(new ThreadStart(DeleteHistoricalImageFileThreadWorking));
			_deleteHistoryThreadWorker.IsBackground = true;
			_deleteHistoryThreadWorker.Start();
			_lastDeleteHistoryDate = latestDate;

			void DeleteHistoricalImageFileThreadWorking()
			{
				try
				{
					string log = "";
					string[] fileNameArr = Directory.GetFiles(ImageFolderPath);

					if ((fileNameArr.Length > 0))
					{
						DateTime nowTime = DateTime.Now;
						DateTime exp = nowTime.AddDays((_maxFileStoringDay + 2) * -1);
						DateTime expiredTime = new DateTime(exp.Year, exp.Month, exp.Day, 0, 0, 0, 0);
						foreach (string filePath in fileNameArr)
						{
							try
							{
								FileInfo fInf = new FileInfo(filePath);
								if (fInf.LastWriteTime.Ticks < expiredTime.Ticks)
								{
									log += $@"{filePath}; LastWriteTime : {fInf.LastWriteTime:dd MMM yyyy hh:mm:ss tt)}; CreationTime : {fInf.CreationTime:dd MMM yyyy hh:mm:ss tt)}{"\r\n"}";
									File.Delete(filePath);
								}
							}
							catch (Exception ex)
							{
								string msg = ex.Message;
								Log.LogError(LogChannel, "-", ex, "EX01", "ScreenImageManager.DeleteHistoricalImageFileThreadWorking");
							}
						}

						string tt1 = log;
						Log.LogText(LogChannel, "-", log, "LOG_DELETED_FILES", "ScreenImageManager.DeleteHistoricalImageFileThreadWorking");
					}
				}
				catch (ThreadAbortException) { }
				catch (Exception ex2)
				{
					Log.LogError(LogChannel, "-", ex2, "EX02", "ScreenImageManager.DeleteHistoricalImageFileThreadWorking");
				}
			}
		}
	}
}
