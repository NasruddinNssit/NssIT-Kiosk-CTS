using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Admin
{
    public class KioskServiceSwitching
    {
        public const string ServiceName = @"NssIT.Kiosk.Server";

        /// <summary>
        /// Return true is the service is valid to start
        /// </summary>
        /// <returns></returns>
        public static bool StartService()
        {
            List<ServiceController> svcListX = (from svc in ServiceController.GetServices()
                                                where ServiceName.Equals(svc.ServiceName?.Trim())
                                                select svc).ToList();
            if (svcListX.Count > 0)
            {
                if (svcListX[0].Status == ServiceControllerStatus.Stopped)
                {
                    Thread thr = new Thread(new ThreadStart(StartServiceThreadWorking));
                    thr.IsBackground = true;
                    thr.Start();
                    return true;
                }
            }

            return false;

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void StartServiceThreadWorking()
            {
                try
                {
                    List<ServiceController> svcList = (from svc in ServiceController.GetServices()
                                                       where ServiceName.Equals(svc.ServiceName)
                                                       select svc).ToList();
                    if (svcList.Count > 0)
                    {
                        if (svcList[0].Status == ServiceControllerStatus.Stopped)
                        {
                            frmWaiting fm = new frmWaiting("Start NssIT.Kiosk.Server (in 10 sec)");
                            fm.Show();
                            System.Windows.Forms.Application.DoEvents();

                            svcList[0].Start();

                            Task.Delay(1000 * 10).Wait();

                            string msg = $@"Service -{ServiceName}- should be started";
                        }
                        else if (svcList[0].Status == ServiceControllerStatus.Running)
                        {
                            string msg = $@"Start Service -{ServiceName}- aborted; This service is running";
                        }
                        else
                        {
                            string msg = $@"Start service fail; The service -{ServiceName}- status is {Enum.GetName(typeof(ServiceControllerStatus), svcList[0].Status)}";
                        }
                    }
                    else
                    {
                        string msg = $@"Start service fail; Unable to find service -{ServiceName}-";
                    }
                }
                catch (Exception ex)
                {
                    string msg = $@"Error starting service; {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Return true is the service is valid to stop
        /// </summary>
        /// <returns></returns>
        public static bool StopService()
        {
            List<ServiceController> svcListX = (from svc in ServiceController.GetServices()
                                                where ServiceName.Equals(svc.ServiceName?.Trim())
                                                select svc).ToList();
            if (svcListX.Count > 0)
            {
                if (svcListX[0].Status == ServiceControllerStatus.Running)
                {
                    Thread thr = new Thread(new ThreadStart(StopServiceThreadWorking));
                    thr.IsBackground = true;
                    thr.Start();
                    return true;
                }
            }

            return false;

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void StopServiceThreadWorking()
            {
                try
                {
                    List<ServiceController> svcList = (from svc in ServiceController.GetServices()
                                                       where ServiceName.Equals(svc.ServiceName?.Trim())
                                                       select svc).ToList();
                    if (svcList.Count > 0)
                    {
                        if (svcList[0].Status == ServiceControllerStatus.Stopped)
                        {
                            string msg = $@"Stop Service -{ServiceName}- aborted; This service has already stop";
                        }
                        else if (svcList[0].Status == ServiceControllerStatus.Running)
                        {
                            frmWaiting fm = new frmWaiting("Stop NssIT.Kiosk.Server (in 10 sec)");
                            fm.Show();
                            System.Windows.Forms.Application.DoEvents();

                            svcList[0].Stop();

                            Task.Delay(1000 * 10).Wait();

                            string msg = $@"Service -{ServiceName}- should be stoped";
                        }
                        else
                        {
                            string msg = $@"Stop service fail; The service -{ServiceName}- status is {Enum.GetName(typeof(ServiceControllerStatus), svcList[0].Status)}";
                        }
                    }
                    else
                    {
                        string msg = $@"Stop service fail; Unable to find service -{ServiceName}-";
                    }
                }
                catch (Exception ex)
                {
                    string msg = $@"Error starting service; {ex.Message}";
                    //CYA-DEBUG .. need to handle error here..
                }
            }
        }
    }
}
