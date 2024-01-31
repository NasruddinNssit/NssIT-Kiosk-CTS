using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NssIT.Kiosk.Client.Base
{
    /// <summary>
    /// ClassCode:EXIT80.01
    /// </summary>
    public class WebImageCacheX
    {
        public delegate Task<BitmapImage> GetImageFromCache(string imageUrl);

        private string _logChannel = "ViewPage";

        private Dictionary<string, BitmapImage> _imagesCache = new Dictionary<string, BitmapImage>();

        private int _maxStorePeriodHours = 12;
        private DateTime _expiredTime = DateTime.MinValue;

        public WebImageCacheX(int maxStorePeriodHours)
        {
            _maxStorePeriodHours = (maxStorePeriodHours > 0) ? maxStorePeriodHours : 0;
        }

        private SemaphoreSlim _imagesCacheLock = new SemaphoreSlim(1);

        /// <summary>
        /// FuncCode:EXIT80.0102
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public async Task<BitmapImage> GetImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            string urlStr = imageUrl.Trim();

            if (_imagesCache.TryGetValue(urlStr, out BitmapImage imgFound) == true)
                return imgFound;

            try
            {
                BitmapImage bitmapImage = new BitmapImage();

                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync(urlStr))
                    {
                        response.EnsureSuccessStatusCode();

                        byte[] imgByteArr = await response.Content.ReadAsByteArrayAsync();

                        bitmapImage = new BitmapImage();
                        using (var mem = new System.IO.MemoryStream(imgByteArr))
                        {
                            mem.Position = 0;
                            bitmapImage.BeginInit();
                            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = mem;
                            bitmapImage.EndInit();
                        }
                        bitmapImage.Freeze();

                        if (_imagesCache.ContainsKey(urlStr) == false)
                        {
                            try
                            {
                                await _imagesCacheLock.WaitAsync();

                                if (_imagesCache.ContainsKey(urlStr) == false)
                                    _imagesCache.Add(urlStr, bitmapImage);
                            }
                            finally
                            {
                                if (_imagesCacheLock.CurrentCount == 0)
                                    _imagesCacheLock.Release();
                            }
                        }

                        return bitmapImage;
                    }

                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT80.0102.EX01)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT80.0102.EX01); imageUrl: {imageUrl}", ex), "EX01", "ImageCache.GetImage");
            }

            return null;
        }

        public void ClearAllCache()
        {
            if (_imagesCacheLock.CurrentCount == 0)
                _imagesCacheLock.Release();

            _imagesCache.Clear();
            _expiredTime = DateTime.Now.AddHours(_maxStorePeriodHours);
        }

        public void ClearCacheOnTimeout()
        {
            if (_imagesCacheLock.CurrentCount == 0)
                _imagesCacheLock.Release();

            if (_expiredTime.Ticks < DateTime.Now.Ticks)
            {
                _imagesCache.Clear();
                _expiredTime = DateTime.Now.AddHours(_maxStorePeriodHours);
            }
        }
    }
}