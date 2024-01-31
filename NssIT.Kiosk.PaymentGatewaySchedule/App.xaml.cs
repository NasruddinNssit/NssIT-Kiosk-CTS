using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.PaymentGatewaySchedule.Base;
using NssIT.Kiosk.PaymentGatewaySchedule.Base.Solution1;
using NssIT.Kiosk.PaymentGatewaySchedule.Solution1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.PaymentGatewaySchedule
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string LogChannel = "";

        public static string SystemVersion = "V1.R220405.1";

        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine($@"NssIT.Train.Kiosk.PaymentGatewaySchedule Version : {App.SystemVersion}{"\r\n"}");
            Console.WriteLine("Init Payment Gateway Schedule ..");

            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            String thisprocessname = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
            {
                Console.WriteLine("Twice access to PaymentGatewaySchedule Application is prohibited !!");
                Console.WriteLine("End in 5 seconds ..");
                Task.Delay(5000).Wait();
                System.Windows.Application.Current.Shutdown();
                return;
            }

            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

            SysInit.Start(SystemVersion);

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///// Console App.
            Task.Factory.StartNew(new Action(() =>
            {
                RunBTnGCleanUpConsole();
            })).Wait();

            Console.WriteLine("End in 5 seconds ..");

            Task.Delay(5000).Wait();

            System.Windows.Application.Current.Shutdown();
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            ///// Testing - Windows App.
            //Schedule1Win main = new Schedule1Win();
            //main.Show();
            //System.Windows.Forms.Application.DoEvents();
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        }

        private void RunBTnGCleanUpConsole()
        {
            try
            {
                Console.WriteLine("Start 'Boost/Touch n Go' CleanUp ..");

                BTnGCleanUpSalesResult res = Trigger1.Run(null).GetAwaiter().GetResult();

                Console.WriteLine($@"{"\r\n"}{"\r\n"}
Clean-up Outstanding BTnG Sales Result
=============================================================
{JsonConvert.SerializeObject(res, Formatting.Indented)}
{"\r\n"}");
                DbLog.GetDbLog().LogText(LogChannel, "*", res, "B01", "App.RunBTnGCleanUpConsole");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                DbLog.GetDbLog().LogText(LogChannel, "*", ex, "EX01", "App.RunBTnGCleanUpConsole");
            }
        }
    }
}
