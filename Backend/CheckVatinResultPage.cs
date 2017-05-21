using Nippin;
using OpenQA.Selenium.Remote;
using System.Threading.Tasks;

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
                var clearButton = driver.AsButton(d => d.FindElementById("b-9"));

                Clear = clearButton;
            });
        }

        public string Print()
        {
            return driver.GetScreenshot().AsBase64EncodedString;
        }
    }
}
