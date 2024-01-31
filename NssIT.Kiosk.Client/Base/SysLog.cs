using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client.Base
{
    public class SysLog
    {
        public SysLog()
        {
            string _executionFilePath = Assembly.GetExecutingAssembly().Location;

            FileInfo fInf = new FileInfo(_executionFilePath);
            CurrentExecutionFolder = fInf.DirectoryName;
        }

        public void WriteLog(string logMsg)
        {
            try
            {
                logMsg = logMsg ?? "";

                string borderLine = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
                string txt = $@"{"\r\n"}{borderLine}{"\r\n"}Text Log at : {DateTime.Now}{"\r\n"}{logMsg}";

                using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter($@"{CurrentExecutionFolder}\{TextLogName}", true, Encoding.UTF8))
                {
                    file.WriteLine(txt);
                    file.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fail to log message (X); " + ex.ToString() + $@";{"\r\n"}*****; {logMsg}");
            }
        }

        private string CurrentExecutionFolder
        {
            get; set;
        }

        private string TextLogName
        {
            get
            {
                return @"KioskSysLog.txt";
            }
        }
    }
}