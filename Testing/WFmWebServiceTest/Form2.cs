using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NssIT.Kiosk.Server.AccessDB;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;

namespace WFmWebServiceTest
{
	public partial class Form2 : Form
	{
		private LibShowMessageWindow.MessageWindow _msg = new LibShowMessageWindow.MessageWindow();
		private ServerAccess _access = null;

		public Form2()
		{
			InitializeComponent();

			_access = ServerAccess.GetAccessServer();

			_access.OnSendMessage += _access_OnSendMessage;
		}

		private void _access_OnSendMessage(object sender, SendMessageEventArgs e)
		{
			try
			{
				if (e.ResultState == NssIT.Kiosk.AppDecorator.Common.AppService.ResultStatus.Success)
				{
					if (e.KioskMessage is UIDestinationListAck uiDest)
					{
						if (uiDest.MessageData is destination_status dest)
						{
							if (dest.code != 0)
							{
								throw new Exception($@"Error Code: {dest.code}; Message : {dest.msg}");
							}
							else
							{
								this.Invoke(new Action(() => {
									txtMsg.Text = $@"Destination Count : {dest.details.Length}; State Count : {dest.statedetails.Length}";
								}));
							}
						}
						else
							throw new Exception($@"Unhandle destination data type ({uiDest.MessageData.GetType().ToString()})");
					}
					else
						throw new Exception($@"Unhandle sussess result type ({e.KioskMessage.GetType().ToString()})");
				}
				else
				{
					if (e.KioskMessage?.ErrorMessage != null)
						throw new Exception($@"State I : {Enum.GetName(typeof(NssIT.Kiosk.AppDecorator.Common.AppService.ResultStatus), e.ResultState)}; Error : {e.KioskMessage.ErrorMessage}");
					else if (e.Message != null)
						throw new Exception($@"State II : {Enum.GetName(typeof(NssIT.Kiosk.AppDecorator.Common.AppService.ResultStatus), e.ResultState)}; Error : {e.Message}");
					else
						throw new Exception($@"State III : {Enum.GetName(typeof(NssIT.Kiosk.AppDecorator.Common.AppService.ResultStatus), e.ResultState)}; Error : Unknown Error");
				}
			}
			catch (Exception ex)
			{
				_msg.ShowMessage(ex.ToString());
			}
		}

		private void Form2_Load(object sender, EventArgs e)
		{

		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			try
			{
				bool isLoginSuccess = NssIT.Kiosk.Server.AccessDB.Security.ReDoLogin(out bool networkTimeout, out bool isValidAuthentication);

				if (isLoginSuccess)
				{
					string token = NssIT.Kiosk.Server.AccessDB.Security.AccessToken;

					if (string.IsNullOrWhiteSpace(token))
					{
						_msg.ShowMessage("Unable to get access token at the moment");
					}
					else
						txtToken.Text = NssIT.Kiosk.Server.AccessDB.Security.AccessToken;
				}
				else
				{
					if (networkTimeout)
					{
						_msg.ShowMessage("Network Timeout");
					}
					else if (isValidAuthentication == false)
					{
						_msg.ShowMessage("Invalid authentication");
					}
					else 
					{
						_msg.ShowMessage("Unable to get access token at the moment");
					}
				}
				
			}
			catch (Exception ex)
			{
				_msg.ShowMessage(ex.ToString());
			}
		}

		private void btnGetDestination_Click(object sender, EventArgs e)
		{
			try
			{
				//destination_status dest =
				bool addResponse = _access.AddCommand(new AccessCommandPack(new DestinationListRequestCommand("-", Guid.NewGuid(), "")), out string errMsg);

				if (addResponse == true)
				{
					_msg.ShowMessage("SalesGetDestListCommand send successful");
				}
				else
					_msg.ShowMessage($@"Fail to send SalesGetDestListCommand; {errMsg}");
			}
			catch (Exception ex)
			{
				_msg.ShowMessage(ex.ToString());
			}
		}

		
	}
}
