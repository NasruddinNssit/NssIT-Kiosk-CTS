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

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    /// <summary>
    /// Interaction logic for pgPrintTicket2.xaml
    /// </summary>
    public partial class pgPrintTicket2 : Page
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
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public pgPrintTicket2()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentEnglish.xaml");

            _greenButton = StpButtons.FindResource("btnGreen") as Style;
            _grayButton = StpButtons.FindResource("btnGray") as Style;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _screenHasPaused = false;

            TxtTransNo.Text = _transactionNo;
            TxtTransNo2.Text = _transactionNo;

            if (_language == LanguageCode.Malay)
                this.Resources.MergedDictionaries.Add(_langMal);
            else
                this.Resources.MergedDictionaries.Add(_langEng);

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
            _printType = PrintType.Success;
        }

        public void InitFailPaymentCompleted(string transactionNo, LanguageCode language)
        {
            _transactionNo = transactionNo;
            _language = language;
            _printType = PrintType.Fail;
        }

        public void UpdateCompleteTransactionState(bool isTransactionSuccess, LanguageCode language)
        {
            _language = language;

            this.Dispatcher.Invoke(new Action(() => {

                if (_language == LanguageCode.Malay)
                    this.Resources.MergedDictionaries.Add(_langMal);
                else
                    this.Resources.MergedDictionaries.Add(_langEng);

                BtnDone.Visibility = Visibility.Visible;
                System.Windows.Forms.Application.DoEvents();

                if (isTransactionSuccess) 
                {
                    BtnPause.Visibility = Visibility.Collapsed;
                }
                else
                {
                    GrdSuccessPrintMsg.Visibility = Visibility.Collapsed;
                    GrdFailPrintMsg.Visibility = Visibility.Visible;
                    BtnPause.Visibility = Visibility.Visible;
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
                App.Log.LogError(_logChannel, "", new Exception("Unhandled event exception.", ex), "EX01", "pgPrintTicket.RaiseOnDoneClick");
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
                App.Log.LogError(_logChannel, "", new Exception("Unhandled event exception.", ex), "EX01", "pgPrintTicket.RaiseOnPauseClick");
            }
        }

        enum PrintType
        {
            Fail = 0,
            Success = 1
        }
    }
}
