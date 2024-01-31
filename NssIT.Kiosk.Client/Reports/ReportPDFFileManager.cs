using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Reports
{
    public class ReportPDFFileManager
    {
        private const string _ticketFolderName = @"Tickets";
        public string _executionFilePath;
        public string _executionFolderPath;

        public ReportPDFFileManager()
        {
            //_executionFilePath = Assembly.GetExecutingAssembly().Location;

            //FileInfo fInf = new FileInfo(_executionFilePath);
            //_executionFolderPath = fInf.DirectoryName;

            //TicketFolderPath = $@"{_executionFolderPath}\{_ticketFolderName}";

            //if (Directory.Exists(TicketFolderPath) == false)
            //{
            //    Directory.CreateDirectory(TicketFolderPath);
            //}

            TicketFolderPath = @"C:\KioskTicket";
        }

        public string TicketFolderPath { get; private set; }

        ///public string TicketPreFixName { get; private set; } = "TICK";
    }
}
