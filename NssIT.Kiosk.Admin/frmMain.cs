using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using LibShowMessageWindow;
using System.Reflection;
using System.IO;

namespace NssIT.Kiosk.Admin
{
    public partial class frmMain : Form
    {
        bool _isDebugOn = false;

        private MessageWindow _msgWin = null;

        private const string _logDBName = @"NssITKioskLog";
        private const string _serverLogPath = @"LocalServer\LogDB";
        private const string _clientLogPath = @"Client\LogDB";

        public string _executionFilePath;
        public string _executionFolderPath;
        public string _baseFolderPath;

        private LogQuery _svrLogQuery = null;
        private LogQuery _clientLogQuery = null;

        public frmMain()
        {
            InitializeComponent();

            dtpLogDate.Value = DateTime.Now;

            _executionFilePath = Assembly.GetExecutingAssembly().Location;

            FileInfo fInf = new FileInfo(_executionFilePath);
            _executionFolderPath = fInf.DirectoryName;
            _baseFolderPath = fInf.Directory.Parent.FullName;

            if (_isDebugOn)
                _msgWin = new MessageWindow();

            grdLog.AutoGenerateColumns = false;
        }

        private DateTime GetSeletedDate()
        {
            return dtpLogDate.Value;
        }

        private LogQuery SvrLogQuery
        {
            get
            {
                string svrLogPath = GetDBFilePath(_baseFolderPath, _serverLogPath, GetSeletedDate());

                if (_svrLogQuery is null)
                {
                    _svrLogQuery = new LogQuery(svrLogPath);
                }
                else
                {
                    _svrLogQuery.Init(svrLogPath);
                }
                return _svrLogQuery;
            }
        }

        private LogQuery ClientLogQuery
        {
            get
            {
                string clientLogPath = GetDBFilePath(_baseFolderPath, _clientLogPath, GetSeletedDate());
                if (_clientLogQuery is null)
                {
                    _clientLogQuery = new LogQuery(clientLogPath);
                }
                else
                {
                    _clientLogQuery.Init(clientLogPath);
                }
                return _clientLogQuery;
            }
        }

        private void btnQueryLog_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable("KioskLog");
            dt.Columns.AddRange(new DataColumn[] {
                new DataColumn(){ AllowDBNull = true, ColumnName = "TimeStr", DataType = typeof(string), ReadOnly = true, DefaultValue = null },
                new DataColumn(){ AllowDBNull = true, ColumnName = "AdminMsg", DataType = typeof(string), ReadOnly = true, DefaultValue = null },
                new DataColumn(){ AllowDBNull = true, ColumnName = "ProcId", DataType = typeof(string), ReadOnly = true, DefaultValue = null },
                new DataColumn(){ AllowDBNull = true, ColumnName = "Time", DataType = typeof(Int64), ReadOnly = true, DefaultValue = null }
            });
            dt.AcceptChanges();

            try
            {
                DateTime selectedDate = GetSeletedDate();
                // Read Log From Server
                DataTable svrLog = SvrLogQuery.GetLog(selectedDate);
                foreach (DataRow rw in svrLog.Rows)
                {
                    DataRow newRW = dt.NewRow();
                    newRW["TimeStr"] = rw["TimeStr"];
                    newRW["AdminMsg"] = rw["AdminMsg"];
                    newRW["ProcId"] = rw["ProcId"];
                    newRW["Time"] = rw["Time"];

                    dt.Rows.Add(newRW);
                }
                dt.AcceptChanges();
                //------------------------------------------------------
                // Read Log From Client
                DataTable clientLog = ClientLogQuery.GetLog(selectedDate);
                foreach (DataRow rw in clientLog.Rows)
                {
                    DataRow newRW = dt.NewRow();
                    newRW["TimeStr"] = rw["TimeStr"];
                    newRW["AdminMsg"] = rw["AdminMsg"];
                    newRW["ProcId"] = rw["ProcId"];
                    newRW["Time"] = rw["Time"];

                    dt.Rows.Add(newRW);
                }
                dt.AcceptChanges();
                //------------------------------------------------------
                dt.DefaultView.Sort = "Time";

                grdLog.DataSource = dt.DefaultView;
                grdLog.Refresh();

                lblLatestQueryDate.Text = $@"Latest Query Date : {selectedDate.ToString("dd MMM yyyy")}";

                ShowMsg("Query - Done");
            }
            catch (Exception ex)
            {
                ShowMsg(ex.ToString());
            }
        }

        private string GetDBFilePath(string basePath, string appLogPath, DateTime refTime)
        {
            string LogDBFolderPath = $@"{basePath}\{appLogPath}";
            string fileFolder = $@"{LogDBFolderPath}\{refTime.ToString("yyyy")}";

            DateTime logDBPostFixTime = GetLogDBPostFixDate(refTime);
            string filePath = $@"{fileFolder}\{_logDBName}{logDBPostFixTime.ToString("yyyyMMdd")}.db";

            FileInfo fInf = new FileInfo(filePath);
            if (fInf.Exists == false)
            {
                MessageBox.Show($@"Log DB file ({filePath}) not exist.");
            }

            return filePath;

        }

        public static DateTime GetLogDBPostFixDate(DateTime refTime)
        {
            //10 Days for one Log DB file.

            DateTime logDBPostFixTime = DateTime.Now;

            int expYear = 0;
            int expMonth = 0;
            int expDay = 0;

            if (refTime.Day > 20)
            {
                expYear = refTime.Year;
                expMonth = refTime.Month;
                expDay = 21;
            }
            else if (refTime.Day > 10)
            {
                expYear = refTime.Year;
                expMonth = refTime.Month;
                expDay = 11;
            }
            else
            {
                expYear = refTime.Year;
                expMonth = refTime.Month;
                expDay = 1;
            }

            logDBPostFixTime = new DateTime(expYear, expMonth, expDay, 0, 0, 0, 0);

            return logDBPostFixTime;
        }

        private void ShowMsg(string msg)
        {
            if (_isDebugOn)
            {
                _msgWin.ShowMessage(msg);
            }
            else
            {
                txtMsg.Text += $@"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {msg}{"\r\n"}";

                if (txtMsg.Text.Length > 8000)
                {
                    txtMsg.Text = txtMsg.Text.Substring(3000);
                }

                if (txtMsg.Text.Length > 5)
                {
                    txtMsg.SelectionStart = txtMsg.Text.Length - 1;
                    txtMsg.SelectionLength = 1;
                    txtMsg.ScrollToCaret();
                }
            }

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                if (KioskServiceSwitching.StopService())
                {
                    ShowMsg($@"Service -Nssit.Kiosk.Server- should be stoped");
                }
                else
                {
                    ShowMsg($@"-Nssit.Kiosk.Server- may already stoped");
                }
            }
            catch (Exception ex)
            {
                ShowMsg($@"Error stopping -NssIT.Kiosk.Server service-; {ex.Message}");
            }
        }
    }
}
