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
using System.Windows.Shapes;
using NssIT.Kiosk.Common.WebService.KioskWebService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;

namespace KioskClientTcpTest
{
    /// <summary>
    /// Interaction logic for WFmMainWindowSales.xaml
    /// </summary>
    public partial class WFmMainWindowSales : Window
    {
        LibShowMessageWindow.MessageWindow _msg = null;

        private string _logChannel = "WFmMainWindow";

        private string _logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\Testing\KioskClientTcpTest\LogDB\NssITKioskLog01.db;Version=3";

        private NssIT.Kiosk.Log.DB.DbLog _log = null;
        private NetClientService _netClientSvc = null;
        private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;

        public WFmMainWindowSales()
        {
            InitializeComponent();

            _log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();
            _msg = new LibShowMessageWindow.MessageWindow();
            _netClientSvc = new NetClientService();

            _netClientSvc.SalesService.OnDataReceived += SalesService_OnDataReceived;
        }

        private void SalesService_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (e?.ReceivedData?.MsgObject is UIDestinationListAck uiDest)
                {
                    if (uiDest.MessageData is destination_status destList)
                    {
                        if (destList.code == 0)
                            _msg.ShowMessage($@"Destination => Code: {destList.code}; Detail Count: {destList.details.Length}; State Count: {destList.statedetails.Length}");
                        else
                            _msg.ShowMessage($@"Destination => Code: {destList.code};");
                    }
                    else if (string.IsNullOrWhiteSpace(uiDest.ErrorMessage) == false)
                    {
                        _msg.ShowMessage($@"Error Reading Destination Data; {uiDest.ErrorMessage}");
                        throw new Exception($@"Error Reading Destination Data; {uiDest.ErrorMessage}");
                    }
                    else
                    {
                        _msg.ShowMessage($@"Unable to read destination data (EXIT500021)");
                        throw new Exception($@"Unable to read destination data (EXIT500021)");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogText(_logChannel, "-", e, "EX01", "WFmMainWindowSales.SalesService_OnDataReceived", NssIT.Kiosk.AppDecorator.Log.MessageType.Error,
                    extraMsg: "Error : " + ex.ToString() + "; \r\n\r\n ObjType: DataReceivedEventArgs",
                    netProcessId: e?.ReceivedData?.NetProcessId);
            }
        }


        private void BtnCheckServerStatus_Click(object sender, RoutedEventArgs e)
        {
            BtnCheckServerStatus.IsEnabled = false;

            Thread threadWorker = new Thread(new ThreadStart(new Action(() => {
                _msg.ShowMessage("Checking Sales Server status ..");

                try
                {
                    _netClientSvc.SalesService.QuerySalesServerStatus(out bool isLocalServerResponded, out bool serverAppHasDisposed, out bool serverAppHasShutdown, out bool serverWebServiceIsDetected);

                    if (serverWebServiceIsDetected == true)
                    {
                        _msg.ShowMessage("Sales Server Service is ready");
                    }
                    else
                    {
                        if (isLocalServerResponded == false)
                            _msg.ShowMessage("Local Server no response");
                        else if (serverAppHasDisposed)
                            _msg.ShowMessage("Local Server has disposed");
                        else if (serverAppHasShutdown)
                            _msg.ShowMessage("Local Server has shutdown");
                        else if (serverWebServiceIsDetected == false)
                            _msg.ShowMessage("Web Server is not detected");
                        else
                            _msg.ShowMessage("Local Server has unknown error (EXIT500001)");
                    }
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage($@"Error when start new sales; (EXIT500002); {ex.ToString()}");
                }
                finally
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        BtnCheckServerStatus.IsEnabled = true;
                    }));
                }

            })));

            threadWorker.IsBackground = true;
            threadWorker.Start();
        }

        private void BtnStartNewSales_Click(object sender, RoutedEventArgs e)
        {
            BtnStartNewSales.IsEnabled = false;

            Thread threadWorker = new Thread(new ThreadStart(new Action(() => {
                _msg.ShowMessage("Start new sales ..");

                try
                {
                    _netClientSvc.SalesService.StartSales(out bool isServerResponded);

                    if (isServerResponded == true)
                    {
                        _msg.ShowMessage("Start Sales Command successfully sent to Local Server.");
                    }
                    else
                    {
                         _msg.ShowMessage("Start Sales Command fail to send to Local Server.");
                    }
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage($@"Error when start new sales; (EXIT500003); {ex.ToString()}");
                }
                finally
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        BtnStartNewSales.IsEnabled = true;
                    }));
                }

            })));

            threadWorker.IsBackground = true;
            threadWorker.Start();
        }

        private void BtnReLogon_Click(object sender, RoutedEventArgs e)
        {
            BtnReLogon.IsEnabled = false;

            Thread threadWorker = new Thread(new ThreadStart(new Action(() => {
                _msg.ShowMessage("Logon Again ..");

                try
                {
                    _netClientSvc.SalesService.WebServerLogon(out bool isServerResponded, out bool isLogonSuccess, out bool isNetworkTimeout, out bool isValidAuthentication, out bool isLogonErrorFound, out string errorMessage, 30);

                    if (isLogonSuccess == true)
                        _msg.ShowMessage("Logon is Success");
                    else if (isServerResponded == false)
                        _msg.ShowMessage("Local Server is not responded");
                    else if(isLogonErrorFound == true)
                        _msg.ShowMessage("Error when logon.." + errorMessage);
                    else if (isNetworkTimeout)
                        _msg.ShowMessage("Network Timeout when logon");
                    else if (isValidAuthentication)
                        _msg.ShowMessage("Invalid Authentication");
                    else
                        _msg.ShowMessage($@"Sales Server Service has unknown error (EXIT500031); isNetworkTimeout: {isNetworkTimeout}; isValidAuthentication: {isValidAuthentication}; isLogonErrorFound: {isLogonErrorFound}; errorMessage: {errorMessage}");

                }
                catch (Exception ex)
                {
                    _msg.ShowMessage($@"Error when start new sales; (EXIT500032); {ex.ToString()}");
                }
                finally
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        BtnReLogon.IsEnabled = true;
                    }));
                }

            })));

            threadWorker.IsBackground = true;
            threadWorker.Start();
        }
    }
}
