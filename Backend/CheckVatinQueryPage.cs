using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Backend
{
    public sealed class CheckVatinQueryPage : IPage
    {
        public Action Submit { get; private set; }
        public Action<string> Vatin { get; private set; }

        private RemoteWebDriver driver;

        public void Initialize(RemoteWebDriver driver)
        {
            this.driver = driver;
            this.Submit = OnSubmit;
            this.Vatin = OnVatin;
        }

        public Task Identified(CancellationToken deadline)
        {
            var tcs = new TaskCompletionSource<object>();

            Task.Run(async () =>
            {
                while (true)
                {
                    if (deadline.IsCancellationRequested)
                    {
                        tcs.SetCanceled();
                        return;
                    }

                    await Task.Delay(100);

                    {
                        // button 'Sprawdź' need to be visible
                        var element = driver.FindElements(By.Id("b-8")).FirstOrDefault();
                        if (element == null) continue;
                        if (!element.Displayed) continue;
                        if (element.Text != "Sprawdź") continue;
                    }

                    {
                        // button 'Wyczyść' need to be hidden
                        var element = driver.FindElements(By.Id("b-9")).FirstOrDefault();
                        if (element == null) continue;
                        if (element.Displayed) continue;
                    }

                    tcs.SetResult(null);
                    return;
                }
            });

            return tcs.Task;
        }

        private void OnSubmit()
        {
            var element = default(IWebElement);

            var now = DateTime.Now;
            while (DateTime.Now - now < TimeSpan.FromSeconds(10))
            {
                element = driver.FindElements(By.Id("b-8")).FirstOrDefault();

                if (element == null) continue;

                break;
            }

            if (element == null) throw new NoSuchElementException();

            element.Click();
        }

        private void OnVatin(string value)
        {
            var element = default(IWebElement);

            var now = DateTime.Now;
            while (DateTime.Now - now < TimeSpan.FromSeconds(10))
            {
                element = driver.FindElements(By.Id("b-7")).FirstOrDefault();
                if (element != null) break;
            }

            if (element == null) throw new NoSuchElementException();

            element.SendKeys(value);
        }
    }
}
