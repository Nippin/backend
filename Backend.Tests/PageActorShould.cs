using Akka.Actor;
using Akka.TestKit.Xunit2;
using NSubstitute;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static Backend.PageActor;

namespace Backend
{
    public sealed class PageActorShould : TestKit
    {
        /// <summary>
        /// When PageActor can't initialize browser, need to make suicide because the actor
        /// and browser instance are double linked in term of lifecycle.
        /// </summary>
        [Fact]
        public void FailWhenCantConnectToBrowser()
        {
            var browser = Substitute.For<IBrowser>();
            browser.Initialize().Returns(Task.FromException(new Exception()));

            var actor = ActorOfAsTestFSMRef<PageActor, States, dynamic>(() => new PageActor(() => browser));
            var prober = CreateTestProbe();
            prober.Watch(actor);
            prober.ExpectTerminated(actor);

        }

        [Fact]
        public void ReturnExpectedValue()
        {
        }

        public void ReturnTooBusyError()
        {
        }

        [Fact]
        public async Task ShouldCloseBrowserWhenStopped()
        {
            var client = new HttpClient();
            var sessionCount = (await client.GetSessionsAsync()).value?.Count;

            var actor = ActorOf(() => new PageActor(() => new Browser()));
            AwaitCondition(() => client.GetSessionsAsync().Result.value?.Count == sessionCount + 1);

            Sys.Stop(actor);
            AwaitCondition(() => client.GetSessionsAsync().Result.value?.Count == sessionCount, TimeSpan.FromSeconds(20));
            Assert.Equal(sessionCount, client.GetSessionsAsync().Result.value?.Count);
        }

        [Fact]
        public async Task ShouldCheckWellKnownTaxPayer()
        {
            var actor = ActorOf(() => new PageActor(() => new Browser()));
            var reply = await actor.Ask<CheckVatinReply>(new PageActor.CheckVatinAsk("5213017228", DateTime.Now), TimeSpan.FromSeconds(20)); // 521 301 72 28 - VATIN of ZUS
            Assert.True(reply.Done);
        }

    }
}
