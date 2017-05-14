using Nancy;
using Nancy.Testing;
using System.Threading.Tasks;
using Xunit;


namespace Endpoint
{
    public sealed class EndpointShould
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
            });

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
            });

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
            });

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.Body.AsString();
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ShouldReturnScreenshot()
        {
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);

            // When
            var response = await browser.Get("/api/screenshot/5213017228", with =>
            {
                with.HttpRequest();
            });

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
