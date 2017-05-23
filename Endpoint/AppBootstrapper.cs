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

        /// <summary>
        /// We need to release Actor system because it keeps opened browsers.
        /// Can't do that in Dispose menthod because it is not invoked
        /// so will install a handler when application is stopped 
        /// to shoutdown Actors system.
        /// 
        /// because we use applicationStopper token instead of disposing, use them properly in tests
        /// </summary>
        public AppBootstrapper(CancellationToken applicationStopped)
            : base()
        {
            applicationStopped.Register(() =>
            {
                instanceDisposer.Dispose();
            });
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            var actorSystem = ActorSystem.Create("nippin");

            // PageActors need to close browsers, so it is expensive so let allow them to finish it in long time window (30s);
            Disposable.Create(() => actorSystem.Terminate().Wait(new CancellationTokenSource(30 * 1000).Token)).DisposeWith(instanceDisposer);

            var pageActor = actorSystem
                .ActorOf(Props.Create(() => new PageActor(() => new Browser()))
                .WithRouter(new SmallestMailboxPool(10, new DefaultResizer(5, 50), SupervisorStrategy.StoppingStrategy, null)));
            container.Register(pageActor);

            base.ApplicationStartup(container, pipelines);
        }

        public override void Configure(INancyEnvironment environment)
        {
            environment.Tracing(enabled: false, displayErrorTraces: true);
            base.Configure(environment);
        }
    }
}
