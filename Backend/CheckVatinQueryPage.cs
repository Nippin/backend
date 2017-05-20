﻿using OpenQA.Selenium.Remote;
using System.Threading.Tasks;

namespace Backend
{
    public sealed class CheckVatinQueryPage : IPage
    {
        public IButton Submit { get; private set; }
        public IInput Vatin { get; private set; }

        private readonly RemoteWebDriver driver;
        public CheckVatinQueryPage(RemoteWebDriver driver)
        {
            this.driver = driver;
        }

        public Task Initialize()
        {
            return Task.Run(() =>
            {
                var vatinInput = driver.FindElementById("b-7").AsInput();
                var submitButton = driver.FindElementById("b-8").AsButton();

                Vatin = vatinInput;
                Submit = submitButton;
            });
        }
    }
}