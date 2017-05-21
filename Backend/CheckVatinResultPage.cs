using OpenQA.Selenium.Remote;
using System.Threading.Tasks;
using System;

namespace Backend
{
    public sealed class CheckVatinResultPage : IPage
    {
        public IButton Clear { get; private set; }

        private RemoteWebDriver driver;

        public Task Initialize(RemoteWebDriver driver)
        {
            this.driver = driver;

            return Task.Run(() =>
            {
                var clearButton = driver.FindElementById("b-9").AsButton();

                Clear = clearButton;
            });
        }

        public string Print()
        {
            return driver.GetScreenshot().AsBase64EncodedString;
        }
    }
}
