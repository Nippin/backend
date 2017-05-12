using System;
using Akka.Actor;
using OpenQA.Selenium.Remote;

namespace Backend
{
    /// <summary>
    /// Represents operations related to using WebBrowser with service ppuslugi.mf.gov.pl.
    /// </summary>
    public sealed class BrowserActor : ReceiveActor
    {
        RemoteWebDriver browser;

        public sealed class TakeScreenshot
        {
        }

        public sealed class TakeScreenshotReply
        {
            public TakeScreenshotReply(string screenshot)
            {
                this.Screenshot = screenshot;
            }

            // serialized as string with with Screenshot.AsBase64EncodedString
            public string Screenshot { get; set; }
        }

        public BrowserActor()
        {
            Receive<TakeScreenshot>(OnTakeScreenshot);
        }

        protected override void PreStart()
        {
            browser = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), DesiredCapabilities.Chrome());

            base.PreStart();
        }

        protected override void PostStop()
        {
            browser.Dispose();

            base.PostStop();
        }

        private bool OnTakeScreenshot(TakeScreenshot msg)
        {
            // It seems to be pretty easy to send simply reference to a Screenshot object 
            // but it could be transferred between Akka Cluster so is better to deserialize it
            // to something transferrable between Actors independely where they are located
            // - either on the same or different machines
            var screenshot = browser.GetScreenshot().AsBase64EncodedString;

            Sender.Tell(new TakeScreenshotReply(screenshot), Self);
            return true;
        }
    }
}
