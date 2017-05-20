using OpenQA.Selenium;
using System;

namespace Backend
{
    public static class WebElementExtensions
    {
        public static IButton AsButton(this IWebElement element)
        {
            return new SeleniumButton(element);
        }

        public static IInput AsInput(this IWebElement element)
        {
            return new SeleniumInput();
        }
    }
}
