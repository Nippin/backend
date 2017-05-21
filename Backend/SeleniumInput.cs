using OpenQA.Selenium;

namespace Backend
{
    sealed class SeleniumInput : IInput
    {
        private readonly IWebElement linked;

        public SeleniumInput(IWebElement nativeElement)
        {
            linked = nativeElement;
        }

        public void SetText(string value)
        {
            linked.SendKeys(value);
        }
    }
}
