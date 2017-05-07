using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backend
{

    /// <summary>
    /// Checks whether given customer is an active VAT taxpayer.
    /// </summary>
    /// <remarks>
    /// Service 'https://ppuslugi.mf.gov.pl/' (hereinafter ppuslugi) requires (in practise) to queue requests per 
    /// one connection so we need to use MaxConnectionsPerServer=1 as well as add some metadata for calls about 
    /// order of call.
    /// </remarks>
    public sealed class VatPaymentChecker : IDisposable
    {
        private readonly HttpClient client;

        // dynamically recognized token used for querying web services @ ppuslugi
        private string callToken = null;

        public VatPaymentChecker()
        {
            var handler = new HttpClientHandler()
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };

            client = new HttpClient(handler, true);
        }

        /// <summary>
        /// Post construction mentod. Need to be invoked once after constructor.
        /// </summary>
        /// <remarks>
        /// Initializes connection with ppuslugi and initializes metadata required by ppuslugi to queue calls.
        /// </remarks>
        public async Task Initialize()
        {
            var result = await client.GetAsync("https://ppuslugi.mf.gov.pl/_/");
            callToken = result.Headers.First(it => it.Key == "Fast-Ver-Last").Value.First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="VATIN">VAT identification number.</param>
        /// <returns></returns>
        public async ValueTask<string> Check(string VATIN)
        {
            using (var formContent = CreateForm(("ACTION__", "1005"), ("FAST_VERLAST__", callToken)))
            {
                var result = await client.PostAsync("https://ppuslugi.mf.gov.pl/_/ExecuteAction", formContent);
                result.EnsureSuccessStatusCode();
                callToken = result.Headers.First(it => it.Key == "Fast-Ver-Last").Value.First();
            }

            using (var formContent =
                CreateForm(
                    ("b-6", "NIP"), ("b-7", VATIN), ("DOC_MODAL_ID__", "0"), ("EVENT__", "b-8"), ("FAST_VERLAST__", callToken)
                ))
            {
                var result = await client.PostAsync("https://ppuslugi.mf.gov.pl/_/EventOccurred", formContent);
                result.EnsureSuccessStatusCode();
                callToken = result.Headers.First(it => it.Key == "Fast-Ver-Last").Value.First();

                var content = await result.Content.ReadAsStringAsync();
                var jsonContent = JsonConvert.DeserializeObject<EventOccurredResponseModel>(content);

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(jsonContent.html);

                var information = doc.GetElementbyId("caption2_b-3");
                return information.InnerText;
            };

        }

        private static HttpContent CreateForm(params (string key, string value)[] formData)
        {
            var values = formData
                .Select(it => new KeyValuePair<string, string>(it.key, it.value))
                .ToArray();

            return new FormUrlEncodedContent(values);
        }

        /// <summary>
        /// Disposes internal HtmlClient used to obtain html content from ppuslugi.
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
        }
    }

    struct EventOccurredResponseModel
    {
        public int result;
        public string pagetitle;
        public string html;
    }
}
