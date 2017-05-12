using Akka.Actor;
using Akka.TestKit.Xunit2;
using OpenQA.Selenium;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Backend
{
    public sealed class BrowserActorShould : TestKit
    {
        [Fact]
        public async Task ShouldCloseBrowserWhenStopped()
        {
            var client = new HttpClient();
            var sessionCount = (await client.GetSessionsAsync()).value?.Count;

            var actor = ActorOf<BrowserActor>();
            AwaitCondition(() => client.GetSessionsAsync().Result.value?.Count == sessionCount + 1);

            Sys.Stop(actor);
            AwaitCondition(() => client.GetSessionsAsync().Result.value?.Count == sessionCount);
            Assert.Equal(sessionCount, client.GetSessionsAsync().Result.value?.Count);
        }

        [Fact]
        public async Task ShouldDownloadScreenshot()
        {
            var actor = ActorOf<BrowserActor>();
            var pictureAsRawData = await actor.Ask<BrowserActor.TakeScreenshotReply>(new BrowserActor.TakeScreenshot());

            // for test purpose is enough when Screenshot is able to accept returned value as a screenshot.
            new Screenshot(pictureAsRawData.Screenshot);
        }
    }
}
