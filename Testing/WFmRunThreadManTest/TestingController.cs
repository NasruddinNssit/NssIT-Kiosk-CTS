using NssIT.Kiosk.Common.Tools.ThreadMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFmRunThreadManTest
{
    public class TestingController
    {
        private static List<RunThreadMan> _tManList = new List<RunThreadMan>();
        private static List<RunThreadMan> _tManListX = new List<RunThreadMan>();
        private static List<RunThreadMan> _tManListY = new List<RunThreadMan>();
        private static List<RunThreadMan> _tManListZ = new List<RunThreadMan>();
        private static LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

        public static void Start()
        {
            Guid id = Guid.NewGuid();

            _msg.ShowMessage($@"TestingController.Start; Start Testing - {id}");

            Task.Factory.StartNew(new Action(() => 
            {
                TestWorkingThread tW = null;

                for (int testCount = 0; testCount < 10; testCount++)
                {
                    tW = new TestWorkingThread(5);
                    _tManList.Add(new RunThreadMan(new Action(() =>
                    {
                        tW.StartThreadWorking();
                    }), $@"TestingController.Start->Type_A; Section ID: {id}", 20, "TestingController"));
                }

                /////For AbortThreadManStates test
                for (int testCount = 0; testCount < 10; testCount++)
                {
                    tW = new TestWorkingThread(20);
                    RunThreadMan tMan = new RunThreadMan(new Action(() =>
                    {
                        tW.StartThreadWorking();
                    }), $@"TestingController.Start->Type_A; Section ID: {id}", 20, "TestingController");
                    _tManList.Add(tMan);
                    _tManListX.Add(tMan);
                }

                for (int testCount = 0; testCount < 10; testCount++)
                {
                    tW = new TestWorkingThread(20);
                    _tManList.Add(new RunThreadMan(new Action(() =>
                    {
                        tW.StartThreadWorking();
                    }), $@"TestingController.Start->Type_A; Section ID: {id}", 20, "TestingController"));
                }

                ///// For WaitToEndThreadMan test
                for (int testCount = 0; testCount < 10; testCount++)
                {
                    tW = new TestWorkingThread(30);
                    RunThreadMan tMan = new RunThreadMan(new Action(() =>
                    {
                        tW.StartThreadWorking();
                    }), $@"TestingController.Start->Type_A; Section ID: {id}", 40, "TestingController");
                    _tManList.Add(tMan);
                    _tManListY.Add(tMan);
                }

            }));

            _msg.ShowMessage($@"TestingController.Start; End Testing  - {id}");
        }

        public static void ShowAllThreadManStates()
        {
            try
            {
                int countInx = 0;
                foreach (RunThreadMan tMan in _tManList)
                {
                    countInx++;
                    _msg.ShowMessage($@"RunThreadMan ==> {tMan.GetLogString()}");
                }
                _msg.ShowMessage($@"Total Number of RunThreadThreadMan : {countInx} ; List Count: {_tManList.Count};..ended");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage("Error at ShowAllThreadManStates; " + ex.ToString());
            }
        }

        public static void ClearAllThreadManStates()
        {
            try
            {
                _tManList.Clear();
                _tManListX.Clear();
                _tManListY.Clear();

                _msg.ShowMessage($@"Clear All RunThreadMan .. Done");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage("Error at ClearAllThreadManStates; " + ex.ToString());
            }
        }

        public static void AbortThreadManStates()
        {
            try
            {
                int countInx = 0;
                foreach (RunThreadMan tMan in _tManListX)
                {
                    countInx++;
                    tMan.AbortRequest(out bool isThreadEnd);
                    _msg.ShowMessage($@"Abort ThreadProcessId : {tMan.ThreadProcessId}; Last Status - isThreadEnd: {isThreadEnd}");
                }
                _msg.ShowMessage($@"Total Number of aborted RunThreadThreadMan : {countInx} ; Aborted List Count: {_tManListX.Count};..ended");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage("Error at AbortThreadManStates; " + ex.ToString());
            }
        }

        public static void WaitToEndThreadMan()
        {
            try
            {
                int countInx = 0;
                foreach (RunThreadMan tMan in _tManListY)
                {
                    countInx++;

                    RunThreadMan tM = new RunThreadMan(new Action(() =>
                    {
                        _msg.ShowMessage($@"Start WaitToEndThreadMan =====> ThreadProcessId: {tMan.ThreadProcessId}; IsEnd: {tMan.IsEnd}; IsAbortRequested: {tMan.IsAbortRequested}");
                        tMan.WaitUntilCompleted();
                        _msg.ShowMessage($@"End WaitToEndThreadMan =====> ThreadProcessId: {tMan.ThreadProcessId}; IsEnd: {tMan.IsEnd}; IsAbortRequested: {tMan.IsAbortRequested}");
                    }), $@"TestingController.Start->Type_Z; (WaitToEndThreadMan); Thread Proc.ID: {tMan.ThreadProcessId}", 45, "TestingController");
                    _tManList.Add(tM);
                    _tManListZ.Add(tM);

                    _msg.ShowMessage($@"WaitToEnd ThreadProcessId : {tMan.ThreadProcessId}; Last Status - isThreadEnd: {tMan.IsEnd}");
                }
                _msg.ShowMessage($@"Total Number of WaitToEnd RunThreadThreadMan : {countInx} ; WaitToEnd List Count: {_tManListY.Count};..ended");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage("Error at WaitToEndThreadMan; " + ex.ToString());
            }
        }
    }
}
