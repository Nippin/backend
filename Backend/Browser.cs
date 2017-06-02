using OpenQA.Selenium.Remote;
using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using System.Reactive.Disposables;
using System.Threading;
using Nippin;

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

        private SeleniumOptions options;

        public Browser(SeleniumOptions options)
        {
            this.options = options;
        }

        public void Initialize()
        {
            try
            {
                var url = $"{options.SeleniumGridAddress}wd/hub";
                Console.WriteLine("Initialize driver... Start with url:" + $"{options.SeleniumGridAddress}wd/hub");
                driver = new RemoteWebDriver(new Uri(url), DesiredCapabilities.Chrome())
                        .DisposeWith(instanceDisposer);
                Console.WriteLine("Initialize driver...End");
            }
            catch (Exception e)
            {
                Console.WriteLine("Initialize driver...ERROR");
                Console.WriteLine(e);
                throw;
            }
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
