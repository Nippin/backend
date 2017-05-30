using Endpoint;
using Nancy;
using Nancy.Testing;
using System;
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
            using (var bootstrapper = new AppBootstrapper(new SeleniumOptions { SeleniumGridAddress = "http://localhost:4444/" } , cts.Token, 1, 1))
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
    }
}
