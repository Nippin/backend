using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
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
                var vatinInput = driver.FindElementById("b-7").AsInput();
                var submitButton = driver.FindElementById("b-8").AsButton();

                Vatin = vatinInput;
                Submit = submitButton;
            });
        }
    }
}
