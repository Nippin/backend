using Nancy.Bootstrapper;
using Akka.Actor;
using Backend;
using Akka.Routing;

namespace Endpoint
{
    public class Actors : IApplicationStartup
    {
        public static ActorSystem ActorSystem;
        //here you would store your toplevel actor-refs
        public static IActorRef PageActor;

        public void Initialize(IPipelines pipelines)
        {
            ActorSystem = ActorSystem.Create("nippin");
            PageActor = ActorSystem
                .ActorOf(Props.Create(() => new PageActor(() => new Browser()))
                .WithRouter(new SmallestMailboxPool(10, new DefaultResizer(5, 50), SupervisorStrategy.StoppingStrategy, null)));
        }
    }
}
