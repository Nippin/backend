using Endpoint;
using Nancy;
using Nancy.Testing;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Xunit;


namespace Nippin
{
    public sealed class EndpointShould
    {
        [Fact]
        public async Task ReturnHomePage()
        {
            var cts = new CancellationTokenSource();
            using (Disposable.Create(() => cts.Cancel()))
            using (var bootstrapper = new AppBootstrapper(new SeleniumOptions { SeleniumGridAddress = "http://localhost:4444/" }, cts.Token, 1, 1))
            {
                var browser = new Browser(bootstrapper);

                // When
                var result = await browser.Get("/", with =>
                {
                    with.HttpRequest();
                }).WithTimeout(20);

                // Then
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Fact]
        public async Task ShouldCheckWellKnownTaxPayer()
        {
            var cts = new CancellationTokenSource();
            using (Disposable.Create(() => cts.Cancel()))
            using (var bootstrapper = new AppBootstrapper(new SeleniumOptions { SeleniumGridAddress = "http://localhost:4444/" }, cts.Token, 1, 1))
            {
                var browser = new Browser(bootstrapper);

                // When
                var response = await browser.Get("/vatin/5213017228", with =>
                {
                    with.HttpRequest();
                    with.Accept(new Nancy.Responses.Negotiation.MediaRange("application/json"));
                }).WithTimeout(20);


                // Then
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = response.Body.DeserializeJson<dynamic>();
                Assert.NotEmpty(result.Response);
            }
        }

        [Fact]
        public async Task ShouldCheckWellKnownTaxPayerDescription()
        {
            var cts = new CancellationTokenSource();
            using (Disposable.Create(() => cts.Cancel()))
            using (var bootstrapper = new AppBootstrapper(new SeleniumOptions { SeleniumGridAddress = "http://localhost:4444/" }, cts.Token, 1, 1))
            {
                var browser = new Browser(bootstrapper);

                // When
                var response = await browser.Get("/excel/5213017228/with/description", with =>
                {
                    with.HttpRequest();
                    with.Accept(new Nancy.Responses.Negotiation.MediaRange("application/json"));
                }).WithTimeout(20);

                // Then
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = response.Body.AsString();
                Assert.NotEmpty(result);
            }
        }

        [Fact]
        public async Task ReturnScreenshot()
        {
            var cts = new CancellationTokenSource();
            using (Disposable.Create(() => cts.Cancel()))
            using (var bootstrapper = new AppBootstrapper(new SeleniumOptions { SeleniumGridAddress = "http://localhost:4444/" }, cts.Token, 1, 1))
            {
                var browser = new Browser(bootstrapper);

                // When
                var response = await browser.Get("/api/screenshot/5213017228", with =>
                {
                    with.HttpRequest();
                }).WithTimeout(60);

                // Then
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public void ReturnBulkOfScreenshots()
        {
            var source = new[]  {
                "1080000059",
                "1132466405",
                "1132479224",
                "1132619524",
                "1132690108",
                "1180038498",
                "1230855007",
                "4990495148",
                "5211295779",
                "5213369861",
                "5213596290",
                "5220104690",
                "5222038690",
                "5222357343",
                "5222472699",
                "5222627417",
                "5222845574",
                "5222862526",
                "5242503440",
                "5242772631",
                "5250007313"
            };

            var cts = new CancellationTokenSource();
            using (Disposable.Create(() => cts.Cancel()))
            using (var bootstrapper = new AppBootstrapper(new SeleniumOptions { SeleniumGridAddress = "http://localhost:4444/" }, cts.Token, 3, 3))
            {
                var browser = new Browser(bootstrapper);

                var responses = new List<Task<BrowserResponse>>();

                foreach (var item in source)
                {
                    var response = browser.Get($"/api/screenshot/{item}", with =>
                    {
                        with.HttpRequest();
                    }).WithTimeout(600);
                    responses.Add(response);
                }

                foreach (var item in responses)
                {
                    var response = item.Result;
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            }
        }
    }
}
