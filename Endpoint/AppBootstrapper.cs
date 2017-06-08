using Akka.Actor;
using Akka.Routing;
using Backend;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using System.Reactive.Disposables;
using Nancy.Configuration;
using System.Threading;

namespace Nippin
{
    public sealed class AppBootstrapper : DefaultNancyBootstrapper
    {
        private CompositeDisposable instanceDisposer = new CompositeDisposable();
        private readonly int minNumberOfBrowsers, maxNumberOfBrowsers;
        private readonly SeleniumOptions options;

        /// <summary>
        /// We need to release Actor system because it keeps opened browsers.
        /// Can't do that in Dispose menthod because it is not invoked
        /// so will install a handler when application is stopped 
        /// to shoutdown Actors system.
        /// 
        /// because we use applicationStopper token instead of disposing, use them properly in tests
        /// </summary>
        public AppBootstrapper(SeleniumOptions options, CancellationToken applicationStopped, int minNumberOfBrowsers, int maxNumberOfBrowsers)
            : base()
        {

            this.options = options;
            this.minNumberOfBrowsers = minNumberOfBrowsers;
            this.maxNumberOfBrowsers = maxNumberOfBrowsers;

            applicationStopped.Register(() =>
            {
                instanceDisposer.Dispose();
            });
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            var actorSystem = ActorSystem.Create("nippin");

            // Every PageActors is linked with a remote browser.
            // Closing a browser could be an expensive timely operation so 
            // let allow them to finish closing all browsers in a quite long time window (30s);
            Disposable.Create(() => actorSystem.Terminate().Wait(new CancellationTokenSource(30 * 1000).Token)).DisposeWith(instanceDisposer);

            var pageActor = actorSystem
                .ActorOf(Props.Create(() => new PageActor(() => new Browser(options)))
                .WithRouter(new RoundRobinPool(minNumberOfBrowsers, new DefaultResizer(minNumberOfBrowsers, maxNumberOfBrowsers), SupervisorStrategy.DefaultStrategy, null)));
            container.Register(pageActor);

            base.ApplicationStartup(container, pipelines);
        }

        public override void Configure(INancyEnvironment environment)
        {
            environment.Tracing(enabled: false, displayErrorTraces: true);
            base.Configure(environment);
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            // CORS Enable
            // https://stackoverflow.com/questions/15658627/is-it-possible-to-enable-cors-using-nancyfx
            pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) =>
            {
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                                .WithHeader("Access-Control-Allow-Methods", "POST,GET")
                                .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type");

            });
        }
}

