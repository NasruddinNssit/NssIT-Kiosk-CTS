using NssIT.Kiosk.AppDecorator.Common.Access;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access.Echo;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Sqlite.DB.AccessDB;
using NssIT.Kiosk.Sqlite.DB.AccessDB.Works;
using NssIT.Kiosk.Tools.ThreadMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp.JobApp
{
    /// <summary>
    /// ClassCode:EXIT62.02
    /// </summary>
    public class KioskStatesJobMan : IDisposable 
    {
        private const string LogChannel = "ServerApplication";

        private DbLog _log = null;
        private DbLog Log => _log;

        /// <summary>
        /// FuncCode:EXIT62.0201
        /// </summary>
        public KioskStatesJobMan()
        {
            _log = DbLog.GetDbLog();
        }

        /// <summary>
        /// FuncCode:EXIT62.0299
        /// </summary>
        public void Dispose() {  }

        /// <summary>
        /// FuncCode:EXIT62.0205
        /// </summary>
        public bool UpdateKioskLastRebootTime(DateTime lastRebootTime, out string errMsg)
        {
            errMsg = null;
            string errMessage = null;
            bool relVal = false;

            RunThreadMan tMan = new RunThreadMan(new Action(() => 
            {
                try
                {
                    DatabaseAx dbAx = DatabaseAx.GetAccess();
                    using (var newPayAx = new UpSertKioskLastRebootTimeAx<SuccessXEcho>(lastRebootTime))
                    {
                        using (var transResult = (UpSertKioskLastRebootTimeAx<SuccessXEcho>)dbAx.ExecCommand(newPayAx, waitDelaySec: 20))
                        {
                            if (transResult.ResultStatus.IsSuccess)
                            {
                                // return transResult.SuccessEcho;
                                relVal = true;
                            }
                            else if (transResult.ResultStatus.Error?.Message?.Length > 0)
                            {
                                throw transResult.ResultStatus.Error;
                            }
                            else
                            {
                                throw new Exception("Unknown error when adding new Kiosk States record; (EXIT62.0205.X01)");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                    Log.LogError(LogChannel, "*", ex, "EX01", "KioskStatesJobMan.UpdateKioskLastRebootTime");
                }
            }), "KioskStatesJobMan.UpdateKioskLastRebootTime", 30, LogChannel, System.Threading.ThreadPriority.AboveNormal);
            tMan.WaitUntilCompleted();

            errMsg = errMessage;

            return relVal;
        }

        /// <summary>
        /// FuncCode:EXIT62.0206
        /// </summary>
        public KioskLastRebootTimeEcho ReadKioskLastRebootTime()
        {
            KioskLastRebootTimeEcho retEcho = null;
            Exception errX = null;

            RunThreadMan tMan = new RunThreadMan(new Action(() =>
            {
                try
                {
                    DatabaseAx dbAx = DatabaseAx.GetAccess();
                    using (var newPayAx = new GetKioskLastRebootTimeAx<KioskLastRebootTimeEcho>())
                    {
                        using (var transResult = (GetKioskLastRebootTimeAx<KioskLastRebootTimeEcho>)dbAx.ExecCommand(newPayAx, waitDelaySec: 20))
                        {
                            if (transResult.ResultStatus.IsSuccess)
                            {
                                retEcho = transResult.SuccessEcho;
                            }
                            else if (transResult.ResultStatus.Error?.Message?.Length > 0)
                            {
                                throw transResult.ResultStatus.Error;
                            }
                            else
                            {
                                throw new Exception("Unknown error when Read Kiosk Last Reboot Time; (X01)");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errX = new Exception($@"Error when Read Kiosk Last Reboot Time; {ex.Message}; (EXIT62.0206.EX01)", ex);
                    Log.LogError(LogChannel, "*", ex, "EX01", "KioskStatesJobMan.ReadKioskLastRebootTime");
                }
            }), "KioskStatesJobMan.ReadKioskLastRebootTime", 30, LogChannel, System.Threading.ThreadPriority.AboveNormal);
            tMan.WaitUntilCompleted();

            if (retEcho is null)
            {
                if (errX != null)
                    throw errX;

                else
                    throw new Exception("Unknown exception when Read Kiosk Last Reboot Time; (EXIT62.0206.X20)");
            }

            return retEcho;
        }

        /// <summary>
        /// FuncCode:EXIT62.0207
        /// </summary>
        public bool UpdateKioskSettlementSuccessTime(DateTime lastSettlementSuccessTime, out SuccessXEcho successXEcho, out string errMsg)
        {
            errMsg = null;
            string errMessage = null;
            bool relVal = false;
            successXEcho = null;

            SuccessXEcho successXEchoXX = null;

            RunThreadMan tMan = new RunThreadMan(new Action(() =>
            {
                try
                {
                    DatabaseAx dbAx = DatabaseAx.GetAccess();
                    using (var newPayAx = new UpSertLastCardSettlementSuccessTimeAx<SuccessXEcho>(lastSettlementSuccessTime))
                    {
                        using (var transResult = (UpSertLastCardSettlementSuccessTimeAx<SuccessXEcho>)dbAx.ExecCommand(newPayAx, waitDelaySec: 20))
                        {
                            if (transResult.ResultStatus.IsSuccess)
                            {
                                successXEchoXX = transResult.SuccessEcho;
                                relVal = true;
                            }
                            else if (transResult.ResultStatus.Error?.Message?.Length > 0)
                            {
                                throw transResult.ResultStatus.Error;
                            }
                            else
                            {
                                throw new Exception("Unknown error when adding new Kiosk States record; (EXIT62.0207.X01)");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                    Log.LogError(LogChannel, "*", ex, "EX01", "KioskStatesJobMan.UpdateKioskLastSettlementSuccessTime");
                }
            }), "KioskStatesJobMan.UpdateKioskLastSettlementSuccessTime", 30, LogChannel, System.Threading.ThreadPriority.AboveNormal);
            tMan.WaitUntilCompleted();

            errMsg = errMessage;
            successXEcho = successXEchoXX;

            return relVal;
        }

        ///// <summary>
        ///// FuncCode:EXIT62.0208
        ///// </summary>
        //public CardSettlementSuccessTimeEcho ReadKioskLastSettlementSuccessTime(out Exception error)
        //{
        //    CardSettlementSuccessTimeEcho retEcho = null;
        //    error = null;

        //    try
        //    {
        //        DatabaseAx dbAx = DatabaseAx.GetAccess();
        //        using (var newPayAx = new GetLastCardSettlementSuccessTimeAx<CardSettlementSuccessTimeEcho>())
        //        {
        //            using (var transResult = (GetLastCardSettlementSuccessTimeAx<CardSettlementSuccessTimeEcho>)dbAx.ExecCommand(newPayAx, waitDelaySec: 20))
        //            {
        //                if (transResult.ResultStatus.IsSuccess)
        //                {
        //                    retEcho = transResult.SuccessEcho;
        //                }
        //                else if (transResult.ResultStatus.Error?.Message?.Length > 0)
        //                {
        //                    throw transResult.ResultStatus.Error;
        //                }
        //                else
        //                {
        //                    throw new Exception("Unknown error when Read Kiosk Last Settlement Success Time; (X01)");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        error = new Exception($@"Error when Read Kiosk Last Settlement Success Time; {ex.Message}; (EXIT62.0208.EX01)", ex);
        //        Log.LogError(LogChannel, "*", ex, "EX01", "KioskStatesJobMan.ReadKioskLastSettlementSuccessTime");
        //    }

        //    return retEcho;
        //}
    }
}