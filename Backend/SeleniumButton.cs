using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Backend
{
    sealed class SeleniumButton : IButton
    {
        private readonly RemoteWebDriver driver;
        Func<RemoteWebDriver, IWebElement> getElement;

        public SeleniumButton(RemoteWebDriver driver, Func<RemoteWebDriver, IWebElement> getElement)
        {
            this.driver = driver;
            this.getElement = getElement;
        }

        public void Click()
        {
            var now = DateTime.Now;
            while (DateTime.Now - now < TimeSpan.FromSeconds(5))
            {

                try
                {
                    var item = getElement(driver);
                    
                    item.Click();
                }
                catch (StaleElementReferenceException ignored) { }
                catch (NoSuchElementException ignored) { }
                catch (ElementNotVisibleException ignored) { }

                break;
            }
        }
    }
}
