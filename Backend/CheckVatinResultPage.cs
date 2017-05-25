using Nippin;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Backend
{
    public sealed class CheckVatinResultPage : IPage
    {
        public Action Clear { get; private set; }

        private RemoteWebDriver driver;

        public void Initialize(RemoteWebDriver driver)
        {
            this.driver = driver;
            Clear = OnClear;
        }

        /// <summary>
        /// The page contains 
        /// * button titled Wyczyść
        /// * {span} starting with 'Data sprawdzenia:'
        /// </summary>
        /// <param name="deadline">Maksimum waiting time for page characteristic elements.</param>
        /// <returns>Task completed if elements have been found.</returns>
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
                        // button 'Wyczyść'
                        var element = driver.FindElements(By.Id("b-9")).FirstOrDefault();
                        if (element == null) continue;
                        if (!element.Displayed) continue;
                    }

                    {
                        // button 'Wyczyść'
                        var element = driver.FindElements(By.Id("caption2_b-b")).FirstOrDefault();
                        if (element == null) continue;
                        if (!element.Displayed) continue;
                        var content = element.Text;
                        if (!content.StartsWith("Data sprawdzenia:")) continue;
                    }

                    tcs.SetResult(null);
                    return;
                }
            })
            .ContinueWith(t => tcs.SetException(t.Exception), TaskContinuationOptions.OnlyOnFaulted);

            return tcs.Task;
        }

        public string Print()
        {
            return driver.GetScreenshot().AsBase64EncodedString;
        }

        private void OnClear()
        {
            repeat:
            try
            {
                var element = default(IWebElement);
                var now = DateTime.Now;
                while (DateTime.Now - now < TimeSpan.FromSeconds(10))
                {
                    // try to recognize page base on string: Data sprawdzenia:
                    var found = driver.FindElements(By.Id("b-9")).FirstOrDefault();
                    if (found == null) continue;
                    if (!found.Displayed) continue;

                    element = found;
                    break;
                }

                if (element == null) throw new NoSuchElementException();

                element.Click();
            }
            catch (StaleElementReferenceException repeat)
            {
                goto repeat;
            }

        }
    }
}
