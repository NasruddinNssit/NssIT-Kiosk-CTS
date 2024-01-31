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

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NssIT.Kiosk.Client.Base.LocalDevices;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    /// <summary>
    /// Interaction logic for pgMyKad.xaml
    /// </summary>
    public partial class pgMyKad : Page
    {
        private const string LogChannel = "ViewPage";

        public event EventHandler<EndOfMyKadScanEventArgs> OnEndScan;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private int _passengerRerordIndex = -1;
        private IIdentityReader _pssgIdReader = null;
        private PassengerIdentity _pssgId = null;
        private int _waitDelaySec = 20;
        private bool _stopReading = true;
        private bool _isPageUnloaded = true;

        public pgMyKad()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoEnglish.xaml");

            _pssgIdReader = new ICPassHttpReader(@"http://localhost:1234/Para=2");
        }

        public void InitPageData(LanguageCode language, int passengerRerordIndex)
        {
            _passengerRerordIndex = passengerRerordIndex;
            _language = language;
            _pssgId = null;
            _waitDelaySec = 30;
            _stopReading = true;
            _isPageUnloaded = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

            if (_language == LanguageCode.Malay)
                this.Resources.MergedDictionaries.Add(_langMal);
            else
                this.Resources.MergedDictionaries.Add(_langEng);

            TxtPassengerNo.Text = (_passengerRerordIndex + 1).ToString();

            TxtInsertMyKad.Visibility = Visibility.Visible;
            TxtRemoveMyKad.Visibility = Visibility.Collapsed;
            BdOK.Visibility = Visibility.Collapsed;

            StartReading(_waitDelaySec);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _isPageUnloaded = true;
            StopScanning();
        }

        private void StopScanning()
        {
            _stopReading = true;
            
            if (_scanManThreadWorker != null)
            {
                if (_scanManThreadWorker.ThreadState.IsState(ThreadState.Stopped) == false)
                {
                    try
                    {
                        _scanManThreadWorker.Abort();
                        Task.Delay(300).Wait();
                    }
                    catch { }

                }
                _scanManThreadWorker = null;
            }
        }

        private void Button_Cancel(object sender, MouseButtonEventArgs e)
        {
            StopScanning();

            _pssgId = null;
            RaiseOnEndScan(_pssgId);
        }

        private Thread _scanManThreadWorker = null;
        private void StartReading(int waitDelaySec = 10)
        {
            System.Windows.Forms.Application.DoEvents();

            _waitDelaySec = waitDelaySec;
            _pssgId = null;

            _scanManThreadWorker = new Thread(new ThreadStart(new Action(() => {
                ReadingManagerThreadWorking();
            })));
            _scanManThreadWorker.IsBackground = true;
            _scanManThreadWorker.Priority = ThreadPriority.AboveNormal;
            _scanManThreadWorker.Start();

            void ReadingManagerThreadWorking()
            {
                Thread tWorker = null;

                try
                {
                    _stopReading = false;
                    tWorker = new Thread(new ThreadStart(new Action(() => {
                        ReadIC();
                    })));
                    tWorker.IsBackground = true;
                    tWorker.Priority = ThreadPriority.AboveNormal;
                    tWorker.Start();

                    DateTime startTime = DateTime.Now;
                    DateTime endTime = startTime.AddSeconds(_waitDelaySec);

                    int countDown = _waitDelaySec - 1;
                    while ((countDown > 0) && (_isPageUnloaded == false) && (_stopReading == false))
                    {
                        PassengerIdentity passgId = _pssgId;

                        if ((passgId == null) || (passgId.IsIDReadSuccess == false))
                        {
                            //if (passgId?.IsIDReadSuccess == false)
                            //{
                            //    this.Dispatcher.Invoke(new Action(() => {
                            //        txtMsg.Text = passgId.Message;
                            //    }));
                            //}

                            this.Dispatcher.Invoke(new Action(() => {
                                TxtCountDown.Text = $@"{countDown}";
                            }));
                            Task.Delay(1000).Wait();
                        }
                        countDown--;
                    }

                    _stopReading = true;

                    if (_pssgId is null)
                    {
                        Task.Delay(500).Wait();
                        _pssgId = new PassengerIdentity(false, null, null, null, "IC not found (IV); ");
                    }

                    if (tWorker.ThreadState.IsState(ThreadState.Stopped) == false)
                    {
                        try
                        {
                            tWorker.Abort();
                        }
                        catch (Exception) { }
                        Task.Delay(500).Wait();

                        tWorker = null;
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.ReadingManagerThreadWorking");
                }
                finally
                {
                    if (tWorker != null)
                    {
                        if (tWorker.ThreadState.IsState(ThreadState.Stopped) == false)
                        {
                            try
                            {
                                tWorker.Abort();
                                Task.Delay(100).Wait();
                            }
                            catch { }
                        }
                        tWorker = null;
                    }
                }

                if (_pssgId.IsIDReadSuccess == false)
                    RaiseOnEndScan(_pssgId);
                else
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        TxtInsertMyKad.Visibility = Visibility.Collapsed;
                        TxtRemoveMyKad.Visibility = Visibility.Visible;
                        BdOK.Visibility = Visibility.Visible;
                    }));                    
                }
            }
        }

        private void Button_OK(object sender, MouseButtonEventArgs e)
        {
            RaiseOnEndScan(_pssgId);
        }

        private void RaiseOnEndScan(PassengerIdentity pssgId)
        {
            if (_isPageUnloaded)
                return;

            try
            {
                if (OnEndScan != null)
                {
                    OnEndScan.Invoke(null, new EndOfMyKadScanEventArgs(pssgId));
                }
            }
            catch(Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.RaiseOnEndScan");
            }
        }

        public PassengerIdentity ReadPassengerId()
        {
            return _pssgId;
        }

        private void ReadIC()
        {
            try
            {
                while (_stopReading == false)
                {
                    try
                    {
                        _pssgId = _pssgIdReader.ReadIC(waitDelaySec: (_waitDelaySec));
                    }
                    catch (Exception ex)
                    {
                        _pssgId = new PassengerIdentity(false, null, null, null, "Error when IC reading (III); " + ex.Message);
                    }

                    if (_pssgId?.IsIDReadSuccess == true)
                    {
                        _stopReading = true;
                        break;
                    }
                    else
                        Task.Delay(800).Wait();
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.ReadIC");
            }
        }

        
    }
}
