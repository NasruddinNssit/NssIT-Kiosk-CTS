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
using static NssIT.Kiosk.Client.ViewPage.Payment.pgPaymentTypes;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    /// <summary>
    /// Interaction logic for uscPaymentGateway.xaml
    /// </summary>
    public partial class uscPaymentGateway : UserControl
    {
        private string _logChannel = "Payment";

        private string _paymentGatewayDesc = null;

        private WebImageCacheX _imageCache = null;
        private StartBTngPaymentDelg _startBTngPaymentDelgHandle = null;

        public string LogoUrl = null;
        public string PaymentGateway { get; private set; }
        public string PaymentMethod { get; private set; }

        public uscPaymentGateway()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            imgPaymentType.Source = null;

            if (string.IsNullOrWhiteSpace(LogoUrl) == false)
                UpdateImage(LogoUrl);

            TxtPaymentDesc.Text = _paymentGatewayDesc;
        }

        public void InitBTnGPaymentGateway(string paymentGateway, string paymentGatewayDesc, string logoUrl, WebImageCacheX imageCache, string paymentMethod, StartBTngPaymentDelg startBTngPaymentDelgHandle)
        {
            PaymentGateway = paymentGateway;
            PaymentMethod = paymentMethod;
            LogoUrl = logoUrl;
            _paymentGatewayDesc = paymentGatewayDesc;
            _imageCache = imageCache;
            _startBTngPaymentDelgHandle = startBTngPaymentDelgHandle;
        }

        private async void UpdateImage(string url)
        {
            try
            {
                BitmapImage bitImp = await _imageCache.GetImage(url);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    imgPaymentType.Source = bitImp;
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "*", ex, "EX01", "uscPaymentGateway.UpdateImage");
            }
        }

        private void PaymentType_Click(object sender, MouseButtonEventArgs e)
        {
            if (_startBTngPaymentDelgHandle != null)
            {
                _startBTngPaymentDelgHandle(PaymentGateway, LogoUrl, PaymentMethod);
            }
        }
    }
}
