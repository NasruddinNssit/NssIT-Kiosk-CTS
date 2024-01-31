using Newtonsoft.Json;
using NssIT.Kiosk.Common.WebAPI.Data.Response.BTnG;
using NssIT.Kiosk.Common.WebAPI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.PaymentGatewaySchedule.Base.Solution1
{
    public class ExecSchedule1
    {
        private LibShowMessageWindow.MessageWindow _debugDsg = null;
        private string _webApiUrl = "";

        private string _webUrlExtension = @"PaymentGateway/cleanUpOutstandingSales";

        public ExecSchedule1(LibShowMessageWindow.MessageWindow msgDebugWindows, string webApiUrl)
        {
            _debugDsg = msgDebugWindows;
            _webApiUrl = webApiUrl;
        }

        public async Task<BTnGCleanUpSalesResult> ExecSchedule()
        {
            BTnGCleanUpSalesResult oRet = null;
            HttpResponseMessage response;
            try
            {
                ShowMsg("");

                TimeSpan apiTimeOutSec = new TimeSpan(0, 10, 0);

                HttpClientHandler httpClientHandler = new HttpClientHandler();
                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                    client.Timeout = apiTimeOutSec;
                    client.BaseAddress = new Uri(_webApiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Authentication
                    string sign = SecurityHelper.getSignature();
                    client.DefaultRequestHeaders.Add("RequestSignature", sign);
                    // --------------------

                    oRet = null;
                    response = null;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _webUrlExtension);

                    //string StrRequest = null;
                    //if (parameters != null)
                    //{
                    //StrRequest = @"{""merchantId"": ""KTMB""}";
                    //request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                    //}

                    response = await client.SendAsync(request).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        ShowMsg("Execution successful..");

                        string resultString = await response.Content.ReadAsStringAsync();

                        ShowMsg("Result is ..");
                        ShowMsg(resultString);

                        oRet = JsonConvert.DeserializeObject<BTnGCleanUpSalesResult>(resultString);
                    }
                    else
                    {
                        ShowMsg("fail execution ..");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.ToString());
                ShowMsg("");
            }

            return oRet;
        }

        private void ShowMsg(string message)
        {
            _debugDsg?.ShowMessage(message);
        }
    }
}
