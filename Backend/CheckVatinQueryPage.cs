using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Threading.Tasks;

namespace Backend
{
    public sealed class CheckVatinQueryPage : IPage
    {
        public IButton Submit { get; private set; }
        public IInput Vatin { get; private set; }

        private RemoteWebDriver driver;

        public Task Initialize(RemoteWebDriver driver)
        {
            this.driver = driver;

            return Task.Run(() =>
            {
                var vatinInput = driver.AsInput(d => d.FindElementById("b-7"));
                var submitButton = driver.AsButton(d => d.FindElementById("b-8"));

                Vatin = vatinInput;
                Submit = submitButton;

            });
        }
    }
}
