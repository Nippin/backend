using Akka.Actor;
using Akka.TestKit.Xunit2;
using NSubstitute;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Backend;
using static Backend.PageActor;
using Akka.TestKit;

namespace Nippin
{
    public sealed class PageActorShould : TestKit
    {
        /// <summary>
        /// For testing purpose we need to use TestScheduler to maintain virtual time.
        /// see https://petabridge.com/blog/how-to-unit-test-akkadotnet-actors-akka-testkit/
        /// </summary>
        public PageActorShould()
            : base(@"akka.scheduler.implementation = ""Akka.TestKit.TestScheduler, Akka.TestKit""") { }

        /// <summary>
        /// When PageActor can't initialize browser, need to make suicide because the actor
        /// and browser instance are double linked in term of lifecycle.
        /// </summary>
        [Fact]
        public void FailWhenCantConnectToBrowser()
        {
            var browser = Substitute.For<IBrowser>();
            browser.When(b => b.Initialize())
                .Throw(new Exception());

            var actor = ActorOfAsTestFSMRef<PageActor, States, (IActorRef, string, DateTime)>(() => new PageActor(() => browser));
            var prober = CreateTestProbe();
            prober.Watch(actor);
            prober.ExpectTerminated(actor);

        }

        /// <summary>
        /// If underlying browser can't be instaniated by any PageActor, client need to be notified 
        /// About System error. In fact, No available browsers means system cannot proceed any message
        /// </summary>
        [Fact(Skip = "Not yet implemented")]
        public void ReturnTooBusyError()
        {
            var browser = Substitute.For<IBrowser>();
            browser.When(b => b.Initialize())
                .Throw(new Exception());

            var actor = ActorOfAsTestFSMRef<PageActor, States, (IActorRef, string, DateTime)>(() => new PageActor(() => browser));
            // ??
        }

        [Fact]
        public async Task ShouldCloseBrowserWhenStopped()
        {
            var client = new HttpClient();
            var sessionCount = (await client.GetSessionsAsync()).value?.Count;

            var actor = ActorOf(() => new PageActor(() => new Browser(new SeleniumOptions { SeleniumGridAddress = "http://localhost:4444/" })));
            AwaitCondition(() => client.GetSessionsAsync().Result.value?.Count == sessionCount + 1);

            Sys.Stop(actor);
            AwaitCondition(() => client.GetSessionsAsync().Result.value?.Count == sessionCount, TimeSpan.FromSeconds(60));
            Assert.Equal(sessionCount, client.GetSessionsAsync().Result.value?.Count);
        }

        [Fact]
        public async Task CheckWellKnownTaxPayer()
        {
            var actor = ActorOf(() => new PageActor(() => new Browser(new SeleniumOptions { SeleniumGridAddress = "http://localhost:4444/" })));

            var reply = await actor.Ask<CheckVatinReply>(new CheckVatinAsk("5213017228", DateTime.Now), TimeSpan.FromSeconds(60)); // 521 301 72 28 - VATIN of ZUS

            Assert.True(reply.Done);
            Assert.NotEmpty(reply.Screenshot);
        }

        /// <summary>
        /// Browsers managed by SeleniumGrid are shut downed after 30 mins so
        /// PageActor is disconnected without any notification and can't continue work.
        /// 
        /// To avoid this, the simples seems to be repeatable touching browser by Selenium Api.
        /// </summary>
        [Fact]
        public void TouchBrowser()
        {
            var browser = Substitute.For<IBrowser>();

            var actor = ActorOfAsTestFSMRef<PageActor, States, (IActorRef, string, DateTime)>(
                Props.Create(() => new PageActor(() => browser)));

            browser.DidNotReceive().Touch();


            var scheduler = (TestScheduler)Sys.Scheduler;
            scheduler.Advance(10.Minutes() + 1.Seconds());

            browser.Received().Touch();
        }

    }
}
