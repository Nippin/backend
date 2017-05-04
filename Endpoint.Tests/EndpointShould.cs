using Nancy;
using Nancy.Testing;
using System.Threading.Tasks;
using Xunit;


namespace Endpoint
{
    public class EndpointShould
    {
        [Fact]
        public async Task Return_status_ok_when_route_exists()
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
    }
}
