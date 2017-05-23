using Endpoint;
using Nancy;
using Nancy.Testing;
using System;
using System.Threading.Tasks;
using Xunit;


namespace Nippin
{
    public sealed class EndpointShould : IDisposable
    {
        [Fact]
        public async Task ReturnHomePage()
        {
            // Given
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);

            // When
            var result = await browser.Get("/", with =>
            {
                with.HttpRequest();
            }).WithTimeout(20);

            // Then
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task ShouldCheckWellKnownTaxPayer()
        {
            var bootstrapper = new DefaultNancyBootstrapper();
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

        [Fact]
        public async Task ShouldCheckWellKnownTaxPayerDescription()
        {
            var bootstrapper = new DefaultNancyBootstrapper();
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

        [Fact]
        public async Task ReturnScreenshot()
        {
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);

            // When
            var response = await browser.Get("/api/screenshot/5213017228", with =>
            {
                with.HttpRequest();
            }).WithTimeout(60);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public void Dispose()
        {
            Actors.ActorsDisposer.Dispose();
            Actors.ActorSystem.Terminate().Wait();
        }
    }
}
