using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Backend
{
    public sealed class VatPaymentCheckerShould
    {
        [Fact]
        public async Task CheckVIN_of_VATtaxpayer()
        {
            const string validVIN = "7251947829"; // GFT Poland
            using (var checker = new VatPaymentChecker())
            {
                await checker.Initialize();
                var result = await checker.Check(validVIN);

                Assert.Equal(result, "Podmiot o podanym identyfikatorze podatkowym NIP jest zarejestrowany jako podatnik VAT czynny  Dodatkowo informujemy, i¿ w celu potwierdzenia czy podmiot jest zarejestrowany jako podatnik VAT czynny,  podatnik i osoba trzecia maj¹ca interes prawny, mog¹ z³o¿yæ do w³aœciwego naczelnika urzêdu skarbowego wniosek o wydanie zaœwiadczenia.");
            }
        }

        [Fact]
        public async Task CheckVIN_of_nonVATtaxpayer()
        {
            const string validVIN = "7331184612";

            using (var checker = new VatPaymentChecker())
            {
                await checker.Initialize();
                var result = await checker.Check(validVIN);

                Assert.Equal(result, "Podmiot o podanym identyfikatorze podatkowym NIP nie jest zarejestrowany jako podatnik VAT");
            }
        }

        [Fact]
        public async Task CheckVIN_of_fakeTaxPayer()
        {
            const string vin = "7473137775";

            using (var checker = new VatPaymentChecker())
            {
                await checker.Initialize();
                var result = await checker.Check(vin);

                Assert.Equal(result, "Podmiot o podanym identyfikatorze podatkowym NIP nie jest zarejestrowany jako podatnik VAT");
            }
        }


        // The shortest linear path how to recognize valid VAT taxpayer.
        [Fact]
        public async Task DoItFastly()
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };

            string callToken;
            string callSource;

            var client = new HttpClient(handler);
            {
                var result = await client.GetAsync("https://ppuslugi.mf.gov.pl/_/");

                callToken = result.Headers.First(it => it.Key == "Fast-Ver-Last").Value.First();
                callSource = result.Headers.First(it => it.Key == "Fast-Ver-Source").Value.First();
            }


            {
                var values = new[]
                {
                    new KeyValuePair<string, string>("ACTION__", "1005"),
                    new KeyValuePair<string, string>("FAST_VERLAST__", callToken),
                };

                using (var form = new FormUrlEncodedContent(values))
                {
                    var result = await client.PostAsync("https://ppuslugi.mf.gov.pl/_/ExecuteAction", form);
                    result.EnsureSuccessStatusCode();
                    callToken = result.Headers.First(it => it.Key == "Fast-Ver-Last").Value.First();
                    callSource = result.Headers.First(it => it.Key == "Fast-Ver-Source").Value.First();
                }
            }

            {
                var values = new[]
                {
                    new KeyValuePair<string, string>("b-6", "NIP"),
                    new KeyValuePair<string, string>("b-7", "7251947829"),
                    new KeyValuePair<string, string>("DOC_MODAL_ID__", "0"),
                    new KeyValuePair<string, string>("FAST_VERLAST__", callToken),
                };

                using (var form = new FormUrlEncodedContent(values))
                {
                    var result = await client.PostAsync("https://ppuslugi.mf.gov.pl/_/Recalc", form);
                    result.EnsureSuccessStatusCode();
                    callToken = result.Headers.First(it => it.Key == "Fast-Ver-Last").Value.First();
                    callSource = result.Headers.First(it => it.Key == "Fast-Ver-Source").Value.First();
                }
            }

            {
                var values = new[]
                {
                    new KeyValuePair<string, string>("b-6", "NIP"),
                    new KeyValuePair<string, string>("b-7", "7251947829"),
                    new KeyValuePair<string, string>("DOC_MODAL_ID__", "0"),
                    new KeyValuePair<string, string>("EVENT__", "b-8"),
                    new KeyValuePair<string, string>("FAST_VERLAST__", callToken),
                };

                using (var form = new FormUrlEncodedContent(values))
                {
                    var result = await client.PostAsync("https://ppuslugi.mf.gov.pl/_/EventOccurred", form);
                    result.EnsureSuccessStatusCode();
                    callToken = result.Headers.First(it => it.Key == "Fast-Ver-Last").Value.First();
                    callSource = result.Headers.First(it => it.Key == "Fast-Ver-Source").Value.First();

                    var content = await result.Content.ReadAsStringAsync();
                    var jsonContent = JsonConvert.DeserializeObject<EventOccurredResponseModel>(content);

                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(jsonContent.html);

                    var information = doc.GetElementbyId("caption2_b-3");
                    Assert.Equal(
                        information.InnerText, "Podmiot o podanym identyfikatorze podatkowym NIP jest zarejestrowany jako podatnik VAT czynny  Dodatkowo informujemy, i¿ w celu potwierdzenia czy podmiot jest zarejestrowany jako podatnik VAT czynny,  podatnik i osoba trzecia maj¹ca interes prawny, mog¹ z³o¿yæ do w³aœciwego naczelnika urzêdu skarbowego wniosek o wydanie zaœwiadczenia.");


                }
            }

        }

        struct EventOccurredResponseModel
        {
            public int result;
            public string pagetitle;
            public string html;
        }

        public static string ToUnixTime()
        {
            var date = DateTime.Now;
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds).ToString();
        }
    }
}
