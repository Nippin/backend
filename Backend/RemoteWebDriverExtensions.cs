using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;

namespace Backend
{
    public static class RemoteWebDriverExtensions
    {
        public static IButton AsButton(this RemoteWebDriver driver, Func<RemoteWebDriver, IWebElement> element)
        {
            return new SeleniumButton(driver, element);
        }

        public static IInput AsInput(this RemoteWebDriver driver, Func<RemoteWebDriver, IWebElement> element)
        {
            return new SeleniumInput(driver, element);
        }

        public static Action AsScript(this RemoteWebDriver driver, string script)
        {
            return () => driver.ExecuteScript(script);
        }
    }
}
