using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Admin
{
    public class LogQuery
    {
        private string _dbFileFullPath = "";

		public LogQuery(string dbFileFullPath)
        {
            _dbFileFullPath = dbFileFullPath;
		}

        public void Init(string dbFileFullPath)
        {
            _dbFileFullPath = dbFileFullPath;
        }

		public DataTable GetLog(DateTime logDay)
        {
            FileInfo fInf = new FileInfo(_dbFileFullPath);
            if (fInf.Exists == false)
                throw new Exception($@"{_dbFileFullPath} file not found.");

            string dbSvrConnStr = $@"Data Source={_dbFileFullPath};Version=3";

            DateTime startTime = new DateTime(logDay.Year, logDay.Month, logDay.Day, 0, 0, 0, 000);
            DateTime endTime = new DateTime(logDay.Year, logDay.Month, logDay.Day, 23, 59, 59, 999);

            SQLiteConnection oConn = new SQLiteConnection(dbSvrConnStr);
            SQLiteDataAdapter oAdp = new SQLiteDataAdapter($@"SELECT * FROM KioskLog WHERE GotAdminMsg=1 AND TIME >= {startTime.Ticks} AND TIME <= {endTime.Ticks} ORDER BY LogId", oConn);

            DataTable dt = new DataTable("KioskLog");
            oAdp.Fill(dt);

            oAdp.Dispose();
            oConn.Dispose();

            return dt;
        }

		

		
	}
}
