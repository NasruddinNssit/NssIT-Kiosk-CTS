using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Reports
{
    public class ImagePrintingTools
    {
        private const string _logChannel = "Printing";

        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private static ImagePrintingTools _printService = null;

        private ConcurrentQueue<PrintImageDocument> _printDoc = new ConcurrentQueue<PrintImageDocument>();
        private bool _isBusy = false;

        private PrintImageDocument _currentPrintDoc = null;
        private string _currentTransactionNo = "#####";
        private int _currentPageIndex = 0;
        private Stream[] _currentPageStreamList = null;
        private ReportImageSize _currentReportImageSize = null;

        private ImagePrintingTools()
        { }

        private static ImagePrintingTools PrintSvc
        {
            get
            {
                if (_printService == null)
                {
                    try
                    {
                        _semaphore.WaitAsync().Wait();

                        if (_printService == null)
                            _printService = new ImagePrintingTools();
                    }
                    catch (Exception ex)
                    { }
                    finally
                    {
                        if (_semaphore.CurrentCount == 0)
                            _semaphore.Release();
                    }
                }

                return _printService;
            }
        }

        /// <summary>
        /// Caution, only call once in one sales transaction !!
        /// </summary>
        public static void InitService()
        {
            try
            {
                if (_semaphore.CurrentCount == 0)
                    _semaphore.Release();

                while (PrintSvc._printDoc.Count > 0)
                    PrintSvc._printDoc.TryDequeue(out PrintImageDocument extraDoc);

                PrintSvc._currentPageIndex = 0;
                PrintSvc._currentPageStreamList = null;
                PrintSvc._currentTransactionNo = "#####";
                PrintSvc._currentPrintDoc = null;
                PrintSvc._isBusy = false;
            }
            catch { }
        }

        private void PrintPage(object sender, PrintPageEventArgs evt)
        {
            try
            {
                // Initiate First Printing Document -------------------------------------------
                if (_currentPageStreamList == null)
                {
                    if (LoadPrintDocument() == false)
                    {
                        evt.HasMorePages = false;
                        _currentPageStreamList = null;
                        _isBusy = false;

                        App.Log.LogText(_logChannel, _currentTransactionNo ?? "*****", "End Printing - No print doc found", "A05", "ImagePrintingTools.PrintPage",
                            adminMsg: "End Printing");

                        return;
                    }
                }

                // Send Image for Printing -----------------------------------------------------
                Metafile pageImage = new Metafile(_currentPageStreamList[_currentPageIndex]);

                // .. up to this moment, system only support Inch unit.
                Rectangle adjustedRect = new Rectangle(0, 0,
                    Convert.ToInt32((_currentReportImageSize.Width * 300)),
                    Convert.ToInt32((_currentReportImageSize.Height * 300)));

                //ev.Graphics.FillRectangle(Brushes.White, adjustedRect);

                // Draw the report content :: GraphicsUnit.Document (300 pixel per inch)
                evt.Graphics.PageUnit = GraphicsUnit.Document;
                evt.Graphics.DrawImage(pageImage, adjustedRect);

                //..set page index to next number.
                _currentPageIndex++;

                // End of Printing Page  ------------------------------------------------------
                if (_currentPageStreamList.Length > _currentPageIndex)
                {
                    evt.HasMorePages = true;
                }
                else
                {
                    if (LoadPrintDocument() == true)
                        evt.HasMorePages = true;
                    else
                    {
                        evt.HasMorePages = false;
                        _currentPageStreamList = null;
                        _isBusy = false;

                        App.Log.LogText(_logChannel, _currentTransactionNo ?? "*****", "End Printing", "A15", "ImagePrintingTools.PrintPage",
                            adminMsg: "End Printing");

                        return;
                    }
                }
                //-----------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, _currentTransactionNo, ex, "EX01", "ImagePrintingTools.PrintPage",
                    adminMsg: $@"Printing error occured; Error: {ex.Message}");
            }

            return;

            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            bool LoadPrintDocument()
            {
                if (_printDoc.TryDequeue(out PrintImageDocument doc) == true)
                {
                    _currentPrintDoc = doc;
                    _currentPageIndex = 0;
                    _currentPageStreamList = doc.ImageStreamList;
                    _currentReportImageSize = doc.ReportImageSize;
                    _currentTransactionNo = doc.TransactionNo;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// return false when print service is busy.
        /// </summary>
        /// <param name="imageStreamList"></param>
        /// <param name="transactionNo"></param>
        /// <param name="imageSize"></param>
        /// <returns></returns>
        public static bool AddPrintDocument(Stream[] imageStreamList, string transactionNo, ReportImageSize imageSize)
        {
            if (PrintSvc._isBusy == false)
            {
                if ((imageStreamList?.Length > 0) && (imageSize != null))
                {
                    PrintSvc._printDoc.Enqueue(new PrintImageDocument(transactionNo, imageStreamList, imageSize));
                }

            }
            return !PrintSvc._isBusy;
        }

        public static void WaitForPrinting(int maxWaitSec)
        {
            if (maxWaitSec < 0)
                return;

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(maxWaitSec);

            while ((endTime.Subtract(DateTime.Now).TotalSeconds > 0) && (PrintSvc._isBusy))
            {
                Thread.Sleep(300);
                System.Windows.Forms.Application.DoEvents();
            }
        }

        /// <summary>
        /// return false when print service is busy.
        /// </summary>
        /// <returns></returns>
        public static bool ExecutePrinting(string transactionNo)
        {
            bool isBusy = PrintSvc._isBusy;

            if (!isBusy)
            {
                PrintDocument printDoc = new PrintDocument();

                if (!printDoc.PrinterSettings.IsValid)
                {
                    throw new Exception("Error: cannot find the default printer.");
                }
                else
                {
                    try
                    {
                        PrintSvc._isBusy = true;
                        /////App.MainScreenControl?.ToTopMostScreenLayer();

                        App.Log?.LogText(_logChannel, transactionNo ?? "*****", "Start Printing ..", "A05", "ImagePrintingTools.ExecutePrinting",
                            adminMsg: "Start Printing ..");

                        printDoc.PrintPage += new PrintPageEventHandler(PrintSvc.PrintPage);
                        printDoc.Print();
                    }
                    catch (Exception ex)
                    {
                        PrintSvc._isBusy = false;
                        /////App.MainScreenControl?.ToNormalScreenLayer();
                        throw ex;
                    }
                }
            }

            return !isBusy;
        }

        class PrintImageDocument : IDisposable
        {
            public Stream[] ImageStreamList { get; private set; }
            public string TransactionNo { get; private set; }
            public ReportImageSize ReportImageSize { get; private set; }

            public PrintImageDocument(string transactionNo, Stream[] imageStreamList, ReportImageSize reportImageSize)
            {
                if ((imageStreamList is null))
                    throw new Exception("Invalid print image;");

                if ((reportImageSize is null))
                    throw new Exception("Invalid report size definition;");

                TransactionNo = transactionNo ?? "*";
                ImageStreamList = imageStreamList;
                ReportImageSize = reportImageSize;
            }

            public void Dispose()
            {
                ImageStreamList = null;
                ReportImageSize = null;
            }
        }
    }
}