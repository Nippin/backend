using OpenQA.Selenium.Remote;
using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using System.Reactive.Disposables;

namespace Backend
{
    internal sealed class Browser : IBrowser
    {
        /// <summary>
        /// Creates a new instance of Browser and connects with Selenium Hub.
        /// </summary>
        private RemoteWebDriver driver;

        /// <summary>
        /// Single contener to handle all disposable subparts of the current instance.
        /// </summary>
        private readonly CompositeDisposable instanceDisposer = new CompositeDisposable();

        public Browser()
        {
        }

        public Task Initialize()
        {
            return Task.Run(() =>
            {
                driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), DesiredCapabilities.Chrome())
                        .DisposeWith(instanceDisposer);
            });
        }

        public void Dispose()
        {
            instanceDisposer.Dispose();
        }

        public async Task<TPage> Expect<TPage>() where TPage : IPage, new()
        {
            var page = new TPage();
            await page.Initialize(driver);
            return page;
        }

        public Task<Screenshot> GetScreenshot()
        {
            throw new NotImplementedException();
        }

        public Task GoToUrl(string url)
        {
            return Task.Run(() =>
            {
                driver.Navigate().GoToUrl(url);
            });
        }
    }
}
