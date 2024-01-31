using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using NssIT.Kiosk.Common.WebService.KioskWebService;

namespace WFmWebServiceTest
{
	public partial class Form1 : Form
	{
		private LibShowMessageWindow.MessageWindow _msg = new LibShowMessageWindow.MessageWindow();
		private ServiceSoapClient _soap = null;
		public Form1()
		{
			InitializeComponent();

			BasicHttpBinding binding = new BasicHttpBinding();
			EndpointAddress address = new EndpointAddress(ConfigurationManager.AppSettings.Get("WebServiceURL"));
			_soap = new ServiceSoapClient(binding, address);
		}

		private void btnGetTimeStamp_Click(object sender, EventArgs e)
		{
			try
			{
				timestamp_status tim = _soap.Timestamp();
				txtTimeStamp.Text = tim.expdatetime;

				_msg.ShowMessage($@"expdatetime : {tim.expdatetime}");
			}
			catch (Exception ex)
			{
				_msg.ShowMessage(ex.ToString());
			}
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			try
			{
				// Get User Id And Password From Registry
				string userId = null;
				string passWord = null;

				userId = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\eTicketing", "UserID", null);
				passWord = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\eTicketing", "UserPass", null);

				if (userId == null)
				{
					userId = (string)Registry.GetValue(@"HKCU\SOFTWARE\eTicketing", "UserID", null);
					passWord = (string)Registry.GetValue(@"HKCU\SOFTWARE\eTicketing", "UserPass", null);
				}

				if (userId is null)
					throw new Exception("Unable to read logon user id");

				if (passWord is null)
					throw new Exception("Unable to read logon password");

				//---------------------------------------------------------------------------

				string passWordHashed = SecurityHelper.PassEncrypt(passWord);
				string strToBeEncr = $@"{userId},{txtTimeStamp.Text}";

				string encStr = SecurityHelper.Encrypt(strToBeEncr, passWordHashed);
				login_status lgn = _soap.Login(userId, encStr, "10.1.1.111");

				// lgn.Code : 0 for success, 2: No such User Id; 3 for expired, 4: Inactive User Id; 5: Invalid IP; 6: User Id not match with encrpted data; 99 is other error

				if (lgn.code != 0)
				{
					if (lgn.code == 3)
						txtLogin.Text = $@"{lgn.msg ?? "Fail login web service"}; Code : {lgn.code}";
					else
						throw new Exception($@"{lgn.msg ?? "Fail login web service"}; Code : {lgn.code}");
				}
				else
					txtLogin.Text = $@"{DateTime.Now.ToString("HH:mm:ss.fff")} - kiosktoken : {lgn.kiosktoken};";

				_msg.ShowMessage($@"kiosktoken : {lgn.kiosktoken}");
			}
			catch (Exception ex)
			{
				_msg.ShowMessage(ex.ToString());
			}
		}

		private void btnWriteReg_Click(object sender, EventArgs e)
		{
			try
			{
				WriteRegistry();

				_msg.ShowMessage("btnWriteReg_Click : Done");
			}
			catch (Exception ex)
			{
				_msg.ShowMessage(ex.ToString());
			}
		}

		private void WriteRegistry()
		{
			// The name of the key must include a valid root.
			const string userRoot = @"HKEY_LOCAL_MACHINE\SOFTWARE";
			const string subkey = "eTicketing";
			const string keyName = userRoot + "\\" + subkey;

			// Note : for Windows 64 Bits will be re-route to HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\eTicketing

			// An int value can be stored without specifying the
			// registry data type, but long values will be stored
			// as strings unless you specify the type. Note that
			// the int is stored in the default name/value
			// pair.
			Registry.SetValue(keyName, "", 0);
			Registry.SetValue(keyName, "UserID", "hong2", RegistryValueKind.String);
			Registry.SetValue(keyName, "UserPass", "123456", RegistryValueKind.String);

			string tt1 = "";
		}

		
	}
}
