using NssIT.Kiosk.AppDecorator;
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

namespace NssIT.Kiosk.Client.ViewPage.Alert
{
    /// <summary>
    /// Interaction logic for pgOutofOrder.xaml
    /// </summary>
    public partial class pgOutofOrder : Page, IAlertPage
    {
        private const string LogChannel = "ViewPage";
        private string _lastDetailMsg = null;

        public pgOutofOrder()
        {
            InitializeComponent();
        }

        public void ShowAlertMessage(string malayShortMsg = "TIDAK BERFUNGSI", string engShortMsg = "OUT OF ORDER", string detailMsg = "")
        {
            this.Dispatcher.Invoke(new Action(() => {
                TxtMalMsg.Text = malayShortMsg;
                TxtEngMsg.Text = engShortMsg;
                TxtTimeStr.Text = DateTime.Now.ToString("yyyyMMdd-HHmmss-fffff");
                TxtProblemMsg.Text = detailMsg;
            }));

            if (_lastDetailMsg?.Trim().Equals(detailMsg ?? "") == true)
            { /*By Pass*/ }
            else if (detailMsg != null)
                App.Log.LogText(LogChannel, "*", $@"{malayShortMsg} :: {engShortMsg} :: {detailMsg}", "A01", "pgOutofOrder.ShowAlertMessage");

            _lastDetailMsg = detailMsg;
        }

        private bool _isPageActive = false;
        private Thread _systemHealthCheckThreadWorker = null;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _lastDetailMsg = null;

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
            else if(App.SysParam.PrmAppGroup == AppDecorator.Common.AppGroup.Genting)
            {
                ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/genting.png", UriKind.RelativeOrAbsolute));

            }
            else
            {
                ImgStationLogo.Source = new BitmapImage(new Uri("/Resources/MelakaSentral-logo.png", UriKind.RelativeOrAbsolute));
            }

            UpdateVersionText();

            _systemHealthCheckThreadWorker = new Thread(new ThreadStart(SystemHealthCheckThreadWorking));
            _systemHealthCheckThreadWorker.IsBackground = true;
            _systemHealthCheckThreadWorker.Start();

            _isPageActive = true;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _isPageActive = true;

            if ((_systemHealthCheckThreadWorker != null) && (_systemHealthCheckThreadWorker.ThreadState.IsState(ThreadState.Stopped) == false))
            {
                try
                {
                    _lastDetailMsg = null;
                    _systemHealthCheckThreadWorker.Abort();
                }
                catch { }
                finally
                {
                    _systemHealthCheckThreadWorker = null;
                }
            }
        }

        private void SystemHealthCheckThreadWorking()
        {
            try
            {
                bool threadAbort = false;
                App.IsLocalServerReady = false;

                Thread.Sleep(1000 * 15);

                do
                {
                    try
                    {
                        UpdateVersionText();

                        if (App.AppHelp.SystemHealthCheck())
                        {
                            if (App.ValidateSystemSetting(out string errMsg) == false)
                            {
                                ShowAlertMessage(detailMsg: (errMsg ?? "Error when Validate System Setting; (EXIT10000062)"));
                            }
                            else
                                App.IsLocalServerReady = true;
                        }
                        else
                            throw new Exception("Sales Server has unknown error; (EXIT10000061)");
                    }
                    catch (ThreadAbortException) { threadAbort = true; }
                    catch (Exception ex)
                    {
                        App.Log.LogError(LogChannel, "-", ex, "EX01", "pgOutofOrder.SystemHealthCheckThreadWorking");
                        ShowAlertMessage(detailMsg: ex.Message);
                        Thread.Sleep(5000);
                    }
                    finally 
                    {
                        UpdateVersionText();
                    }
                } while (App.IsLocalServerReady == false);

                if ((threadAbort == false) && (App.IsLocalServerReady))
                {
                    App.MainScreenControl.ShowWelcome();
                }
            }
            catch (ThreadAbortException ex)
            {
                App.ShowDebugMsg("Thread aborted; At pgOutofOrder.SystemHealthCheckThreadWorking");
                App.Log.LogError(LogChannel, "-", ex, "EX02", "pgOutofOrder.SystemHealthCheckThreadWorking");
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX03", "pgOutofOrder.SystemHealthCheckThreadWorking");
            }

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        }

        public void UpdateVersionText()
        {
            this.Dispatcher.Invoke(new Action(() => {
                TxtSysVer.Text = App.GetFullSystemVersion();
            }));
        }
    }
}
