using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
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

namespace NssIT.Kiosk.Client.ViewPage.Intro
{
	/// <summary>
	/// Interaction logic for pgIntro.xaml
	/// </summary>
	public partial class pgIntro : Page
	{
		private const string LogChannel = "ViewPage";
		private bool _pageLoaded = false; 
		private IntroUIAnimateHelper _introAniHelp = null;

		private Thread _sysInitThreadWorker = null;
		private Thread _operationTimeThreadWorker = null;
        private Thread _maintenanceThreadStartingWorker = null;
        private static SemaphoreSlim _manLock = new SemaphoreSlim(1);

        private bool _maintenanceRequest = false;
       

        private bool? _isSystemHealthy = null;
        public pgIntro()
		{
			InitializeComponent();

			_introAniHelp = new IntroUIAnimateHelper(this, CvIntroFrame, ScvIntro);

			_introAniHelp.OnIntroBegin += _introAniHelp_OnIntroBegin;
		}

		private bool _hasBegun = false;
		private void _introAniHelp_OnIntroBegin(object sender, BeginEventArgs e)
		{
			if (_hasBegun)
				return;
			try 
			{
				_introAniHelp.SetStartButtonEnabled(false);
				System.Windows.Forms.Application.DoEvents();

				App.IsLocalServerReady = false;
				//if (App.AppHelp.SystemHealthCheck())
				//{
				//	App.IsLocalServerReady = true;
				//	_hasBegun = true;

				App.ResetMaxTicketAdvanceDate();
				
				Submit(e.TransactionType);

			    
					//App.NetClientSvc.SalesService.StartNewSessionCountDown(out bool isServerResponded);

					//if (isServerResponded == false)
					//	App.MainScreenControl.Alert(detailMsg: "Unable to start new transaction at the moment.");
				//}
				//else
				//	throw new Exception("Sales Server has unknown error; (EXIT10000041)");
			}
			catch(Exception ex)
			{
				App.Log.LogError(LogChannel, "-", ex, classNMethodName: "pgIntro._introAniHelp_OnIntroBegin");
				App.MainScreenControl.Alert(detailMsg: ex.Message);
			}
		}

        private DateTime _lastMaintenanceRequestTime = DateTime.MinValue;
        public void MaintenanceScheduler_OnRequestSettlement(object sender, EventArgs e)
		{

			try
			{
                _manLock.WaitAsync().Wait();
                if (_pageLoaded == false) return;

                if ((_isSystemHealthy.HasValue && _isSystemHealthy.Value == true) || (_isSystemHealthy.HasValue == false))
                {
                    _maintenanceRequest = true;

                    int maxWaitingSec = 180;

                    if (_maintenanceThreadStartingWorker != null)
                    {
                        if (((_maintenanceThreadStartingWorker?.ThreadState & ThreadState.Aborted) == ThreadState.Aborted)
                                || ((_maintenanceThreadStartingWorker?.ThreadState & ThreadState.Stopped) == ThreadState.Stopped)
                                )
                        {
                            _maintenanceThreadStartingWorker = null;
                        }
                        else
                        {
                            if (_maintenanceThreadStartingWorker != null)
                            {
                                DateTime expiredTime = _lastMaintenanceRequestTime.AddSeconds(maxWaitingSec);

                                if (expiredTime.Subtract(DateTime.Now).TotalSeconds <= 0)
                                {
                                    try
                                    {
                                        _maintenanceThreadStartingWorker?.Abort();
                                    }
                                    catch { /* By Pass Any Error */}
                                    Thread.Sleep(1000);

                                    _maintenanceThreadStartingWorker = null;
                                }
                            }
                        }

                    }

                    if (_maintenanceThreadStartingWorker == null)
                    {
                        _maintenanceThreadStartingWorker = new Thread(new ThreadStart(StartMaintenanceThreadWorking));
                        _maintenanceThreadStartingWorker.IsBackground = true;
                        _maintenanceThreadStartingWorker.Start();
                        _lastMaintenanceRequestTime = DateTime.Now;
                    }
                }
                else
                {
                    string debugStr = "*debugStr";

                }

            }
            catch(Exception ex)
			{
                App.Log.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "pgIntro.MaintenanceScheduler_OnRequestSettlement");
			}
			finally
			{
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
			
        }

		private void StartMaintenanceThreadWorking()
		{
            try
            {
                _manLock.WaitAsync().Wait();

                if ((_pageLoaded == false) || (_hasBegun))
                {
                    App.ShowDebugMsg("Request Maintenance - Intro Page not loaded OR Sale has begun.");
                    return;
                }

                _introAniHelp.SetStartButtonEnabled(false);
                System.Windows.Forms.Application.DoEvents();

                bool healthChkResult = true;
                bool proceed = true;
                try
                {
                    healthChkResult = App.AppHelp.SystemHealthCheck();
                }
                catch (Exception ex)
                {
                    proceed = false;
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000050)");
                }

                if (proceed)
                {
                    if (healthChkResult == false)
                    {
                        App.MainScreenControl.Alert(detailMsg: $@"Sales Server has unknown error; (EXIT10000042)");
                    }
                    else
                    {
                        App.ShowDebugMsg("Start Request Maintenance");
                      
                        App.ShowDebugMsg("Running .. TowerLight.ShowBusyState; DEBUG01; pgIntro.StartMaintenanceThreadWorking");
                        App.Log.LogText(LogChannel, "*", "Running .. TowerLight.ShowBusyState", "DEBUG01", "pgIntro.StartMaintenanceThreadWorking", AppDecorator.Log.MessageType.Debug);

                        App.NetClientSvc.SalesService.RequestMaintenance(out bool isServerResponded);
                        App.ShowDebugMsg("End Request Maintenance");

                        if (isServerResponded == false)
                        {
                            App.ShowDebugMsg("Request Maintenance found no server response.");

                            App.Log.LogError(LogChannel, "-", new Exception("Local Server no response when Request Maintenance"), "EX01",
                                classNMethodName: "pgIntro.MaintenanceScheduler_OnRequestSettlement");

                            App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000047); Error on Maintenance Event;");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX05", classNMethodName: "pgIntro.StartMaintenanceThreadWorking");
            }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }


        public void MaintenanceScheduler_OnSettlementDone(object sender, SettlementDoneEventArgs e)
        {

            try
            {
                _manLock.WaitAsync().Wait();
                _maintenanceRequest = false;
            }
            catch
            { }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }
        public void InitPage()
		{
			try 
			{
				_manLock.WaitAsync().Wait();
				_maintenanceRequest = false;
				_isSystemHealthy = null;
			}
			catch { }
			finally
			{
				if (_manLock.CurrentCount == 0)
					_manLock.Release();
			}
		}


        public void AppOperationTimeScheduler_OnRequestOnOperation(object sender, EventArgs e)
		{
			try
			{
                //if (_pageLoaded == false) return;

                if (_operationTimeThreadWorker != null)
                {
                    if (((_operationTimeThreadWorker?.ThreadState & ThreadState.Aborted) == ThreadState.Aborted)
                              || ((_operationTimeThreadWorker?.ThreadState & ThreadState.Stopped) == ThreadState.Stopped)
                              )
                    {
                        _operationTimeThreadWorker = null;
                    }
                    else
                    {
                        if (_operationTimeThreadWorker != null)
                        {

                            try
                            {
                                _operationTimeThreadWorker?.Abort();
                            }
                            catch { /* By Pass Any Error */}
                            Thread.Sleep(1000);

                            _operationTimeThreadWorker = null;
                        }
                    }
                }

                if (_operationTimeThreadWorker == null)
                {
                    _operationTimeThreadWorker = new Thread(new ThreadStart(StartOperationTimeTreadWorking));
                    _operationTimeThreadWorker.IsBackground = true;
                    _operationTimeThreadWorker.Start();

                }
            }
            catch(Exception ex)
			{
                App.Log.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "pgIntro.MaintenanceScheduler_OnRequestSettlement");

            }

        }

		public void AppOperationTimeScheduler_OnRequestOffOperation(object sender, OnCloseOperationTimeEventArgs e)
		{
			
			_introAniHelp.SetOperationState(false, e.StartOperationTime ,e.EndOperationTime);
		}


		private void StartOperationTimeTreadWorking()
		{
			
            _introAniHelp.SetOperationState(true, null, null);
        }


        private NetServiceAnswerMan _netClientSvcAnswerMan = null;
		private void Submit(TransactionType transactionType)
		{
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					if (App.AppHelp.SystemHealthCheck())
					{
						if (App.ValidateSystemSetting(out string errMsg) == false)
						{
							App.MainScreenControl.Alert(detailMsg: (errMsg ?? "Error when Validate System Setting; (EXIT10000042)"));
							return;
						}
						else
                        {
							_hasBegun = true;
							App.IsLocalServerReady = true;
							_netClientSvcAnswerMan?.Dispose();

							if (transactionType == TransactionType.CollectTicket)
							{

								_netClientSvcAnswerMan = App.NetClientSvc.CollectTicketService.StartCollectTicket("Unable to start new transaction at the moment.",
									new NetServiceAnswerMan.FailLocalServerResponseCallBackDelg(delegate (string failMessage)
									{
										App.MainScreenControl.Alert(detailMsg: failMessage);
									}));
							}
							else
                            {
								_netClientSvcAnswerMan = App.NetClientSvc.SalesService.StartNewSessionCountDown("Unable to start new transaction at the moment.",
								new NetServiceAnswerMan.FailLocalServerResponseCallBackDelg(delegate (string failMessage)
								{
									App.MainScreenControl.Alert(detailMsg: failMessage);
								}), waitDelaySec: 30);
							}
						}						
					}
					else
						throw new Exception("Sales Server has unknown error; (EXIT10000041)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000044)");
					App.Log.LogError(LogChannel, "", new Exception("(EXIT10000044)", ex), "EX01", "pgIntro.Submit");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			_pageLoaded = true;
			App.ShowDebugMsg("pgIntro->Page_Loaded : Check Event Routing; Loaded event found; ");

			_introAniHelp.InitOnLoad();
			_introAniHelp.SetStartButtonEnabled(true);

			if (App.IsLocalServerReady == false)
			{
				_sysInitThreadWorker = new Thread(new ThreadStart(SystemInit));
				_sysInitThreadWorker.IsBackground = true;
				_sysInitThreadWorker.Start();
			}

			System.Windows.Forms.Application.DoEvents();

			_hasBegun = false;
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			_pageLoaded = false;
			_introAniHelp.OnPageUnload();
			try
			{
				if (_sysInitThreadWorker != null)
				{
					if (_sysInitThreadWorker.ThreadState.IsState(ThreadState.Aborted) == false)
					{
						_sysInitThreadWorker.Abort();
					}
				}
				_netClientSvcAnswerMan?.Dispose();
			}
			catch (Exception ex) { }		
		}

		
		private void SystemInit()
		{
			if (App.IsLocalServerReady == false)
			{
				try
				{
					_introAniHelp.SetStartButtonEnabled(false);

					if (App.AppHelp.SystemHealthCheck())
					{
						_introAniHelp.SetStartButtonEnabled(true);
						App.IsLocalServerReady = true;
						_isSystemHealthy = true;
					}
					else
						throw new Exception("Sales Server has unknown error; (EXIT10000043)");
				}
				catch (ThreadAbortException) { }
				catch (Exception ex)
				{
					App.Log.LogError(LogChannel, "-", ex, "EX01", "pgIntro.SystemInit");
					App.MainScreenControl.Alert(detailMsg: ex.Message);
				}
			}
		}
	}
}
