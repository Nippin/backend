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

                if (element == null) continue;
            }

            if (element == null) throw new NoSuchElementException();

            // variable 'element' represents button titled 'Sprawdź' used for submit data.
            // But clicking the element sometimes work, sometimes not and I can't understand how
            // 
            // we need reliable functionality we will click the baton as long as will be 
            // ,arked as 'stale' so to stabilize application to have always working *submit* functionality.
            // The solution is trying to click them as long as the element is available on the screen
            try
            {
                do
                {
                    element.Click();

                    // in fact we don't need it selected, but it is known property
                    // wchich throws StaleElementReferenceException when element is not yet on the browser DOM model.
                } while (!element.Selected);
            } catch (StaleElementReferenceException)
            {

            }
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
