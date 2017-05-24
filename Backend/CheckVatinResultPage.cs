﻿using Nippin;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backend
{
    public sealed class CheckVatinResultPage : IPage
    {
        public Action Clear { get; private set; }

        private RemoteWebDriver driver;

        public Task Initialize(RemoteWebDriver driver)
        {
            this.driver = driver;

            return Task.Run(() =>
            {
                var element = default(IWebElement);

                var now = DateTime.Now;
                while (DateTime.Now - now < TimeSpan.FromSeconds(10))
                {
                    // try to recognize page base on string: Data sprawdzenia:
                    element = driver.FindElements(By.Id("caption2_b-b")).FirstOrDefault();
                    if (element != null) break;
                }

                if (element == null) throw new NoSuchElementException();


                // run script located @ Clear button CLick event directly
                // because trying to click Clear button directly was very tiought
                // - clicking resulted with some Selenium exceptions.
                Clear = OnClear;
            });
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
