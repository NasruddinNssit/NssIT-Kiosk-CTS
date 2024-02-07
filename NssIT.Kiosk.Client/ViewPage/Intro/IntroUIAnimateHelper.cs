using Microsoft.ReportingServices.Interfaces;
using NssIT.Kiosk.AppDecorator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Xml;

namespace NssIT.Kiosk.Client.ViewPage.Intro
{
	public class IntroUIAnimateHelper : IDisposable
	{
		private const string LogChannel = "ViewPage";

		public event EventHandler<BeginEventArgs> OnIntroBegin;

		private string _cvResetIntroFrameStoryScript = @"<Storyboard x:Key=""CvResetIntroFrameStory"" 
			xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">

            <ThicknessAnimationUsingKeyFrames x:Name=""ResetMovingMarginCollection"" 
                Storyboard.TargetProperty=""Margin"" BeginTime=""0:0:0""
                DecelerationRatio=""0.5"" Storyboard.TargetName=""CvIntroFrame"">
                <LinearThicknessKeyFrame x:Name=""ResetMovingMargin1"" KeyTime=""0:0:0.05"" Value=""-4000, 0, 0, 0"" />
            </ThicknessAnimationUsingKeyFrames>
        </Storyboard>";

		private string _cvIntroFrameStoryScript = @"<Storyboard x:Key=""cvIntroFrameStory"" 
			xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            
            <ThicknessAnimationUsingKeyFrames x:Name=""MovingMarginCollection"" 
                Storyboard.TargetProperty=""Margin"" BeginTime=""0:0:0""
                DecelerationRatio=""0.5"" Storyboard.TargetName=""CvIntroFrame"">
                <LinearThicknessKeyFrame x:Name=""MovingMargin1"" KeyTime=""0:0:1"" Value=""-4000, 0, 0, 0"" />
            </ThicknessAnimationUsingKeyFrames>

        </Storyboard>";

		private double _outFlameMargin = 4000;
		private bool _isPageActive = false;
		private bool _isIntroAnimationRunning = false;

		private Page _pgIntro;
		private Canvas _cvIntroFrame;
		private ScrollViewer _scvIntro;

		private UserControl[] _introPicture = new UserControl[2];
		private UserControl _mostLeftPicture = null;

		public IntroUIAnimateHelper(Page pgIntro, Canvas cvIntroFrame, ScrollViewer scvIntro)
		{
			_pgIntro = pgIntro;
			_cvIntroFrame = cvIntroFrame;
			_scvIntro = scvIntro;

			_cvIntroFrame.Margin = new Thickness((_outFlameMargin * -1), 0, 0, 0);

			_cvIntroFrame.Children.Clear();

			_introPicture[0] = new uscIntroMalay();
			_introPicture[1] = new uscIntroEnglish();

			((uscIntroMalay)_introPicture[0]).OnBegin += Intro_OnBegin;
			((uscIntroEnglish)_introPicture[1]).OnBegin += Intro_OnBegin;

			cvIntroFrame.Children.Add(_introPicture[0]);
			cvIntroFrame.Children.Add(_introPicture[1]);

			Thread worker = new Thread(new ThreadStart(IntroAnimationThreadWorking));
			worker.IsBackground = true;
			worker.Start();
		}

		private bool _allowSelection = true;
		public void SetStartButtonEnabled(bool enabled)
		{
			if (enabled)
				_allowSelection = true;
			else
				_allowSelection = false;

			((uscIntroMalay)_introPicture[0]).SetStartButtonEnabled(enabled);
			((uscIntroEnglish)_introPicture[1]).SetStartButtonEnabled(enabled);
			System.Windows.Forms.Application.DoEvents();
		}

		public void SetOperationState(bool state, DateTime? startTime, DateTime? endTime)
		{
            ((uscIntroMalay)_introPicture[0]).SetOperationState(state, startTime, endTime);
            ((uscIntroEnglish)_introPicture[1]).SetOperationState(state, startTime, endTime);
            System.Windows.Forms.Application.DoEvents();
        }

		private void Intro_OnBegin(object sender, BeginEventArgs e)
		{
			if (_allowSelection == false)
				return;
			try
			{
				AdjustPageSize();
				if (OnIntroBegin != null)
				{
					OnIntroBegin.Invoke(this, e);
				}
				_allowSelection = false;
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception($@"Unhandled error exception; {ex.Message}" , ex), classNMethodName: "IntroUIAnimateHelper:Intro_OnBegin");
			}
		}

		private static int _readVersionCount = 0;
		private Thread _updateVersionThreadWorker = null;
		public void InitOnLoad()
		{
			NssIT.Kiosk.AppDecorator.Config.Setting sett = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

			_isPageActive = true;

			// use Animate to init margin 
			ResetIntroFrame();

			EndVersionThreadWorker();

			_updateVersionThreadWorker = new Thread(new ThreadStart(UpdateVersionThreadWorking));
			_updateVersionThreadWorker.IsBackground = true;
			_updateVersionThreadWorker.Start();

			return;

			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			void UpdateVersionThreadWorking()
			{
				AdjustPageSize();
				if (_readVersionCount > 2)
				{
					if (App.ValidateSystemSetting(out string errMsg) == false)
                    {
						App.MainScreenControl.Alert(detailMsg: (errMsg ?? "Error when Validate System Setting; (EXIT10000032)"));
                    }
					//return;
				}

				_readVersionCount++;

				DateTime endTime = DateTime.Now.AddSeconds(3);

				while ((endTime.Subtract(DateTime.Now).TotalSeconds > 0) && (_isPageActive))
				{
					Thread.Sleep(500);
				}

				if (_isPageActive)
                {
					foreach(var ctrl in _introPicture)
                    {
						if (ctrl is uscIntroMalay mal)
							mal.UpdateVersion();
						else if (ctrl is uscIntroEnglish eng)
							eng.UpdateVersion();
					}

					if (App.ValidateSystemSetting(out string errMsg) == false)
					{
						App.MainScreenControl.Alert(detailMsg: (errMsg ?? "Error when Validate System Setting; (EXIT10000031)"));
					}
				}
			}

			void EndVersionThreadWorker()
            {
				if (_updateVersionThreadWorker != null)
                {
					if ((_updateVersionThreadWorker.ThreadState.IsState(ThreadState.Aborted) == false)
							&& (_updateVersionThreadWorker.ThreadState.IsState(ThreadState.Stopped) == false)
							)
                    {
						try
						{
							_updateVersionThreadWorker.Abort();
							Thread.Sleep(300);
						}
						catch { }
                        finally
                        {
							_updateVersionThreadWorker = null;
						}
                    }
				}
            }
		}

		public void OnPageUnload()
		{
			_isPageActive = false;

			try
			{
				IntroAnimator.Stop();
			}
			catch { }

			try
			{
				ResetIntroFrameAnimator.Stop();
			}
			catch { }
		}

		

		#region -- Story -- ResetIntroFrame --
		private void ResetIntroFrame()
		{
			if (_isPageActive == false)
			{
				_isIntroAnimationRunning = false;
				return;
			}

			Storyboard currStoryboard = ResetIntroFrameAnimator;

			foreach (var obj in currStoryboard.Children)
			{
				if (obj is ThicknessAnimationUsingKeyFrames movMarColl)
					if (movMarColl.Name.Equals("ResetMovingMarginCollection"))
						foreach (var obj2 in movMarColl.KeyFrames)
							if (obj2 is LinearThicknessKeyFrame resMar)
							{
								resMar.Value = new Thickness((_outFlameMargin * -1), 0, 0, 0);
								break;
							}
			}

			_isIntroAnimationRunning = true;
			currStoryboard.Begin(_pgIntro);
		}

		private Storyboard _resetIntroFrameAnimator = null;
		public Storyboard ResetIntroFrameAnimator
		{
			get
			{
				if (_resetIntroFrameAnimator is null)
				{
					MemoryStream storyStream = null;
					StreamReader storyReader = null;
					try
					{
						storyStream = new MemoryStream(Encoding.UTF8.GetBytes(_cvResetIntroFrameStoryScript));
						storyReader = new StreamReader(storyStream);

						storyStream.Seek(0, SeekOrigin.Begin);
						XmlReader xmlReaderRow = XmlReader.Create(storyReader);
						_resetIntroFrameAnimator = (Storyboard)XamlReader.Load(xmlReaderRow);

						_resetIntroFrameAnimator.Completed += _resetIntroFrameAnimator_Completed;
					}
					finally
					{
						if (storyReader != null)
							storyReader.Dispose();
						if (storyStream != null)
							storyStream.Dispose();
					}
				}
				return _resetIntroFrameAnimator;
			}
		}

		private void _resetIntroFrameAnimator_Completed(object sender, EventArgs e)
		{
			_scvIntro.Height = _pgIntro.ActualHeight;
			_scvIntro.Width = _pgIntro.ActualWidth;

			_cvIntroFrame.Margin = new Thickness((_outFlameMargin * -1), 0, 0, 0);
			_cvIntroFrame.Height = _scvIntro.ActualHeight;

			_introPicture[0].Height = _scvIntro.ActualHeight;
			_introPicture[0].Width = _scvIntro.ActualWidth;

			_introPicture[1].Height = _scvIntro.ActualHeight;
			_introPicture[1].Width = _scvIntro.ActualWidth;

			Canvas.SetLeft(_introPicture[0], _outFlameMargin - _scvIntro.ActualWidth);
			Canvas.SetTop(_introPicture[0], 0);

			Canvas.SetLeft(_introPicture[1], _outFlameMargin);
			Canvas.SetTop(_introPicture[1], 0);

			_isIntroAnimationRunning = false;
			System.Windows.Forms.Application.DoEvents();
		}
		#endregion

		#region -- Story -- Animate Into Frame --
		private Storyboard _introAnimator = null;
		public Storyboard IntroAnimator
		{
			get
			{
				if (_introAnimator is null)
				{
					MemoryStream storyStream = null;
					StreamReader storyReader = null;
					try
					{
						storyStream = new MemoryStream(Encoding.UTF8.GetBytes(_cvIntroFrameStoryScript));
						storyReader = new StreamReader(storyStream);

						storyStream.Seek(0, SeekOrigin.Begin);
						XmlReader xmlReaderRow = XmlReader.Create(storyReader);
						_introAnimator = (Storyboard)XamlReader.Load(xmlReaderRow);

						_introAnimator.Completed += _introAnimator_Completed;
					}
					finally
					{
						if (storyReader != null)
							storyReader.Dispose();
						if (storyStream != null)
							storyStream.Dispose();
					}
				}

				return _introAnimator;
			}
		}

		private void AnimateIntroFrame()
		{
			if (_isPageActive == false)
			{
				_isIntroAnimationRunning = false;
				return;
			}

			AdjustPageSize();

			Storyboard currStoryboard = IntroAnimator;

			foreach (var obj in currStoryboard.Children)
			{
				if (obj is ThicknessAnimationUsingKeyFrames movMarColl)
					if (movMarColl.Name.Equals("MovingMarginCollection"))
						foreach (var obj2 in movMarColl.KeyFrames)
							if (obj2 is LinearThicknessKeyFrame resMar)
							{
								resMar.Value = new Thickness(_cvIntroFrame.Margin.Left + _introPicture[0].ActualWidth, 0, 0, 0);
								break;
							}
			}
			_isIntroAnimationRunning = true;
			currStoryboard.Begin(_pgIntro);
		}

		private void IntroAnimationThreadWorking()
		{
			int intervalSec = 5;

			do
			{
				Task.Delay(1000 * intervalSec).Wait();

				if (_isPageActive)
				{
					try
					{
						if (_isIntroAnimationRunning == false)
						{
							_pgIntro.Dispatcher.Invoke(new Action(() => {
								AnimateIntroFrame();
							}));
						}
					}
					catch (Exception ex)
					{
						string tt1 = ex.Message;
					}
				}
			} while (!_disposed);
		}

		private void _introAnimator_Completed(object sender, EventArgs e)
		{
			try
			{
				IntroAnimator.Stop();

				if (Canvas.GetLeft(_introPicture[0]) > Canvas.GetLeft(_introPicture[1]))
				{
					System.Windows.Forms.Application.DoEvents();
					Task.Delay(100).Wait();
					ResetIntroFrame();
				}
				else
				{
					Canvas.SetLeft(_introPicture[1], Canvas.GetLeft(_introPicture[0]) - _introPicture[1].ActualWidth);
					_isIntroAnimationRunning = false;
				}
			}
			finally
			{

			}
		}


		#endregion

		private bool _pageAdjusted = false;
		private void AdjustPageSize()
        {
			if ((_pgIntro != null) && (_pageAdjusted == false))
			{
				_pgIntro.Dispatcher.Invoke(new Action(() => {
					if (_pgIntro.ActualWidth < 1500)
					{
						if (_introPicture[0] is IIntroAdjustPage pg1)
							pg1.AdjustSize(_pgIntro.ActualWidth);

						if (_introPicture[1] is IIntroAdjustPage pg2)
							pg2.AdjustSize(_pgIntro.ActualWidth);
					}
				}));
				_pageAdjusted = true;
			}
		}

		private bool _disposed = false;
		public void Dispose()
		{
			_disposed = true;
		}
	}
}

