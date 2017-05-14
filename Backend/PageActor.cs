using System;
using Akka.Actor;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace Backend
{
    /// <summary>
    /// * Represents simple operations related to using web pages with service ppuslugi.mf.gov.pl.
    /// * Controls lifecycle of browser instance: creates a new browser instance on start, close them on exit
    /// </summary>
    public sealed class PageActor : ReceiveActor
    {
        RemoteWebDriver browser;

        /// <summary>
        /// Asks about current screenshot weom underlying Web Browser.
        /// </summary>
        public sealed class TakeScreenshotAsk
        {
        }

        public sealed class TakeScreenshotReply
        {
            public TakeScreenshotReply(string screenshot)
            {
                this.Screenshot = screenshot;
            }

            /// <summary>
            /// Serialized as string with with Screenshot.AsBase64EncodedString. Can be constructed back with Selenium Screenshot object.
            /// </summary>
            public string Screenshot { get; set; }
        }

        /// <summary>
        /// Request about validation of a subject with provided VATIN
        /// </summary>
        public sealed class CheckVatinAsk
        {
            public CheckVatinAsk(string vatin)
            {
                this.Vatin = vatin;
            }
            public string Vatin { get; }
        }

        /// <summary>
        /// Checking VATIN has finished, current status is visible in browser.
        /// 
        /// Returned 'CheckVatinReply' contains some data recognized from the page.
        /// Important is recognition about if operation has finished with success or not.
        /// </summary>
        public sealed class CheckVatinReply
        {
            public CheckVatinReply(bool done, string screenshot)
            {
                Done = done;
                Screenshot = screenshot;
            }
            public bool Done { get; set; }
            public string Screenshot { get; set; }
        }

        public PageActor()
        {
            Receive<TakeScreenshotAsk>(OnTakeScreenshot);
            Receive<CheckVatinAsk>(OnCheckVatinAsk);
        }

        protected override void PreStart()
        {
            browser = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), DesiredCapabilities.Chrome());
            browser.Navigate().GoToUrl("https://ppuslugi.mf.gov.pl/?link=VAT");

            base.PreStart();
        }

        protected override void PostStop()
        {
            browser.Dispose();

            base.PostStop();
        }

        private bool OnTakeScreenshot(TakeScreenshotAsk msg)
        {
            // It seems to be pretty easy to send simply reference to a Screenshot object 
            // but it could be transferred between Akka Cluster so is better to deserialize it
            // to something transferrable between Actors independely where they are located
            // - either on the same or different machines
            var screenshot = browser.GetScreenshot().AsBase64EncodedString;

            Sender.Tell(new TakeScreenshotReply(screenshot), Self);
            return true;
        }

        private bool OnCheckVatinAsk(CheckVatinAsk msg)
        {
            var vatinInput = default(IWebElement);
            var submitButton = default(IWebElement);

            try
            {
                vatinInput = browser.FindElementById("b-7");
                submitButton = browser.FindElementById("b-8");
            }
            catch (NoSuchElementException ex)
            {
                Sender.Tell(new CheckVatinReply(false, null));
                return true;
            }

            vatinInput.SendKeys(msg.Vatin);
            submitButton.Click();

            // two possible scenarios
            // exists: 
            // id: indicator_b-7. Error message about invalid NIP
            // means: operation finished, data non accepted. Ready for screenshot
            // id: b-9. Button to allow clear and repeat operation

            var now = DateTime.Now;
            while ((DateTime.Now - now) < TimeSpan.FromSeconds(10))
            {
                try { var clearButton = browser.FindElementById("b-9"); }
                catch (NoSuchElementException ignored) { }

                try { var errorInfo = browser.FindElementById("indicator_b-7"); }
                catch (NoSuchElementException ignored) { }
            }

            var screenshot = browser.GetScreenshot().AsBase64EncodedString;
            Sender.Tell(new CheckVatinReply(true, screenshot));

            browser.Navigate().GoToUrl("https://ppuslugi.mf.gov.pl/?link=VAT");
            return true;
        }
    }
}
