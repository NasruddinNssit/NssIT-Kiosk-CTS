using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client
{
	/// <summary>
	/// Interaction logic for WndTestingMonitor.xaml
	/// </summary>
	public partial class WndTestingMonitor : Window
	{
		private IMainScreenControl _mainScreenControl;

		public WndTestingMonitor(IMainScreenControl mainScreenControl)
		{
			InitializeComponent();

			_mainScreenControl = mainScreenControl;
		}

		private void BtnIntro_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				App.ShowDebugMsg("BtnIntro_Click");
				_mainScreenControl.ShowWelcome();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg(ex.ToString());
			}
		}
	}
}
