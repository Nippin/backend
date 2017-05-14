using Backend;
using Nancy;

namespace Endpoint
{
    public sealed class ScreenshotModule : NancyModule
    {
        public ScreenshotModule()
            : base ("/api/screenshot")
        {
            Get("{id}", async _ =>
            {
                var response = await Response.AsFile(@"NoData.png", "image/png");
                return response;
            });
        }
    }
}
