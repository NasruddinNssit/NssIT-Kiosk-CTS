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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using NssIT.Kiosk.Device.B2B.B2BDecorator.Data;

namespace CashMachineTCPTest
{
	/// <summary>
	/// Interaction logic for pgBanknoteTest.xaml
	/// </summary>
	public partial class pgBanknoteTest : Page
	{
		NetClientService _netClientService = null;

		public pgBanknoteTest(NetClientService netClientService)
		{
			InitializeComponent();

			_netClientService = netClientService;
		}

		private async void BtnReadCassetteData_Click(object sender, RoutedEventArgs e)
		{
			//MessageBox.Show("BtnReadCassetteData_Click");
			try
			{
				txtCassetteData.Text = "";
				System.Windows.Forms.Application.DoEvents();

				B2BCassetteInfoCollection caseInfo = await _netClientService.BanknoteSvcClient.GetAllCassetteInfo(DateTime.Now.ToString("mm_ss.f_fff_fff"), 120);

				if (caseInfo != null)
				{
					for (int inx=0; inx < caseInfo.GetNumberOfCassette(); inx++)
					{
						B2BCassetteInfo cInfo = caseInfo[inx];
						txtCassetteData.AppendText($@"Bill Type : {cInfo.BillType.ToString()} ;{"\t\t"}Value : {cInfo.DigitBillType.ToString()} ;{"\t\t"}Qty : {cInfo.BillQty.ToString()}{"\r\n"}");
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	}
}
