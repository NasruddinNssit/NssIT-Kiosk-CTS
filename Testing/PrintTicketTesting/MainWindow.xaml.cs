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

namespace PrintTicketTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibShowMessageWindow.MessageWindow _msg = null;
        public MainWindow()
        {
            InitializeComponent();

            _msg = new LibShowMessageWindow.MessageWindow();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void TestPrintTicket_Click(object sender, RoutedEventArgs e)
        {

            string adobeReaderFullFilePath = TxtAdobe.Text;
            string fileNm = TxtPDFFileLocation.Text;

            Thread printThread = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    //NssIT.Kiosk.Client.Reports.PDFTools.AdobeReaderFullFilePath = adobeReaderFullFilePath;
                    //NssIT.Kiosk.Client.Reports.PDFTools.PrintPDFsX2(fileNm, "-", this.Dispatcher);
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }

            })));

            printThread.IsBackground = true;
            printThread.Start();


        }        
    }
}
