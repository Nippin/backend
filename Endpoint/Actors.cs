using Nancy.Bootstrapper;
using Akka.Actor;
using Backend;
using Akka.Routing;
using System.Reactive.Disposables;

namespace Endpoint
{
    public class Actors : IApplicationStartup
    {
        public static readonly CompositeDisposable ActorsDisposer = new CompositeDisposable();
        public static ActorSystem ActorSystem;

        //here you would store your toplevel actor-refs
        public static IActorRef PageActor;

        public void Initialize(IPipelines pipelines)
        {
            ActorSystem = ActorSystem.Create("nippin").DisposeWith(ActorsDisposer);
            PageActor = ActorSystem
                .ActorOf(Props.Create(() => new PageActor(() => new Browser().DisposeWith(ActorsDisposer)))
                .WithRouter(new SmallestMailboxPool(10, new DefaultResizer(5, 50), SupervisorStrategy.StoppingStrategy, null)));
        }
    }
}
