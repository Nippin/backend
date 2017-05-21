using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;

namespace Backend
{
    sealed class SeleniumInput : IInput
    {

        private readonly RemoteWebDriver driver;
        private readonly Func<RemoteWebDriver, IWebElement> getElement;

        public SeleniumInput(RemoteWebDriver driver, Func<RemoteWebDriver, IWebElement> getElement)
        {
            this.driver = driver;
            this.getElement = getElement;
        }

        public void SetText(string value)
        {
            var now = DateTime.Now;
            while (DateTime.Now - now < TimeSpan.FromSeconds(5))
            {

                try
                {
                    var item = getElement(driver);
                    item.SendKeys(value);
                }
                catch (StaleElementReferenceException ignored) { }
                catch (NoSuchElementException ignored) { }

                break;
            }
        }
    }
}
