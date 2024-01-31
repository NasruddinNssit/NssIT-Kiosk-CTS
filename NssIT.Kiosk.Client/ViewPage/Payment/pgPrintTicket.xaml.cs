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
    /// Interaction logic for pgPrintTicket.xaml
    /// </summary>
    public partial class pgPrintTicket : Page
    {
        private string _logChannel = "Payment";

        public event EventHandler OnDoneClick;

        private Style _buttonEnabledStyle = null;
        private Style _buttonDisabledStyle = null;

        public pgPrintTicket()
        {
            InitializeComponent();

            _buttonEnabledStyle = this.FindResource("btnDone") as Style;
            _buttonDisabledStyle = this.FindResource("btnDoneDisabled") as Style;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            if (BtnDone.Tag.ToString().Trim().Equals("1"))
            {
                SetEnableDoneButton(false);
                RaiseOnDoneClick();
            }
        }

        public void InitPage(string message)
        {
            SetEnableDoneButton(false);
            this.Dispatcher.Invoke(new Action(() => {
                TxtMessage.Text = message;
            }));
        }

        public void UpdateMessage(PrintTicketMsgCode messageCode, string transactionNo)
        {
            this.Dispatcher.Invoke(new Action(() => {
                if (messageCode == PrintTicketMsgCode.FailTicketPrinting)
                {
                    TxtMessage.Text = $@"Unable to print your ticket ({transactionNo}). Please collect Sales Receipt.";
                }
                else
                {
                    TxtMessage.Text = $@"Unknown error for Transaction No. {transactionNo}. Please collect Sales Receipt.";
                }
            }));
        }

        public void SetEnableDoneButton(bool enabled)
        {
            this.Dispatcher.Invoke(new Action(() => {
                if (enabled)
                {
                    BtnDone.Tag = "1";
                    BtnDone.Style = _buttonEnabledStyle;
                }
                else
                {
                    BtnDone.Tag = "0";
                    BtnDone.Style = _buttonDisabledStyle;
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
    }
}
