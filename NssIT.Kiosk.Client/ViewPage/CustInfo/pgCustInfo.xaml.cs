using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
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

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    /// <summary>
    /// Interaction logic for pgCustInfo.xaml
    /// </summary>
    public partial class pgCustInfo : Page, ICustInfo, IKioskViewPage
    {
        private const string LogChannel = "ViewPage";

        private CustInfoVerticalMover _custInfoVerticalMover = null;
        private CustInfoKeyBoardEntry _keyBoardDataEntry = null;
        private PassengerInfoManager _passengerInfoMan = null;
        private List<PassengerSeatNo> _passengerSeatList = new List<PassengerSeatNo>();

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private pgMyKad _myKadPage = null;
        private int _currentMyKadPssgScanInx = -1;


        public pgCustInfo()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoEnglish.xaml");

            _myKadPage = new pgMyKad();
            _custInfoVerticalMover = new CustInfoVerticalMover(this, SvPassengerInfoList);
            _keyBoardDataEntry = new CustInfoKeyBoardEntry(KbKeys);
            _passengerInfoMan = new PassengerInfoManager(this, TxtError, TxtConfirmWait, GrdPassengerList);

            _myKadPage.OnEndScan += _myKadPage_OnEndScan;
        }

        public void InitPassengerInfo(UICustInfoAck uiCustInfo)
        {
            CustomerInfoList custInfoList = (CustomerInfoList)uiCustInfo.MessageData;

            if (uiCustInfo.Session != null)
                _language = uiCustInfo.Session.Language;
            else
                _language = LanguageCode.English;

            _passengerSeatList.Clear();

            // DEBUG-Testing ----------------------------------------------------------------------------------------
            //int testMaxSeatCount = 5;
            //for (int testInx = 0; testInx < testMaxSeatCount; testInx++)
            //{
            //    _passengerSeatList.Add(new PassengerSeatNo($@"B{testInx}", $@"SIDxxxxxxxxxx-{testInx}"));
            //}
            //---------------------------------------------------------------------------------------------------

            if (custInfoList?.CustSeatInfoList?.Length > 0)
            {
                foreach (CustSeatDetail seat in custInfoList.CustSeatInfoList)
                {
                    _passengerSeatList.Add(new PassengerSeatNo(seat.Desn, seat.SeatId.ToString().Trim(), seat.SeatType));
                }
            }
            else
                throw new Exception("No seats info available.");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GrdScreenShield.Visibility = Visibility.Collapsed;

                this.Resources.MergedDictionaries.Clear();
                if (_language == LanguageCode.Malay)
                    this.Resources.MergedDictionaries.Add(_langMal);
                else
                    this.Resources.MergedDictionaries.Add(_langEng);

                App.TimeoutManager.ResetCustomerInfoTimeoutCounter();

                _currentMyKadPssgScanInx = -1;
                _hasConfirmed = false;

                FrmIdentityEntry.Content = null;
                FrmIdentityEntry.NavigationService.RemoveBackEntry();
                GrdPopUp.Visibility = Visibility.Collapsed;

                _custInfoVerticalMover.PageInit();
                _passengerInfoMan.InitPassengerContainer(_passengerSeatList.ToArray(), _language);
                ChangeScreenSize();
                _keyBoardDataEntry.FocusedTextBox(TxtPassName0);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.Page_Loaded");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.Page_Loaded");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                App.TimeoutManager.RemoveCustomerInfoTimeoutExtension();

                _currentMyKadPssgScanInx = -1;
                FrmIdentityEntry.Content = null;
                FrmIdentityEntry.NavigationService.RemoveBackEntry();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.TextBoxData_GotFocus");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.TextBoxData_GotFocus");
            }
        }

        private void TextBoxData_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                _keyBoardDataEntry.FocusedTextBox((TextBox)sender);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.TextBoxData_GotFocus");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.TextBoxData_GotFocus");
            }
        }


        private bool _hasConfirmed = false;
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_hasConfirmed == true)
                    return;
                
                bool checkResult = _passengerInfoMan.ConfirmPassengerInfo(out PassengerInfo[] passengerInfoList);

                if (checkResult)
                {
                    App.TimeoutManager.RemoveCustomerInfoTimeoutExtension();

                    App.MainScreenControl.ExecMenu.ShieldMenu();

                    App.ShowDebugMsg("pgCustInfo.Confirm_Click : Confirm Success");

                    CustSeatDetail[] submtList = new CustSeatDetail[passengerInfoList.Length];

                    int arrInx = -1;
                    foreach (PassengerInfo pssg in passengerInfoList)
                    {
                        arrInx++;
                        submtList[arrInx] = new CustSeatDetail() { Contact = pssg.Contact, CustIC = pssg.Id, CustName = pssg.Name, Desn = pssg.SeatDesn, SeatId = long.Parse(pssg.SeatId), SeatType = pssg.SeatType };
                    }

                    _hasConfirmed = true;
                    Submit(submtList);
                    //App.NetClientSvc.SalesService.SubmitPassengerInfo(submtList, out bool isServerResponded);
                    //if (isServerResponded == false)
                    //    App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000801)");
                }
                else
                    App.ShowDebugMsg("pgCustInfo.Confirm_Click : Confirm not success");
                
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.Confirm_Click");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.Confirm_Click");
            }
        }

        private void Submit(CustSeatDetail[] custSeatDetailList)
        {
            ShieldPage();
            System.Windows.Forms.Application.DoEvents();

            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.SubmitPassengerInfo(custSeatDetailList, out bool isServerResponded);

                    if (isServerResponded == false)
                        App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000801)");
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000803)");
                    App.Log.LogError(LogChannel, "", new Exception("(EXIT10000803)", ex), "EX01", "pgCustInfo.Submit");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        public void ShieldPage()
        {
            GrdScreenShield.Visibility = Visibility.Visible;
        }

        private void ScanMyKad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button scanIcButton = (Button)sender;
                _currentMyKadPssgScanInx = int.Parse(scanIcButton.Tag.ToString());

                App.MainScreenControl.ExecMenu.ShieldMenu();

                GrdPopUp.Visibility = Visibility.Visible;

                System.Windows.Forms.Application.DoEvents();

                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

                _myKadPage.InitPageData(_language, _currentMyKadPssgScanInx);
                FrmIdentityEntry.NavigationService.Navigate(_myKadPage);

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.ScanMyKad_Click");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.ScanMyKad_Click");
            }
        }

        private void _myKadPage_OnEndScan(object sender, EndOfMyKadScanEventArgs e)
        {
            try
            {
                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

                PassengerIdentity pssgId = null;
                pssgId = e.Identity;
                if (pssgId is null)
                {
                    //_currentMyKadPssgScanInx

                    App.ShowDebugMsg("pgCustInfo.ScanMyKad_Click : Unable to read Identity");
                }
                else
                {
                    if (pssgId.IsIDReadSuccess)
                    {
                        _passengerInfoMan.UpdateCustomerInfo(_currentMyKadPssgScanInx, pssgId.IdNumber, pssgId.Name);
                        App.ShowDebugMsg($@"pgCustInfo.ScanMyKad_Click : IC: {pssgId.IdNumber}; Name: {pssgId.Name}");
                    }
                    else if (string.IsNullOrWhiteSpace(pssgId.Message) == false)
                    {
                        App.ShowDebugMsg($@"pgCustInfo.ScanMyKad_Click : Error: {pssgId.Message}; ");
                    }
                    else
                    {
                        App.ShowDebugMsg($@"pgCustInfo.ScanMyKad_Click : Not response");
                    }
                }

                App.ShowDebugMsg("pgCustInfo.ScanMyKad_Click : End of MyKad Scanning");

                this.Dispatcher.Invoke(new Action(() => {
                    FrmIdentityEntry.Content = null;
                    FrmIdentityEntry.NavigationService.RemoveBackEntry();
                    GrdPopUp.Visibility = Visibility.Collapsed;

                    if ((pssgId != null) && (pssgId.IsIDReadSuccess))
                    {
                        _passengerInfoMan.SetMobileFocus(_currentMyKadPssgScanInx);
                        _passengerInfoMan.SetMobileBorderFocusEffect(_currentMyKadPssgScanInx);
                    }
                    else
                        _passengerInfoMan.SetNameFocus(_currentMyKadPssgScanInx);
                    
                }));
                
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo._myKadPage_OnEndScan");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo._myKadPage_OnEndScan");
            }
            finally
            {
                App.MainScreenControl.MainFormDispatcher.Invoke(new Action(() => {
                    App.MainScreenControl.ExecMenu.UnShieldMenu();
                }));
                System.Windows.Forms.Application.DoEvents();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _passengerInfoMan.ResetBorderFocusEffect((TextBox)sender);
        }

        private int _changeScreenSizeCount = 0;
        private void ChangeScreenSize()
        {
            if (App.MainScreenControl.QueryWindowSize
                (out double winWidth, out double winHeight) == true)
            {
                if (_changeScreenSizeCount <= 3)
                {
                    if (winHeight > 1500)
                    {
                        BdConfirm.Height = 80;
                        BtnConfirm.Height = 60;
                    }
                    _changeScreenSizeCount++;
                }
            }
        }
    }
}
