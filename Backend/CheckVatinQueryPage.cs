using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backend
{
    public sealed class CheckVatinQueryPage : IPage
    {
        public Action Submit { get; private set; }
        public Action<string> Vatin { get; private set; }

        private RemoteWebDriver driver;

        public Task Initialize(RemoteWebDriver driver)
        {
            this.driver = driver;
            this.Submit = OnSubmit;
            this.Vatin = OnVatin;

            return Task.CompletedTask;
        }

        private void OnSubmit()
        {
            var element = default(IWebElement);

            var now = DateTime.Now;
            while (DateTime.Now - now < TimeSpan.FromSeconds(10))
            {
                element = driver.FindElements(By.Id("b-8")).FirstOrDefault();
                if (element != null) break;
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
