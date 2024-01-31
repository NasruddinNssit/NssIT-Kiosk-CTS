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
    /// Interaction logic for pgPaymentInfo.xaml
    /// </summary>
    public partial class pgPaymentInfo : Page
    {
        private decimal _departTotalPricePerTicket = 0M;
        private string _currency = "RM";
        private int _noOfPssg = 0;
        private decimal _departTotalAmount = 0M;
        private decimal _returnTotalAmount = 0M;
        private decimal _totalAmount = 0M;
        private string _transactionNo = "";

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public pgPaymentInfo()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentEnglish.xaml");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Resources.MergedDictionaries.Clear();

            TxtDepartDesn.Text = "";
            if (_language == LanguageCode.Malay)
            {
                this.Resources.MergedDictionaries.Add(_langMal);
                TxtDepartDesn.Text = string.Format(_langMal["PAYMENT_DEPART_AMOUNT_DESCRIPTION_Label"]?.ToString(), _currency, $@"{_departTotalPricePerTicket:#,##0.00}", _noOfPssg.ToString());
            }
            else
            {
                this.Resources.MergedDictionaries.Add(_langEng);
                TxtDepartDesn.Text = string.Format(_langEng["PAYMENT_DEPART_AMOUNT_DESCRIPTION_Label"]?.ToString(), _currency, $@"{_departTotalPricePerTicket:#,##0.00}", _noOfPssg.ToString());
            }

            if (string.IsNullOrWhiteSpace(TxtDepartDesn.Text))
                TxtDepartDesn.Text = $@"Adult ({_currency} {_departTotalPricePerTicket:#,##0.00} x {_noOfPssg})";

            TxtTransNo.Text = $@"({_transactionNo})";
            TxtDepartPrice.Text = $@"{_currency} {_departTotalAmount:#,##0.00}";
            TxtTotalAmount.Text = $@"{_currency} {_totalAmount:#,##0.00}";
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        public void InitPaymentInfo(string currency, string transactionNo, decimal departTotalPricePerTicket, int numberOfPassenger, 
            decimal departTotalAmount, decimal totalAmount, LanguageCode language)
        {
            _language = language;
            _currency = currency;
            _departTotalPricePerTicket = departTotalPricePerTicket;
            _noOfPssg = numberOfPassenger;
            _departTotalAmount = departTotalAmount;
            _totalAmount = totalAmount;
            _transactionNo = transactionNo;
        }
    }
}
