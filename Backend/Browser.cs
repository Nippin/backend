using OpenQA.Selenium.Remote;
using System;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace Backend
{
    internal sealed class Browser : IBrowser
    {
        /// <summary>
        /// Creates a new instance of Browser and connects with Selenium Hub.
        /// </summary>
        private RemoteWebDriver driver;

        public Browser()
        {
        }

        public Task Initialize()
        {
            return Task.Run(() =>
            {
                driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), DesiredCapabilities.Chrome());
            });
        }

        public void Dispose()
        {
            driver.Dispose();
        }

        public Task<TPage> Expect<TPage>() where TPage : IPage
        {
            throw new NotImplementedException();
        }

        public Task<Screenshot> GetScreenshot()
        {
            throw new NotImplementedException();
        }

        public Task GoToUrl(string url)
        {
            throw new NotImplementedException();
        }
    }
}
