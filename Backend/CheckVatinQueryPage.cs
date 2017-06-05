using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Nippin;
using System.Reactive.Linq;

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
            var req1 = driver.Repeat(() =>
            {
                var element = driver.FindElements(By.Id("b-8")).FirstOrDefault();
                if (element == null) return false;
                if (!element.Displayed) return false;

                if (element.Text != "Sprawdź") return false;

                return true;
            }, deadline);

            var req2 = driver.Repeat(() =>
            {
                // button 'Wyczyść' need to be hidden
                var element = driver.FindElements(By.Id("b-9")).FirstOrDefault();
                if (element == null) return false; ;
                if (element.Displayed) return false;

                return true;
            }, deadline);

            return Task.WhenAll(req1, req2);
        }


        private void OnSubmit()
        {
            var element = default(IWebElement);

            driver.Repeat(() =>
            {
                var current = driver.FindElements(By.Id("b-8")).FirstOrDefault();

                if (current == null) return false;
                if (!current.Displayed) return false;

                element = current;
                return true;
            }, new CancellationTokenSource(10.Seconds()).Token)
            .Wait();

            element.Click();
        }

        private void OnVatin(string value)
        {
            var element = default(IWebElement);

            {
                var now = DateTime.Now;
                while (DateTime.Now - now < TimeSpan.FromSeconds(10))
                {
                    element = driver.FindElements(By.Id("b-7")).FirstOrDefault();
                    if (element != null) break;
                }
            }

            if (element == null) throw new NoSuchElementException();

            {
                var now = DateTime.Now;
                while (DateTime.Now - now < TimeSpan.FromSeconds(10))
                {
                    // in some cases only part of value is sent to element
                    element.SendKeys(value);

                    var actual = element.GetAttribute("value");
                    if (actual == value) break;

                    element.Clear();
                }
            }

        }
    }
}
