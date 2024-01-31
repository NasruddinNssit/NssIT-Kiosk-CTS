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

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.Navigation
{
    /// <summary>
    /// Interaction logic for uscCTNavigation.xaml
    /// </summary>
    public partial class uscCTNavigation : UserControl
    {
        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _currentRosLang = null;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public uscCTNavigation()
        {
            InitializeComponent();
        }

        public void LoadControl(LanguageCode language)
        {
            LoadRos();
            if (language == LanguageCode.Malay)
            {
                TxtExit.Text = _langMal["EXITLabel"].ToString();
                TxtPrevioust.Text = _langMal["PREVIOUSLabel"].ToString();
            }
            else
            {
                TxtExit.Text = _langEng["EXITLabel"].ToString();
                TxtPrevioust.Text = _langEng["PREVIOUSLabel"].ToString();
            }
        }

        private void LoadRos()
        {
            if ((_langMal is null) || (_langEng is null))
            {
                _langMal = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\Navigation\rocNavigationMalay.xaml");
                _langEng = CommonFunc.GetXamlResource(@"ViewPage\BoardingPass\Navigation\rocNavigationEnglish.xaml");
            }
        }

        private void BdExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                bool isServerResponse = false;

                App.NetClientSvc?.CollectTicketService?.ResetUserSession(out isServerResponse, 20);

                if (isServerResponse)
                {
                    App.MainScreenControl.ShowWelcome();
                }
                else
                {
                    throw new Exception("Local Server not responding upon EXIT.");
                }
            }
            catch (Exception ex)
            {
                App.MainScreenControl.Alert(detailMsg: ex.Message);
            }
        }

        private void BdPrevious_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
