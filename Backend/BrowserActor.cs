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
    }
}
