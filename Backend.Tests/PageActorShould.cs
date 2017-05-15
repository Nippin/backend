using Akka.Actor;
using Akka.TestKit.Xunit2;
using OpenQA.Selenium;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Backend
{
    public sealed class PageActorShould : TestKit
    {
        [Fact]
        public async Task ShouldCloseBrowserWhenStopped()
        {
            var client = new HttpClient();
            var sessionCount = (await client.GetSessionsAsync()).value?.Count;

            var actor = ActorOf<PageActor>();
            AwaitCondition(() => client.GetSessionsAsync().Result.value?.Count == sessionCount + 1);

            Sys.Stop(actor);
            AwaitCondition(() => client.GetSessionsAsync().Result.value?.Count == sessionCount, TimeSpan.FromSeconds(20));
            Assert.Equal(sessionCount, client.GetSessionsAsync().Result.value?.Count);
        }

        /// <summary>
        /// We can download current screenshot from Actor. Whatever it is, we would like to download it
        /// and see if this action is doable without exception.
        /// </summary>
        [Fact]
        public async Task ShouldDownloadScreenshot()
        {
            var actor = ActorOf<PageActor>();
            var pictureAsRawData = await actor.Ask<PageActor.TakeScreenshotReply>(new PageActor.TakeScreenshotAsk(), TimeSpan.FromSeconds(20));
            // No exceptions => Success
            new Screenshot(pictureAsRawData.Screenshot);
        }

        [Fact]
        public async Task ShouldCheckWellKnownTaxPayer()
        {
            var actor = ActorOf<PageActor>();
            var reply = await actor.Ask<PageActor.CheckVatinReply>(new PageActor.CheckVatinAsk("5213017228"), TimeSpan.FromSeconds(20)); // 521 301 72 28 - VATIN of ZUS
            Assert.True(reply.Done);
        }

    }
}
