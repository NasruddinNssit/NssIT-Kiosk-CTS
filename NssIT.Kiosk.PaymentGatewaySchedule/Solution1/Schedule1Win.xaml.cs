using Newtonsoft.Json;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.PaymentGatewaySchedule.Base.Solution1;
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
using System.Windows.Shapes;

namespace NssIT.Kiosk.PaymentGatewaySchedule.Solution1
{
    /// <summary>
    /// Interaction logic for Schedule1Win.xaml
    /// </summary>
    public partial class Schedule1Win : Window
    {
        private const string LogChannel = "BTnGCleanUp";

        private LibShowMessageWindow.MessageWindow _msg = null;
        public Schedule1Win()
        {
            InitializeComponent();
            _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        }

        private SemaphoreSlim _lock = new SemaphoreSlim(1);
        private async void CleanUp_Click01(object sender, RoutedEventArgs e)
        {
            bool isUnLockSuccess = false;
            try
            {
                ShowMsg($@"{"\r\n"}Version : {App.SystemVersion}{"\r\n"}");
                ShowMsg("CleanUp_Click01 ..");

                isUnLockSuccess = await _lock.WaitAsync(500);

                if (isUnLockSuccess)
                {
                    ShowMsg("CleanUp_Click01 .. start ..");

                    BTnGCleanUpSalesResult res = await Trigger1.Run(_msg);

                    ShowMsg($@"{"\r\n"}{"\r\n"}
Clean-up Outstanding BTnG Sales Result
=============================================================
{JsonConvert.SerializeObject(res, Formatting.Indented)}
{"\r\n"}");
                    DbLog.GetDbLog().LogText(LogChannel, "*", res, "B01", "Schedule1Win.CleanUp_Click01");
                }
                else
                {
                    ShowMsg(".. system busy ..");

                    DbLog.GetDbLog().LogText(LogChannel, "*", ".. system busy ..", "C01", "Schedule1Win.CleanUp_Click01");
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.ToString());

                DbLog.GetDbLog().LogText(LogChannel, "*", ex, "EX01", "Schedule1Win.CleanUp_Click01");
            }
            finally
            {
                if (isUnLockSuccess)
                {
                    if (_lock.CurrentCount == 0)
                        _lock.Release();
                }
            }
        }

        private void ShowMsg(string message)
        {
            _msg?.ShowMessage(message);
        }
    }
}
