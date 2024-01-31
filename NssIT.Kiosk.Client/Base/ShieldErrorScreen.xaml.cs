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
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.Base
{
    /// <summary>
    /// Interaction logic for ShieldErrorScreen.xaml
    /// </summary>
    public partial class ShieldErrorScreen : Window
    {
        public ShieldErrorScreen()
        {
            InitializeComponent();
        }

        public static void ShowMessage(string message)
        {
            ShieldErrorScreen win = new ShieldErrorScreen();
            win.TxtMsg.Text = message;
            win.ShowDialog();
        }
    }
}
