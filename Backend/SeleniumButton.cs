using System;
using OpenQA.Selenium;

namespace Backend
{
    sealed class SeleniumButton : IButton
    {
        private readonly IWebElement nativeElement;
        public SeleniumButton(IWebElement nativeElement)
        {
            this.nativeElement = nativeElement;
        }

        public void Click()
        {
            nativeElement.Click();
        }
    }
}
