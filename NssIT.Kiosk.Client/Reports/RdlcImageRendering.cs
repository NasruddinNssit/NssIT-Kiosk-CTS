using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Reports
{
    public class RdlcImageRendering
    {
        public static LocalReport CreateLocalReport(string fullReportPath, ReportDataSource[] dataList)
        {
            if ((dataList is null) || (dataList?.Length == 0))
            {
                throw new Exception("Report data not found");
            }

            LocalReport report = new LocalReport();
            report.EnableHyperlinks = true;
            report.EnableExternalImages = true;
            report.ReportPath = fullReportPath;

            for (int inx = 0; inx < dataList.Length; inx++)
            {
                report.DataSources.Add(dataList[inx]);
            }

            return report;
        }

        public static Stream[] Export(LocalReport report, ReportImageSize imgSize)
        {
            string deviceInfo =
                $@"<DeviceInfo>
                <OutputFormat>EMF</OutputFormat>
                <PageWidth>{ReportImageSize.GetDeviceInfoSize(imgSize.Width, imgSize.UnitMeasurement)}</PageWidth>
                <PageHeight>{ReportImageSize.GetDeviceInfoSize(imgSize.Height, imgSize.UnitMeasurement)}</PageHeight>
                <MarginTop>{ReportImageSize.GetDeviceInfoSize(imgSize.TopMargin, imgSize.UnitMeasurement)}</MarginTop>
                <MarginLeft>{ReportImageSize.GetDeviceInfoSize(imgSize.LeftMargin, imgSize.UnitMeasurement)}</MarginLeft>
                <MarginRight>{ReportImageSize.GetDeviceInfoSize(imgSize.RightMargin, imgSize.UnitMeasurement)}</MarginRight>
                <MarginBottom>{ReportImageSize.GetDeviceInfoSize(imgSize.BottomMargin, imgSize.UnitMeasurement)}</MarginBottom>
            </DeviceInfo>";

            Warning[] warnList;

            using (PageImageStream pageImgStream = new PageImageStream())
            {
                report.Render("Image", deviceInfo, pageImgStream.GetStreamCallback, out warnList);

                int errorCount = 0;


                if (warnList != null)
                    errorCount = (from warn in warnList
                                  where warn.Severity == Severity.Error
                                  select warn).ToArray().Length;

                if (errorCount > 0)
                {
                    string eMsg = "Report Image Rendering Error; ";

                    foreach (Warning warn in warnList)
                    {
                        eMsg += $@"Type: {Enum.GetName(typeof(Severity), warn.Severity)}; Msg : {warn.Message}; Code: {warn.Code}; ###";
                    }

                    throw new Exception(eMsg);
                }

                // CYA-DEBUG -- Save Images To Files
                // foreach(Stream stream in pageImgStream._pageStreamList)
                //    stream.Position = 0;

                foreach (Stream stream in pageImgStream._pageStreamList)
                    stream.Position = 0;

                return pageImgStream._pageStreamList.ToArray();
            }
        }

        class PageImageStream : IDisposable
        {
            public List<Stream> _pageStreamList = new List<Stream>();

            public Stream GetStreamCallback(string name, string fileNameExtension, Encoding encoding, string mineType, bool willSeek)
            {
                Stream stream = new MemoryStream();
                _pageStreamList.Add(stream);
                return stream;
            }

            public void Dispose()
            {
                try
                {
                    if (_pageStreamList != null)
                        _pageStreamList?.Clear();

                    _pageStreamList = null;
                }
                catch { /*By Pass*/ }
            }
        }
    }

    public enum ReportImageOrientation
    {
        Potrait = 0
    }

    public class RdlcData : IDisposable
    {
        public ReportDataSource DataSource { get; private set; }
        public string Name { get; private set; }

        public RdlcData(ReportDataSource dataSource, string name)
        {
            if (dataSource is null)
                throw new Exception("Invalid report data source");

            if ((name ?? "").Trim().Length == 0)
                throw new Exception("Invalid report data name");

            DataSource = dataSource;
            Name = name;
        }

        public void Dispose()
        {
            DataSource = null;
        }
    }

    public class ReportImageSize
    {
        public decimal Width { get; private set; }
        public decimal Height { get; private set; }
        public decimal TopMargin { get; private set; }
        public decimal BottomMargin { get; private set; }
        public decimal LeftMargin { get; private set; }
        public decimal RightMargin { get; private set; }
        public ReportImageSizeUnitMeasurement UnitMeasurement { get; private set; }

        public ReportImageSize(decimal width, decimal height,
            decimal topMargin, decimal bottomMargin,
            decimal leftMargin, decimal rightMargin,
            ReportImageSizeUnitMeasurement unitMeasurement)
        {
            Width = width;
            Height = height;
            UnitMeasurement = unitMeasurement;

            TopMargin = topMargin;
            BottomMargin = bottomMargin;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
        }

        public static string GetDeviceInfoSize(decimal size, ReportImageSizeUnitMeasurement imgUnitM)
        {
            if (imgUnitM == ReportImageSizeUnitMeasurement.Inch)
                return $@"{size:0.00}in";
            else
                return "1.00in";
        }
    }

    public enum ReportImageSizeUnitMeasurement
    {
        Inch = 0
    }
}