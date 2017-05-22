using Nippin;
using OpenQA.Selenium.Remote;
using System;
using System.Threading.Tasks;

namespace Backend
{
    public sealed class CheckVatinResultPage : IPage
    {
        public Action Clear { get; private set; }

        private RemoteWebDriver driver;

        public Task Initialize(RemoteWebDriver driver)
        {
            this.driver = driver;

            return Task.Run(() =>
            {
                // run script located @ Clear button CLick event directly
                // because trying to click Clear button directly was very tiought
                // - clicking resulted with some Selenium exceptions.
                var clearButton = driver.AsScript("FWDC.eventOccurred(event, 'b-9');");

                Clear = clearButton;
            });
        }

        public string Print()
        {
            return driver.GetScreenshot().AsBase64EncodedString;
        }
    }
}
