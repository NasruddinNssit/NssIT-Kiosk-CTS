using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk.Common.WebAPI.Data.PostRequestParam;
using NssIT.Kiosk.Common.WebAPI.Common.WebApi;
using NssIT.Kiosk.Common.WebAPI.Helper;

namespace NssIT.Kiosk.Common.WebAPI
{
    public class WebAPIAgent : IDisposable 
    {
        private const string LogChannel = "WepAPI";

        public const string ApiCodeOK = "0";

        private string _baseAPIUrl = "";

        private DbLog _log = DbLog.GetDbLog();

        private Thread _postThreadWorker = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseAPIUrl">Like "https://localhost:44305/api/". Must have "/" at the end.</param>
        public WebAPIAgent(string baseAPIUrl)
        {
            baseAPIUrl = string.IsNullOrEmpty(baseAPIUrl) ? "" : baseAPIUrl.Trim();

            if (baseAPIUrl.Substring(baseAPIUrl.Length - 1, 1).Equals(@"/") || baseAPIUrl.Substring(baseAPIUrl.Length - 1, 1).Equals(@"\"))
                _baseAPIUrl = baseAPIUrl;
            else
                _baseAPIUrl = baseAPIUrl + "/";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRet">Return Type</typeparam>
        /// <param name="apiUrl">Like "Station/getStation". Must not have "/" at the beginning</param>
        /// <param name="actionMethodTag">Class and Method Name. Also can add line no. of the method</param>
        /// <param name="waitSec"></param>
        /// <param name="maxRetryTimes"></param>
        /// <returns></returns>
        public async Task<dynamic> Post<TRet>(IPostRequestParam parameters,string apiUrl, string actionMethodTag = "", int waitSec = 55, int maxRetryTimes = 3) 
            where TRet : iWebApiResult
        {
            Guid processId = Guid.NewGuid();

            apiUrl = string.IsNullOrEmpty(apiUrl) ? "" : apiUrl.Trim();
            if (apiUrl.Substring(0, 1).Equals(@"/") || apiUrl.Substring(0, 1).Equals(@"\"))
            {
                if (apiUrl.Length == 1)
                    apiUrl = "";
                else
                    apiUrl = apiUrl.Substring(1);
            }

            bool endThreadWork = false;
            dynamic oReturn = null;
            TimeSpan apiTimeOutSec = new TimeSpan(0, 0, 45);
            string paramSumm = $@"URL : {apiUrl}; Expected Type : {typeof(TRet).ToString()}; Action : {actionMethodTag}; waitSec: {waitSec}; maxRetryTimes: {maxRetryTimes}";

            
            try
            {
                for (int retryInx = 0; retryInx < maxRetryTimes; retryInx++)
                {
                    oReturn = null;
                    endThreadWork = false;
                    _postThreadWorker = new Thread(new ThreadStart(PostExecThreadWorking));
                    _postThreadWorker.IsBackground = true;

                    DateTime timeOut = DateTime.Now.AddSeconds(waitSec);
                    _postThreadWorker.Start();

                    //CYA-PENDING-CODE .. Try to use this statement => threadWorker.Join(new TimeSpan(0,0, waitSec));

                    while ((timeOut.Subtract(DateTime.Now).TotalMilliseconds > 0) && (endThreadWork == false) && ((_postThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped))
                    {
                        Task.Delay(50).Wait();
                    }

                    if (endThreadWork == false)
                    {
                        try
                        {
                            if (_postThreadWorker != null)
                            {
                                _postThreadWorker?.Abort();
                                await Task.Delay(500);
                            }
                        }
                        catch { }
                        finally
                        {
                            _postThreadWorker = null;
                        }

                        if ((retryInx + 1) == maxRetryTimes)
                        {
                            oReturn = new WebApiException($@"Web Timeout; Busy; (EXIT700000101)");
                        }
                    }
                    else if ((oReturn is WebApiException wEx) && ((retryInx + 1) < maxRetryTimes) && (wEx.AllowRetry))
                    {
                        oReturn = null; // try re-do again
                    }

                    if (oReturn != null)
                        break;
                    else
                        //Task.Delay(ServerAccess.RetryIntervalSec * 1000).Wait();
                        Task.Delay(2 * 1000).Wait();
                }
            }
            //catch (ThreadAbortException ex) 
            //{
            //    if (threadWorker != null)
            //    {
            //        try
            //        {
            //            threadWorker.Abort();
            //            await Task.Delay(500);
            //            threadWorker = null;
            //        }
            //        catch { }
            //    }
            //    throw ex;
            //}
            catch (ThreadAbortException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                string tMsg = "end";

                if (_postThreadWorker != null)
                {
                    try
                    {
                        _postThreadWorker?.Abort();
                        await Task.Delay(500);
                    }
                    catch { }
                    finally
                    {
                        _postThreadWorker = null;
                    }
                }
            }

            return oReturn;

            void PostExecThreadWorking()
            {
                HttpResponseMessage response;
                
                try
                {
                    var httpClientHandler = new HttpClientHandler();
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 ;

                        client.Timeout = apiTimeOutSec;
                        client.BaseAddress = new Uri(_baseAPIUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Authentication
                        string sigStr = SecurityHelper.getSignature();
                        client.DefaultRequestHeaders.Add("RequestSignature", sigStr);
                        // --------------------

                        //Station oStation = new Station();

                        response = null;
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                        string StrRequest = null;
                        if (parameters != null)
                        {
                            StrRequest = JsonConvert.SerializeObject(parameters);
                            request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                        }

                        //request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                        //request.Content = new System.Net.Http.StringContent("", Encoding.UTF8, "application/json");

                        _log?.LogText(LogChannel, processId.ToString(), parameters, "A01", "WebAPIAgent.PostExecThreadWorking", extraMsg: $@"Start; IPostRequestParam Type: {parameters?.GetType().Name}; Parameters Summary: {paramSumm}");
                        response = client.SendAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
                        _log?.LogText(LogChannel, processId.ToString(), "End", "A100", "WebAPIAgent.PostExecThreadWorking");

                        if (response.IsSuccessStatusCode)
                        {
                            dynamic oRet = null;
                            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            //oRet = (TRet)Activator.CreateInstance(typeof(TRet));
                            oRet = JsonConvert.DeserializeObject<TRet>(result);
                            if ((oRet is iWebApiResult apiRes) && (apiRes.Status == true) && (apiRes.Code != null) && (apiRes.Code.Trim().Equals(ApiCodeOK) == true))
                            {
                                /* Success - By Pass */
                                oReturn = oRet;
                            }
                            else if ((oRet is iWebApiResult apiRes2))
                            {
                                string errMsg1 = $@"Web Error Found; Status : {apiRes2.Status}; Code : {apiRes2.Code}; (EXIT700000103)";
                                if (apiRes2.Messages?.Count > 0)
                                    apiRes2.Messages.Add(errMsg1);
                                else
                                    apiRes2.Messages = new List<string>(new string[] { errMsg1 });

                                oReturn = new WebApiException(apiRes2.Messages, apiRes2.Code);
                                _log?.LogText(LogChannel, processId.ToString(), oRet, "E03", "WebAPIAgent.PostExecThreadWorking", MessageType.Error, extraMsg: $@"Err.Msg: {apiRes2.MessageString()}; params {paramSumm}; (EXIT700000103); MsgObj : {oRet.GetType().ToString()}");
                            }
                            else
                            {
                                oReturn = new WebApiException("Unrecognized data; (EXIT700000104)");
                                _log?.LogText(LogChannel, processId.ToString(), oRet, "E04", "WebAPIAgent.PostExecThreadWorking", MessageType.Error, extraMsg: $@"params {paramSumm}; (EXIT700000104); MsgObj : {oRet?.GetType().ToString()}");
                            }
                        }
                        else
                        {
                            oReturn = new WebApiException($@"Web no response ; R.Code : {response?.StatusCode}; (EXIT700000105)");
                            _log?.LogText(LogChannel, processId.ToString(), response, "E05", "WebAPIAgent.PostExecThreadWorking", MessageType.Error, extraMsg: $@"params {paramSumm}; (EXIT700000105); MsgObj : HttpResponseMessage");
                        }
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    _log?.LogError(LogChannel, processId.ToString(), new Exception($@"Error with params {paramSumm}; (EXIT700000110)", ex), "EX01", "WebAPIAgent.PostExecThreadWorking");
                    string errMsg = $@"{ex.Message}; {ex.InnerException?.Message}; (EXIT700000110)";
                    oReturn = new WebApiException(new Exception(errMsg, ex), allowRetry: true);
                }
                finally
                {
                    endThreadWork = true;
                    _log?.LogText(LogChannel, processId.ToString(), "Quit", "A200", "WebAPIAgent.PostExecThreadWorking");
                }
            }
        }

        public async void Dispose()
        {
            ThreadState workerStt = _postThreadWorker?.ThreadState ?? ThreadState.Stopped;
            if ((_postThreadWorker != null) && ((workerStt & ThreadState.Stopped) != ThreadState.Stopped))
            {
                try
                {
                    _postThreadWorker?.Abort();
                    await Task.Delay(500);
                }
                catch { }
            }

            _postThreadWorker = null;
            _log = null;
        }
    }
}
