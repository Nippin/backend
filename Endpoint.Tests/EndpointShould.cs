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
        public async Task ReturnScreenshot()
        {
            var cts = new CancellationTokenSource();
            using (Disposable.Create(() => cts.Cancel()))
            using (var bootstrapper = new AppBootstrapper(cts.Token, 1, 1))
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
