using OpenQA.Selenium.Remote;
using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using System.Reactive.Disposables;
using System.Threading;

namespace Backend
{
    public sealed class Browser : IBrowser
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
            driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), DesiredCapabilities.Chrome())
                    .DisposeWith(instanceDisposer);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            instanceDisposer.Dispose();
        }

        public Task<TPage> Expect<TPage>(CancellationToken deadline) where TPage : IPage, new()
        {
            var page = new TPage();
            var result = new TaskCompletionSource<TPage>();

            page.Initialize(driver);

            page
                .Identified(deadline)
                .ContinueWith(t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            result.SetResult(page);
                            break;
                        case TaskStatus.Faulted:
                            result.SetException(t.Exception);
                            break;
                        default:
                            result.SetCanceled();
                            break;
                    }
                });

            return result.Task;
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
