using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
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

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.CTPayment
{
    /// <summary>
    /// Interaction logic for pgCTPrintTicket.xaml
    /// </summary>
    public partial class pgCTPrintTicket : Page
    {
        public event EventHandler OnDoneClick;
        public event EventHandler OnPauseClick;

        private string _logChannel = "Payment";

        private string _transactionNo = "";
        private PrintType _printType = PrintType.Fail;

        private Style _greenButton = null;
        private Style _grayButton = null;

        private bool _screenHasPaused = false;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _currentRosLang = null;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public pgCTPrintTicket()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\CTPayment\rosCTPaymentMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\CTPayment\rosCTPaymentEnglish.xaml");
            _currentRosLang = _langEng;

            _greenButton = StpButtons.FindResource("btnGreen") as Style;
            _grayButton = StpButtons.FindResource("btnGray") as Style;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _screenHasPaused = false;

            TxtTransNo.Text = _transactionNo;
            TxtTransNo2.Text = _transactionNo;

            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(_currentRosLang);

            if (_printType == PrintType.Success)
            {
                BtnPause.Style = _greenButton;
                BtnDone.Visibility = Visibility.Collapsed;

                GrdSuccessPrintMsg.Visibility = Visibility.Visible;
                GrdFailPrintMsg.Visibility = Visibility.Collapsed;
            }
            else
            {
                BtnPause.Style = _greenButton;
                BtnDone.Visibility = Visibility.Visible;

                GrdSuccessPrintMsg.Visibility = Visibility.Collapsed;
                GrdFailPrintMsg.Visibility = Visibility.Visible;
            }

            System.Windows.Forms.Application.DoEvents();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            RaiseOnDoneClick();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (_screenHasPaused == false)
            {
                RaiseOnPauseClick();
                _screenHasPaused = true;
                BtnPause.Style = _grayButton;
            }
        }

        public void InitSuccessPaymentCompleted(string transactionNo, LanguageCode language)
        {
            _transactionNo = transactionNo;
            _language = language;

            if (_language == LanguageCode.Malay)
                _currentRosLang = _langMal;
            else
                _currentRosLang = _langEng;

            _printType = PrintType.Success;
        }

        public void InitFailPaymentCompleted(string transactionNo, LanguageCode language)
        {
            _transactionNo = transactionNo;
            _language = language;

            if (_language == LanguageCode.Malay)
                _currentRosLang = _langMal;
            else
                _currentRosLang = _langEng;

            _printType = PrintType.Fail;
        }

        public void UpdateCompleteTransactionState(bool isTransactionSuccess, LanguageCode language)
        {
            _language = language;

            this.Dispatcher.Invoke(new Action(() => {

                if (_language == LanguageCode.Malay)
                    _currentRosLang = _langMal;
                else
                    _currentRosLang = _langEng;

                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_currentRosLang);

                BtnDone.Visibility = Visibility.Visible;
                System.Windows.Forms.Application.DoEvents();

                if (isTransactionSuccess) { }
                else
                {
                    GrdSuccessPrintMsg.Visibility = Visibility.Collapsed;
                    GrdFailPrintMsg.Visibility = Visibility.Visible;
                }
            }));
        }

        public void RaiseOnDoneClick()
        {
            try
            {
                OnDoneClick?.Invoke(null, new EventArgs());
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "", new Exception("Unhandled event exception.", ex), "EX01", "pgCTPrintTicket.RaiseOnDoneClick");
            }
        }

        public void RaiseOnPauseClick()
        {
            try
            {
                OnPauseClick?.Invoke(null, new EventArgs());
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "", new Exception("Unhandled event exception.", ex), "EX01", "pgCTPrintTicket.RaiseOnPauseClick");
            }
        }

        enum PrintType
        {
            Fail = 0,
            Success = 1
        }
    }
}
