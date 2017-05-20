using OpenQA.Selenium.Remote;
using System.Threading.Tasks;

namespace Backend
{
    public sealed class CheckVatinPage : IPage
    {
        public IButton Clear { get; private set; }

        private readonly RemoteWebDriver driver;
        public CheckVatinPage(RemoteWebDriver driver)
        {
            this.driver = driver;
        }

        public Task Initialize()
        {
            return Task.Run(() =>
            {
                var clearButton = driver.FindElementById("b-9").AsButton();

                Clear = clearButton;
            });
        }
    }
}
