using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Server.ServerApp;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFmServerApplicationTest
{
    public partial class Form1 : Form
    {
        private LibShowMessageWindow.MessageWindow _msg = new LibShowMessageWindow.MessageWindow();
        private IUIApplicationJob _serverApp = null;
        public Form1()
        {
            InitializeComponent();

            _serverApp = new ServerSalesApplication(NssIT.Kiosk.AppDecorator.Common.AppGroup.MelakaSentral, false);

            _serverApp.OnShowResultMessage += _serverApp_OnShowResultMessage;
        }

        private void _serverApp_OnShowResultMessage(object sender, UIMessageEventArgs e)
        {
            if (e != null)
            {
                if (e.MsgType == MessageType.NormalType)
                {
                    if (e.KioskMsg is UIServerApplicationStatusAck appStt)
                    {
                        _msg.ShowMessage($@"ServerAppHasDisposed : {appStt.ServerAppHasDisposed}; ServerAppHasShutdown : {appStt.ServerAppHasShutdown}; ServerWebServiceIsDetected : {appStt.ServerWebServiceIsDetected};");
                    }
                    else if (e.KioskMsg is UIDestinationListAck dList)
                    {
                        destination_status dtStt = (destination_status)dList.MessageData;

                        if (dtStt.code == 0)
                            _msg.ShowMessage($@"Destination Detail Count : {dtStt.details.Length}; State Detail Count : {dtStt.statedetails.Length}");
                        else
                            _msg.ShowMessage($@"Destination Code : {dtStt.code};");
                    }
                    else if (e.KioskMsg is UIWebServerLogonStatusAck logStt)
                    {
                        if (logStt.LogonErrorFound)
                        {
                            _msg.ShowMessage($@"Logon Error : {logStt.ErrorMessage}");
                        }
                        else
                        {
                            _msg.ShowMessage($@"Logon => Logon Success: {logStt.LogonSuccess} NetworkTimeout: {logStt.NetworkTimeout}; IsValidAuthentication: {logStt.IsValidAuthentication};");
                        }
                    }

                }
                else if (e.MsgType == MessageType.ErrorType)
                {
                    if ((e.KioskMsg != null) && (string.IsNullOrWhiteSpace(e.KioskMsg.ErrorMessage) == false))
                    {
                        _msg.ShowMessage($@"Error result on {Enum.GetName(typeof(CommInstruction), e.KioskMsg.Instruction)}; {e.KioskMsg.ErrorMessage}");
                    }
                    else if (e.KioskMsg != null)
                    {
                        _msg.ShowMessage($@"Error result on {Enum.GetName(typeof(CommInstruction), e.KioskMsg.Instruction)}; Unknown error");
                    }
                    else if (e.Message != null)
                    {
                        _msg.ShowMessage($@"Error with unknown request; ** {e.Message}");
                    }
                    else
                    {
                        _msg.ShowMessage($@"Error with unknown request; --");
                    }
                }
                else
                {
                     _msg.ShowMessage($@"Error with unknown message type; *** {e.Message}");
                }
            }
        }

        private async void btnIsServerReady_Click(object sender, EventArgs e)
        {
            IKioskMsg commandMsg = new UIServerApplicationStatusRequest("-", DateTime.Now);

            try
            {
                if ((await _serverApp.SendInternalCommand("", Guid.NewGuid(), commandMsg)) == true)
                {
                    _msg.ShowMessage("ServerAppliactionStatusRequest command sent successful.");
                }
                else
                    throw new Exception("Fail to send ServerAppliactionStatusRequest command");

            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private async void btnReLogon_Click(object sender, EventArgs e)
        {
            IKioskMsg commandMsg = new UIWebServerLogonRequest("-", DateTime.Now);

            try
            {
                if ((await _serverApp.SendInternalCommand("", Guid.NewGuid(), commandMsg)) == true)
                {
                    _msg.ShowMessage("DestinationListRequest command sent successful.");
                }
                else
                    throw new Exception("Fail to send DestinationListRequest command");

            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private async void btnGetDestinationList_Click(object sender, EventArgs e)
        {
            IKioskMsg commandMsg = new UIStartNewSalesRequest("-", DateTime.Now);

            try
            {
                if ((await _serverApp.SendInternalCommand("", Guid.NewGuid(), commandMsg)) == true)
                {
                    _msg.ShowMessage("DestinationListRequest command sent successful.");
                }
                else
                    throw new Exception("Fail to send DestinationListRequest command");

            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        
    }
}
