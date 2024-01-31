using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
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

namespace NssIT.Kiosk.Client.ViewPage.Language
{
	/// <summary>
	/// ClassCode:EXIT80.15; Interaction logic for pgLanguage.xaml
	/// </summary>
	public partial class pgLanguage : Page, ILanguage
	{
		private const string LogChannel = "ViewPage";

		private bool _selectedFlag = false;

		private AppModule _selectedModule = AppModule.UIKioskSales;

		private NetServiceAnswerMan _submitAnswerMan = null;

		/// <summary>
		/// FuncCode:EXIT80.1501
		/// </summary>
		public pgLanguage()
		{
			InitializeComponent();
		}

		/// <summary>
		/// FuncCode:EXIT80.1505
		/// </summary>
		public void InitData(AppModule module)
        {
			///// Note : Only UICollectTicket or UIKioskSales is allowed.
			if (module == AppModule.UICollectTicket)
				_selectedModule = module;

			else
				_selectedModule = AppModule.UIKioskSales;
		}

		/// <summary>
		/// FuncCode:EXIT80.1510
		/// </summary>
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			_selectedFlag = false;

			if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Larkin)
			{
				ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/larkin-logo-reverse.png", UriKind.RelativeOrAbsolute));
			}
			else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Gombak)
			{
				ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/TerminalBersepaduGombak-logo.png", UriKind.RelativeOrAbsolute));
			}
			else if (App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Klang)
			{
				ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/Klang Sentral Terminal 00.jpeg", UriKind.RelativeOrAbsolute));
			}
			else
			{
				ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/MelakaSentral-logo.png", UriKind.RelativeOrAbsolute));
			}

			System.Windows.Forms.Application.DoEvents();
		}

		/// <summary>
		/// FuncCode:EXIT80.1515
		/// </summary>
		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			_selectedFlag = false;
			_submitAnswerMan?.Dispose();
		}

		/// <summary>
		/// FuncCode:EXIT80.1520
		/// </summary>
		private void Malay_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DoSelectLanguage(LanguageCode.Malay);
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, classNMethodName: "pgIntro._introAniHelp_OnIntroBegin");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1525
		/// </summary>
		private void English_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DoSelectLanguage(LanguageCode.English);
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, classNMethodName: "pgIntro._introAniHelp_OnIntroBegin");
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1530
		/// </summary>
		private void DoSelectLanguage(LanguageCode langCode)
		{
			try
			{
				if (_selectedFlag == false)
				{
					_selectedFlag = true;

					Submit(langCode);
					//App.NetClientSvc.SalesService.SubmitLanguage(langCode, out bool isServerResponded);

					//if (isServerResponded == false)
					//	App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000101)");
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000102)", ex), "EX01", classNMethodName: "pgLanguage.DoSelectLanguage");
				App.MainScreenControl.Alert(detailMsg: $@"Error on language selection; (EXIT10000102)");
			}

		}

		/// <summary>
		/// FuncCode:EXIT80.1535
		/// </summary>
		private void Submit(LanguageCode langCode)
		{
			System.Windows.Forms.Application.DoEvents();
            _submitAnswerMan?.Dispose();

			if (_selectedModule == AppModule.UICollectTicket)
			{
                _submitAnswerMan = App.NetClientSvc.CollectTicketService.SubmitLanguage(langCode,
					"Local Server not responding (EXIT80.1535.X1)",
                    new NetServiceAnswerMan.FailLocalServerResponseCallBackDelg(delegate (string failMessage)
                    {
                        App.MainScreenControl.Alert(detailMsg: failMessage);
                    }));
            }
			else
			{
				_submitAnswerMan = App.NetClientSvc.SalesService.SubmitLanguage(langCode,
					"Local Server not responding (EXIT80.1535.X2)",
					new NetServiceAnswerMan.FailLocalServerResponseCallBackDelg(delegate (string failMessage)
					{
						App.MainScreenControl.Alert(detailMsg: failMessage);
					}), waitDelaySec: 30);
			}
        }
	}
}
