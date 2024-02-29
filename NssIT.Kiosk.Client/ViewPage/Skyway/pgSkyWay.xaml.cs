using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Skyway
{
    /// <summary>
    /// Interaction logic for pgInsuranse.xaml
    /// </summary>
    public partial class pgSkyWay : Page
	{
		private Brush _activateButtomColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x43, 0xD8, 0x2C));
		private Brush _deactivateButtomColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));

		private const string LogChannel = "ViewPage";

		private CultureInfo _provider = CultureInfo.InvariantCulture;
		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private LanguageCode _language = LanguageCode.English;


		public pgSkyWay()
        {
            InitializeComponent();

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\Skyway\rosInsuranceMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\Skyway\rosInsuranceEnglish.xaml");
		}


		public void InitSkyWayData(UserSession session)
		{
			_language = session.Language;
		}
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			ResourceDictionary currLanguage = _langEng;
			try
			{
				_hasResult = false;
				if (_resultLock.CurrentCount == 0)
					_resultLock.Release();

				this.Resources.MergedDictionaries.Clear();
				if (_language == LanguageCode.Malay)
					currLanguage = _langMal;
				else
					currLanguage = _langEng;

				this.Resources.MergedDictionaries.Add(currLanguage);

				ChangeScreenSize();
				SetShield(isOn: false);
				LoadSkyWayMsg();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"Error: {ex.Message}; pgInsuranse.Page_Loaded");
				App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsuranse.Page_Loaded");
			}

			void LoadSkyWayMsg()
			{
				TxtLabel.Text = string.Format(currLanguage["Buy_Label"].ToString());
			}

		}

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

		private bool _hasResult = false;
		private SemaphoreSlim _resultLock = new SemaphoreSlim(1);
		private async void BdNo_Click(object sender, MouseButtonEventArgs e)
		{
			if ((await _resultLock.WaitAsync(3 * 1000)) == true)
            {
				try
				{
					if (_hasResult)
						return;

					Submit(isIncludeSkyWay: false);
					_hasResult = true;
				}
				catch (Exception ex)
				{
					App.ShowDebugMsg($@"Error: {ex.Message}; pgInsuranse.BdNo_Click");
					App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsuranse.BdNo_Click");
				}
				finally
				{
					if (_resultLock.CurrentCount == 0)
						_resultLock.Release();
				}
			}			
		}

		private async void BdYes_Click(object sender, MouseButtonEventArgs e)
		{
			if ((await _resultLock.WaitAsync(3 * 1000)) == true)
			{
				try
				{
					if (_hasResult)
						return;

					Submit(isIncludeSkyWay: true);
					_hasResult = true;
				}
				catch (Exception ex)
				{
					App.ShowDebugMsg($@"Error: {ex.Message}; pgInsuranse.BdYes_Click");
					App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsuranse.BdYes_Click");
				}
				finally
				{
					if (_resultLock.CurrentCount == 0)
						_resultLock.Release();
				}
			}
		}
		private void Submit(bool isIncludeSkyWay)
		{
			SetShield(isOn: true);
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitSkyWay(isIncludeSkyWay, out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000111)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000112)");
					App.Log.LogError(LogChannel, "", new Exception("(EXIT10000112)", ex), "EX01", "pgInsuranse.Submit");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		private void SetShield(bool isOn)
        {
			if (isOn)
            {
				BdNo.Background = _deactivateButtomColor;
				BdYes.Background = _deactivateButtomColor;
			}
			else
            {
				BdNo.Background = _activateButtomColor;
				BdYes.Background = _activateButtomColor;
			}
        }

		private int _changeScreenSizeCount = 0;
		private void ChangeScreenSize()
		{
			if (App.MainScreenControl.QueryWindowSize
				(out double winWidth, out double winHeight) == true)
			{
				if (_changeScreenSizeCount <= 3)
				{
					if (winHeight > 1500)
					{
						BdNo.Height = 60;
						BdYes.Height = 60;
					}
					_changeScreenSizeCount++;
				}
			}
		}
	}
}
