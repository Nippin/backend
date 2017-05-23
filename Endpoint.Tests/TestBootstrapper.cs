using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Nippin
{
    public sealed class TestBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
